using UnityEngine;

public class LaneSwitcher : MonoBehaviour
{
    [Header("Lane Switching")]
    [SerializeField] private float switchDuration = 0.5f;
    [SerializeField] private float switchCooldown = 0.3f;

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
    }

    private void Update()
    {
        if (isSwitchingLanes) UpdateLaneSwitch();
    }

    public bool TrySwitchToLeftLane()
    {
        if (!CanSwitchLanes()) return false;
        SplinePath leftLane = splineTraveler.SplinePath?.GetLeftLane();
        if (leftLane == null) return false;
        return StartLaneSwitch(leftLane);
    }

    public bool TrySwitchToRightLane()
    {
        if (!CanSwitchLanes()) return false;
        SplinePath rightLane = splineTraveler.SplinePath?.GetRightLane();
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

    private bool StartLaneSwitch(SplinePath newLane)
    {
        if (!CanSwitchLanes()) return false;
        switchStartPosition = transform.position;
        switchStartRotation = transform.rotation;
        targetLane = newLane;
        isSwitchingLanes = true;
        switchTimer = 0f;
        lastSwitchTime = Time.time;

        OnLaneSwitchStarted?.Invoke(this, newLane);
        return true;
    }

    private void UpdateLaneSwitch()
    {
        switchTimer += Time.deltaTime;
        float progress = switchTimer / switchDuration;

        if (progress >= 1f) CompleteLaneSwitch();
        else UpdateSwitchAnimation(progress);
    }

    private void UpdateSwitchAnimation(float progress)
    {
        if (targetLane == null) return;

        float currentDistance = splineTraveler.CurrentDistance;
        Vector3 targetPosition = targetLane.GetPositionAtDistance(currentDistance);
        Vector3 targetDirection = targetLane.GetDirectionAtDistance(currentDistance);

        if (!splineTraveler.MovingForward) targetDirection = -targetDirection;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        transform.position = Vector3.Lerp(switchStartPosition, targetPosition, progress);
        transform.rotation = Quaternion.Slerp(switchStartRotation, targetRotation, progress);
    }

    private void CompleteLaneSwitch()
    {
        if (targetLane == null) return;
        float currentDistance = splineTraveler.CurrentDistance;
        splineTraveler.SetSplinePath(targetLane, currentDistance);

        isSwitchingLanes = false;
        targetLane = null;

        OnLaneSwitchCompleted?.Invoke(this, splineTraveler.SplinePath);
    }

    public bool IsSwitchingLanes => isSwitchingLanes;

    // difficulty tuning fro switching
    public float SwitchDuration
    {
        get => switchDuration;
        set => switchDuration = value;
    }
}
