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
    [DisallowMultipleComponent]
    public class TowerBuilder : MonoBehaviour
    {
        private const int LumberCostPerTower = 1;

        [SerializeField] private Camera mainCamera;
        [SerializeField] private GridHelper grid;
        [SerializeField] private Transform gameBoard;
        [SerializeField] private GameObject[] towerPrefabs;
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private Transform towersRoot;
        [SerializeField] private PathBlocker pathBlocker;
        [SerializeField] private HUDController hudController;
        [SerializeField] private float towerY = 0.8f;

        private readonly List<Tower> _draftTowers = new();
        private readonly HashSet<Vector2Int> _occupiedCells = new();

        private bool _isPlacing;

        public bool IsSelectionMode { get; private set; }

        private void Update()
        {
            if (Mouse.current == null)
                return;

            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return;

            if (!GetRaycastHit(out var hit))
                return;

            if (TrySelectTower(hit))
                return;

            var gm = GameManager.Instance;
            if (gm == null) return;

            if (gm.State != GameState.BuildPhase)
                return;

            TryBuildTower(hit, gm);
        }

        public bool IsDraftTower(Tower tower)
        {
            return tower != null && _draftTowers.Contains(tower);
        }

        public void SelectDraftTower(Tower selectedTower)
        {
            if (selectedTower == null)
                return;

            if (!_draftTowers.Contains(selectedTower))
                return;

            if (!IsSelectionMode)
                return;

            var gm = GameManager.Instance;
            if (gm == null)
                return;

            foreach (var tower in _draftTowers)
            {
                if (tower == null)
                    continue;

                if (tower == selectedTower)
                    continue;

                var position = tower.transform.position;

                if (rockPrefab != null)
                {
                    var rock = Instantiate(
                        rockPrefab,
                        position,
                        Quaternion.identity,
                        towersRoot ? towersRoot : transform
                    );
                    rock.name = "Rock";
                }

                Destroy(tower.gameObject);
            }

            _draftTowers.Clear();
            _draftTowers.Add(selectedTower);

            IsSelectionMode = false;
        }

        private bool GetRaycastHit(out RaycastHit hit)
        {
            hit = default;

            if (mainCamera == null)
                return false;

            var mousePos = Mouse.current.position.ReadValue();
            var ray = mainCamera.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0f));

            return Physics.Raycast(ray, out hit, 1000f);
        }

        private bool TrySelectTower(RaycastHit hit)
        {
            var tower = hit.collider.GetComponentInParent<Tower>();
            if (tower == null)
                return false;

            if (hudController != null)
                hudController.ShowTowerPanel(tower);

            return true;
        }

        private void TryBuildTower(RaycastHit hit, GameManager gm)
        {
            if (_isPlacing)
                return;

            if (grid == null || pathBlocker == null)
                return;

            if (hit.collider.transform != gameBoard)
                return;

            if (!grid.TryGetCellFromWorld(hit.point, out var column, out var row))
                return;

            var cell = new Vector2Int(column, row);

            if (!ValidateBuildConditions(cell, gm))
                return;

            var prefab = ChooseRandomTowerPrefab(gm);
            if (prefab == null)
            {
                ShowError("No towers configured.");
                return;
            }

            var center = grid.GetCellCenter(column, row);
            var position = new Vector3(center.x, towerY, center.z);

            StartCoroutine(PlaceTowerRoutine(cell, position, prefab));
        }

        private bool ValidateBuildConditions(Vector2Int cell, GameManager gm)
        {
            if (_occupiedCells.Contains(cell))
                return false;

            if (pathBlocker.IsPathCell(cell))
            {
                ShowError("Cannot build on the path.");
                return false;
            }

            if (gm.Resources.CanAfford(0, LumberCostPerTower)) return true;
            ShowError("Not enough lumber.");
            return false;
        }

        private IEnumerator PlaceTowerRoutine(Vector2Int cell, Vector3 position, GameObject prefab)
        {
            _isPlacing = true;

            var parent = towersRoot ? towersRoot : transform;
            var instance = Instantiate(prefab, position, Quaternion.identity, parent);

            _occupiedCells.Add(cell);

            yield return null;

            if (!pathBlocker.IsPathValid())
            {
                _occupiedCells.Remove(cell);
                Destroy(instance);
                ShowError("Placing this tower would block the path.");
            }
            else
            {
                OnTowerPlacedSuccessfully(instance);
            }

            _isPlacing = false;
        }

        private void OnTowerPlacedSuccessfully(GameObject instance)
        {
            var gm = GameManager.Instance;
            if (gm == null)
                return;

            var currentLumber = gm.Resources.Lumber;

            gm.Resources.TrySpend(0, LumberCostPerTower);

            var tower = instance.GetComponent<Tower>();
            if (tower != null)
                _draftTowers.Add(tower);

            if (IsSelectionMode ||
                currentLumber <= 0 ||
                gm.Resources.Lumber != 0 ||
                _draftTowers.Count <= 0) return;
            IsSelectionMode = true;
            ShowError("Choose one tower. Others will become rocks.", 3f);
        }

        private GameObject ChooseRandomTowerPrefab(GameManager gm)
        {
            if (towerPrefabs == null || towerPrefabs.Length == 0)
                return null;

            var quality = gm != null
                ? gm.Lottery.RollQuality(gm.PlayerLevel)
                : GemQuality.Chipped;

            var candidates = towerPrefabs
                .Where(p => p != null)
                .Select(p => p.GetComponent<Tower>())
                .Where(t => t != null && t.Config != null && t.Config.Quality == quality)
                .Select(t => t.gameObject)
                .ToList();

            if (candidates.Count == 0)
                return null;

            var index = Random.Range(0, candidates.Count);
            return candidates[index];
        }

        private void ShowError(string message, float duration = 2f)
        {
            if (hudController == null)
                return;

            hudController.ShowBuildError(message, duration);
        }
    }
}