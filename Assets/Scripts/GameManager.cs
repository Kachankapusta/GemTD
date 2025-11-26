using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [field: SerializeField] public int Gold { get; private set; } = 0;
    [field: SerializeField] public int Lumber { get; private set; } = 0;
    [field: SerializeField] public int Lives { get; private set; } = 50;
    [field: SerializeField] public int Wave { get; private set; } = 1;

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
        Gold += amount;
    }

    public void AddLumber(int amount)
    {
        Lumber += amount;
    }

    public void ChangeLives(int delta)
    {
        Lives += delta;
    }

    public void NextWave()
    {
        Wave++;
        // здесь позже будет логика запуска волны
    }
}
