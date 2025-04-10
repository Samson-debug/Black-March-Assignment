using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObstacleManager : MonoBehaviour
{
    public ObstacleData obstacleData;
    public GameObject obstaclePrefab;
    public GridManager gridManager;

    public string obstacleContainerName = "ObstacleVisualsContainer";
    
    private List<GameObject> runtimeObstacleObjects = new List<GameObject>();
    private Transform obstacleContainer;

    void Start()
    {
        if (gridManager == null || obstacleData == null || obstaclePrefab == null)
            Debug.LogError("Obstacle Manager reference has not been set!");
    }
    
    
    public void GenerateObstacles()
    {
        if (obstacleData == null || gridManager == null || !gridManager.IsGridGenerated())
        {
             Debug.LogWarning("Cannot generate obstacles cuz ObstacleData or GridManager is missing or grid not generated.");
             return;
        }

        FindOrCreateContainer(); //check container exists

        //clear present obstacles
        ClearObstacleVisuals();

        if (obstaclePrefab == null) return;

        int gridWidth = gridManager.gridCountX;
        int gridHeight = gridManager.gridCountY;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Tile tile = gridManager.GetTile(x, y);
                if (tile == null) continue; // Skip if no tile found

                bool isBlocked = obstacleData.IsTileBlocked(x, y);
                
                tile.isWalkable = !isBlocked;

                if (isBlocked)
                {
                    Vector3 tilePos = tile.transform.position;
                    //adjust Y position
                    Vector3 obstaclePosition = new Vector3(tilePos.x, tilePos.y + 1f, tilePos.z);
                    
                    GameObject obstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity, obstacleContainer);
                    
                     #if UNITY_EDITOR
                    //store creation for Undo
                        Undo.RegisterCreatedObjectUndo(obstacle, "Create Obstacle Visual");
                     #endif
                }
            }
        }
    }

    void FindOrCreateContainer()
    {
        if (obstacleContainer != null) return;
            
        obstacleContainer = transform.Find(obstacleContainerName);

        if (obstacleContainer == null)
        {
            //create container if needed
            GameObject containerGO = new GameObject(obstacleContainerName);
            obstacleContainer = containerGO.transform;
            obstacleContainer.SetParent(transform); 
        }
    }

    void ClearObstacleVisuals()
    {
        FindOrCreateContainer();
        
        
        for (int i = obstacleContainer.childCount - 1; i >= 0; i--)
        {
            GameObject child = obstacleContainer.GetChild(i).gameObject; 
            DestroyImmediate(child);
        }
    }
}