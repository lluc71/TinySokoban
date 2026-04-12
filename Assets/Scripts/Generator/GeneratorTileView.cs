using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelEditorTileView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField] private Image image;

    private Vector2Int gridPos;
    private GeneratorManager editor;

    public void Init(GeneratorManager editor, Vector2Int pos)
    {
        this.editor = editor;
        gridPos = pos;
    }

    public void Refresh(Sprite sprite)
    {
        if (image == null) return;

        image.sprite = sprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (editor == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
            editor.PaintAt(gridPos);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (editor == null) return;

        if (Input.GetMouseButton(0))
            editor.PaintAt(gridPos);
    }
}