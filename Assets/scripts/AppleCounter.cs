using System;
using UnityEngine;
using HorseCity.Core;

public class AppleCounter : MonoBehaviour
{
    public static int Apples { get; private set; }
    public static event Action<int> OnChanged;

    private void OnEnable()
    {
        EventManager.OnAppleCollected += AddApple;
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        EventManager.OnAppleCollected -= AddApple;
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.WaitingToStart)
        {
            Reset();
        }
    }

    private void AddApple()
    {
        Apples++;
        OnChanged?.Invoke(Apples);
    }

    public static void Reset()
    {
        Apples = 0;
        OnChanged?.Invoke(Apples);  // UI back to 0
        Debug.Log("[AppleCounter] Reset apples to 0");
    }
}
