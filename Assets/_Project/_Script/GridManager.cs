using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class GridManager : MonoBehaviour
{
    public static GridManager instance{get; private set;}
    
    [Header("Grid Settings")]
    public Vector2 gridWorldSize;
    Tile[,] grid;
    int gridCountX;
    int gridCountY;
    
    [Header("Tile Settings")]
    public GameObject tilePrefab;
    public float tileRadius;
    float tileDiameter;
    
    [Header("Visual Setting")]
    public float visualTileLength;
    
    Vector3 worldBottomLeft;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        
        //Calculating Rows and Colums
        tileDiameter = tileRadius * 2;
        gridCountX = Mathf.RoundToInt(gridWorldSize.x / tileDiameter);
        gridCountY = Mathf.RoundToInt(gridWorldSize.y / tileDiameter);
        
        CreateGrid();
    }

    private void CreateGrid()
    { 
        grid = new Tile[gridCountX, gridCountY];
        worldBottomLeft = transform.position - transform.right * gridWorldSize.x / 2 - transform.forward * gridWorldSize.y / 2;
        
        for (int x = 0; x < gridCountX; x++)
        {
            for (int y = 0; y < gridCountY; y++)
            {
                Vector3 tilePostion = worldBottomLeft 
                                      + Vector3.right * (x * tileDiameter + tileRadius) 
                                      + Vector3.forward * (y * tileDiameter + tileRadius);
                
                CreateTile(tilePostion, x, y);
            }
        }
    }
    
    private void CreateTile(Vector3 tilePosition, int x, int y)
    {
        Tile tile = Instantiate(tilePrefab, tilePosition, quaternion.identity, transform).GetComponent<Tile>();
        tile.gridX = x;
        tile.gridY = y;
        tile.isWalkable = true;
        tile.hasEntity = false;
        
        grid[x, y] = tile;
        
        //for better visualization
        tile.transform.localScale = new Vector3(visualTileLength, visualTileLength, visualTileLength);
    }
    
    public Tile GetTile(int x, int y) => grid[x, y];

    public List<Tile> GetNeighbourTiles(Tile tile)
    {
        List<Tile> neighbours = new List<Tile>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if(x == 0 && y == 0) continue; //skip for the middle one
                
                int checkX = tile.gridX + x;
                int checkY = tile.gridY + y;

                //if tile position is in range of row and column
                if (checkX >= 0 && checkX < gridCountX && checkY >= 0 && checkY < gridCountY)
                    neighbours.Add(grid[checkX, checkY]);
            }
        }

        return neighbours;
    }

    #region PathFinding

    public List<Tile> FindPath(Tile startTile, Tile endTile, GridManager grid)
    {
        List<Tile> openTiles = new List<Tile>();
        HashSet<Tile> closed = new HashSet<Tile>();
        
        openTiles.Add(startTile);
        while (openTiles.Count > 0)
        {
            Tile currentTile = openTiles[0];
            foreach (var openTile in openTiles)
            {
                if(openTile.fCost < currentTile.fCost || openTile.fCost == currentTile.fCost && openTile.hCost < currentTile.hCost)
                    currentTile = openTile;
            }
            
            openTiles.Remove(currentTile);
            closed.Add(currentTile);

            if (currentTile == endTile)
            {
                List<Tile> path = RetracePath(startTile, endTile);
                return path;
            }

            foreach (var neighbour in grid.GetNeighbourTiles(currentTile))
            {
                if (Mathf.Abs(neighbour.gridX - currentTile.gridX) == 1 &&
                    Mathf.Abs(neighbour.gridY - currentTile.gridY) == 1)
                    continue;  // does not allowes direct diagonal movement
                
                if(closed.Contains(neighbour) || !neighbour.isWalkable)
                    continue;
                
                int newMovementCostToNeighbour = currentTile.gCost + GetDistance(currentTile, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openTiles.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, endTile);
                    neighbour.parent = currentTile;
                    
                    if(!openTiles.Contains(neighbour))
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
        while (currentTile != startTile)
        {
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
        
        //Considering the distance between two grid is 10 and distance between two diagonal grid is 14
        if(disX > disY)
            return 14*disY + 10*(disX-disY); 
        
        return 14*disX + 10*(disY-disX);
    }

    #endregion
}