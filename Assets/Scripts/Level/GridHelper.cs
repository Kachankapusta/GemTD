using UnityEngine;

namespace Level
{
    [DisallowMultipleComponent]
    public class GridHelper : MonoBehaviour
    {
        [SerializeField] private Transform gameBoard;

        [Min(1)] [SerializeField] private int columns = 37;

        [Min(1)] [SerializeField] private int rows = 37;

        private Vector3 _boardCenter;
        private float _cellSizeX;
        private float _cellSizeZ;
        private float _halfHeight;
        private float _halfWidth;

        private void Awake()
        {
            Recalculate();
        }

        private void OnDrawGizmos()
        {
            if (gameBoard == null)
                return;

            if (_cellSizeX <= 0f || _cellSizeZ <= 0f)
                Recalculate();

            var minX = _boardCenter.x - _halfWidth;
            var maxX = _boardCenter.x + _halfWidth;
            var minZ = _boardCenter.z - _halfHeight;
            var maxZ = _boardCenter.z + _halfHeight;

            Gizmos.color = Color.gray;

            for (var i = 0; i <= columns; i++)
            {
                var x = minX + i * _cellSizeX;
                Gizmos.DrawLine(new Vector3(x, _boardCenter.y, minZ), new Vector3(x, _boardCenter.y, maxZ));
            }

            for (var j = 0; j <= rows; j++)
            {
                var z = minZ + j * _cellSizeZ;
                Gizmos.DrawLine(new Vector3(minX, _boardCenter.y, z), new Vector3(maxX, _boardCenter.y, z));
            }
        }

        private void OnValidate()
        {
            Recalculate();
        }

        private void Recalculate()
        {
            if (gameBoard == null)
                return;

            var scale = gameBoard.localScale;
            var sizeX = scale.x;
            var sizeZ = scale.z;

            _halfWidth = sizeX * 0.5f;
            _halfHeight = sizeZ * 0.5f;

            _cellSizeX = sizeX / columns;
            _cellSizeZ = sizeZ / rows;

            _boardCenter = gameBoard.position;
        }

        public Vector3 GetCellCenter(int col, int row)
        {
            var minX = _boardCenter.x - _halfWidth;
            var maxZ = _boardCenter.z + _halfHeight;

            var x = minX + (col - 0.5f) * _cellSizeX;
            var z = maxZ - (row - 0.5f) * _cellSizeZ;

            return new Vector3(x, _boardCenter.y, z);
        }

        public bool TryGetCellFromWorld(Vector3 worldPosition, out int column, out int row)
        {
            column = 0;
            row = 0;

            var minX = _boardCenter.x - _halfWidth;
            var maxX = _boardCenter.x + _halfWidth;
            var minZ = _boardCenter.z - _halfHeight;
            var maxZ = _boardCenter.z + _halfHeight;

            if (worldPosition.x < minX || worldPosition.x > maxX || worldPosition.z < minZ || worldPosition.z > maxZ)
                return false;

            var localX = worldPosition.x - minX;
            var localZ = maxZ - worldPosition.z;

            var col = Mathf.FloorToInt(localX / _cellSizeX) + 1;
            var r = Mathf.FloorToInt(localZ / _cellSizeZ) + 1;

            col = Mathf.Clamp(col, 1, columns);
            r = Mathf.Clamp(r, 1, rows);

            column = col;
            row = r;
            return true;
        }
    }
}