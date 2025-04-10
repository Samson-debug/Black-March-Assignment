using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Visual Elements")]
    public Material defaultMaterial;
    public Material selectedMaterial;
    public Material PathMaterial;
    public TileType tileType = TileType.None;
    
    //node position in grid
    public int gridX;
    public int gridY;
    
    //Pathfinding Variables
    public Tile parent;
    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;

    public bool hasEntity;
    public bool isWalkable;

    Renderer renderer;

    private void Awake()
    {
        renderer = GetComponentInChildren<Renderer>();
    }

    public void ChangeVisual(TileType _tileType)
    {
        tileType = _tileType;
        if (tileType == TileType.None)
            renderer.material = defaultMaterial;
        else if (tileType == TileType.Selected)
            renderer.material = selectedMaterial;
        else
            renderer.material = PathMaterial;
    }
}

//Only for Visualization purpose
public enum TileType
{
    None,
    Selected,
    Path
}