using System;
using UnityEngine;

// script for counting the apples
public class AppleCounter : MonoBehaviour
{
    public static int Apples { get; private set; }
    public static event Action<int> OnChanged;   // change UI apple value

    private void OnEnable() => EventManager.OnAppleCollected += AddApple;
    private void OnDisable() => EventManager.OnAppleCollected -= AddApple;

    private void AddApple()
    {
        Apples++;
        // fires after event
        OnChanged?.Invoke(Apples);
    }
}
