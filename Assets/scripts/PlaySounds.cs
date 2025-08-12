using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlaySounds : MonoBehaviour
{
    [Header("UI Sound Effects")]
    public AudioClip moveSound;   // e.g. for hover
    public AudioClip selectSound; // e.g. for click

    private AudioSource sound;

    private void Awake()
    {
        sound = GetComponent<AudioSource>();
        sound.playOnAwake = false;
        sound.spatialBlend = 0f; // make it 2D for UI
    }

    public void PlayMoveSound()
    {
        if (moveSound != null)
            sound.PlayOneShot(moveSound);
        else
            Debug.LogWarning("Move sound not assigned!");
    }

    public void PlaySelectSound()
    {
        if (selectSound != null)
            sound.PlayOneShot(selectSound);
        else
            Debug.LogWarning("Select sound not assigned!");
    }

    //public void PlayASound(AudioClip clip)
    //{
    //    if (clip != null)
    //        sound.PlayOneShot(clip);
    //    else
    //        Debug.LogWarning("AudioClip not assigned to PlayASound!");
    //}
}
