using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(RoadSpline))]
public class RoadMeshGenerator : MonoBehaviour
{
    public float width = 2f;
    public int subdivisions = 100;

    void Start()
    {
        GenerateMesh();
    }

    public void GenerateMesh()
    {
        RoadSpline spline = GetComponent<RoadSpline>();
        if (spline.controlPoints.Count < 4) return;

        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        float totalLength = 0f;
        Vector3 prevPos = Vector3.zero;

        for (int seg = 0; seg <= spline.controlPoints.Count - 4; seg += 3)
        {
            for (int i = 0; i <= subdivisions; i++)
            {
                float t = i / (float)subdivisions;
                Vector3 pos = RoadSpline.EvaluateBezier(
                    transform.TransformPoint(spline.controlPoints[seg]),
                    transform.TransformPoint(spline.controlPoints[seg + 1]),
                    transform.TransformPoint(spline.controlPoints[seg + 2]),
                    transform.TransformPoint(spline.controlPoints[seg + 3]),
                    t
                );

                Vector3 forward = (i < subdivisions) ?
                    RoadSpline.EvaluateBezier(
                        transform.TransformPoint(spline.controlPoints[seg]),
                        transform.TransformPoint(spline.controlPoints[seg + 1]),
                        transform.TransformPoint(spline.controlPoints[seg + 2]),
                        transform.TransformPoint(spline.controlPoints[seg + 3]),
                        Mathf.Min(t + 0.01f, 1f)
                    ) - pos : pos - prevPos;

                Vector3 right = Vector3.Cross(Vector3.up, forward.normalized);

                verts.Add(pos + right * (width * 0.5f));
                verts.Add(pos - right * (width * 0.5f));

                uvs.Add(new Vector2(0, totalLength));
                uvs.Add(new Vector2(1, totalLength));

                if (i > 0)
                {
                    int baseIndex = verts.Count - 4;
                    tris.Add(baseIndex + 0);
                    tris.Add(baseIndex + 2);
                    tris.Add(baseIndex + 1);

                    tris.Add(baseIndex + 2);
                    tris.Add(baseIndex + 3);
                    tris.Add(baseIndex + 1);
                }

                totalLength += (pos - prevPos).magnitude;
                prevPos = pos;
            }
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
