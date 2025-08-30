using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using HorseCity.Core;

public class MenuSelection : MonoBehaviour
{
    // delay time for menu selection
    [SerializeField] private float sceneLoadDelay = 0.8f;

    public void StartEasyLevel()
    {
        if (GameManager.Instance)
        {
            GameManager.SetDifficulty(Difficulty.Easy); // set difficulty
            GameManager.Instance.SetGameState(GameState.WaitingToStart);
        }
        StartCoroutine(LoadSceneWithDelay("EasyLevel"));
    }

    public void StartHardLevel()
    {
        if (GameManager.Instance)
        {
            GameManager.SetDifficulty(Difficulty.Hard); // set difficulty
            GameManager.Instance.SetGameState(GameState.WaitingToStart);
        }
        StartCoroutine(LoadSceneWithDelay("HardLevel"));
    }

    public void LoadMainMenu()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.SetGameState(GameState.InMenu);
        }
        StartCoroutine(LoadSceneWithDelay("MainMenu"));
    }

    public void LoadInstructions()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.SetGameState(GameState.InMenu);
        }
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
