using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FloatListVisualizer))]
public class LineVisualizer : Editor
{
    void OnSceneGUI()
    {
        // Get the target object (the one that has the float list)
        FloatListVisualizer visualizer = (FloatListVisualizer)target;

        // Early exit if the list is empty
        if (visualizer.floatList == null || visualizer.floatList.Count == 0) return;

        // Define the drawing area and offset
        Handles.color = Color.green;
        Vector3 startPos = new Vector3(10, 10, 0); // Starting position of the graph
        float spacing = 20f; // Spacing between points

        for (int i = 0; i < visualizer.floatList.Count - 1; i++)
        {
            // Define positions for current and next points in the list
            Vector3 pointA = startPos + new Vector3(i * spacing, visualizer.floatList[i] * 10, 0);
            Vector3 pointB = startPos + new Vector3((i + 1) * spacing, visualizer.floatList[i + 1] * 10, 0);

            // Draw line between points
            Handles.DrawLine(pointA, pointB);
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector layout
        Repaint(); // Forces the scene to repaint when the inspector changes
    }
}
