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
            if (currentDistance >= splinePath.GetTotalLength())
            {
                OnReachedPathEnd?.Invoke(this);
                StopMoving();
            }
        }
        else
        {
            currentDistance -= distanceToMove;
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

        LaneSwitcher laneSwitcher = GetComponent<LaneSwitcher>();
        if (laneSwitcher != null && laneSwitcher.IsSwitchingLanes)
            return;

        targetPosition = splinePath.GetPositionAtDistance(currentDistance);
        Vector3 splineDirection = splinePath.GetDirectionAtDistance(currentDistance);
        if (!movingForward) splineDirection = -splineDirection;

        targetRotation = Quaternion.LookRotation(splineDirection);

        transform.position = Vector3.Lerp(transform.position, targetPosition, travelSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, travelSpeed * Time.deltaTime);
    }

    public void FindAndSnapToNearestPath()
    {
        SplinePath[] allPaths = FindObjectsOfType<SplinePath>();
        if (allPaths.Length == 0)
        {
            return;
        }

        foreach (SplinePath path in allPaths)
        {
            if (path.GetTotalLength() <= 0f)
                path.GenerateSplinePoints();
        }

        float closestDistance = float.MaxValue;
        SplinePath closestPath = null;
        float closestSplineDistance = 0f;

        foreach (SplinePath path in allPaths)
        {
            float distanceAlongSpline = FindClosestDistanceOnSpline(path, transform.position);
            Vector3 closestPointOnSpline = path.GetPositionAtDistance(distanceAlongSpline);
            float distanceToSpline = Vector3.Distance(transform.position, closestPointOnSpline);

            if (distanceToSpline < closestDistance)
            {
                closestDistance = distanceToSpline;
                closestPath = path;
                closestSplineDistance = distanceAlongSpline;
            }
        }

        if (closestPath != null)
            SetSplinePath(closestPath, closestSplineDistance);
    }

    public void ForceSnapToNearestPath()
    {
        SplinePath currentPath = splinePath;
        splinePath = null;
        FindAndSnapToNearestPath();
        if (splinePath == null) splinePath = currentPath;
    }

    private float FindClosestDistanceOnSpline(SplinePath path, Vector3 worldPosition)
    {
        float closestDistance = 0f;
        float closestDistanceToPoint = float.MaxValue;

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
            currentDistance = Mathf.Clamp(distance, 0f, splinePath.GetTotalLength());
        else
            currentDistance = distance;
    }

    public void SetIsTravelingForward(bool forward) => movingForward = forward;

    public void StartMoving()
    {
        if (splinePath == null)
        {
            return;
        }

        isMoving = true;
        OnStartedMoving?.Invoke(this);
    }

    public void StopMoving()
    {
        isMoving = false;
        OnStoppedMoving?.Invoke(this);
    }

    public Vector3 GetCurrentPosition() =>
        splinePath == null ? transform.position : splinePath.GetPositionAtDistance(currentDistance);

    public Vector3 GetCurrentDirection()
    {
        if (splinePath == null) return transform.forward;
        Vector3 direction = splinePath.GetDirectionAtDistance(currentDistance);
        return movingForward ? direction : -direction;
    }
}
