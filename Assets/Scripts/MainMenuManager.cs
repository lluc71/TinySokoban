using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private float loadDelay = 1f;

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithDelay(sceneName));
    }

    private IEnumerator LoadSceneWithDelay(string sceneName)
    {
        yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(sceneName);
    }

}
