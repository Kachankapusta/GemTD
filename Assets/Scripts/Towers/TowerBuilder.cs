using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private GameObject towerPrefab;
        [SerializeField] private GameObject secondTowerPrefab;
        [SerializeField] private GameObject thirdTowerPrefab;
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
            if (Mouse.current == null)
                return;

            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return;

            if (mainCamera == null)
                return;

            var mousePos = Mouse.current.position.ReadValue();
            var ray = mainCamera.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0f));

            if (Physics.Raycast(ray, out var hit, 1000f))
            {
                var tower = hit.collider.GetComponentInParent<Tower>();
                if (tower != null)
                {
                    if (hudController != null)
                        hudController.ShowTowerPanel(tower);

                    return;
                }
            }

            HandleBuildClick();
        }

        private void HandleBuildClick()
        {
            if (mainCamera == null || grid == null || gameBoard == null || pathBlocker == null)
                return;

            if (_isPlacing)
                return;

            var mousePos = Mouse.current.position.ReadValue();
            var ray = mainCamera.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0f));

            if (!Physics.Raycast(ray, out var hit, 1000f))
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

            if (!pathBlocker.IsPathValid())
            {
                _occupiedCells.Remove(cell);

                if (hudController != null)
                    hudController.ShowBuildError("You must leave a path from start to end.", 2f);

                Destroy(instance);
                _isPlacing = false;
                yield break;
            }

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

            _isPlacing = false;
        }

        private GameObject GetRandomTowerPrefab()
        {
            var candidates = new List<GameObject>(3);

            if (towerPrefab != null)
                candidates.Add(towerPrefab);

            if (secondTowerPrefab != null)
                candidates.Add(secondTowerPrefab);

            if (thirdTowerPrefab != null)
                candidates.Add(thirdTowerPrefab);

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
            if (rockPrefab == null)
            {
                _draftTowers.Clear();
                IsSelectionMode = false;
                return;
            }

            foreach (var tower in _draftTowers)
            {
                if (tower == null)
                    continue;

                if (tower == selectedTower)
                    continue;

                var tr = tower.transform;
                var position = tr.position;
                var rotation = tr.rotation;

                var rockInstance = towersRoot != null
                    ? Instantiate(rockPrefab, position, rotation, towersRoot)
                    : Instantiate(rockPrefab, position, rotation);

                Destroy(tower.gameObject);
            }

            _draftTowers.Clear();
            IsSelectionMode = false;
        }
    }
}