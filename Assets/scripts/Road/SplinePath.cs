using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SplinePath : MonoBehaviour
{
    [Header("Spline Settings")]
    public List<Vector3> controlPoints = new List<Vector3>();
    [Range(4, 100)]
    public int segmentsPerCurve = 20;

    [Header("Lane Settings")]
    public int laneIndex = 0;
    public float laneWidth = 3.5f;
    public SplinePath leftLane;
    public SplinePath rightLane;

    [Header("Path Connections")]
    public SplinePath nextPath;
    public SplinePath previousPath;

    private List<Vector3> splinePoints = new List<Vector3>();
    private List<float> splineDistances = new List<float>();
    private float totalLength;

    private void Awake()
    {
        GenerateSplinePoints();
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        GenerateSplinePoints();
    }

    public void GenerateSplinePoints()
    {
        splinePoints.Clear();
        splineDistances.Clear();

        if (controlPoints.Count < 4) 
        {
            Debug.LogWarning("GenerateSplinePoints: Not enough control points (need 4+)");
            return;
        }

        totalLength = 0f;
        
        // Convert first control point to world coordinates
        Vector3 firstWorldPoint = transform.TransformPoint(controlPoints[0]);
        splinePoints.Add(firstWorldPoint);
        splineDistances.Add(0f);

        for (int i = 0; i <= controlPoints.Count - 4; i += 3)
        {
            // Convert control points to world coordinates
            Vector3 p0 = transform.TransformPoint(controlPoints[i]);
            Vector3 p1 = transform.TransformPoint(controlPoints[i + 1]);
            Vector3 p2 = transform.TransformPoint(controlPoints[i + 2]);
            Vector3 p3 = transform.TransformPoint(controlPoints[i + 3]);

            Vector3 prevPoint = p0;

            for (int j = 1; j <= segmentsPerCurve; j++)
            {
                float t = j / (float)segmentsPerCurve;
                Vector3 point = EvaluateBezier(p0, p1, p2, p3, t);

                totalLength += Vector3.Distance(prevPoint, point);
                splinePoints.Add(point);
                splineDistances.Add(totalLength);

                prevPoint = point;
            }
        }
    }

    public static Vector3 EvaluateBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        return uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
    }

    public Vector3 GetPositionAtDistance(float distance)
    {
        if (splinePoints.Count == 0) return transform.position;

        distance = Mathf.Clamp(distance, 0f, totalLength);

        // Find the segment containing this distance
        for (int i = 0; i < splineDistances.Count - 1; i++)
        {
            if (distance <= splineDistances[i + 1])
            {
                float segmentStart = splineDistances[i];
                float segmentEnd = splineDistances[i + 1];
                float segmentT = (distance - segmentStart) / (segmentEnd - segmentStart);

                Vector3 startPoint = splinePoints[i];
                Vector3 endPoint = splinePoints[i + 1];

                // Return world coordinates directly since splinePoints are now in world space
                return Vector3.Lerp(startPoint, endPoint, segmentT);
            }
        }

        return splinePoints[splinePoints.Count - 1];
    }
    
    public Vector3 GetDirectionAtDistance(float distance)
    {
        if (splinePoints.Count < 2) return transform.forward;

        distance = Mathf.Clamp(distance, 0f, totalLength);

        // Find the segment containing this distance
        for (int i = 0; i < splineDistances.Count - 1; i++)
        {
            if (distance <= splineDistances[i + 1])
            {
                Vector3 startPoint = splinePoints[i];
                Vector3 endPoint = splinePoints[i + 1];

                return (endPoint - startPoint).normalized;
            }
        }

        // Return direction from last two points
        if (splinePoints.Count >= 2)
        {
            return (splinePoints[splinePoints.Count - 1] - splinePoints[splinePoints.Count - 2]).normalized;
        }

        return transform.forward;
    }

    public float GetTotalLength()
    {
        return totalLength;
    }

    public SplinePath GetNextPath()
    {
        return nextPath;
    }

    public SplinePath GetLeftLane()
    {
        return leftLane;
    }

    public SplinePath GetRightLane()
    {
        return rightLane;
    }

    private void OnDrawGizmos()
    {
        if (controlPoints.Count < 2) return;

        Gizmos.color = Color.blue;

        // Draw straight lines between control points for simple visualization
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Vector3 start = transform.TransformPoint(controlPoints[i]);
            Vector3 end = transform.TransformPoint(controlPoints[i + 1]);
            Gizmos.DrawLine(start, end);
        }

        // Draw Bezier curves if we have enough points
        if (controlPoints.Count >= 4)
        {
            Gizmos.color = Color.cyan;

            for (int i = 0; i <= controlPoints.Count - 4; i += 3)
            {
                Vector3 p0 = transform.TransformPoint(controlPoints[i]);
                Vector3 p1 = transform.TransformPoint(controlPoints[i + 1]);
                Vector3 p2 = transform.TransformPoint(controlPoints[i + 2]);
                Vector3 p3 = transform.TransformPoint(controlPoints[i + 3]);

                Vector3 prevPoint = p0;

                for (int j = 1; j <= segmentsPerCurve; j++)
                {
                    float t = j / (float)segmentsPerCurve;
                    Vector3 point = EvaluateBezier(p0, p1, p2, p3, t);
                    Gizmos.DrawLine(prevPoint, point);
                    prevPoint = point;
                }
            }
        }

        // Draw lane connections
        if (leftLane != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, leftLane.transform.position);
        }

        if (rightLane != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, rightLane.transform.position);
        }

        // Draw path connections
        if (nextPath != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, nextPath.transform.position);
        }
    }

    // In-game rendering for debugging
    private void OnRenderObject()
    {
        if (!Application.isPlaying || splinePoints.Count < 2) return;

        // Get the current distance from any SplinePathManager using this spline
        float currentDistance = GetCurrentDistanceFromManager();

        // Draw the spline with traveled/untraveled portions
        DrawSplineWithProgress(currentDistance);
    }

    private float GetCurrentDistanceFromManager()
    {
        // Find any SplinePathManager and get the current distance
        // This will show progress on all splines based on the horse's overall progress
        var pathManagers = FindObjectsOfType<SplinePathManager>();
        foreach (var manager in pathManagers)
        {
            // Return the current distance regardless of which spline is active
            // All splines show the same progress
            return manager.GetCurrentDistance();
        }
        return 0f;
    }

    private void DrawSplineWithProgress(float currentDistance)
    {
        if (splinePoints.Count < 2) return;

        // Create line material if it doesn't exist
        if (lineMaterial == null)
        {
            CreateLineMaterial();
        }

        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);

        // Draw traveled portion (red)
        GL.Color(Color.red);
        DrawSplinePortion(0f, currentDistance);

        // Draw untraveled portion (blue)
        GL.Color(Color.blue);
        DrawSplinePortion(currentDistance, totalLength);

        GL.End();
    }
    
    private void DrawSplinePortion(float startDistance, float endDistance)
    {
        if (startDistance >= endDistance) return;

        // Find the spline segments that fall within the distance range
        for (int i = 0; i < splineDistances.Count - 1; i++)
        {
            float segmentStart = splineDistances[i];
            float segmentEnd = splineDistances[i + 1];

            if (segmentEnd < startDistance || segmentStart > endDistance) continue;

            float clampedStart = Mathf.Max(segmentStart, startDistance);
            float clampedEnd = Mathf.Min(segmentEnd, endDistance);

            float startT = (clampedStart - segmentStart) / (segmentEnd - segmentStart);
            float endT = (clampedEnd - segmentStart) / (segmentEnd - segmentStart);

            Vector3 startPoint = splinePoints[i];
            Vector3 endPoint = splinePoints[i + 1];

            Vector3 clampedStartPoint = Vector3.Lerp(startPoint, endPoint, startT);
            Vector3 clampedEndPoint = Vector3.Lerp(startPoint, endPoint, endT);

            GL.Vertex(clampedStartPoint);
            GL.Vertex(clampedEndPoint);
        }
    }

    private Material lineMaterial;

    private void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }
}
