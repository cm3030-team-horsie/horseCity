using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoadMeshGenerator))]
public class RoadMeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoadMeshGenerator generator = (RoadMeshGenerator)target;

        if (GUILayout.Button("Generate Road Mesh"))
        {
            generator.GenerateMesh();
        }
    }
}
