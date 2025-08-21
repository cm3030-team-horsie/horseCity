using System;

public static class EventManager
{
    public static event Action OnAppleCollected;
    public static event Action OnCarCollision;
    public static event Action OnObstacleCollision;

    public static void RaiseAppleCollected() => OnAppleCollected?.Invoke();
    public static void RaiseCarCollision() => OnCarCollision?.Invoke();
    public static void RaiseObstacleCollision() => OnObstacleCollision?.Invoke();
}