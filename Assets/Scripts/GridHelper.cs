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

    private void Awake()
    {
        RecalculateFromBoard();
    }

    private void OnDrawGizmos()
    {
        if (gameBoard == null)
            return;

        RecalculateFromBoard();

        Gizmos.color = Color.yellow;

        var minX = boardCenter.x - halfWidth;
        var maxX = boardCenter.x + halfWidth;
        var minZ = boardCenter.z - halfHeight;
        var maxZ = boardCenter.z + halfHeight;

        for (var x = 0; x <= columns; x++)
        {
            var worldX = minX + x * cellSizeX;
            Gizmos.DrawLine(
                new Vector3(worldX, boardCenter.y + 0.01f, minZ),
                new Vector3(worldX, boardCenter.y + 0.01f, maxZ)
            );
        }

        for (var y = 0; y <= rows; y++)
        {
            var worldZ = minZ + y * cellSizeZ;
            Gizmos.DrawLine(
                new Vector3(minX, boardCenter.y + 0.01f, worldZ),
                new Vector3(maxX, boardCenter.y + 0.01f, worldZ)
            );
        }
    }

    private void OnValidate()
    {
        if (gameBoard != null)
            RecalculateFromBoard();
    }

    private void RecalculateFromBoard()
    {
        var scale = gameBoard.localScale;

        var widthWorld = scale.x;
        var heightWorld = scale.z;

        cellSizeX = widthWorld / columns;
        cellSizeZ = heightWorld / rows;

        halfWidth = widthWorld * 0.5f;
        halfHeight = heightWorld * 0.5f;

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
}