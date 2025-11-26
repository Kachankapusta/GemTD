using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [field: SerializeField] public int Gold { get; private set; } = 0;
    [field: SerializeField] public int Lumber { get; private set; } = 0;
    [field: SerializeField] public int Lives { get; private set; } = 50;
    [field: SerializeField] public int Wave { get; private set; } = 1;

    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private int baseEnemiesPerWave = 10;
    [SerializeField] private int enemiesPerWaveIncrement = 0;

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

    public void NextWave()
    {
        if (waveSpawner == null)
            return;

        if (waveSpawner.IsSpawning)
            return;

        var enemyCount = GetEnemyCountForWave(Wave);
        waveSpawner.StartWave(enemyCount);
        Wave++;
    }

    private int GetEnemyCountForWave(int waveNumber)
    {
        var result = baseEnemiesPerWave + enemiesPerWaveIncrement * (waveNumber - 1);
        if (result < 1)
            result = 1;
        return result;
    }
}