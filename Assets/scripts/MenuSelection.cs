using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSelection : MonoBehaviour
{
    // delay time for menu selection
    [SerializeField] private float sceneLoadDelay = 0.8f;

    public void StartEasyLevel()
    {
        GameManager.SetDifficulty(Difficulty.Easy);   // set difficulty
        StartCoroutine(LoadSceneWithDelay("EasyLevel"));
    }

    public void StartHardLevel()
    {
        GameManager.SetDifficulty(Difficulty.Hard); // set difficulty
        StartCoroutine(LoadSceneWithDelay("HardLevel"));
    }

    public void LoadMainMenu()
    {
        StartCoroutine(LoadSceneWithDelay("MainMenu"));
    }

    public void LoadInstructions()
    {
        StartCoroutine(LoadSceneWithDelay("Instructions"));
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game exited.");
    }

    private IEnumerator LoadSceneWithDelay(string sceneName)
    {
        yield return new WaitForSeconds(sceneLoadDelay);
        SceneManager.LoadScene(sceneName);
    }
}
