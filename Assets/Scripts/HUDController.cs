using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI lumberText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private Button startWaveButton;
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        startWaveButton.onClick.AddListener(OnStartWaveClicked);
    }

    private void Update()
    {
        if (gameManager == null)
            return;

        goldText.text = $"Gold: {gameManager.Gold}";
        lumberText.text = $"Lumber: {gameManager.Lumber}";
        livesText.text = $"Lives: {gameManager.Lives}";
        waveText.text = $"Wave: {gameManager.Wave}";
    }

    private void OnStartWaveClicked()
    {
        if (gameManager == null)
            return;

        gameManager.NextWave();
    }
}