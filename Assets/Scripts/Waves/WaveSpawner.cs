using System.Collections;
using Core;
using Enemies;
using Level;
using UnityEngine;

namespace Waves
{
    [DisallowMultipleComponent]
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private PathController pathController;
        [SerializeField] private Transform enemiesRoot;

        [Min(0f)] [SerializeField] private float spawnInterval = 0.5f;

        public bool IsSpawning { get; private set; }

        public void StartWave(WaveDefinition waveDefinition)
        {
            if (waveDefinition == null)
                return;

            if (!gameObject.activeInHierarchy)
                return;

            StopAllCoroutines();
            StartCoroutine(SpawnWaveRoutine(waveDefinition));
        }

        private IEnumerator SpawnWaveRoutine(WaveDefinition waveDefinition)
        {
            IsSpawning = true;

            var waypoints = pathController != null ? pathController.Waypoints : null;
            if (waypoints == null || waypoints.Length == 0)
            {
                IsSpawning = false;
                yield break;
            }

            foreach (var spawn in waveDefinition.Enemies)
            {
                if (spawn == null)
                    continue;

                var config = spawn.EnemyConfig;
                var count = spawn.Count;

                if (config == null || config.Prefab == null)
                    continue;

                if (count <= 0)
                    continue;

                for (var i = 0; i < count; i++)
                {
                    var startPosition = waypoints[0].position;
                    var parent = enemiesRoot ? enemiesRoot : transform;

                    var instance = Instantiate(config.Prefab, startPosition, Quaternion.identity, parent);

                    var enemy = instance.GetComponent<Enemy>();
                    if (enemy != null)
                        enemy.Init(config);

                    var follower = instance.GetComponent<EnemyPathFollower>();
                    if (follower != null && pathController != null)
                        follower.Init(pathController);

                    var gameManager = GameManager.Instance;
                    if (gameManager != null)
                        gameManager.RegisterEnemySpawn();

                    if (spawnInterval > 0f)
                        yield return new WaitForSeconds(spawnInterval);
                    else
                        yield return null;
                }
            }

            IsSpawning = false;
        }
    }
}