using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private PathController pathController;
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

        var spawnPosition = waypoints[0].position;

        for (var i = 0; i < enemyCount; i++)
        {
            var instance = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            var follower = instance.GetComponent<EnemyPathFollower>();
            if (follower != null)
                follower.Init(pathController);

            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }
}