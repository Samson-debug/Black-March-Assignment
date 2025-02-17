using System;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public ObstacleData obstacleData;
    public GameObject obstaclePrefab;
    
    private GridManager gridManager;
    private GameObject[,] obstacleObjects;
    
    void Start()
    {
        gridManager = GetComponent<GridManager>();
        obstacleObjects = new GameObject[10, 10];
        GenerateObstacles();
    }

    public void GenerateObstacles()
    {
        // Clear existing obstacles
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                if (obstacleObjects[x, y] != null)
                    Destroy(obstacleObjects[x, y]);
            }
        }
        
        // Generating new obstacles
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                if (!obstacleData.GetObstacle(x, y))
                {
                    gridManager.GetTile(x, y).isWalkable = true;
                    continue;
                }

                Tile tile = gridManager.GetTile(x, y);
                Vector3 tilePos = tile.transform.position;
                Vector3 obstaclePosition = new Vector3(tilePos.x, tilePos.y + 1, tilePos.z);
                GameObject obstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity, transform);
                obstacleObjects[x,y] = obstacle;
                tile.isWalkable = false;
            }
        }
    }
}