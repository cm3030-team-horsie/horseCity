using UnityEngine;
using HorseCity.Core;

[RequireComponent(typeof(AudioSource))]
public class TrafficAudio : MonoBehaviour
{
    private AudioSource trafficAudio;

    private void Awake()
    {
        trafficAudio = GetComponent<AudioSource>();
        trafficAudio.loop = true;
        trafficAudio.playOnAwake = true;  // plays when scene loads

        // play audio when the level starts
        trafficAudio.Play();

        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.GameOver && trafficAudio.isPlaying)
        {
            trafficAudio.Stop();
        }
    }
}