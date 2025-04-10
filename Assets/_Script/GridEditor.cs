using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManager))]
public class GridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GridManager gridManager = (GridManager)target;

        if (GUILayout.Button("Create Grid"))
        {
            gridManager.CreateGrid();
        }
    }
}
