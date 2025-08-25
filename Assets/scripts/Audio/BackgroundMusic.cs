using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance;

    private AudioSource musicSource;

    // scenes that must carry the music through
    private string[] allowedScenes = { "Instructions", "MainMenu" };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = true;
        musicSource.Play();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // check if this scene should play music
        bool shouldPlay = false;
        foreach (string name in allowedScenes)
        {
            if (scene.name == name)
            {
                shouldPlay = true;
                break;
            }
        }

        if (shouldPlay && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
        else if (!shouldPlay && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
}
