using System;
using UnityEngine;
using HorseCity.Core;
using UnityEngine.InputSystem;

// Singleton
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GameState> OnGameStateChanged;

    private GameState currentState;

    private InputActions inputActions;

    // Unity lifecycle method. Automatically called when script- or game object is enabled
    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Gameplay.BreakOut.performed += OnBreakOutPerformed;
    }

    // Unity lifecycle method. Automatically called when script- or game object is disabled
    private void OnDisable()
    {
        inputActions.Gameplay.BreakOut.performed -= OnBreakOutPerformed;
        inputActions.Disable();
    }

    private void OnBreakOutPerformed(InputAction.CallbackContext context)
    {
        if (currentState == GameState.WaitingToStart)
        {
            // Break out of the trailer!
            SetGameState(GameState.Playing);
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        inputActions = new InputActions();
    }

    public void SetGameState(GameState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            // Invoke state changed event if we have event listeners
            OnGameStateChanged?.Invoke(currentState);
        }
    }

    public GameState GetGameState() => currentState;
}
