using System;
using UnityEngine;
using Waves;

namespace Core
{
    public enum GameState
    {
        BuildPhase,
        CombatPhase,
        GameOver
    }

    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        [Header("Default settings")] [SerializeField]
        private ResourceManager resources = new();

        [SerializeField] private TowerLottery lottery = new();

        [Header("Waves / Player")] [Min(1)] [SerializeField]
        private int playerLevel = 1;

        [Min(0)] [SerializeField] private int lumberRewardPerWave = 5;

        [Header("Refs")] [SerializeField] private WaveSpawner waveSpawner;

        [SerializeField] private LevelWavesConfig levelWavesConfig;

        private int _activeEnemies;
        private bool _canRewardWaveEnd;

        public static GameManager Instance { get; private set; }

        public ResourceManager Resources => resources;
        public TowerLottery Lottery => lottery;
        public int PlayerLevel => playerLevel;
        public GameState State { get; private set; } = GameState.BuildPhase;
        public int Wave { get; private set; }

        private bool IsGameOver => State == GameState.GameOver;

        private bool CanStartNextWave
        {
            get
            {
                if (State != GameState.BuildPhase) return false;
                if (levelWavesConfig == null || waveSpawner == null) return false;
                if (IsGameOver) return false;
                if (HasActiveEnemies()) return false;
                return !waveSpawner.IsSpawning;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            var startLives = Mathf.Max(1, resources.Lives);
            resources.Init(startLives);
        }

        private void Start()
        {
            resources.NotifyAll();
            WaveChanged?.Invoke(Wave);
            GameStateChanged?.Invoke(State);
        }

        public event Action<int> WaveChanged;
        public event Action<GameState> GameStateChanged;

        public void SetPlayerLevel(int level)
        {
            playerLevel = Mathf.Max(1, level);
        }

        public void NextWave()
        {
            if (!CanStartNextWave) return;

            var nextWaveNum = Wave + 1;
            var waveDef = levelWavesConfig.GetWaveByNumber(nextWaveNum);

            if (waveDef == null)
            {
                Debug.Log("Waves finished or not configured.");
                return;
            }

            _activeEnemies = 0;
            _canRewardWaveEnd = true;

            waveSpawner.StartWave(waveDef);
            SetState(GameState.CombatPhase);

            Wave = nextWaveNum;
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

            CheckWaveEnd();
        }

        public bool HasActiveEnemies()
        {
            return _activeEnemies > 0;
        }

        public void OnLivesDepleted()
        {
            if (State == GameState.GameOver)
                return;

            SetState(GameState.GameOver);
        }

        private void CheckWaveEnd()
        {
            if (!_canRewardWaveEnd) return;
            if (HasActiveEnemies()) return;
            if (waveSpawner != null && waveSpawner.IsSpawning) return;

            if (resources.Lives > 0)
                resources.AddLumber(lumberRewardPerWave);

            _canRewardWaveEnd = false;
            SetState(GameState.BuildPhase);
        }

        private void SetState(GameState newState)
        {
            if (State == newState) return;

            State = newState;
            GameStateChanged?.Invoke(State);
        }
    }
}