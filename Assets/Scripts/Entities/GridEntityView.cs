using UnityEngine;

public class GridEntityView : MonoBehaviour
{
    protected Grid grid;

    public Vector2Int TilePosition { get; private set; }

    public virtual void Init(Grid targetGrid, Vector2Int startTile)
    {
        grid = targetGrid;
        SetTile(startTile);
    }

    public virtual void SetTile(Vector2Int newPos)
    {
        TilePosition = newPos;

        Vector3Int tile = new Vector3Int(newPos.x, newPos.y, 0);
        Vector3 worldPos = grid.GetCellCenterWorld(tile);
        transform.position = worldPos;
    }
}
