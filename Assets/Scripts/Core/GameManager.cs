using System;
using Towers;
using UnityEngine;
using Waves;
using Random = UnityEngine.Random;

namespace Core
{
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
                switch (quality)
                {
                    case GemQuality.Chipped:
                        return chipped;
                    case GemQuality.Flawed:
                        return flawed;
                    case GemQuality.Normal:
                        return normal;
                    case GemQuality.Flawless:
                        return flawless;
                    case GemQuality.Perfect:
                        return perfect;
                    default:
                        return 0;
                }
            }
        }

        [field: SerializeField] public int Gold { get; private set; }
        [field: SerializeField] public int Lumber { get; private set; }
        [field: SerializeField] public int Lives { get; private set; } = 50;
        [field: SerializeField] public int Wave { get; private set; }

        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private LevelWavesConfig levelWavesConfig;
        [SerializeField] private int lumberRewardPerWave = 5;
        [SerializeField] private int playerLevel = 1;
        [SerializeField] private TowerQualityWeights[] towerQualityLevels;

        public int PlayerLevel => playerLevel;

        public void SetPlayerLevel(int level)
        {
            if (level < 1)
                level = 1;
            playerLevel = level;
        }

        private int _activeEnemies;
        private bool _canRewardWaveEnd;

        public static GameManager Instance { get; private set; }

        public bool HasActiveEnemies => _activeEnemies > 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
                return;

            Gold += amount;
        }

        public void AddLumber(int amount)
        {
            if (amount <= 0)
                return;

            Lumber += amount;
        }

        public void ChangeLives(int delta)
        {
            Lives += delta;
        }

        public void SpendResources(int goldCost, int lumberCost)
        {
            if (goldCost > 0)
                Gold -= goldCost;

            if (lumberCost > 0)
                Lumber -= lumberCost;
        }

        public void NextWave()
        {
            if (waveSpawner == null)
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
            Wave = nextWaveNumber;
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

            if (lumberRewardPerWave <= 0)
            {
                _canRewardWaveEnd = false;
                return;
            }

            AddLumber(lumberRewardPerWave);
            _canRewardWaveEnd = false;
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

                if (row.playerLevel <= level && row.playerLevel > bestLevel)
                {
                    best = row;
                    bestLevel = row.playerLevel;
                }
            }

            return best;
        }
    }
}