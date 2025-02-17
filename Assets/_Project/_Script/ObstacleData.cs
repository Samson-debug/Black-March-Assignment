using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleData", menuName = "Grid System/Obstacle Data")]
public class ObstacleData : ScriptableObject
{
    public bool[,] obstacles = new bool[10,10];
    
    public bool GetObstacle(int x, int y) => obstacles[x,y];

    public void SetObstacle(int x, int y, bool value) => obstacles[x,y] = value;
}