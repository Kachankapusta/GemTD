using System.Text;
using TMPro;
using Towers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [DisallowMultipleComponent]
    public class TowerPanelUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private TextMeshProUGUI qualityText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI abilityText;
        [SerializeField] private Button showRangeButton;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button selectButton;
        [SerializeField] private TowerBuilder towerBuilder;
        private TowerRangeIndicator _currentRangeIndicator;

        private Tower _currentTower;
        private bool _rangeVisible;

        private void Awake()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);

            if (showRangeButton != null)
                showRangeButton.onClick.AddListener(OnShowRangeClicked);

            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradeClicked);

            if (selectButton != null)
                selectButton.onClick.AddListener(OnSelectClicked);
        }

        public void Show(Tower tower)
        {
            _currentTower = tower;

            if (tower == null || tower.Config == null)
            {
                Hide();
                return;
            }

            if (panelRoot != null)
                panelRoot.SetActive(true);

            nameText.text = tower.DisplayName;
            typeText.text = tower.Type.ToString();
            qualityText.text = tower.Quality.ToString();

            var sb = new StringBuilder();
            sb.AppendLine("Damage: " + tower.Damage);
            sb.AppendLine("Range: " + tower.RangeInCells + " cells");
            sb.AppendLine("Fire interval: " + tower.FireInterval.ToString("0.00") + " s");
            statsText.text = sb.ToString();

            var ability = tower.Config.AbilityDescription;
            abilityText.text = string.IsNullOrEmpty(ability) ? "No special ability." : ability;

            _currentRangeIndicator = tower.GetComponentInChildren<TowerRangeIndicator>(true);
            if (_currentRangeIndicator != null)
                _currentRangeIndicator.SyncWithTower(tower);

            _rangeVisible = false;
            if (_currentRangeIndicator != null)
                _currentRangeIndicator.SetVisible(false);

            var canSelect = towerBuilder != null && towerBuilder.IsSelectionMode && towerBuilder.IsDraftTower(tower);
            if (selectButton != null)
                selectButton.gameObject.SetActive(canSelect);
        }

        private void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);

            if (_currentRangeIndicator != null)
                _currentRangeIndicator.SetVisible(false);

            _currentTower = null;
            _currentRangeIndicator = null;
            _rangeVisible = false;

            if (selectButton != null)
                selectButton.gameObject.SetActive(false);
        }

        private void OnShowRangeClicked()
        {
            if (_currentRangeIndicator == null)
                return;

            _rangeVisible = !_rangeVisible;
            _currentRangeIndicator.SetVisible(_rangeVisible);
        }

        private void OnUpgradeClicked()
        {
            if (_currentTower == null)
                return;

            abilityText.text = "Upgrade system is not implemented yet.";
        }

        private void OnSelectClicked()
        {
            if (towerBuilder == null || _currentTower == null)
                return;

            towerBuilder.SelectDraftTower(_currentTower);
            Hide();
        }
    }
}