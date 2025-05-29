using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RoadSpline : MonoBehaviour
{
    public List<Vector3> controlPoints = new List<Vector3>();

    [Range(4, 100)]
    public int segmentsPerCurve = 20;

    private void OnDrawGizmos()
    {
        if (controlPoints.Count < 4) return;

        Gizmos.color = Color.blue;

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

    public static Vector3 EvaluateBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        return uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
    }
}
