using System;
using System.Linq;
using Enemies;
using UnityEngine;

namespace Waves
{
    [Serializable]
    public class EnemySpawn
    {
        [SerializeField] private EnemyConfig enemyConfig;
        [SerializeField] private int count = 10;

        public EnemyConfig EnemyConfig => enemyConfig;
        public int Count => count < 0 ? 0 : count;
    }

    [Serializable]
    public class WaveDefinition
    {
        [SerializeField] private int waveNumber = 1;
        [SerializeField] private EnemySpawn[] enemies;

        public int WaveNumber => waveNumber;
        public EnemySpawn[] Enemies => enemies;
    }

    [CreateAssetMenu(menuName = "GemTD/Level Waves Config", fileName = "LevelWavesConfig")]
    public class LevelWavesConfig : ScriptableObject
    {
        [SerializeField] private WaveDefinition[] waves;

        public WaveDefinition[] Waves => waves;

        public WaveDefinition GetWaveByNumber(int waveNumber)
        {
            if (waves == null || waves.Length == 0)
                return null;

            return waves.FirstOrDefault(t => t.WaveNumber == waveNumber);
        }

        public WaveDefinition GetWaveByIndex(int index)
        {
            if (waves == null)
                return null;

            if (index < 0 || index >= waves.Length)
                return null;

            return waves[index];
        }
    }
}