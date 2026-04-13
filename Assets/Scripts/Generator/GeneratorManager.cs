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
    [SerializeField] private string levelsFolderName = "Resources/Levels";
    [SerializeField] private string filePrefix = "level_";

    private TextAsset levelToLoad;

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
        if (LevelSelectorManager.Instance != null && LevelSelectorManager.Instance.IsEditingLoadedLevel)
        {
            GetFileToLoad(LevelSelectorManager.Instance.CurrentLevelName);
            LoadLevelFromFile();
        }
        else
        {
            UpdateGrid();
        }
    }

    /**
     * Genera un Grid con Suelos basicos a partir del valor de los Inputs
     */
    public void UpdateGrid()
    {
        if(!CheckInputs()) return;

        ResetGridData();
        UpdateVisualGrid();

        SetFeedback($"Se ha creado un Grid de {width}x{height}.");
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

    /**
     * Limpiamos los datos del Grid actual
     */
    private void ResetGridData()
    {
        baseTiles = new TileType[width, height];
        tileViews = new LevelEditorTileView[width, height];
        boxPositions.Clear();
        playerPos = null;
    }

    private void ClearVisualBoard()
    {
        if (boardParent == null) return;

        for (int i = boardParent.childCount - 1; i >= 0; i--)
        {
            Destroy(boardParent.GetChild(i).gameObject);
        }
    }

    private void ConfigureGridLayout()
    {
        if (gridLayoutGroup == null) return;

        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = width;
    }

    private void SpawnGridTiles()
    {
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                //Spawn Tile
                LevelEditorTileView tile = Instantiate(tilePrefab, boardParent);
                tile.Init(this, new Vector2Int(x, y));
                tileViews[x, y] = tile;
                RefreshTileVisual(x, y);
            }
        }
    }

    private void UpdateVisualGrid()
    {
        ClearVisualBoard();
        ConfigureGridLayout();
        SpawnGridTiles();
    }

    /**
     * Pinta dependiendo del @currentBrush seleccionado en la posición @pos
     */
    public void PaintAt(Vector2Int pos)
    {
        if (baseTiles == null || !IsInside(pos)) return;

        switch (currentBrush)
        {
            case GeneratorBrushType.Floor:
            case GeneratorBrushType.Wall:
            case GeneratorBrushType.Goal:
                PaintBaseTile(pos, currentBrush);
                break;

            case GeneratorBrushType.Box:
                EnsureWalkableBase(pos);
                RemovePlayerAt(pos);

                if (!boxPositions.Contains(pos))
                    boxPositions.Add(pos);
                break;

            case GeneratorBrushType.Player:
                EnsureWalkableBase(pos);
                RemoveBoxAt(pos);
                playerPos = pos;
                break;
        }

        RefreshAllVisuals();
    }

    /**
     * Pinta un Tile Base (Floor, Wall, Goal). Elimina el objeto que habia en esa posicion.
     */
    private void PaintBaseTile(Vector2Int pos, GeneratorBrushType brush)
    {
        baseTiles[pos.x, pos.y] = brush switch
        {
            GeneratorBrushType.Floor => TileType.Floor,
            GeneratorBrushType.Wall => TileType.Wall,
            GeneratorBrushType.Goal => TileType.Goal,
            _ => baseTiles[pos.x, pos.y]
        };

        RemoveBoxAt(pos);
        RemovePlayerAt(pos);
    }

    /**
     * Si el Player se coloca encima de un Wall, lo convierte a Floor.
     */
    private void EnsureWalkableBase(Vector2Int pos)
    {
        if (baseTiles[pos.x, pos.y] == TileType.Wall)
            baseTiles[pos.x, pos.y] = TileType.Floor;
    }

    public void ExportLevel()
    {
        if (!CanExportLevel()) return;

        string folderPath = Path.Combine(Application.dataPath, levelsFolderName);
        Directory.CreateDirectory(folderPath);

        string filePath = GetExportFilePath(folderPath);
        File.WriteAllText(filePath, BuildLevelText());

        string exportedLevelName = Path.GetFileNameWithoutExtension(filePath);

        if (LevelSelectorManager.Instance != null)
        {
            LevelSelectorManager.Instance.SetLoadedLevel(exportedLevelName);
        }

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        SetFeedback($"Nivel exportado en:\n{filePath}");
    }

    private bool CanExportLevel()
    {
        if (baseTiles == null)
        {
            SetFeedback("Primero crea un grid.");
            return false;
        }

        if (!playerPos.HasValue)
        {
            SetFeedback("Debe existir al menos 1 Player.");
            return false;
        }

        int goalCount = CountGoals();
        int boxCount = boxPositions.Count;

        if (goalCount < boxCount)
        {
            SetFeedback($"Faltan Goals. Hay {boxCount} cajas y {goalCount} objetivos.");
            return false;
        }

        return true;
    }

    private string GetExportFilePath(string folderPath)
    {
        if (IsEditingExistingLevel())
            return Path.Combine(folderPath, LevelSelectorManager.Instance.CurrentLevelName + ".txt");

        return GetNextAvailableLevelPath(folderPath);
    }

    private bool IsEditingExistingLevel()
    {
        return LevelSelectorManager.Instance != null &&
               LevelSelectorManager.Instance.IsEditingLoadedLevel &&
               !string.IsNullOrEmpty(LevelSelectorManager.Instance.CurrentLevelName);
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

    private void SetFeedback(string msg)
    {
        if (feedbackText != null)
            feedbackText.text = msg;

        Debug.Log(msg);
    }

    //#### CARGAR UN NIVEL ####
    public void LoadLevelFromFile()
    {
        if (levelToLoad == null)
        {
            SetFeedback("No hay ningún archivo de nivel asignado.");
            return;
        }

        if (!TryParseLevelText(levelToLoad.text, out int loadedWidth, out int loadedHeight))
            return;

        width = loadedWidth;
        height = loadedHeight;

        if (widthInput != null) widthInput.text = width.ToString();
        if (heightInput != null) heightInput.text = height.ToString();

        UpdateVisualGrid();

        SetFeedback($"Nivel cargado: {levelToLoad.name}");
    }

    private bool TryParseLevelText(string levelText, out int loadedWidth, out int loadedHeight)
    {
        loadedWidth = 0;
        loadedHeight = 0;

        if (string.IsNullOrWhiteSpace(levelText))
        {
            SetFeedback("El archivo está vacío.");
            return false;
        }

        string[] lines = levelText
            .Replace("\r", "")
            .Split('\n', System.StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
        {
            SetFeedback("Formato inválido. Falta cabecera o mapa.");
            return false;
        }

        string[] sizeParts = lines[0].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

        if (sizeParts.Length < 2 ||
            !int.TryParse(sizeParts[0], out loadedWidth) ||
            !int.TryParse(sizeParts[1], out loadedHeight))
        {
            SetFeedback("La primera línea debe tener: width height");
            return false;
        }

        if (lines.Length - 1 < loadedHeight)
        {
            SetFeedback("El archivo no tiene suficientes filas.");
            return false;
        }

        baseTiles = new TileType[loadedWidth, loadedHeight];
        tileViews = new LevelEditorTileView[loadedWidth, loadedHeight];
        boxPositions.Clear();
        playerPos = null;

        for (int y = 0; y < loadedHeight; y++)
        {
            string line = lines[y + 1];

            if (line.Length < loadedWidth)
            {
                SetFeedback($"La fila {y + 1} tiene menos columnas de las esperadas.");
                return false;
            }

            for (int x = 0; x < loadedWidth; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                char c = line[x];

                switch (c)
                {
                    case '#':
                        baseTiles[x, y] = TileType.Wall;
                        break;

                    case '.':
                        baseTiles[x, y] = TileType.Floor;
                        break;

                    case 'G':
                        baseTiles[x, y] = TileType.Goal;
                        break;

                    case 'B':
                        baseTiles[x, y] = TileType.Floor;
                        boxPositions.Add(pos);
                        break;

                    case 'P':
                        baseTiles[x, y] = TileType.Floor;
                        playerPos = pos;
                        break;

                    case '*':
                        baseTiles[x, y] = TileType.Goal;
                        boxPositions.Add(pos);
                        break;

                    case '+':
                        baseTiles[x, y] = TileType.Goal;
                        playerPos = pos;
                        break;

                    default:
                        SetFeedback($"Carácter inválido '{c}' en ({x}, {y}).");
                        return false;
                }
            }
        }

        if (playerPos == null)
        {
            SetFeedback("El nivel no contiene Player.");
            return false;
        }

        return true;
    }

    private void GetFileToLoad(string levelName)
    {
        if (string.IsNullOrEmpty(levelName))
        {
            SetFeedback("Nombre de nivel inválido.");
            UpdateGrid();
            return;
        }

        TextAsset file = Resources.Load<TextAsset>($"Levels/{levelName}");

        if (file == null)
        {
            SetFeedback($"No se encontró el nivel: {levelName}");
            UpdateGrid();
            return;
        }

        levelToLoad = file;
    }

}
