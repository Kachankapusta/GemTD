using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Level
{
    [DisallowMultipleComponent]
    public class PathBlocker : MonoBehaviour
    {
        [SerializeField] private PathController pathController;
        [SerializeField] private GridHelper grid;

        private readonly HashSet<Vector2Int> _pathCells = new();
        private bool _initialized;
        private NavMeshPath _navMeshPath;

        private void Awake()
        {
            _navMeshPath = new NavMeshPath();
        }

        private void Start()
        {
            Initialize();
        }

        public bool IsPathCell(Vector2Int cell)
        {
            Initialize();
            return _pathCells.Contains(cell);
        }

        public bool IsPathValid()
        {
            Initialize();

            if (pathController == null)
                return false;

            var waypoints = pathController.Waypoints;
            if (waypoints == null || waypoints.Length < 2)
                return false;

            return IsPathValidInternal(waypoints);
        }

        private void Initialize()
        {
            if (_initialized)
                return;

            if (pathController == null)
            {
                Debug.LogError("PathBlocker: PathController is not assigned");
                return;
            }

            if (grid == null)
            {
                Debug.LogError("PathBlocker: GridHelper is not assigned");
                return;
            }

            var waypoints = pathController.Waypoints;
            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogError("PathBlocker: no waypoints configured");
                return;
            }

            _pathCells.Clear();

            foreach (var wp in waypoints)
            {
                if (!grid.TryGetCellFromWorld(wp.position, out var column, out var row))
                {
                    Debug.LogError($"PathBlocker: cannot map waypoint '{wp.name}' to grid cell");
                    continue;
                }

                var cell = new Vector2Int(column, row);
                _pathCells.Add(cell);
            }

            if (!IsPathValidInternal(waypoints))
                Debug.LogError("PathBlocker: base layout has no valid NavMesh path through all waypoints");

            _initialized = true;
        }

        private bool IsPathValidInternal(Transform[] waypoints)
        {
            for (var i = 0; i < waypoints.Length - 1; i++)
            {
                var from = waypoints[i].position;
                var to = waypoints[i + 1].position;

                if (!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, _navMeshPath))
                    return false;

                if (_navMeshPath.status != NavMeshPathStatus.PathComplete)
                    return false;
            }

            return true;
        }
    }
}