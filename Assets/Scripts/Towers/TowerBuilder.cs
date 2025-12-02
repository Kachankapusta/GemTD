using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Level;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Towers
{
    public class TowerBuilder : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GridHelper grid;
        [SerializeField] private Transform gameBoard;
        [SerializeField] private GameObject[] towerPrefabs;
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private Transform towersRoot;
        [SerializeField] private PathBlocker pathBlocker;
        [SerializeField] private HUDController hudController;
        [SerializeField] private float towerY = 0.8f;

        private const int LumberCostPerTower = 1;
        private readonly HashSet<Vector2Int> _occupiedCells = new();
        private readonly List<Tower> _draftTowers = new();

        private bool _isPlacing;

        public bool IsSelectionMode { get; private set; }

        public bool IsDraftTower(Tower tower)
        {
            return tower != null && _draftTowers.Contains(tower);
        }

        private void Update()
        {
            if (!IsBuildInputTriggered())
                return;

            if (!TryGetMouseHit(out var hit))
                return;

            if (TryHandleTowerSelection(hit))
                return;

            var gameManager = GameManager.Instance;
            if (gameManager == null)
                return;

            if (gameManager.State != GameState.BuildPhase)
                return;

            HandleBuildClick(hit);
        }

        private static bool IsBuildInputTriggered()
        {
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        }

        private bool TryGetMouseHit(out RaycastHit hit)
        {
            hit = default;

            if (mainCamera == null)
                return false;

            var mousePos = Mouse.current.position.ReadValue();
            var ray = mainCamera.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0f));

            return Physics.Raycast(ray, out hit, 1000f);
        }

        private bool TryHandleTowerSelection(RaycastHit hit)
        {
            var tower = hit.collider.GetComponentInParent<Tower>();
            if (tower == null)
                return false;

            if (hudController != null)
                hudController.ShowTowerPanel(tower);

            return true;
        }

        private void HandleBuildClick(RaycastHit hit)
        {
            if (grid == null || gameBoard == null || pathBlocker == null)
                return;

            if (_isPlacing)
                return;

            if (hit.collider.transform != gameBoard)
                return;

            if (!grid.TryGetCellFromWorld(hit.point, out var column, out var row))
                return;

            var cell = new Vector2Int(column, row);
            if (_occupiedCells.Contains(cell))
                return;

            if (pathBlocker.IsPathCell(cell))
            {
                if (hudController != null)
                    hudController.ShowBuildError("You cannot build on the path.", 2f);

                return;
            }

            var gameManager = GameManager.Instance;
            if (gameManager != null && LumberCostPerTower > 0 && gameManager.Lumber < LumberCostPerTower)
            {
                if (hudController != null)
                    hudController.ShowBuildError("Not enough lumber.", 2f);

                return;
            }

            var selectedPrefab = GetRandomTowerPrefab();
            if (selectedPrefab == null)
            {
                if (hudController != null)
                    hudController.ShowBuildError("No towers configured.", 2f);

                return;
            }

            var center = grid.GetCellCenter(column, row);
            var position = new Vector3(center.x, towerY, center.z);

            StartCoroutine(PlaceTower(cell, position, selectedPrefab));
        }

        private IEnumerator PlaceTower(Vector2Int cell, Vector3 position, GameObject prefab)
        {
            _isPlacing = true;

            var instance = towersRoot != null
                ? Instantiate(prefab, position, Quaternion.identity, towersRoot)
                : Instantiate(prefab, position, Quaternion.identity);

            _occupiedCells.Add(cell);

            yield return null;

            var gameManager = GameManager.Instance;
            if (gameManager != null && LumberCostPerTower > 0)
            {
                var lumberBefore = gameManager.Lumber;

                gameManager.SpendResources(0, LumberCostPerTower);

                var tower = instance.GetComponent<Tower>();
                if (tower != null)
                    _draftTowers.Add(tower);

                if (!IsSelectionMode && lumberBefore > 0 && gameManager.Lumber == 0 && _draftTowers.Count > 0)
                {
                    IsSelectionMode = true;

                    if (hudController != null)
                        hudController.ShowBuildError("Choose one tower in its menu. Others will turn into rocks.", 3f);
                }
            }

            if (pathBlocker != null && !pathBlocker.IsPathValid())
            {
                _occupiedCells.Remove(cell);
                Destroy(instance);

                if (hudController != null)
                    hudController.ShowBuildError("Placing this tower would block the path.", 2f);
            }

            _isPlacing = false;
        }

        private GameObject GetRandomTowerPrefab()
        {
            if (towerPrefabs == null || towerPrefabs.Length == 0)
                return null;

            var gameManager = GameManager.Instance;

            var useQualityFilter = gameManager != null;
            var targetQuality = GemQuality.Chipped;

            if (gameManager != null)
                targetQuality = gameManager.GetRandomQualityForCurrentLevel();

            var candidates = new List<GameObject>();

            foreach (var prefab in towerPrefabs)
            {
                if (prefab == null)
                    continue;

                var tower = prefab.GetComponent<Tower>();
                if (tower == null)
                    continue;

                var config = tower.Config;
                if (config == null)
                    continue;

                if (useQualityFilter && config.Quality != targetQuality)
                    continue;

                candidates.Add(prefab);
            }

            if (candidates.Count == 0)
                return null;

            var index = Random.Range(0, candidates.Count);
            return candidates[index];
        }

        public void SelectDraftTower(Tower selectedTower)
        {
            if (!IsSelectionMode)
                return;

            if (selectedTower == null)
                return;

            if (!_draftTowers.Contains(selectedTower))
                return;

            FinalizeDraft(selectedTower);
        }

        private void FinalizeDraft(Tower selectedTower)
        {
            foreach (var tower in from tower in _draftTowers
                     where tower != null
                     where tower != selectedTower
                     where rockPrefab != null
                     let tr = tower.transform
                     let position = tr.position
                     let rotation = tr.rotation
                     let rockInstance = towersRoot != null
                         ? Instantiate(rockPrefab, position, rotation, towersRoot)
                         : Instantiate(rockPrefab, position, rotation)
                     select tower)
                Destroy(tower.gameObject);

            _draftTowers.Clear();
            IsSelectionMode = false;
        }
    }
}