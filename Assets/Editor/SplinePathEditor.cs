#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplinePath))]
public class SplinePathEditor : Editor
{
    void OnSceneGUI()
    {
        SplinePath spline = (SplinePath)target;

        for (int i = 0; i < spline.controlPoints.Count; i++)
        {
            Vector3 worldPos = spline.transform.TransformPoint(spline.controlPoints[i]);
            EditorGUI.BeginChangeCheck();
            Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Control Point");
                spline.controlPoints[i] = spline.transform.InverseTransformPoint(newWorldPos);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SplinePath spline = (SplinePath)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Add Point"))
        {
            Undo.RecordObject(spline, "Add Control Point");

            Vector3 newPoint = spline.controlPoints.Count > 0
                ? spline.controlPoints[spline.controlPoints.Count - 1] + Vector3.forward
                : Vector3.zero;

            spline.controlPoints.Add(newPoint);
        }

        if (GUILayout.Button("Clear Points"))
        {
            Undo.RecordObject(spline, "Clear Control Points");
            spline.controlPoints.Clear();
        }
    }
}
#endif
