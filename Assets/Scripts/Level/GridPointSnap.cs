using UnityEngine;

[ExecuteAlways]
public class GridPointSnap : MonoBehaviour
{
    [SerializeField] private GridHelper grid;
    [SerializeField] private int column = 1;
    [SerializeField] private int row = 1;
    [SerializeField] private float y;

    private void OnValidate()
    {
        Apply();
    }

    private void OnEnable()
    {
        Apply();
    }

    private void Apply()
    {
        if (grid == null)
            return;

        var center = grid.GetCellCenter(column, row);
        transform.position = new Vector3(center.x, y, center.z);
    }
}