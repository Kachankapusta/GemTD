using System.Collections;
using Core;
using Enemies;
using Level;
using UnityEngine;

namespace Waves
{
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private PathController pathController;
        [SerializeField] private Transform enemiesRoot;
        [SerializeField] private float spawnInterval = 0.5f;

        public bool IsSpawning { get; private set; }

        public void StartWave(WaveDefinition waveDefinition)
        {
            if (IsSpawning)
                return;

            if (waveDefinition == null)
                return;

            if (pathController == null)
                return;

            StartCoroutine(SpawnWave(waveDefinition));
        }

        private IEnumerator SpawnWave(WaveDefinition waveDefinition)
        {
            IsSpawning = true;

            var waypoints = pathController.Waypoints;
            if (waypoints == null || waypoints.Length == 0)
            {
                IsSpawning = false;
                yield break;
            }

            var enemies = waveDefinition.Enemies;
            if (enemies == null || enemies.Length == 0)
            {
                IsSpawning = false;
                yield break;
            }

            var startPoint = waypoints[0].position;
            var secondPoint = waypoints.Length > 1 ? waypoints[1].position : startPoint;
            var backDir = (startPoint - secondPoint).normalized;

            var spawnIndex = 0;

            foreach (var spawnInfo in enemies)
            {
                var config = spawnInfo.EnemyConfig;
                var count = spawnInfo.Count;

                if (config == null || config.Prefab == null || count <= 0)
                    continue;

                for (var j = 0; j < count; j++)
                {
                    var spawnPosition = startPoint + backDir * (spawnIndex * 0.5f);
                    spawnIndex++;

                    var instance = enemiesRoot != null
                        ? Instantiate(config.Prefab, spawnPosition, Quaternion.identity, enemiesRoot)
                        : Instantiate(config.Prefab, spawnPosition, Quaternion.identity);

                    var follower = instance.GetComponent<EnemyPathFollower>();
                    if (follower != null)
                        follower.Init(pathController);

                    var enemy = instance.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.Init(config);

                        var gameManager = GameManager.Instance;
                        if (gameManager != null)
                            gameManager.RegisterEnemySpawn();
                    }

                    yield return new WaitForSeconds(spawnInterval);
                }
            }

            IsSpawning = false;
        }
    }
}