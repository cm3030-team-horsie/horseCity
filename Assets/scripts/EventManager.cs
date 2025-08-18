using System;

public static class EventManager
{
    public static event Action OnAppleCollected;
    //public static event Action OnPlayerDied;
    //public static event Action OnEnemyDied;

    public static void RaiseAppleCollected() => OnAppleCollected?.Invoke();
    //public static void RaiseLifeLoss() => OnPlayerDied?.Invoke();
    //public static void RaiseEnemyDied() => OnEnemyDied?.Invoke();
}