using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI lumberText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI buildErrorText;
    [SerializeField] private Button startWaveButton;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private float buildErrorDefaultDuration = 2f;

    private float buildErrorTimer;

    private void Awake()
    {
        if (startWaveButton != null)
            startWaveButton.onClick.AddListener(OnStartWaveClicked);

        if (buildErrorText != null)
            buildErrorText.text = string.Empty;
    }

    private void Update()
    {
        if (gameManager != null)
        {
            if (goldText != null)
                goldText.text = $"Gold: {gameManager.Gold}";

            if (lumberText != null)
                lumberText.text = $"Lumber: {gameManager.Lumber}";

            if (livesText != null)
                livesText.text = $"Lives: {gameManager.Lives}";

            if (waveText != null)
                waveText.text = $"Wave: {gameManager.Wave}";
        }

        if (buildErrorTimer > 0f)
        {
            buildErrorTimer -= Time.deltaTime;

            if (buildErrorTimer <= 0f && buildErrorText != null)
                buildErrorText.text = string.Empty;
        }
    }

    private void OnDestroy()
    {
        if (startWaveButton != null)
            startWaveButton.onClick.RemoveListener(OnStartWaveClicked);
    }

    private void OnStartWaveClicked()
    {
        if (gameManager != null)
            gameManager.NextWave();
    }

    public void ShowBuildError(string message, float duration)
    {
        if (buildErrorText == null)
            return;

        buildErrorText.text = message;
        buildErrorTimer = duration > 0f ? duration : buildErrorDefaultDuration;
    }
}