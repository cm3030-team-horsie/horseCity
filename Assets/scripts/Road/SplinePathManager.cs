using System.Collections.Generic;
using UnityEngine;

public class SplinePathManager : MonoBehaviour
{
    [Header("Path Management")]
    public List<SplinePath> allPaths = new List<SplinePath>();
    public SplinePath currentPath;
    public int currentLaneIndex = 0;

    [Header("Lane Switching")]
    public float laneSwitchDuration = 0.5f;
    public AnimationCurve laneSwitchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Movement")]
    public float forwardSpeed = 15f;
    public float currentDistance = 0f;

    private bool isSwitchingLanes = false;
    private SplinePath targetPath;
    private float laneSwitchStartTime;
    private float laneSwitchStartDistance;
    private Vector3 laneSwitchStartPosition;
    private Vector3 laneSwitchTargetPosition;

    private void Start()
    {
        if (allPaths.Count > 0 && currentPath == null)
        {
            currentPath = allPaths[0];
        }
    }

    public void SetCurrentPath(SplinePath path)
    {
        currentPath = path;
        currentLaneIndex = path.laneIndex;
    }

    public bool CanSwitchToLeftLane()
    {
        return currentPath != null && currentPath.GetLeftLane() != null && !isSwitchingLanes;
    }

    public bool CanSwitchToRightLane()
    {
        return currentPath != null && currentPath.GetRightLane() != null && !isSwitchingLanes;
    }

    public void TrySwitchToLeftLane()
    {
        if (!CanSwitchToLeftLane()) return;
        
        StartLaneSwitch(currentPath.GetLeftLane());
    }

    public void TrySwitchToRightLane()
    {
        if (!CanSwitchToRightLane()) return;
        
        StartLaneSwitch(currentPath.GetRightLane());
    }

    private void StartLaneSwitch(SplinePath newPath)
    {
        if (newPath == null) return;

        isSwitchingLanes = true;
        targetPath = newPath;
        laneSwitchStartTime = Time.time;
        laneSwitchStartDistance = currentDistance;
        laneSwitchStartPosition = currentPath.GetPositionAtDistance(currentDistance);

        // Calculate the distance the horse will travel during the lane switch
        float distanceTraveledDuringSwitch = forwardSpeed * laneSwitchDuration;
        float targetDistance = currentDistance + distanceTraveledDuringSwitch;

        // Clamp to the target path's total length
        targetDistance = Mathf.Min(targetDistance, newPath.GetTotalLength());

        laneSwitchTargetPosition = newPath.GetPositionAtDistance(targetDistance);
    }
    
    public Vector3 GetCurrentPosition()
    {
        if (currentPath == null) return transform.position;
        if (isSwitchingLanes)
        {
            float switchProgress = (Time.time - laneSwitchStartTime) / laneSwitchDuration;
            switchProgress = Mathf.Clamp01(switchProgress);
            float curveValue = laneSwitchCurve.Evaluate(switchProgress);

            Vector3 interpolatedPosition = Vector3.Lerp(laneSwitchStartPosition, laneSwitchTargetPosition, curveValue);

            if (switchProgress >= 1f)
            {
                // Lane switch complete
                currentPath = targetPath;
                currentLaneIndex = currentPath.laneIndex;
                isSwitchingLanes = false;
                targetPath = null;
            }
            return interpolatedPosition;
        }
        return currentPath.GetPositionAtDistance(currentDistance);
    }

    public Vector3 GetCurrentDirection()
    {
        if (currentPath == null) return transform.forward;
        if (isSwitchingLanes && targetPath != null)
        {
            // During lane switch, interpolate between directions
            float switchProgress = (Time.time - laneSwitchStartTime) / laneSwitchDuration;
            switchProgress = Mathf.Clamp01(switchProgress);
            float curveValue = laneSwitchCurve.Evaluate(switchProgress);

            // Calculate the distance traveled during the switch
            float distanceTraveledDuringSwitch = forwardSpeed * laneSwitchDuration;
            float targetDistance = laneSwitchStartDistance + distanceTraveledDuringSwitch;
            targetDistance = Mathf.Min(targetDistance, targetPath.GetTotalLength());

            Vector3 startDirection = currentPath.GetDirectionAtDistance(laneSwitchStartDistance);
            Vector3 targetDirection = targetPath.GetDirectionAtDistance(targetDistance);

            return Vector3.Slerp(startDirection, targetDirection, curveValue).normalized;
        }

        return currentPath.GetDirectionAtDistance(currentDistance);
    }

    public void MoveForward(float deltaTime)
    {
        if (currentPath == null) return;

        float distanceToMove = forwardSpeed * deltaTime;
        float oldDistance = currentDistance;
        currentDistance += distanceToMove;
        
        // Check if we've reached the end of the current path
        if (currentDistance >= currentPath.GetTotalLength())
        {
            // Move to next path if available
            SplinePath nextPath = currentPath.GetNextPath();
            if (nextPath != null)
            {
                currentDistance = currentDistance - currentPath.GetTotalLength();
                currentPath = nextPath;
                currentLaneIndex = currentPath.laneIndex;
            }
            else
            {
                // End of the road - could trigger level complete?
                currentDistance = currentPath.GetTotalLength();
            }
        }
    }

    public float GetCurrentDistance()
    {
        return currentDistance;
    }

    public SplinePath GetCurrentPath()
    {
        return currentPath;
    }

    public bool IsSwitchingLanes()
    {
        return isSwitchingLanes;
    }

    public SplinePath GetPathByLaneIndex(int laneIndex)
    {
        foreach (SplinePath path in allPaths)
        {
            if (path.laneIndex == laneIndex)
            {
                return path;
            }
        }
        return null;
    }

    public List<SplinePath> GetPathsAtDistance(float distance)
    {
        List<SplinePath> pathsAtDistance = new List<SplinePath>();
        
        foreach (SplinePath path in allPaths)
        {
            if (distance <= path.GetTotalLength())
            {
                pathsAtDistance.Add(path);
            }
        }
        
        return pathsAtDistance;
    }
}
