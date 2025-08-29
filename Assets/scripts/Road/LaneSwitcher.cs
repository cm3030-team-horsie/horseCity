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
    private float switchTargetDistance;
    private Vector3 switchTargetPosition;
    private Quaternion switchTargetRotation;

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

        if (!CalculateSwitchTarget(newLane, out Vector3 targetPos, out Quaternion targetRot))
        {
            Debug.LogWarning("Failed to calculate switch target position");
            return false;
        }

        switchStartPosition = transform.position;
        switchStartRotation = transform.rotation;
        switchTargetDistance = FindClosestDistanceOnSpline(newLane, targetPos) + (switchDuration * splineTraveler.TravelSpeed);
        switchTargetPosition = newLane.GetPositionAtDistance(switchTargetDistance);
        switchTargetRotation = Quaternion.LookRotation(newLane.GetDirectionAtDistance(switchTargetDistance)); //targetRot;
        targetLane = newLane;
        isSwitchingLanes = true;
        switchTimer = 0f;
        lastSwitchTime = Time.time;

        OnLaneSwitchStarted?.Invoke(this, newLane);
        return true;
    }

    private bool CalculateSwitchTarget(SplinePath targetLane, out Vector3 targetPosition, out Quaternion targetRotation)
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;

        if (targetLane == null || targetLane.controlPoints.Count < 2)
            return false;

        // Not really needed since the player is always facing forward, and the player is the only lane traveler capable of switching lanes, but whatever
        Vector3 playerForward = transform.forward;
        if (!splineTraveler.MovingForward)
            playerForward = -playerForward;

        // Create a plane perpendicular to the player's forward direction
        Vector3 planeNormal = playerForward;
        Vector3 planePoint = transform.position;

        Vector3 bestIntersectionPoint = Vector3.zero;
        float bestDistance = float.MaxValue;
        Vector3 bestDirection = Vector3.forward;
        // Find the intersection point with the target lane using control points
        for (int i = 0; i < targetLane.controlPoints.Count - 1; i++)
        {
            Vector3 startPoint = targetLane.transform.TransformPoint(targetLane.controlPoints[i]);
            Vector3 endPoint = targetLane.transform.TransformPoint(targetLane.controlPoints[i + 1]);

            // Check if this segment intersects with the plane
            if (TryGetPlaneLineIntersection(planeNormal, planePoint, startPoint, endPoint, out Vector3 intersectionPoint))
            {
                // Calculate distance from player to intersection point
                float distance = Vector3.Distance(transform.position, intersectionPoint);

                // Choose the closest intersection point
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIntersectionPoint = intersectionPoint;

                    // Calculate direction at this point
                    bestDirection = (endPoint - startPoint).normalized;
                    // Again, not really needed since the player is always facing forward
                    if (!splineTraveler.MovingForward)
                        bestDirection = -bestDirection;
                }
            }
        }

        // If we found a valid intersection point
        if (bestDistance < float.MaxValue)
        {
            targetPosition = bestIntersectionPoint;
            targetRotation = Quaternion.LookRotation(bestDirection);
            return true;
        }

        // Fallback: use the closest control point
        Vector3 closestPoint = targetLane.transform.TransformPoint(targetLane.controlPoints[0]);
        float closestDistance = Vector3.Distance(transform.position, closestPoint);

        for (int i = 1; i < targetLane.controlPoints.Count; i++)
        {
            Vector3 point = targetLane.transform.TransformPoint(targetLane.controlPoints[i]);
            float distance = Vector3.Distance(transform.position, point);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = point;
            }
        }

        targetPosition = closestPoint;

        // Calculate direction based on the closest control point segment
        int closestIndex = 0;
        for (int i = 0; i < targetLane.controlPoints.Count - 1; i++)
        {
            Vector3 startPoint = targetLane.transform.TransformPoint(targetLane.controlPoints[i]);
            Vector3 endPoint = targetLane.transform.TransformPoint(targetLane.controlPoints[i + 1]);
            
            if (Vector3.Distance(closestPoint, startPoint) < Vector3.Distance(closestPoint, endPoint))
            {
                closestIndex = i;
                break;
            }
        }

        if (closestIndex < targetLane.controlPoints.Count - 1)
        {
            Vector3 startPoint = targetLane.transform.TransformPoint(targetLane.controlPoints[closestIndex]);
            Vector3 endPoint = targetLane.transform.TransformPoint(targetLane.controlPoints[closestIndex + 1]);
            bestDirection = (endPoint - startPoint).normalized;
            if (!splineTraveler.MovingForward)
                bestDirection = -bestDirection;
        }

        targetRotation = Quaternion.LookRotation(bestDirection);
        return true;
    }

    private bool TryGetPlaneLineIntersection(Vector3 planeNormal, Vector3 planePoint, Vector3 lineStart, Vector3 lineEnd, out Vector3 intersectionPoint)
    {
        intersectionPoint = Vector3.zero;

        Vector3 lineDirection = (lineEnd - lineStart).normalized;
        Vector3 lineVector = lineEnd - lineStart;

        float dotProduct = Vector3.Dot(lineDirection, planeNormal);

        // If the line is parallel to the plane, no intersection
        if (Mathf.Abs(dotProduct) < 0.0001f) return false;

        Vector3 planeToLineStart = lineStart - planePoint;
        float t = -Vector3.Dot(planeToLineStart, planeNormal) / dotProduct;

        // Check if the intersection point is within the line segment
        if (t < 0f || t > lineVector.magnitude)
            return false;

        // Calculate the intersection point
        intersectionPoint = lineStart + lineDirection * t;
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

        transform.position = Vector3.Lerp(switchStartPosition, switchTargetPosition, progress);
        transform.rotation = Quaternion.Slerp(switchStartRotation, switchTargetRotation, progress);
    }

    private void CompleteLaneSwitch()
    {
        if (targetLane == null) return;
        
        // Find the closest distance on the target lane to our final position
        float targetDistance = FindClosestDistanceOnSpline(targetLane, switchTargetPosition);
        splineTraveler.SetSplinePath(targetLane, targetDistance);

        isSwitchingLanes = false;
        targetLane = null;

        OnLaneSwitchCompleted?.Invoke(this, splineTraveler.SplinePath);
    }

    private float FindClosestDistanceOnSpline(SplinePath spline, Vector3 position)
    {
        if (spline.splinePoints.Count == 0) return 0f;

        float closestDistance = 0f;
        float closestDistSquared = float.MaxValue;

        for (int i = 0; i < spline.splinePoints.Count; i++)
        {
            float distSquared = (spline.splinePoints[i] - position).sqrMagnitude;
            if (distSquared < closestDistSquared)
            {
                closestDistSquared = distSquared;
                closestDistance = spline.splineDistances[i];
            }
        }

        return closestDistance;
    }

    public bool IsSwitchingLanes => isSwitchingLanes;

    // difficulty tuning fro switching
    public float SwitchDuration
    {
        get => switchDuration;
        set => switchDuration = value;
    }
}
