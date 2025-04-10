#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ObstacleEditorWindow : EditorWindow
{
    private ObstacleData obstacleData;
    
    [MenuItem("Tools/Obstacle Editor")]
    public static void ShowWindow()
    {
        GetWindow<ObstacleEditorWindow>("Obstacle Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField("Obstacle Data Configuration", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        
        obstacleData = (ObstacleData)EditorGUILayout.ObjectField(
            "Obstacle Data Asset", obstacleData, typeof(ObstacleData), false);
        //if any changes,update the window
        if (EditorGUI.EndChangeCheck()) { Repaint(); }

        //if no obstacleData, print warning
        if (obstacleData == null) {
            EditorGUILayout.HelpBox("Please assign an ObstacleDataSO.", MessageType.Warning);
            EditorGUILayout.EndVertical(); 
            return;
        }

        //current state of GUI.enabled.
        bool guiWasEnabled = GUI.enabled;
        //in Play Mode,show info box and disable editing
        if (EditorApplication.isPlaying) {
            EditorGUILayout.HelpBox("Cannot edit ObstacleData while in Play Mode.", MessageType.Info);
            GUI.enabled = false;
        }

        GridManager gridManager = FindFirstObjectByType<GridManager>();
        //if no GridManager,show error
        if (gridManager == null) {
            EditorGUILayout.HelpBox("No GridManager found", MessageType.Error);
            GUI.enabled = guiWasEnabled; 
            EditorGUILayout.EndVertical(); 
            return;
        }
        //if grid is not generated, print warning
        if (!gridManager.IsGridGenerated()) {
            EditorGUILayout.HelpBox("GridManager is not generated", MessageType.Warning);
            GUI.enabled = guiWasEnabled; 
            EditorGUILayout.EndVertical(); 
            return;
        }

        int gridWidth = gridManager.gridCountX;
        int gridHeight = gridManager.gridCountY;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Grid Editor (Click to Toggle Obstacles)", EditorStyles.boldLabel);
        //grid buttons
        DrawGridEditor(gridWidth, gridHeight);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear All Obstacles")) {
            ClearAllObstacles(gridWidth, gridHeight);
            TryUpdateObstacleVisuals(); //Update visuals
        }
        EditorGUILayout.EndHorizontal();

        //restore GUI enabled state
        GUI.enabled = guiWasEnabled;
        //end the vertical layout
        EditorGUILayout.EndVertical();
    }

    private void DrawGridEditor(int gridWidth, int gridHeight)
    {
        float buttonSize = 25f;
        GridManager gridManager = FindFirstObjectByType<GridManager>();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        //looping row
        for (int y = gridHeight - 1; y >= 0; y--) {
            EditorGUILayout.BeginHorizontal();
            //looping column
            for (int x = 0; x < gridWidth; x++) {
                Tile currentTile = (gridManager != null && gridManager.IsGridGenerated()) ? gridManager.GetTile(x, y) : null;
                //if tile has entity.
                bool tileHasEntity = (currentTile != null && currentTile.hasEntity);
                bool currentValue = obstacleData.IsTileBlocked(x, y);
                
                Color originalColor = GUI.backgroundColor;
                //change button color
                if (currentValue)
                    GUI.backgroundColor = Color.red; 
                else if (tileHasEntity)
                    GUI.backgroundColor = Color.yellow; 
                else
                    GUI.backgroundColor = Color.green * 0.7f;

                //create buttons with padding
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.padding = new RectOffset(2, 2, 2, 2);

                //button for the grid cell.
                bool newValue = GUILayout.Toggle(currentValue, $"{x},{y}", buttonStyle,
                    GUILayout.Width(buttonSize), GUILayout.Height(buttonSize));

                //on toggle change,update obstacle data.
                if (newValue != currentValue) {
                    obstacleData.SetTileBlocked(x, y, newValue);
                    //update button visual
                    Repaint();
                    // Update scene visual
                    TryUpdateObstacleVisuals();
                }
                //reset color
                GUI.backgroundColor = originalColor;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    private void ClearAllObstacles(int gridWidth, int gridHeight)
    {
        if (obstacleData == null) return;
        //store data for undo
        Undo.RecordObject(obstacleData, "Clear All Obstacles");
        bool changed = false;
        
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                if (obstacleData.IsTileBlocked(x, y)) {
                    obstacleData.SetTileBlocked(x, y, false);
                    changed = true;
                }
            }
        }
        
        //if SO changed then update visual
        if (changed) Repaint();
    }

    private void TryUpdateObstacleVisuals()
    {
        if (Application.isPlaying) return; 

        ObstacleManager manager = FindFirstObjectByType<ObstacleManager>();
        if (manager != null)
        {
            //generate obstacle in scene
            manager.GenerateObstacles();

            //save scene changes
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        }
        else
        {
            //if no manager found, print warning
            Debug.LogWarning("no ObstacleManager by ObstacleEditorWindow");
        }
    }
}
#endif
