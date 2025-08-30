using UnityEngine;
using HorseCity.Core;

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    [SerializeField] private bool isOncomingTraffic = false;

    [Header("Path Following")]
    [SerializeField] private bool followNextPath = true;

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

        // forward or backward
        splineTraveler.SetIsTravelingForward(!isOncomingTraffic);

        splineTraveler.OnReachedPathEnd += OnReachedPathEnd;
        splineTraveler.OnReachedPathStart += OnReachedPathStart;

        // listen for game state change
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void Start()
    {
        // snap to nearest path
        if (splineTraveler != null)
        {
            splineTraveler.ForceSnapToNearestPath();

            // apply difficulty speeds
            ApplyDifficultySettings();
        }
    }

    private void OnDestroy()
    {
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
        else
        {
            // destroy the car slightly before the end of the road
            float totalLength = splineTraveler.SplinePath.GetTotalLength();
            float currentDistance = splineTraveler.CurrentDistance;

            // 90% of the way of the path
            if (currentDistance >= totalLength * 0.90f)
            {
                Destroy(gameObject);
            }
        }
    }


    private void OnReachedPathStart(SplineTraveler traveler)
    {
        if (followNextPath && splineTraveler.SplinePath != null && splineTraveler.SplinePath.GetPreviousPath() != null)
        {
            // transition to previous path
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
