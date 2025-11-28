using UnityEngine;
using Waves;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        [field: SerializeField] public int Gold { get; private set; }
        [field: SerializeField] public int Lumber { get; private set; }
        [field: SerializeField] public int Lives { get; private set; } = 50;
        [field: SerializeField] public int Wave { get; private set; }

        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private LevelWavesConfig levelWavesConfig;
        [SerializeField] private int lumberRewardPerWave = 5;

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

        public bool HasEnoughResources(int goldCost, int lumberCost)
        {
            if (goldCost < 0)
                goldCost = 0;

            if (lumberCost < 0)
                lumberCost = 0;

            return Gold >= goldCost && Lumber >= lumberCost;
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

            Lumber += lumberRewardPerWave;
            _canRewardWaveEnd = false;
        }
    }
}