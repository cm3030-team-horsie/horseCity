using System;
using UnityEngine;
using HorseCity.Core;
using UnityEngine.InputSystem;

// Difficulty options
public enum Difficulty { Easy, Hard }

// Singleton GameManager
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GameState> OnGameStateChanged;

    private GameState currentState;
    private InputActions inputActions;

    // 👇 store current difficulty (default Easy)
    public static Difficulty CurrentDifficulty { get; private set; } = Difficulty.Easy;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] Instance created");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        inputActions = new InputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Gameplay.BreakOut.performed += OnBreakOutPerformed;
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Gameplay.BreakOut.performed -= OnBreakOutPerformed;
            inputActions.Disable();
        }
    }

    private void OnBreakOutPerformed(InputAction.CallbackContext context)
    {
        if (currentState == GameState.WaitingToStart)
        {
            Debug.Log("[GameManager] BreakOut input received → Switching to Playing");
            SetGameState(GameState.Playing);
        }
    }

    public void SetGameState(GameState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            Debug.Log($"[GameManager] GameState changed to {currentState}");
            OnGameStateChanged?.Invoke(currentState);
        }
    }

    public GameState GetGameState() => currentState;

    // 👇 safer way to update difficulty
    public static void SetDifficulty(Difficulty difficulty)
    {
        CurrentDifficulty = difficulty;
        Debug.Log($"[GameManager] Difficulty set to {difficulty}");
    }
}
