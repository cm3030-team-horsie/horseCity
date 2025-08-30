using UnityEngine;
using HorseCity.Core;

public class TrailerController : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        // sets up event listener
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    void OnDisable()
    {
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
