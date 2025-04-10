using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;


public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")] public Vector2 gridWorldSize;
    [SerializeField] List<Tile> grid = new List<Tile>(); //list to save the reference of the tiles
    public int gridCountX;
    public int gridCountY;

    [Header("Tile Settings")] 
    public GameObject tilePrefab;
    public float tileRadius;
    float tileDiameter;

    [Header("Visual Setting")] 
    public float visualTileLength;

    Vector3 worldBottomLeft;

    public void CreateGrid()
    {
        if (Application.isPlaying) return;
        if (tilePrefab == null){
            Debug.LogError("Tile Prefab is not assigned!");
            return;
        }

        //detroy existing Tiles
        for (int i = transform.childCount - 1; i >= 0; i--){
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        grid.Clear();

        //Calculating Rows and Colums
        tileDiameter = tileRadius * 2;
        gridCountX = Mathf.RoundToInt(gridWorldSize.x / tileDiameter);
        gridCountY = Mathf.RoundToInt(gridWorldSize.y / tileDiameter);

        CreateTiles();

        //Saving changes
        EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        Debug.Log($"Grid created with dimensions: {gridCountX}x{gridCountY}.");
    }

    private void CreateTiles()
    {
        grid.Capacity = gridCountX * gridCountY;
        worldBottomLeft = transform.position - transform.right * gridWorldSize.x / 2 -
                          transform.forward * gridWorldSize.y / 2;

        for (int y = 0; y < gridCountY; y++) // Iterate Y first for Row-Major order in list
        {
            for (int x = 0; x < gridCountX; x++){
                Vector3 tilePosition = worldBottomLeft
                                       + Vector3.right * (x * tileDiameter + tileRadius)
                                       + Vector3.forward * (y * tileDiameter + tileRadius);

                InstantiateTile(tilePosition, x, y);
            }
        }
    }

    private void InstantiateTile(Vector3 tilePosition, int x, int y)
    {
#if UNITY_EDITOR
        GameObject tileGO = Instantiate(tilePrefab, tilePosition, quaternion.identity, transform);
        Tile tile = tileGO.GetComponent<Tile>();
        tile.gridX = x;
        tile.gridY = y;

        //set visual scale
        tile.transform.localScale = new Vector3(visualTileLength, visualTileLength, visualTileLength);

        //add to the serialized list
        grid.Add(tile);

        EditorUtility.SetDirty(tileGO);
        EditorUtility.SetDirty(tile);
#endif
    }

    public Tile GetTile(int x, int y)
    {
        if (grid == null || gridCountX <= 0) return null;

        if (x >= 0 && x < gridCountX && y >= 0 && y < gridCountY){
            int index = y * gridCountX + x; //calculate linear index, Row-Major
            if (index >= 0 && index < grid.Count){
                return grid[index];
            }
            else{
                Debug.LogError($"Calculated index {index} is out of bounds");
                return null;
            }
        }

        return null; //out of bounds
    }
    
    public bool IsGridGenerated()
    {
        return grid != null && grid.Count > 0 && grid.Count == (gridCountX * gridCountY) && gridCountX > 0 && gridCountY > 0;
    }

    public List<Tile> GetNeighbourTiles(Tile tile)
    {
        List<Tile> neighbours = new List<Tile>();
        if (tile == null) return neighbours;

        for (int x = -1; x <= 1; x++){
            for (int y = -1; y <= 1; y++){
                if (x == 0 && y == 0) continue; //skip for the middle one

                int checkX = tile.gridX + x;
                int checkY = tile.gridY + y;

                //if tile position is in range of row and column
                if (checkX >= 0 && checkX < gridCountX && checkY >= 0 && checkY < gridCountY)
                    neighbours.Add(GetTile(checkX, checkY));
            }
        }

        return neighbours;
    }

    #region PathFinding

    public List<Tile> FindPath(Tile startTile, Tile endTile)
    {
        List<Tile> openTiles = new List<Tile>();
        HashSet<Tile> closed = new HashSet<Tile>();

        openTiles.Add(startTile);
        while (openTiles.Count > 0){
            Tile currentTile = openTiles[0];
            foreach (var openTile in openTiles){
                if (openTile.fCost < currentTile.fCost ||
                    openTile.fCost == currentTile.fCost && openTile.hCost < currentTile.hCost)
                    currentTile = openTile;
            }

            openTiles.Remove(currentTile);
            closed.Add(currentTile);

            if (currentTile == endTile){
                List<Tile> path = RetracePath(startTile, endTile);
                return path;
            }

            foreach (var neighbour in this.GetNeighbourTiles(currentTile)){
                if (Mathf.Abs(neighbour.gridX - currentTile.gridX) == 1 &&
                    Mathf.Abs(neighbour.gridY - currentTile.gridY) == 1)
                    continue; // does not allowes direct diagonal movement

                if (closed.Contains(neighbour) || !neighbour.isWalkable)
                    continue;

                int newMovementCostToNeighbour = currentTile.gCost + GetDistance(currentTile, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openTiles.Contains(neighbour)){
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, endTile);
                    neighbour.parent = currentTile;

                    if (!openTiles.Contains(neighbour))
                        openTiles.Add(neighbour);
                }
            }
        }

        return null;
    }

    private List<Tile> RetracePath(Tile startTile, Tile endTile)
    {
        List<Tile> path = new List<Tile>();

        Tile currentTile = endTile;
        while (currentTile != startTile){
            path.Add(currentTile);
            currentTile = currentTile.parent;
        }

        path.Reverse();

        return path;
    }

    public int GetDistance(Tile x, Tile y)
    {
        int disX = Mathf.Abs(x.gridX - y.gridX);
        int disY = Mathf.Abs(x.gridY - y.gridY);

        //letting two adjacent grid distance = 10 and distance between two diagonal grid = 14
        if (disX > disY)
            return 14 * disY + 10 * (disX - disY);

        return 14 * disX + 10 * (disY - disX);
    }

    #endregion
}