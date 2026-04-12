using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField widthInput;
    [SerializeField] private TMP_InputField heightInput;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Grid visual")]
    [SerializeField] private Transform boardParent;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private LevelEditorTileView tilePrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite floorSprite;
    [SerializeField] private Sprite wallSprite;
    [SerializeField] private Sprite goalSprite;
    [SerializeField] private Sprite boxSprite;
    [SerializeField] private Sprite playerSprite;

    [Header("Exportación")]
    [SerializeField] private string levelsFolderName = "Levels";
    [SerializeField] private string filePrefix = "level_";

    private int width;
    private int height;

    private TileType[,] baseTiles;
    private List<Vector2Int> boxPositions = new();
    private Vector2Int? playerPos;

    private LevelEditorTileView[,] tileViews;

    private GeneratorBrushType currentBrush = GeneratorBrushType.Floor;

    public void SetBrushFloor() => currentBrush = GeneratorBrushType.Floor;
    public void SetBrushWall() => currentBrush = GeneratorBrushType.Wall;
    public void SetBrushGoal() => currentBrush = GeneratorBrushType.Goal;
    public void SetBrushBox() => currentBrush = GeneratorBrushType.Box;
    public void SetBrushPlayer() => currentBrush = GeneratorBrushType.Player;

    void Start()
    {
        UpdateGrid();
    }

    /**
     * Genera un Grid con Suelos basicos a partir del valor de los Inputs
     */
    public void UpdateGrid()
    {
        if(!CheckInputs()) return;

        baseTiles = new TileType[width, height];
        tileViews = new LevelEditorTileView[width, height];
        boxPositions.Clear();
        playerPos = null;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                baseTiles[x, y] = TileType.Floor;
            }
        }

        ClearBoard();

        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = width;
        }

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                LevelEditorTileView tile = Instantiate(tilePrefab, boardParent);
                tile.Init(this, new Vector2Int(x, y));
                tileViews[x, y] = tile;
                RefreshTileVisual(x, y);
            }
        }

        SetFeedback($"Grid {width}x{height} creado.");
    }

    /**
     * Revisa que los valores de los inputs sean correctos (Entre 3 y 12 incluidos)
     */
    private bool CheckInputs()
    {
        if (!int.TryParse(widthInput.text, out width) || width <= 2)
        {
            SetFeedback("Número de columnas inválido.");
            return false;
        }
        else if (width > 12)
        {
            SetFeedback("Número de columnas demasiado grande.");
            return false;
        }

        if (!int.TryParse(heightInput.text, out height) || height <= 2)
        {
            SetFeedback("Número de filas inválido.");
            return false;
        }
        else if (height > 12)
        {
            SetFeedback("Número de filas demasiado grande.");
            return false;
        }

        return true;
    }

    public void PaintAt(Vector2Int pos)
    {
        if (!IsInside(pos) || baseTiles == null) return;

        switch (currentBrush)
        {
            case GeneratorBrushType.Floor:
                baseTiles[pos.x, pos.y] = TileType.Floor;
                RemoveBoxAt(pos);
                RemovePlayerAt(pos);
                break;

            case GeneratorBrushType.Wall:
                baseTiles[pos.x, pos.y] = TileType.Wall;
                RemoveBoxAt(pos);
                RemovePlayerAt(pos);
                break;

            case GeneratorBrushType.Goal:
                baseTiles[pos.x, pos.y] = TileType.Goal;
                RemoveBoxAt(pos);
                RemovePlayerAt(pos);
                break;

            case GeneratorBrushType.Box:
                if (baseTiles[pos.x, pos.y] == TileType.Wall)
                    baseTiles[pos.x, pos.y] = TileType.Floor;

                RemovePlayerAt(pos);

                if (!boxPositions.Contains(pos))
                    boxPositions.Add(pos);
                break;

            case GeneratorBrushType.Player:
                if (baseTiles[pos.x, pos.y] == TileType.Wall)
                    baseTiles[pos.x, pos.y] = TileType.Floor;

                playerPos = pos;
                break;
        }

        RefreshAllVisuals();
    }

    public void ExportLevel()
    {
        if (baseTiles == null)
        {
            SetFeedback("Primero crea un grid.");
            return;
        }

        int goalCount = CountGoals();
        int boxCount = boxPositions.Count;
        int playerCount = playerPos.HasValue ? 1 : 0;

        if (playerCount < 1)
        {
            SetFeedback("Debe existir al menos 1 Player.");
            return;
        }

        if (goalCount < boxCount)
        {
            SetFeedback($"Faltan Goals. Hay {boxCount} cajas y {goalCount} objetivos.");
            return;
        }

        string txt = BuildLevelText();
        string folderPath = Path.Combine(Application.dataPath, levelsFolderName);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = GetNextAvailableLevelPath(folderPath);

        File.WriteAllText(filePath, txt);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        SetFeedback($"Nivel exportado en:\n{filePath}");
    }

    private string BuildLevelText()
    {
        System.Text.StringBuilder sb = new();
        sb.AppendLine($"{width} {height}");

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                sb.Append(GetCharAt(new Vector2Int(x, y)));
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private char GetCharAt(Vector2Int pos)
    {
        bool isPlayer = playerPos.HasValue && playerPos.Value == pos;
        bool isBox = boxPositions.Contains(pos);
        TileType tile = baseTiles[pos.x, pos.y];

        if (tile == TileType.Wall)
            return '#';

        if (tile == TileType.Goal)
        {
            if (isPlayer) return '+';
            if (isBox) return '*';
            return 'G';
        }

        if (isPlayer) return 'P';
        if (isBox) return 'B';

        return '.';
    }

    private string GetNextAvailableLevelPath(string folderPath)
    {
        int index = 1;

        while (true)
        {
            string fileName = $"{filePrefix}{index:000}.txt";
            string fullPath = Path.Combine(folderPath, fileName);

            if (!File.Exists(fullPath))
                return fullPath;

            index++;
        }
    }

    private int CountGoals()
    {
        int count = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (baseTiles[x, y] == TileType.Goal)
                    count++;
            }
        }

        return count;
    }

    private void RemoveBoxAt(Vector2Int pos)
    {
        boxPositions.Remove(pos);
    }

    private void RemovePlayerAt(Vector2Int pos)
    {
        if (playerPos.HasValue && playerPos.Value == pos)
            playerPos = null;
    }

    private bool IsInside(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    private void RefreshAllVisuals()
    {
        if (tileViews == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                RefreshTileVisual(x, y);
            }
        }
    }

    private void RefreshTileVisual(int x, int y)
    {
        if (tileViews == null || tileViews[x, y] == null) return;

        Vector2Int pos = new Vector2Int(x, y);

        bool isPlayer = playerPos.HasValue && playerPos.Value == pos;
        bool isBox = boxPositions.Contains(pos);
        TileType tile = baseTiles[x, y];

        Sprite sprite;

        if (tile == TileType.Wall)
        {
            sprite = wallSprite;
        }
        else if (isPlayer)
        {
            sprite = playerSprite;
        }
        else if (isBox)
        {
            sprite = boxSprite;
        }
        else if (tile == TileType.Goal)
        {
            sprite = goalSprite;
        }
        else
        {
            sprite = floorSprite;
        }

        tileViews[x, y].Refresh(sprite);
    }

    private void ClearBoard()
    {
        if (boardParent == null) return;

        for (int i = boardParent.childCount - 1; i >= 0; i--)
        {
            Destroy(boardParent.GetChild(i).gameObject);
        }
    }

    private void SetFeedback(string msg)
    {
        if (feedbackText != null)
            feedbackText.text = msg;

        Debug.Log(msg);
    }
}
