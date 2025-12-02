using System;
using Towers;
using UnityEngine;
using Waves;
using Random = UnityEngine.Random;

namespace Core
{
    public enum GameState
    {
        BuildPhase,
        CombatPhase,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        [Serializable]
        public class TowerQualityWeights
        {
            public int playerLevel;
            public int chipped;
            public int flawed;
            public int normal;
            public int flawless;
            public int perfect;

            public int GetWeight(GemQuality quality)
            {
                return quality switch
                {
                    GemQuality.Chipped => chipped,
                    GemQuality.Flawed => flawed,
                    GemQuality.Normal => normal,
                    GemQuality.Flawless => flawless,
                    GemQuality.Perfect => perfect,
                    _ => 0
                };
            }

            public int TotalWeight =>
                chipped + flawed + normal + flawless + perfect;
        }

        [field: SerializeField] public int Gold { get; private set; }
        [field: SerializeField] public int Lumber { get; private set; }
        [field: SerializeField] public int Lives { get; private set; } = 50;
        [field: SerializeField] public int Wave { get; private set; }

        [field: SerializeField] public GameState State { get; private set; } = GameState.BuildPhase;

        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private LevelWavesConfig levelWavesConfig;
        [SerializeField] private int lumberRewardPerWave = 5;
        [SerializeField] private int playerLevel = 1;
        [SerializeField] private TowerQualityWeights[] towerQualityLevels;

        private int _activeEnemies;
        private bool _canRewardWaveEnd;

        public static GameManager Instance { get; private set; }

        public bool HasActiveEnemies => _activeEnemies > 0;
        public int PlayerLevel => playerLevel;

        public event Action<int> GoldChanged;
        public event Action<int> LumberChanged;
        public event Action<int> LivesChanged;
        public event Action<int> WaveChanged;
        public event Action<GameState> GameStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void SetState(GameState newState)
        {
            if (State == newState)
                return;

            State = newState;
            GameStateChanged?.Invoke(State);
        }

        public void SetPlayerLevel(int level)
        {
            if (level < 1)
                level = 1;

            playerLevel = level;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
                return;

            Gold += amount;
            if (Gold < 0)
                Gold = 0;

            GoldChanged?.Invoke(Gold);
        }

        public void AddLumber(int amount)
        {
            if (amount <= 0)
                return;

            Lumber += amount;
            if (Lumber < 0)
                Lumber = 0;

            LumberChanged?.Invoke(Lumber);
        }

        public void ChangeLives(int delta)
        {
            if (delta == 0)
                return;

            Lives += delta;
            if (Lives < 0)
                Lives = 0;

            LivesChanged?.Invoke(Lives);

            if (Lives <= 0 && State != GameState.GameOver)
                SetState(GameState.GameOver);
        }

        public void SpendResources(int goldCost, int lumberCost)
        {
            var changedGold = false;
            var changedLumber = false;

            if (goldCost > 0)
            {
                Gold -= goldCost;
                if (Gold < 0)
                    Gold = 0;
                changedGold = true;
            }

            if (lumberCost > 0)
            {
                Lumber -= lumberCost;
                if (Lumber < 0)
                    Lumber = 0;
                changedLumber = true;
            }

            if (changedGold)
                GoldChanged?.Invoke(Gold);

            if (changedLumber)
                LumberChanged?.Invoke(Lumber);
        }

        public void NextWave()
        {
            if (waveSpawner == null)
                return;

            if (State != GameState.BuildPhase)
                return;

            if (waveSpawner.IsSpawning)
                return;

            if (_activeEnemies > 0)
                return;

            if (levelWavesConfig == null)
                return;

            var nextWaveNumber = Wave + 1;

            var waveDefinition = levelWavesConfig.GetWaveByNumber(nextWaveNumber);
            if (waveDefinition == null)
                return;

            _activeEnemies = 0;
            _canRewardWaveEnd = true;

            waveSpawner.StartWave(waveDefinition);
            SetState(GameState.CombatPhase);

            Wave = nextWaveNumber;
            WaveChanged?.Invoke(Wave);
        }

        public void RegisterEnemySpawn()
        {
            _activeEnemies++;
        }

        public void NotifyEnemyRemoved()
        {
            if (_activeEnemies > 0)
                _activeEnemies--;

            TryRewardWaveEnd();
        }

        private void TryRewardWaveEnd()
        {
            if (!_canRewardWaveEnd)
                return;

            if (_activeEnemies > 0)
                return;

            if (waveSpawner != null && waveSpawner.IsSpawning)
                return;

            if (Lives <= 0)
            {
                _canRewardWaveEnd = false;
                return;
            }

            if (lumberRewardPerWave > 0)
                AddLumber(lumberRewardPerWave);

            _canRewardWaveEnd = false;
            SetState(GameState.BuildPhase);
        }

        public GemQuality RollTowerQuality()
        {
            var row = GetQualityRowForLevel(playerLevel);
            if (row == null)
            {
                if (towerQualityLevels == null || towerQualityLevels.Length == 0)
                    return GemQuality.Chipped;

                row = towerQualityLevels[^1];
            }

            var wChipped = row.chipped;
            var wFlawed = row.flawed;
            var wNormal = row.normal;
            var wFlawless = row.flawless;
            var wPerfect = row.perfect;

            var total = wChipped + wFlawed + wNormal + wFlawless + wPerfect;
            if (total <= 0)
                return GemQuality.Chipped;

            var roll = Random.Range(0, total);

            if (roll < wChipped)
                return GemQuality.Chipped;
            roll -= wChipped;

            if (roll < wFlawed)
                return GemQuality.Flawed;
            roll -= wFlawed;

            if (roll < wNormal)
                return GemQuality.Normal;
            roll -= wNormal;

            if (roll < wFlawless)
                return GemQuality.Flawless;

            return GemQuality.Perfect;
        }

        private TowerQualityWeights GetQualityRowForLevel(int level)
        {
            if (towerQualityLevels == null || towerQualityLevels.Length == 0)
                return null;

            TowerQualityWeights best = null;
            var bestLevel = int.MinValue;

            foreach (var row in towerQualityLevels)
            {
                if (row == null)
                    continue;

                if (row.playerLevel > level || row.playerLevel <= bestLevel) continue;
                best = row;
                bestLevel = row.playerLevel;
            }

            return best;
        }
        
        public GemQuality GetRandomQualityForLevel(int level)
        {
            var row = GetQualityRowForLevel(level);
            if (row == null)
                return GemQuality.Chipped;

            var total = row.TotalWeight;
            if (total <= 0)
                return GemQuality.Chipped;

            var roll = Random.Range(0, total);

            if (roll < row.chipped)
                return GemQuality.Chipped;
            roll -= row.chipped;

            if (roll < row.flawed)
                return GemQuality.Flawed;
            roll -= row.flawed;

            if (roll < row.normal)
                return GemQuality.Normal;
            roll -= row.normal;

            if (roll < row.flawless)
                return GemQuality.Flawless;

            return GemQuality.Perfect;
        }

        public GemQuality GetRandomQualityForCurrentLevel()
        {
            return GetRandomQualityForLevel(playerLevel);
        }

    }
}