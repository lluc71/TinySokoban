using UnityEngine;

[CreateAssetMenu(fileName = "TileSpriteSet", menuName = "TinySokoban/Tile Sprite Set")]
public class TileSpriteSet : ScriptableObject
{
    [Header("Sprites por posición en el Grid")]
    [SerializeField] private Sprite topLeftSprite;
    [SerializeField] private Sprite topSprite;
    [SerializeField] private Sprite topRightSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite centerSprite;
    [SerializeField] private Sprite rightSprite;
    [SerializeField] private Sprite bottomLeftSprite;
    [SerializeField] private Sprite bottomSprite;
    [SerializeField] private Sprite bottomRightSprite;

    public Sprite GetSprite(TileViewPosType posType)
    {
        switch (posType)
        {
            case TileViewPosType.TopLeft:
                return topLeftSprite;

            case TileViewPosType.Top:
                return topSprite;

            case TileViewPosType.TopRight:
                return topRightSprite;

            case TileViewPosType.Left:
                return leftSprite;

            case TileViewPosType.Center:
                return centerSprite;

            case TileViewPosType.Right:
                return rightSprite;

            case TileViewPosType.BottomLeft:
                return bottomLeftSprite;

            case TileViewPosType.Bottom:
                return bottomSprite;

            case TileViewPosType.BottomRight:
                return bottomRightSprite;

            default:
                return centerSprite;
        }
    }
}
