using UnityEngine;
using HorseCity.Core;

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    [SerializeField] private bool isOncomingTraffic = false;

    [Header("Path Following")]
    [SerializeField] private bool followNextPath = true; // Whether to follow connected paths

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

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

        splineTraveler.SetIsTravelingForward(!isOncomingTraffic);

        splineTraveler.OnReachedPathEnd += OnReachedPathEnd;
        splineTraveler.OnReachedPathStart += OnReachedPathStart;
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void Start()
    {
        // Snap to nearest path after all components are initialized
        if (splineTraveler != null)
        {
            splineTraveler.ForceSnapToNearestPath();

            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name} snapped to spline: {splineTraveler.SplinePath?.name} at distance: {splineTraveler.CurrentDistance}");
            }
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
            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name} started moving (Playing state)");
            }
        }
    }

    private void OnNotPlaying()
    {
        if (splineTraveler != null)
        {
            splineTraveler.StopMoving();
            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name} stopped moving (Not Playing state)");
            }
        }
    }

    private void OnReachedPathEnd(SplineTraveler traveler)
    {
        if (followNextPath && splineTraveler.SplinePath != null && splineTraveler.SplinePath.GetNextPath() != null)
        {
            TransitionToPath(splineTraveler.SplinePath.GetNextPath(), 0f);
        }
        // Otherwise, the traveler will stop automatically
    }

    private void OnReachedPathStart(SplineTraveler traveler)
    {
        if (followNextPath && splineTraveler.SplinePath != null && splineTraveler.SplinePath.GetPreviousPath() != null)
        {
            // Transition to previous path
            SplinePath previousPath = splineTraveler.SplinePath.GetPreviousPath();
            TransitionToPath(previousPath, previousPath.GetTotalLength());
        }
        // Otherwise, the traveler will stop automatically
    }

    private void TransitionToPath(SplinePath newPath, float distance)
    {
        // Basically just a wrapper
        splineTraveler.SetSplinePath(newPath, distance);

        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name} transitioning to path: {newPath.name}");
        }
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
}
