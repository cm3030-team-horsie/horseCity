using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // set in Inspector

    private void Awake()
    {
        gameObject.SetActive(false); // hide at start
    }

    public void Show()
    {
        gameObject.SetActive(true); // show the panel
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // make sure time is normal
        SceneManager.LoadScene(mainMenuSceneName);
    }
}