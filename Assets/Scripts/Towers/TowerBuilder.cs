using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerBuilder : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GridHelper grid;
    [SerializeField] private Transform gameBoard;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private Transform towersRoot;
    [SerializeField] private PathBlocker pathBlocker;
    [SerializeField] private HUDController hudController;
    [SerializeField] private float towerY = 0.8f;

    private readonly HashSet<Vector2Int> occupiedCells = new();
    private bool isPlacing;

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (mainCamera == null || grid == null || gameBoard == null || towerPrefab == null || pathBlocker == null)
            return;

        if (isPlacing)
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
        if (occupiedCells.Contains(cell))
            return;

        if (pathBlocker.IsPathCell(cell))
        {
            if (hudController != null)
                hudController.ShowBuildError("You must leave the road free.", 2f);

            return;
        }

        var center = grid.GetCellCenter(column, row);
        var position = new Vector3(center.x, towerY, center.z);

        StartCoroutine(PlaceTower(cell, position));
    }

    private IEnumerator PlaceTower(Vector2Int cell, Vector3 position)
    {
        isPlacing = true;

        var instance = towersRoot != null
            ? Instantiate(towerPrefab, position, Quaternion.identity, towersRoot)
            : Instantiate(towerPrefab, position, Quaternion.identity);

        occupiedCells.Add(cell);

        yield return null;

        if (!pathBlocker.IsPathValid())
        {
            occupiedCells.Remove(cell);

            if (hudController != null)
                hudController.ShowBuildError("You must leave a path from start to end.", 2f);

            Destroy(instance);
        }

        isPlacing = false;
    }
}