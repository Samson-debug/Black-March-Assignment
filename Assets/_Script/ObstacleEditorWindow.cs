using UnityEngine;
using UnityEditor;

public class ObstacleEditorWindow : EditorWindow
{
    private ObstacleData obstacleData;
    private Vector2 scrollPosition;
    private Color previewColor = Color.red*0.3f;

    [MenuItem("Tools/Obstacle Editor")]
    public static void ShowWindow()
    {
        GetWindow<ObstacleEditorWindow>("Obstacle Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Data Asset Management
        EditorGUILayout.LabelField("Obstacle Data Configuration", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        obstacleData = (ObstacleData)EditorGUILayout.ObjectField(
            "Obstacle Data Asset", 
            obstacleData, 
            typeof(ObstacleData), 
            false
        );
        
        if (obstacleData == null)
        {
            EditorGUILayout.HelpBox("Please assign an ObstacleData asset.", MessageType.Warning);
            EditorGUILayout.EndVertical();
            return;
        }

        // Grid Editor
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Grid Editor", EditorStyles.boldLabel);
        DrawGridEditor();

        // Action Buttons
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear All"))
        {
            ClearAllObstacles();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

    }

    private void DrawGridEditor()
    {
        float buttonSize = 25f;
        GridManager grid = GridManager.instance;

        if (grid == null)
        {
            EditorGUILayout.HelpBox("Play game to make grid!", MessageType.Error);
            return;
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        for (int y = 9; y >= 0; y--)  // Reversed to match Unity's coordinate system
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < 10; x++)
            {
                Tile tile = grid.GetTile(x, y);
                bool hasEntity = tile != null && tile.hasEntity;

                // Visual feedback for the button
                Color originalColor = GUI.backgroundColor;
                if (obstacleData.GetObstacle(x, y))
                    GUI.backgroundColor = Color.red;
                else if (hasEntity)
                    GUI.backgroundColor = Color.yellow;

                EditorGUI.BeginDisabledGroup(hasEntity);

                // Custom button
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.padding = new RectOffset(2, 2, 2, 2); //Manage button size here
                
                bool currentValue = obstacleData.GetObstacle(x, y);
                bool newValue = GUILayout.Toggle(currentValue, $"{x},{y}", buttonStyle, 
                    GUILayout.Width(buttonSize), GUILayout.Height(buttonSize));

                if (!hasEntity && newValue != currentValue)
                {
                    /*Undo.RecordObject(obstacleData, "Toggle Obstacle");*/
                    obstacleData.SetObstacle(x, y, newValue);
                    EditorUtility.SetDirty(obstacleData); // save changes to Scriptable object
                    UpdateGridObstacles();
                }

                EditorGUI.EndDisabledGroup();
                GUI.backgroundColor = originalColor;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    private void ClearAllObstacles()
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                obstacleData.SetObstacle(x, y, false);
            }
        }
        EditorUtility.SetDirty(obstacleData);
        UpdateGridObstacles();
    }
    
    private void UpdateGridObstacles()
    {
        if(!Application.isPlaying) return;
        
        ObstacleManager manager = FindObjectOfType<ObstacleManager>();
        if (manager != null)
            manager.GenerateObstacles();
    }
}

/*public class ObstacleEditorWindow : EditorWindow
{
    private ObstacleData obstacleData;
    private GridManager grid;
    private const int GridSize = 10;

    [MenuItem("Tools/Obstacle Editor")]
    public static void ShowWindow()
    {
        GetWindow<ObstacleEditorWindow>("Obstacle Editor");
    }

    private void OnEnable()
    {
        grid = FindObjectOfType<GridManager>();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // Data Asset Management
        obstacleData = (ObstacleData)EditorGUILayout.ObjectField(
            "Obstacle Data Asset",
            obstacleData,
            typeof(ObstacleData),
            false
        );

        if (obstacleData == null)
        {
            EditorGUILayout.HelpBox("Please assign an ObstacleData asset.", MessageType.Warning);
            if (GUILayout.Button("Create New Obstacle Data"))
            {
                CreateNewObstacleData();
            }

            EditorGUILayout.EndVertical();
            return;
        }

        // Grid Editor
        DrawGridEditor();

        // Action Buttons
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Clear All Obstacles"))
        {
            if (EditorUtility.DisplayDialog("Clear All",
                    "Clear all obstacles?", "Yes", "No"))
            {
                ClearAllObstacles();
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawGridEditor()
    {
        if (grid == null)
        {
            EditorGUILayout.HelpBox("Grid not found in scene!", MessageType.Error);
            return;
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        float buttonSize = 25f;
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            padding = new RectOffset(2, 2, 2, 2)
        };

        for (int y = GridSize - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < GridSize; x++)
            {
                DrawGridCell(x, y, buttonSize, buttonStyle);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawGridCell(int x, int y, float size, GUIStyle style)
    {
        Tile tile = grid.GetTile(x, y);
        bool hasEntity = tile != null && tile.hasEntity;
        bool currentValue = obstacleData.GetObstacle(x, y);

        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = currentValue ? Color.red : (hasEntity ? Color.yellow : originalColor);

        EditorGUI.BeginDisabledGroup(hasEntity || !Application.isPlaying);

        bool newValue = GUILayout.Toggle(currentValue, "", style,
            GUILayout.Width(size), GUILayout.Height(size));

        if (!hasEntity && newValue != currentValue)
        {
            Undo.RecordObject(obstacleData, "Toggle Obstacle");
            obstacleData.SetObstacle(x, y, newValue);
            EditorUtility.SetDirty(obstacleData);
            UpdateGridObstacles();
        }

        EditorGUI.EndDisabledGroup();
        GUI.backgroundColor = originalColor;
    }

    private void CreateNewObstacleData()
    {
        ObstacleData newData = ScriptableObject.CreateInstance<ObstacleData>();
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Obstacle Data",
            "NewObstacleData.asset",
            "asset",
            "Save obstacle data"
        );

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newData, path);
            AssetDatabase.SaveAssets();
            obstacleData = newData;
        }
    }

    private void ClearAllObstacles()
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                obstacleData.SetObstacle(x, y, false);
            }
        }
        EditorUtility.SetDirty(obstacleData);
        UpdateGridObstacles();
    }
    
    private void UpdateGridObstacles()
    {
        if(!Application.isPlaying) return;
        
        ObstacleManager manager = FindObjectOfType<ObstacleManager>();
        if (manager != null)
            manager.GenerateObstacles();
    }
}*/