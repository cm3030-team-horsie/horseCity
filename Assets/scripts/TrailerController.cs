using UnityEngine;
using HorseCity.Core;

public class TrailerController : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Unity lifecycle method. Automatically called when script- or game object is enabled
    void OnEnable()
    {
        // Setup event listener
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    // Unity lifecycle method. Automatically called when script- or game object is disabled
    void OnDisable()
    {
        // Tear down event listener
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                OpenDoors();
                break;
            default:
                break;
        }
    }

    void OpenDoors()
    {
        animator.SetTrigger("Open");
        //animator.Play("OpenDoors");
    }
}
