using UnityEngine;

public class TileView : MonoBehaviour
{
    private Grid grid;

    [Header("Referencias")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TileSpriteSet spriteSet;

    public void Init(Grid targetGrid, Vector2Int tilePos, int width, int height)
    {
        grid = targetGrid;
        SetTilePosition(tilePos);
        UpdateSprite(tilePos, width, height);
    }

    public void SetTilePosition(Vector2Int tilePos)
    {
        if (grid == null) return;

        Vector3Int tile = new Vector3Int(tilePos.x, tilePos.y, 0);
        transform.position = grid.GetCellCenterWorld(tile);
    }

    public void UpdateSprite(Vector2Int tilePos, int width, int height)
    {
        if (spriteRenderer == null || spriteSet == null) return;

        TileViewPosType visualPosition = GetViewPositionType(tilePos, width, height);
        spriteRenderer.sprite = spriteSet.GetSprite(visualPosition);
    }

    private TileViewPosType GetViewPositionType(Vector2Int pos, int width, int height)
    {
        bool isLeft = pos.x == 0;
        bool isRight = pos.x == width - 1;
        bool isBottom = pos.y == 0;
        bool isTop = pos.y == height - 1;

        if (isTop && isLeft) return TileViewPosType.TopLeft;
        if (isTop && isRight) return TileViewPosType.TopRight;
        if (isBottom && isLeft) return TileViewPosType.BottomLeft;
        if (isBottom && isRight) return TileViewPosType.BottomRight;

        if (isTop) return TileViewPosType.Top;
        if (isBottom) return TileViewPosType.Bottom;
        if (isLeft) return TileViewPosType.Left;
        if (isRight) return TileViewPosType.Right;

        return TileViewPosType.Center;
    }
}