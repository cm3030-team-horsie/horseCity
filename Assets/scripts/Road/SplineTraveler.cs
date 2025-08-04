using UnityEngine;

public class SplineTraveler : MonoBehaviour
{
    [Header("Travel Settings")]
    [SerializeField] private SplinePath splinePath;
    [SerializeField] private float travelSpeed = 10f;
    [SerializeField] private bool movingForward = true;

    [Header("Position")]
    [SerializeField] private float currentDistance = 0f;
    [SerializeField] private bool snapToNearestPath = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private bool isMoving = false;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    public System.Action<SplineTraveler> OnReachedPathEnd;
    public System.Action<SplineTraveler> OnReachedPathStart;
    public System.Action<SplineTraveler> OnStartedMoving;
    public System.Action<SplineTraveler> OnStoppedMoving;

    #region Properties

    public SplinePath SplinePath 
    { 
        get => splinePath; 
        set => SetSplinePath(value); 
    }

    public float TravelSpeed 
    { 
        get => travelSpeed; 
        set => travelSpeed = value; 
    }

    public bool MovingForward 
    { 
        get => movingForward; 
        set => SetIsTravelingForward(value); 
    }

    public float CurrentDistance 
    { 
        get => currentDistance; 
        set => SetDistance(value); 
    }

    public bool IsMoving => isMoving;

    #endregion

    private void Awake()
    {
        if (snapToNearestPath)
        {
            FindAndSnapToNearestPath();
        }
    }

    private void Update()
    {
        if (!isMoving || splinePath == null) return;

        MoveAlongSpline(Time.deltaTime);
        UpdatePositionAndRotation();
    }

    private void MoveAlongSpline(float deltaTime)
    {
        float distanceToMove = travelSpeed * deltaTime;

        if (movingForward)
        {
            currentDistance += distanceToMove;

            // Check if we've reached the end of the current path
            if (currentDistance >= splinePath.GetTotalLength())
            {
                OnReachedPathEnd?.Invoke(this);
                StopMoving();
            }
        }
        else
        {
            currentDistance -= distanceToMove;

            // Check if we've reached the beginning of the current path
            if (currentDistance <= 0f)
            {
                OnReachedPathStart?.Invoke(this);
                StopMoving();
            }
        }

        currentDistance = Mathf.Clamp(currentDistance, 0f, splinePath.GetTotalLength());
    }

    private void UpdatePositionAndRotation()
    {
        if (splinePath == null) return;

        // Kinda janky, but check if we're currently switching lanes
        // LaneSwitcher should handle transform updates
        LaneSwitcher laneSwitcher = GetComponent<LaneSwitcher>();
        if (laneSwitcher != null && laneSwitcher.IsSwitchingLanes)
        {
            return;
        }

        // Get target position and direction from spline
        targetPosition = splinePath.GetPositionAtDistance(currentDistance);
        Vector3 splineDirection = splinePath.GetDirectionAtDistance(currentDistance);

        // Adjust direction based on movement direction
        if (!movingForward)
        {
            splineDirection = -splineDirection;
        }

        targetRotation = Quaternion.LookRotation(splineDirection);
        transform.position = Vector3.Lerp(transform.position, targetPosition, travelSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, travelSpeed * Time.deltaTime);
    }

    public void FindAndSnapToNearestPath()
    {
        SplinePath[] allPaths = FindObjectsOfType<SplinePath>();

        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name}: Found {allPaths.Length} spline paths to check");
        }

        if (allPaths.Length == 0)
        {
            Debug.LogWarning($"No spline paths found for {gameObject.name} to snap to");
            return;
        }

        // Ensure all spline paths have generated their points
        // TODO: Maybe should not call GenerateSplinePoints here?
        foreach (SplinePath path in allPaths)
        {
            if (path.GetTotalLength() <= 0f)
            {
                path.GenerateSplinePoints();
                if (showDebugInfo)
                {
                    Debug.Log($"{gameObject.name}: Generated spline points for {path.name}");
                }
            }
        }

        float closestDistance = float.MaxValue;
        SplinePath closestPath = null;
        float closestSplineDistance = 0f;

        // Find the closest spline and distance along it
        foreach (SplinePath path in allPaths)
        {
            float distanceAlongSpline = FindClosestDistanceOnSpline(path, transform.position);
            Vector3 closestPointOnSpline = path.GetPositionAtDistance(distanceAlongSpline);
            float distanceToSpline = Vector3.Distance(transform.position, closestPointOnSpline);

            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name}: Checking path {path.name} - distance to spline: {distanceToSpline:F2}, distance along spline: {distanceAlongSpline:F2}");
            }

            if (distanceToSpline < closestDistance)
            {
                closestDistance = distanceToSpline;
                closestPath = path;
                closestSplineDistance = distanceAlongSpline;
            }
        }

        if (closestPath != null)
        {
            SetSplinePath(closestPath, closestSplineDistance);
            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name} snapped to spline {closestPath.name} at distance {closestSplineDistance:F2} (distance to spline: {closestDistance:F2})");
            }
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning($"{gameObject.name}: No closest path found!");
        }
    }

    public void ForceSnapToNearestPath()
    {
        // Temporarily clear the current path
        SplinePath currentPath = splinePath;
        splinePath = null;

        FindAndSnapToNearestPath();
        if (splinePath == null)
        {
            splinePath = currentPath;
            if (showDebugInfo)
            {
                Debug.LogWarning($"{gameObject.name}: No spline path found, keeping original path: {currentPath?.name}");
            }
        }
        else if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name}: Force snapped to spline: {splinePath.name} at distance: {currentDistance}");
        }
    }

    private float FindClosestDistanceOnSpline(SplinePath path, Vector3 worldPosition)
    {
        float closestDistance = 0f;
        float closestDistanceToPoint = float.MaxValue;

        // Check if the spline has any points
        if (path.GetTotalLength() <= 0f)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"{gameObject.name}: Spline {path.name} has no length");
            }
            return 0f;
        }

        // Sample points along the spline to find the closest
        // TODO: This could potentially be pretty slow?
        int samples = 100;
        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            float distanceAlongSpline = t * path.GetTotalLength();
            Vector3 pointOnSpline = path.GetPositionAtDistance(distanceAlongSpline);
            float distanceToPoint = Vector3.Distance(worldPosition, pointOnSpline);

            if (distanceToPoint < closestDistanceToPoint)
            {
                closestDistanceToPoint = distanceToPoint;
                closestDistance = distanceAlongSpline;
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name}: Closest point on spline {path.name} is at distance {closestDistance:F2} (actual distance to point: {closestDistanceToPoint:F2})");
        }

        return closestDistance;
    }

    public void SetSplinePath(SplinePath path, float distance = 0f)
    {
        splinePath = path;
        SetDistance(distance);
        if (path != null)
        {
            transform.position = path.GetPositionAtDistance(currentDistance);
            Vector3 direction = path.GetDirectionAtDistance(currentDistance);
            if (!movingForward) direction = -direction;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    public void SetDistance(float distance)
    {
        if (splinePath != null)
        {
            currentDistance = Mathf.Clamp(distance, 0f, splinePath.GetTotalLength());
        }
        else
        {
            currentDistance = distance;
        }
    }

    public void SetIsTravelingForward(bool forward)
    {
        movingForward = forward;
    }

    public void StartMoving()
    {
        if (splinePath == null)
        {
            Debug.LogWarning($"Cannot start moving - no spline path assigned to {gameObject.name}");
            return;
        }

        isMoving = true;
        OnStartedMoving?.Invoke(this);

        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name} started moving along spline");
        }
    }

    public void StopMoving()
    {
        isMoving = false;
        OnStoppedMoving?.Invoke(this);

        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name} stopped moving");
        }
    }

    public Vector3 GetCurrentPosition()
    {
        if (splinePath == null) return transform.position;
        return splinePath.GetPositionAtDistance(currentDistance);
    }

    public Vector3 GetCurrentDirection()
    {
        if (splinePath == null) return transform.forward;
        Vector3 direction = splinePath.GetDirectionAtDistance(currentDistance);
        return movingForward ? direction : -direction;
    }
}
