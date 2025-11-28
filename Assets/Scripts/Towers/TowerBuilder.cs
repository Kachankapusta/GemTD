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
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private Transform towersRoot;
        [SerializeField] private PathBlocker pathBlocker;
        [SerializeField] private HUDController hudController;
        [SerializeField] private float towerY = 0.8f;

        private readonly HashSet<Vector2Int> _occupiedCells = new();
        private readonly List<Tower> _draftTowers = new();

        private bool _isPlacing;
        private bool _selectionMode;
        private Tower _towerTemplate;

        private void Awake()
        {
            if (towerPrefab != null)
                _towerTemplate = towerPrefab.GetComponent<Tower>();
        }

        private void Update()
        {
            if (Mouse.current == null)
                return;

            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return;

            if (_selectionMode)
            {
                HandleSelectionClick();
                return;
            }

            HandleBuildClick();
        }

        private void HandleBuildClick()
        {
            if (mainCamera == null || grid == null || gameBoard == null || towerPrefab == null || pathBlocker == null)
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

            if (gameManager != null && _towerTemplate != null)
            {
                var goldCost = _towerTemplate.GoldCost;
                var lumberCost = _towerTemplate.LumberCost;

                if (!gameManager.HasEnoughResources(goldCost, lumberCost))
                {
                    if (hudController != null)
                    {
                        var notEnoughGold = gameManager.Gold < goldCost;
                        var notEnoughLumber = gameManager.Lumber < lumberCost;

                        string message;

                        if (notEnoughGold && notEnoughLumber)
                            message = "Not enough gold and lumber.";
                        else if (notEnoughGold)
                            message = "Not enough gold.";
                        else
                            message = "Not enough lumber.";

                        hudController.ShowBuildError(message, 2f);
                    }

                    return;
                }
            }

            var center = grid.GetCellCenter(column, row);
            var position = new Vector3(center.x, towerY, center.z);

            var goldCostToPay = _towerTemplate != null ? _towerTemplate.GoldCost : 0;
            var lumberCostToPay = _towerTemplate != null ? _towerTemplate.LumberCost : 0;

            StartCoroutine(PlaceTower(cell, position, goldCostToPay, lumberCostToPay));
        }

        private void HandleSelectionClick()
        {
            if (mainCamera == null)
                return;

            var mousePos = Mouse.current.position.ReadValue();
            var ray = mainCamera.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0f));

            if (!Physics.Raycast(ray, out var hit, 1000f))
                return;

            var tower = hit.collider.GetComponentInParent<Tower>();
            if (tower == null)
                return;

            if (!_draftTowers.Contains(tower))
                return;

            FinalizeDraft(tower);
        }

        private IEnumerator PlaceTower(Vector2Int cell, Vector3 position, int goldCost, int lumberCost)
        {
            _isPlacing = true;

            var instance = towersRoot != null
                ? Instantiate(towerPrefab, position, Quaternion.identity, towersRoot)
                : Instantiate(towerPrefab, position, Quaternion.identity);

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
            if (gameManager != null)
            {
                var lumberBefore = gameManager.Lumber;

                gameManager.SpendResources(goldCost, lumberCost);

                var tower = instance.GetComponent<Tower>();
                if (tower != null)
                    _draftTowers.Add(tower);

                if (!_selectionMode && lumberBefore > 0 && gameManager.Lumber == 0 && _draftTowers.Count > 0)
                {
                    _selectionMode = true;

                    if (hudController != null)
                        hudController.ShowBuildError("Choose one tower to keep. Others will turn into rocks.", 3f);
                }
            }

            _isPlacing = false;
        }

        private void FinalizeDraft(Tower selectedTower)
        {
            if (rockPrefab == null)
            {
                _draftTowers.Clear();
                _selectionMode = false;
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
            _selectionMode = false;
        }
    }
}