using UnityEngine;
using HorseCity.Core;

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    [SerializeField] private bool isOncomingTraffic = false;

    [Header("Path Following")]
    [SerializeField] private bool followNextPath = true; // Whether to follow connected paths

    private SplineTraveler splineTraveler;

    private void Awake()
    {
        // Get the spline traveler component
        splineTraveler = GetComponent<SplineTraveler>();
        if (splineTraveler == null)
        {
            Debug.LogError($"CarController requires a SplineTraveler component on {gameObject.name}");
            return;
        }

        // Forward or backward depending on traffic type
        splineTraveler.SetIsTravelingForward(!isOncomingTraffic);

        // Hook into spline events
        splineTraveler.OnReachedPathEnd += OnReachedPathEnd;
        splineTraveler.OnReachedPathStart += OnReachedPathStart;

        // Listen for game state
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void Start()
    {
        // Snap to nearest path after all components are initialized
        if (splineTraveler != null)
        {
            splineTraveler.ForceSnapToNearestPath();

            // apply difficulty speeds
            ApplyDifficultySettings();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from game state changes
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                OnPlaying();
                break;
            default:
                OnNotPlaying();
                break;
        }
    }

    private void OnPlaying()
    {
        if (splineTraveler != null)
        {
            splineTraveler.StartMoving();
        }
    }

    private void OnNotPlaying()
    {
        if (splineTraveler != null)
        {
            splineTraveler.StopMoving();
        }
    }

    private void OnReachedPathEnd(SplineTraveler traveler)
    {
        if (followNextPath && splineTraveler.SplinePath != null && splineTraveler.SplinePath.GetNextPath() != null)
        {
            TransitionToPath(splineTraveler.SplinePath.GetNextPath(), 0f);
        }
    }

    private void OnReachedPathStart(SplineTraveler traveler)
    {
        if (followNextPath && splineTraveler.SplinePath != null && splineTraveler.SplinePath.GetPreviousPath() != null)
        {
            // Transition to previous path
            SplinePath previousPath = splineTraveler.SplinePath.GetPreviousPath();
            TransitionToPath(previousPath, previousPath.GetTotalLength());
        }
    }

    private void TransitionToPath(SplinePath newPath, float distance)
    {
        splineTraveler.SetSplinePath(newPath, distance);
    }

    public void SetOncomingTraffic(bool oncoming)
    {
        isOncomingTraffic = oncoming;
        if (splineTraveler != null)
        {
            splineTraveler.SetIsTravelingForward(!isOncomingTraffic);
        }
    }

    public bool IsOncomingTraffic()
    {
        return isOncomingTraffic;
    }

    // difficulty settings for car speed
    private void ApplyDifficultySettings()
    {
        if (GameManager.CurrentDifficulty == Difficulty.Easy)
        {
            splineTraveler.TravelSpeed = 9f;  // easy level car speed
        }
        else if (GameManager.CurrentDifficulty == Difficulty.Hard)
        {
            splineTraveler.TravelSpeed = 14f; // hard level car speed
        }
    }
}
