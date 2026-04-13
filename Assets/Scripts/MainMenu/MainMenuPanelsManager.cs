using System.Linq;
using UnityEngine;

public class MainMenuPanelsManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private LevelEntryUI levelEntryPrefab;
    [SerializeField] private MainMenuManager sceneManager;

    private void Start()
    {
        GenerateLevelsList();
    }

    public void GenerateLevelsList()
    {
        ClearList();

        TextAsset[] levelFiles = Resources.LoadAll<TextAsset>("Levels")
            .OrderBy(file => file.name)
            .ToArray();

        foreach (TextAsset levelFile in levelFiles)
        {
            LevelEntryUI entry = Instantiate(levelEntryPrefab, contentParent);
            entry.Init(levelFile.name, sceneManager);
        }
    }

    private void ClearList()
    {
        if (contentParent == null) return;

        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }
}
