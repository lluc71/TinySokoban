using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEntryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button editButton;

    private string levelName;
    private MainMenuManager sceneManager;

    public void Init(string levelName, MainMenuManager sceneManager)
    {
        this.levelName = levelName;
        this.sceneManager = sceneManager;

        if (levelNameText != null)
            levelNameText.text = levelName;

        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayPressed);
        }

        if (editButton != null)
        {
            editButton.onClick.RemoveAllListeners();
            editButton.onClick.AddListener(OnEditPressed);
        }
    }

    private void OnPlayPressed()
    {
        if (LevelSelectorManager.Instance != null)
            LevelSelectorManager.Instance.SetLoadedLevel(levelName);

        if (sceneManager != null)
            sceneManager.LoadScene("Game");
    }

    private void OnEditPressed()
    {
        if (LevelSelectorManager.Instance != null)
            LevelSelectorManager.Instance.SetLoadedLevel(levelName);

        if (sceneManager != null)
            sceneManager.LoadScene("Editor");
    }
}
