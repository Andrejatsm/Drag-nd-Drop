using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuChanger : MonoBehaviour
{
    public void LoadGame1()
    {
        StartCoroutine(LoadSceneFresh("CityScene"));
    }

    public void LoadGame2()
    {
        StartCoroutine(LoadSceneFresh("HanojasScene"));
    }

    public void BackMenu()
    {
        StartCoroutine(LoadSceneFresh("MainScene"));
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game closed!");
    }

    // ---- FULL RELOAD SYSTEM ----
    IEnumerator LoadSceneFresh(string sceneName)
    {
        // Fully reset time scale (important after slow motion / pauses)
        Time.timeScale = 1f;

        // Clear event system clicks before loading
        EventSystem.current?.SetSelectedGameObject(null);

        // Force unloading everything not in the next scene
        AsyncOperation unload = Resources.UnloadUnusedAssets();
        yield return unload;

        // Load scene normally
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        // Wait until fully loaded
        while (!load.isDone)
            yield return null;

        // Flush remaining assets
        yield return Resources.UnloadUnusedAssets();
    }
}
