using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathBlocker : MonoBehaviour
{
    [SerializeField] private PathController pathController;
    [SerializeField] private GridHelper grid;
    [SerializeField] private Transform pathSegmentsRoot;

    private readonly HashSet<Vector2Int> pathCells = new();
    private NavMeshPath navMeshPath;

    private void Awake()
    {
        navMeshPath = new NavMeshPath();

        if (grid == null || pathSegmentsRoot == null)
            return;

        pathCells.Clear();

        foreach (Transform child in pathSegmentsRoot)
        {
            if (!grid.TryGetCellFromWorld(child.position, out var column, out var row))
                continue;

            var cell = new Vector2Int(column, row);
            pathCells.Add(cell);
        }
    }

    public bool IsPathCell(Vector2Int cell)
    {
        return pathCells.Contains(cell);
    }

    public bool IsPathValid()
    {
        if (pathController == null)
            return false;

        var waypoints = pathController.Waypoints;
        if (waypoints == null || waypoints.Length < 2)
            return false;

        for (var i = 0; i < waypoints.Length - 1; i++)
        {
            var from = waypoints[i].position;
            var to = waypoints[i + 1].position;

            if (!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, navMeshPath))
                return false;

            if (navMeshPath.status != NavMeshPathStatus.PathComplete)
                return false;
        }

        return true;
    }
}