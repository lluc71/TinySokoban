using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Popups")]
    [SerializeField] private GameObject levelSelectorPanel;

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ShowLevelSelectorPanel(bool active)
    {
        levelSelectorPanel.SetActive(active);
    }

    public void LoadNewEditorScene()
    {
        if (LevelSelectorManager.Instance == null) return;

        LevelSelectorManager.Instance.SetNewLevel();
        LoadScene("Editor");
    }

    public void LoadNextLevelGameScene()
    {
        if (LevelSelectorManager.Instance == null) return;

        string nextLevel = LevelSelectorManager.Instance.GetNextLevelName();
        if (string.IsNullOrEmpty(nextLevel))
        {
            LoadScene("MainMenu");
            return;
        }

        LevelSelectorManager.Instance.SetLoadedLevel(nextLevel);
        LoadScene("Game");
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
