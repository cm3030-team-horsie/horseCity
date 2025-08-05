using UnityEngine;

public class LaneSwitcher : MonoBehaviour
{
    [Header("Lane Switching")]
    [SerializeField] private float switchDuration = 0.5f;
    [SerializeField] private float switchCooldown = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private SplineTraveler splineTraveler;
    private float lastSwitchTime;
    private bool isSwitchingLanes = false;
    private float switchTimer = 0f;
    private SplinePath targetLane;
    private Vector3 switchStartPosition;
    private Quaternion switchStartRotation;

    public System.Action<LaneSwitcher, SplinePath> OnLaneSwitchStarted;
    public System.Action<LaneSwitcher, SplinePath> OnLaneSwitchCompleted;

    private void Awake()
    {
        splineTraveler = GetComponent<SplineTraveler>();
        if (splineTraveler == null)
        {
            Debug.LogError($"LaneSwitcher requires a SplineTraveler component on {gameObject.name}");
        }
    }

    private void Update()
    {
        if (isSwitchingLanes)
        {
            UpdateLaneSwitch();
        }
    }

    public bool TrySwitchToLeftLane()
    {
        if (!CanSwitchLanes()) return false;

        SplinePath currentPath = splineTraveler.SplinePath;
        if (currentPath == null) return false;

        SplinePath leftLane = currentPath.GetLeftLane();
        if (leftLane == null) return false;

        return StartLaneSwitch(leftLane);
    }

    public bool TrySwitchToRightLane()
    {
        if (!CanSwitchLanes()) return false;

        SplinePath currentPath = splineTraveler.SplinePath;
        if (currentPath == null) return false;

        SplinePath rightLane = currentPath.GetRightLane();
        if (rightLane == null) return false;

        return StartLaneSwitch(rightLane);
    }

    public bool CanSwitchLanes()
    {
        if (splineTraveler == null) return false;
        if (isSwitchingLanes) return false;
        if (Time.time - lastSwitchTime < switchCooldown) return false;
        if (!splineTraveler.IsMoving) return false;
        return true;
    }

    public bool CanSwitchToLeftLane()
    {
        if (!CanSwitchLanes()) return false;
        SplinePath currentPath = splineTraveler.SplinePath;
        return currentPath != null && currentPath.GetLeftLane() != null;
    }

    public bool CanSwitchToRightLane()
    {
        if (!CanSwitchLanes()) return false;
        SplinePath currentPath = splineTraveler.SplinePath;
        return currentPath != null && currentPath.GetRightLane() != null;
    }

    private bool StartLaneSwitch(SplinePath newLane)
    {
        if (!CanSwitchLanes()) return false;
        switchStartPosition = transform.position;
        switchStartRotation = transform.rotation;
        targetLane = newLane;
        isSwitchingLanes = true;
        switchTimer = 0f;
        lastSwitchTime = Time.time;

        // Notify listeners
        OnLaneSwitchStarted?.Invoke(this, newLane);

        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name} started switching to lane: {newLane.name}");
        }

        return true;
    }

    private void UpdateLaneSwitch()
    {
        switchTimer += Time.deltaTime;
        float progress = switchTimer / switchDuration;

        if (progress >= 1f)
        {
            // Switch completed
            CompleteLaneSwitch();
        }
        else
        {
            // Continue switching animation
            UpdateSwitchAnimation(progress);
        }
    }

    private void UpdateSwitchAnimation(float progress)
    {
        if (targetLane == null) return;

        // Calculate target position on the new lane
        // TODO: Seems like there is something weird going on here? Horse kind of makes an arc when switching lanes
        float currentDistance = splineTraveler.CurrentDistance;
        Vector3 targetPosition = targetLane.GetPositionAtDistance(currentDistance);

        Vector3 targetDirection = targetLane.GetDirectionAtDistance(currentDistance);
        if (!splineTraveler.MovingForward)
        {
            targetDirection = -targetDirection;
        }
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        // Smooth interpolation between start and target
        transform.position = Vector3.Lerp(switchStartPosition, targetPosition, progress);
        transform.rotation = Quaternion.Slerp(switchStartRotation, targetRotation, progress);
    }

    private void CompleteLaneSwitch()
    {
        if (targetLane == null) return;

        // Update the spline traveler to use the new lane
        float currentDistance = splineTraveler.CurrentDistance;
        splineTraveler.SetSplinePath(targetLane, currentDistance);

        isSwitchingLanes = false;
        targetLane = null;

        // Notify listeners
        OnLaneSwitchCompleted?.Invoke(this, splineTraveler.SplinePath);

        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name} completed lane switch to: {splineTraveler.SplinePath.name}");
        }
    }

    public bool IsSwitchingLanes => isSwitchingLanes;

    public SplinePath GetCurrentLane()
    {
        return splineTraveler?.SplinePath;
    }

    public SplinePath GetLeftLane()
    {
        SplinePath currentPath = splineTraveler?.SplinePath;
        return currentPath?.GetLeftLane();
    }

    public SplinePath GetRightLane()
    {
        SplinePath currentPath = splineTraveler?.SplinePath;
        return currentPath?.GetRightLane();
    }
}
