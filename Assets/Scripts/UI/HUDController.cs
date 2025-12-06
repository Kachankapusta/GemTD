using Core;
using TMPro;
using Towers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [DisallowMultipleComponent]
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
        [SerializeField] private TowerBuilder towerBuilder;
        [SerializeField] private TowerPanelUI towerPanel;

        private float _buildErrorTimer;

        private void Update()
        {
            if (buildErrorText == null)
                return;

            if (_buildErrorTimer <= 0f)
                return;

            _buildErrorTimer -= Time.deltaTime;
            if (_buildErrorTimer <= 0f)
                buildErrorText.text = string.Empty;
        }

        private void OnEnable()
        {
            if (gameManager == null)
                gameManager = GameManager.Instance;

            if (startWaveButton != null)
                startWaveButton.onClick.AddListener(OnStartWaveClicked);

            if (gameManager == null)
                return;

            gameManager.Resources.GoldChanged += OnGoldChanged;
            gameManager.Resources.LumberChanged += OnLumberChanged;
            gameManager.Resources.LivesChanged += OnLivesChanged;
            gameManager.WaveChanged += OnWaveChanged;
            gameManager.GameStateChanged += OnGameStateChanged;

            UpdateAllTexts();
            OnGameStateChanged(gameManager.State);
        }

        private void OnDisable()
        {
            if (startWaveButton != null)
                startWaveButton.onClick.RemoveListener(OnStartWaveClicked);

            if (gameManager == null)
                return;

            gameManager.Resources.GoldChanged -= OnGoldChanged;
            gameManager.Resources.LumberChanged -= OnLumberChanged;
            gameManager.Resources.LivesChanged -= OnLivesChanged;
            gameManager.WaveChanged -= OnWaveChanged;
            gameManager.GameStateChanged -= OnGameStateChanged;
        }

        private void OnStartWaveClicked()
        {
            if (gameManager == null)
                return;

            if (gameManager.State != GameState.BuildPhase)
            {
                ShowBuildError("You can start a wave only in build phase.", 2f);
                return;
            }

            if (towerBuilder != null && towerBuilder.IsSelectionMode)
            {
                ShowBuildError("Choose one tower before starting the next wave.", 2f);
                return;
            }

            if (gameManager.Resources.Lumber > 0)
            {
                ShowBuildError("Spend all lumber before starting the next wave.", 2f);
                return;
            }

            if (gameManager.HasActiveEnemies())
            {
                ShowBuildError("You must finish the current wave first.", 2f);
                return;
            }

            gameManager.NextWave();
        }

        public void ShowBuildError(string message, float duration)
        {
            if (buildErrorText == null)
                return;

            buildErrorText.text = message;
            _buildErrorTimer = duration > 0f ? duration : buildErrorDefaultDuration;
        }

        public void ShowTowerPanel(Tower tower)
        {
            if (towerPanel == null)
                return;

            towerPanel.Show(tower);
        }

        private void UpdateAllTexts()
        {
            if (gameManager == null)
                return;

            OnGoldChanged(gameManager.Resources.Gold);
            OnLumberChanged(gameManager.Resources.Lumber);
            OnLivesChanged(gameManager.Resources.Lives);
            OnWaveChanged(gameManager.Wave);
        }

        private void OnGoldChanged(int value)
        {
            if (goldText != null)
                goldText.text = $"Gold: {value}";
        }

        private void OnLumberChanged(int value)
        {
            if (lumberText != null)
                lumberText.text = $"Lumber: {value}";
        }

        private void OnLivesChanged(int value)
        {
            if (livesText != null)
                livesText.text = $"Lives: {value}";
        }

        private void OnWaveChanged(int value)
        {
            if (waveText != null)
                waveText.text = $"Wave: {value}";
        }

        private void OnGameStateChanged(GameState state)
        {
            if (startWaveButton != null)
                startWaveButton.interactable = state == GameState.BuildPhase;
        }
    }
}