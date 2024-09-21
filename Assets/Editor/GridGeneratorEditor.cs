using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridGenerator))]
public class GridGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draw the default inspector

        GridGenerator gridGenerator = (GridGenerator)target;

        if (GUILayout.Button("Generate Grid"))
        {
            gridGenerator.GenerateGrid(); // Trigger grid generation when the button is pressed
        }

        if (GUILayout.Button("Clear Grid"))
        {
            gridGenerator.ClearGrid(); // Trigger grid clearing when the button is pressed
        }
    }
}
