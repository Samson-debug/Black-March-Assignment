using System.Collections.Generic; 
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "ObstacleData", menuName = "Grid System/Obstacle Data")]
public class ObstacleData : ScriptableObject
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;

    [SerializeField] List<bool> obstacles = new List<bool>();

    // Also useful to call in OnEnable to ensure data is valid after loading
    private void OnEnable()
    {
        //if dimensions changed
        EnsureListSize();
    }

     private void EnsureListSize()
    {
        int requiredSize = gridWidth * gridHeight;
        if (obstacles == null)
             obstacles = new List<bool>(requiredSize);

        if (obstacles.Count != requiredSize)
        {
            Debug.LogError("Size Mismatch");
             //Resize
             obstacles.Clear();
             for (int i = 0; i < requiredSize; i++)
             {
                 obstacles.Add(false);
             }
#if UNITY_EDITOR
              EditorUtility.SetDirty(this);
#endif
        }
    }


    //if obstacle is on the Cords
    public bool IsTileBlocked(int x, int y)
    {
        //bounds check
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
        {
            Debug.LogError($"Coordinates ({x},{y}) are out of bounds");
            return true;
        }

         if(obstacles == null || obstacles.Count != gridWidth*gridHeight) {
             Debug.LogError("ObstacleData list not initialized correctly!");
             EnsureListSize();
             return obstacles[y * gridWidth + x];
         }


        // Calculate index
        int index = y * gridWidth + x;
        return obstacles[index];
    }

    // Toggles obstacle in Cords
    public void SetTileBlocked(int x, int y, bool isBlocked)
    {
        //bounds check
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
        {
            Debug.LogError($"Coordinates ({x},{y}) are out of bounds");
            return;
        }

         if(obstacles == null || obstacles.Count != gridWidth*gridHeight) {
             Debug.LogError("Cannot set obstacle cuz ObstacleData list not initialized correctly.");
             EnsureListSize();
             if (obstacles.Count != gridWidth*gridHeight) return;
         }


        // Calculate index
        int index = y * gridWidth + x;

        // if the value changed, update
        if (obstacles[index] != isBlocked)
        {
            obstacles[index] = isBlocked;

#if UNITY_EDITOR
            // save SO
            EditorUtility.SetDirty(this);
#endif
        }
    }
}