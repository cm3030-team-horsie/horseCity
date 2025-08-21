using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlaySounds : MonoBehaviour
{
    [Header("UI Sound Effects")]
    public AudioClip moveSound;
    public AudioClip selectSound;
    public AudioClip appleCollectedSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        EventManager.OnAppleCollected += PlayAppleSound;
    }

    private void OnDisable()
    {
        EventManager.OnAppleCollected -= PlayAppleSound;
    }

    private void PlayAppleSound()
    {
        if (appleCollectedSound != null)
            audioSource.PlayOneShot(appleCollectedSound);
        //else
        //    Debug.LogWarning("No appleCollectedSound assigned!");
    }

    public void PlayMoveSound()
    {
        if (moveSound != null)
                audioSource.PlayOneShot(moveSound);
        //else
        //    Debug.LogWarning("no move sounds");
    }

    public void PlaySelectSound()
    {
        if (selectSound != null)
                audioSource.PlayOneShot(selectSound);
        //else
        //    Debug.LogWarning("no select sound");
    }
}