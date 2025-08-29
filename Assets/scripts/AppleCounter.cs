using System;
using UnityEngine;

public class AppleCounter : MonoBehaviour
{
    public static int Apples { get; private set; }
    public static event Action<int> OnChanged;

    private void OnEnable() => EventManager.OnAppleCollected += AddApple;
    private void OnDisable() => EventManager.OnAppleCollected -= AddApple;

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
