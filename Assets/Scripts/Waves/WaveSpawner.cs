using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private PathController pathController;
    [SerializeField] private Transform enemiesRoot;
    [SerializeField] private float spawnInterval = 0.5f;

    private bool isSpawning;
    public bool IsSpawning => isSpawning;

    public void StartWave(int enemyCount)
    {
        if (isSpawning)
            return;

        if (enemyPrefab == null)
            return;

        if (pathController == null)
            return;

        if (enemyCount <= 0)
            return;

        StartCoroutine(SpawnWave(enemyCount));
    }

    private IEnumerator SpawnWave(int enemyCount)
    {
        isSpawning = true;

        var waypoints = pathController.Waypoints;
        if (waypoints == null || waypoints.Length == 0)
        {
            isSpawning = false;
            yield break;
        }

        var startPoint = waypoints[0].position;
        var secondPoint = waypoints.Length > 1 ? waypoints[1].position : startPoint;
        var backDir = (startPoint - secondPoint).normalized;

        for (var i = 0; i < enemyCount; i++)
        {
            var spawnPosition = startPoint + backDir * (i * 0.5f);

            var instance = enemiesRoot != null ? Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, enemiesRoot) : Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            var follower = instance.GetComponent<EnemyPathFollower>();
            if (follower != null)
                follower.Init(pathController);

            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }
}