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
    [SerializeField] private float towerY = 0.8f;

    private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (mainCamera == null || grid == null || gameBoard == null || towerPrefab == null)
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

        var center = grid.GetCellCenter(column, row);
        var position = new Vector3(center.x, towerY, center.z);

        var instance = towersRoot != null ? Instantiate(towerPrefab, position, Quaternion.identity, towersRoot) : Instantiate(towerPrefab, position, Quaternion.identity);

        occupiedCells.Add(cell);
    }
}