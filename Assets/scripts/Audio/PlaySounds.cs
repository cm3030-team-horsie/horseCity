using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlaySounds : MonoBehaviour
{
    [Header("UI Sound Effects")]
    public AudioClip moveSound;
    public AudioClip selectSound;
    public AudioClip appleCollectedSound;

    [Header("Collision Sound Effects")]
    public AudioClip carCollisionSound;
    public AudioClip obstacleCollisionSound;

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
        EventManager.OnCarCollision += PlayCarSound;
        EventManager.OnObstacleCollision += PlayObstacleSound;
    }

    private void OnDisable()
    {
        EventManager.OnAppleCollected -= PlayAppleSound;
        EventManager.OnCarCollision -= PlayCarSound;
        EventManager.OnObstacleCollision -= PlayObstacleSound;
    }

    private void PlayAppleSound()
    {
        if (appleCollectedSound != null)
            audioSource.PlayOneShot(appleCollectedSound);
        //else
        //    Debug.LogWarning("no appleCollectedSound assigned!");
    }

    private void PlayCarSound()
    {
        if (carCollisionSound != null)
            audioSource.PlayOneShot(carCollisionSound);
        //else
        //    Debug.LogWarning("no car sound assigned!");
    }

    private void PlayObstacleSound()
    {
        if (obstacleCollisionSound != null)
            audioSource.PlayOneShot(obstacleCollisionSound);
        //else
        //    Debug.LogWarning("no obsacle assigned!");
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