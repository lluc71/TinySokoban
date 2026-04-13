using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Grid grid;

    [Header("Prefabs del tablero")]
    [SerializeField] private TileView floorPrefab;
    [SerializeField] private TileView wallPrefab;
    [SerializeField] private TileView goalPrefab;
    [SerializeField] private Transform levelParent;

    [Header("Prefabs de entidades")]
    [SerializeField] private PlayerView playerPrefab;
    [SerializeField] private BoxView boxPrefab;
    [SerializeField] private Transform entitiesParent;

    [Header("Config. Cámara")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraPadding = 0.5f;

    [Header("Elementos UI")]
    [SerializeField] private GameObject victoryPopup;
    [SerializeField] private TMP_Text levelTextUI;


    [Header("Fichero del Nivel")]
    [SerializeField] private TextAsset levelFile;

    private int width = 8;
    private int height = 8;

    private TileType[,] tiles;
    private Vector2Int playerPos;
    private List<Vector2Int> boxPositions = new();
    private Dictionary<Vector2Int, BoxView> boxViews = new();
    private PlayerView playerView;

    private bool levelCompleted = false;

    private void Start()
    {
        if (LevelSelectorManager.Instance == null) return;

        GetFileToLoad(LevelSelectorManager.Instance.CurrentLevelName);
        GenerateLevel();
        SpawnLevelVisuals();
        FitCameraToLevel();

        levelTextUI.text = $"Nivel: {LevelSelectorManager.Instance.GetCurrentLevelNumber()}";
    }

    public bool IsLevelCompleted()
    {
        return levelCompleted;
    }

    public void TryMovePlayer(Vector2Int dir)
    {
        Vector2Int target = playerPos + dir;

        if (IsOutside(target) || IsWall(target)) return;

        if (IsBox(target))
        {
            Vector2Int boxTarget = target + dir;

            if (IsOutside(boxTarget) || IsWall(boxTarget) || IsBox(boxTarget)) return;

            MoveBox(target, boxTarget);
            MovePlayer(target);
        }
        else
        {
            MovePlayer(target);
        }

        CheckVictory();
    }

    private void CheckVictory()
    {
        if (boxPositions.Count == 0) return;

        foreach (Vector2Int boxPos in boxPositions)
        {
            if (tiles[boxPos.x, boxPos.y] != TileType.Goal)
                return;
        }

        levelCompleted = true;

        if (victoryPopup != null)
            victoryPopup.SetActive(true);
    }

    public bool IsOutside(Vector2Int pos)
    {
        return pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height;
    }

    public bool IsWall(Vector2Int pos)
    {
        return tiles[pos.x, pos.y] == TileType.Wall;
    }

    public bool IsBox(Vector2Int pos)
    {
        return boxViews.ContainsKey(pos);
    }

    private void MovePlayer(Vector2Int newPos)
    {
        if (playerView == null) return;

        playerPos = newPos;
        playerView.SetTile(playerPos);
    }

    private void MoveBox(Vector2Int oldPos, Vector2Int newPos)
    {
        int index = boxPositions.IndexOf(oldPos);
        if (index == -1) return;

        boxPositions[index] = newPos;

        BoxView movedBoxView = boxViews[oldPos];
        boxViews.Remove(oldPos);
        boxViews[newPos] = movedBoxView;

        movedBoxView.SetTile(newPos);
    }

    public void GenerateLevel()
    {
        if (levelFile == null)
        {
            Debug.LogError("No se ha encontrado el archivo del nivel.");
            return;
        }

        levelCompleted = false;
        LoadLevelFromText();
    }

    //TODO: Mover a un LevelViewSpawner
    private void SpawnLevelVisuals()
    {
        ClearSpawnedLevel();
        ClearSpawnedEntities();

        SpawnLevelTiles();
        SpawnEntities();
    }

    private void SpawnLevelTiles()
    {
        if (levelParent == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                TileType tileType = tiles[x, y];

                switch (tileType)
                {
                    case TileType.Floor:
                        SpawnTile(floorPrefab, pos);
                        break;

                    case TileType.Wall:
                        SpawnTile(wallPrefab, pos);
                        break;

                    case TileType.Goal:
                        SpawnTile(floorPrefab, pos);
                        SpawnTile(goalPrefab, pos);
                        break;
                }
            }
        }
    }

    private void SpawnTile(TileView prefab, Vector2Int pos)
    {
        if (prefab == null) return;

        TileView tile = Instantiate(prefab, levelParent);
        tile.Init(grid, pos, width, height);
    }

    private void SpawnEntities()
    {
        if (entitiesParent == null) return;

        playerView = Instantiate(playerPrefab, entitiesParent);
        playerView.Init(grid, playerPos);

        foreach (Vector2Int boxPos in boxPositions)
        {
            BoxView box = Instantiate(boxPrefab, entitiesParent);
            box.Init(grid, boxPos);
            boxViews.Add(boxPos, box);
        }
    }

    private void ClearSpawnedLevel()
    {
        if (levelParent == null) return;

        for (int i = levelParent.childCount - 1; i >= 0; i--)
        {
            Destroy(levelParent.GetChild(i).gameObject);
        }
    }

    private void ClearSpawnedEntities()
    {
        if (entitiesParent == null) return;

        for (int i = entitiesParent.childCount - 1; i >= 0; i--)
        {
            Destroy(entitiesParent.GetChild(i).gameObject);
        }
    }
    private void FitCameraToLevel()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (mainCamera == null || !mainCamera.orthographic) return;

        float aspect = (float)Screen.width / Screen.height;

        float halfHeight = height / 2f;
        float halfWidthAdjusted = width / (2f * aspect);

        mainCamera.orthographicSize = Mathf.Max(halfHeight, halfWidthAdjusted) + cameraPadding;

        // Centrar cámara en el mapa
        float centerX = width / 2f;
        float centerY = height / 2f;

        Vector3 camPos = mainCamera.transform.position;
        mainCamera.transform.position = new Vector3(centerX, centerY, camPos.z);
    }


    /*
     * **************************
     * GENERADOR DE NIVELES
     ****************************
     */
    private void LoadLevelFromText()
    {
        string levelText = levelFile.text;
        boxPositions.Clear();
        boxViews.Clear();

        if (string.IsNullOrWhiteSpace(levelText))
        {
            Debug.LogError("El fichero está vacío.");
            return;
        }

        string[] rawLines = levelText
            .Replace("\r", "")
            .Split('\n', System.StringSplitOptions.RemoveEmptyEntries);

        // Comprobamos el tamaño del mapa
        string[] sizeParts = rawLines[0].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        if (sizeParts.Length < 2 || !int.TryParse(sizeParts[0], out width) || !int.TryParse(sizeParts[1], out height))
        {
            Debug.LogError("La primera línia del fichero debe contener: Width y Height");
            return;
        }

        if (rawLines.Length - 1 < height)
        {
            Debug.LogError($"El fichero no tiene suficientes filas. Esperadas: {height}");
            return;
        }

        tiles = new TileType[width, height];

        bool playerFound = false;

        // Leemos desde arriba hacia abajo en el txt, pero en el array lo guardamos con y=0 abajo.
        for (int fileRow = 0; fileRow < height; fileRow++)
        {
            string line = rawLines[fileRow + 1];

            if (line.Length < width)
            {
                Debug.LogError($"La fila {fileRow + 1} no tiene suficientes columnas. Esperadas: {width}");
                return;
            }

            int y = height - 1 - fileRow;

            for (int x = 0; x < width; x++)
            {
                char c = line[x];

                switch (c)
                {
                    case '#':
                        tiles[x, y] = TileType.Wall;
                        break;

                    case '.':
                        tiles[x, y] = TileType.Floor;
                        break;

                    case 'G':
                        tiles[x, y] = TileType.Goal;
                        break;

                    case 'P':
                        tiles[x, y] = TileType.Floor;
                        playerPos = new Vector2Int(x, y);
                        playerFound = true;
                        break;

                    case 'B':
                        tiles[x, y] = TileType.Floor;
                        boxPositions.Add(new Vector2Int(x, y));
                        break;

                    case '+': // Player sobre Goal
                        tiles[x, y] = TileType.Goal;
                        playerPos = new Vector2Int(x, y);
                        playerFound = true;
                        break;

                    case '*': // Box sobre Goal
                        tiles[x, y] = TileType.Goal;
                        boxPositions.Add(new Vector2Int(x, y));
                        break;

                    default:
                        Debug.LogError($"Carácter no reconocido '{c}' en ({x}, {y}).");
                        return;
                }
            }
        }

        if (!playerFound)
        {
            Debug.LogError("El nivel no contiene un jugador ('P' o '+').");
        }
    }

    private void GetFileToLoad(string levelName)
    {
        if (string.IsNullOrEmpty(levelName)) return;

        TextAsset loaded = Resources.Load<TextAsset>($"Levels/{levelName}");

        if (loaded == null)
        {
            Debug.LogError($"No se encontró el nivel: {levelName}");
            SceneManager.LoadScene("MainMenu");
            return;
        }

        levelFile = loaded;
    }
}
