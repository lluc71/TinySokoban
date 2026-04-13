using UnityEngine;

public class LevelSelectorManager : MonoBehaviour
{
    public static LevelSelectorManager Instance { get; private set; }

    private bool isEditingLoadedLevel;
    private string currentLevelName;

    public bool IsEditingLoadedLevel => isEditingLoadedLevel;
    public string CurrentLevelName => currentLevelName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetNewLevel()
    {
        isEditingLoadedLevel = false;
        currentLevelName = string.Empty;
    }

    public void SetLoadedLevel(string levelName)
    {
        isEditingLoadedLevel = true;
        currentLevelName = levelName;
    }

    public int GetCurrentLevelNumber()
    {
        if (string.IsNullOrEmpty(currentLevelName))
            return -1;

        string[] parts = currentLevelName.Split('_');
        if (parts.Length != 2)
            return -1;

        return int.TryParse(parts[1], out int number) ? number : -1;
    }

    public string GetNextLevelName()
    {
        int currentNumber = GetCurrentLevelNumber();
        if (currentNumber < 0)
            return string.Empty;

        return $"level_{(currentNumber + 1):000}";
    }
}
