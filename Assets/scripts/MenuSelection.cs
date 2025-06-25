using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MenuSelection : MonoBehaviour
{
    public void StartEasyLevel()
    {
        SceneManager.LoadScene("EasyLevel");
    }

    public void StartMediumLevel()
    {
        SceneManager.LoadScene("MediumLevel");
    }

    public void StartHardLevel()
    {
        SceneManager.LoadScene("HardLevel");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game exited.");
    }
}