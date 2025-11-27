using UnityEngine;

public class GridHelper : MonoBehaviour
{
    [SerializeField] private Transform gameBoard;
    [SerializeField] private int columns = 37;
    [SerializeField] private int rows = 37;

    private Vector3 boardCenter;
    private float cellSizeX;
    private float cellSizeZ;
    private float halfHeight;
    private float halfWidth;
    public int Columns => columns;
    public int Rows => rows;

    private void Awake()
    {
        Recalculate();
    }

    private void OnDrawGizmos()
    {
        if (gameBoard == null)
            return;

        if (columns <= 0 || rows <= 0)
            return;

        Recalculate();

        Gizmos.color = Color.gray;

        var minX = boardCenter.x - halfWidth;
        var maxX = boardCenter.x + halfWidth;
        var minZ = boardCenter.z - halfHeight;
        var maxZ = boardCenter.z + halfHeight;

        for (var i = 0; i <= columns; i++)
        {
            var x = minX + i * cellSizeX;
            Gizmos.DrawLine(new Vector3(x, boardCenter.y, minZ), new Vector3(x, boardCenter.y, maxZ));
        }

        for (var j = 0; j <= rows; j++)
        {
            var z = minZ + j * cellSizeZ;
            Gizmos.DrawLine(new Vector3(minX, boardCenter.y, z), new Vector3(maxX, boardCenter.y, z));
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

        if (columns <= 0 || rows <= 0)
            return;

        var localScale = gameBoard.localScale;
        var sizeX = localScale.x;
        var sizeZ = localScale.z;

        halfWidth = sizeX * 0.5f;
        halfHeight = sizeZ * 0.5f;

        cellSizeX = sizeX / columns;
        cellSizeZ = sizeZ / rows;

        boardCenter = gameBoard.position;
    }

    public Vector3 GetCellCenter(int col, int row)
    {
        var minX = boardCenter.x - halfWidth;
        var maxZ = boardCenter.z + halfHeight;

        var x = minX + (col - 0.5f) * cellSizeX;
        var z = maxZ - (row - 0.5f) * cellSizeZ;

        return new Vector3(x, boardCenter.y, z);
    }

    public bool TryGetCellFromWorld(Vector3 worldPosition, out int column, out int row)
    {
        column = 0;
        row = 0;

        if (gameBoard == null)
            return false;

        var minX = boardCenter.x - halfWidth;
        var maxX = boardCenter.x + halfWidth;
        var minZ = boardCenter.z - halfHeight;
        var maxZ = boardCenter.z + halfHeight;

        if (worldPosition.x < minX || worldPosition.x > maxX || worldPosition.z < minZ || worldPosition.z > maxZ)
            return false;

        var localX = worldPosition.x - minX;
        var localZ = maxZ - worldPosition.z;

        var col = Mathf.FloorToInt(localX / cellSizeX) + 1;
        var r = Mathf.FloorToInt(localZ / cellSizeZ) + 1;

        col = Mathf.Clamp(col, 1, columns);
        r = Mathf.Clamp(r, 1, rows);

        column = col;
        row = r;
        return true;
    }
}