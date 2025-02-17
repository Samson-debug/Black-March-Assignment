using System;
using System.Text;
using TMPro;
using UnityEngine;

public class TileInfoUiManager : MonoBehaviour
{
    [Range(0f, 3f)] public float Yoffset = 1f;
    
    bool canShowTileInfo;
    TextMeshProUGUI TileInfoText;
    Tile selectedTile;

    Camera mainCamera;
    private void Start()
    {
        mainCamera = Camera.main;
        TileInfoText = GetComponentInChildren<TextMeshProUGUI>();
        TileInfoText.gameObject.SetActive(false); //don't show text at start
    }

    private void Update()
    {
        //when Rigth clicked, start showing Tile info if not showing previously and vise versa
        HandleInput();

        HandleTileInfo();
        
        //Make the text face the camera
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up);
    }

    private void HandleInput()
    {
        if(Input.GetMouseButtonDown(1))
            ToggleTileInfoShow();
    }

    private void HandleTileInfo()
    {
        if(!canShowTileInfo) return;
        if (selectedTile != null)
        {
            selectedTile.ChangeVisual(TileType.None);
            selectedTile = null;
        }
        
        string tileInfo;
        Vector3 textPosition;
        RaycastHit hit;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            Tile tile = hit.collider.GetComponent<Tile>();

            if (tile != null)
            {
                selectedTile = tile;
                
                selectedTile.ChangeVisual(TileType.Selected);
                tileInfo = $"X : {tile.gridX}, Y : {tile.gridY}";
                textPosition = new Vector3(
                    tile.transform.position.x,
                    tile.transform.position.y + Yoffset,
                    tile.transform.position.z);
            }
            else
            {
                tileInfo = "No tile selected";
                textPosition = new Vector3(0f, Yoffset, 0f);
            }
        }
        else
        {
            tileInfo = "No tile selected";
            textPosition = new Vector3(0f, Yoffset, 0f);
        }
        
        TileInfoText.text = tileInfo;
        transform.position = textPosition;
    }

    private void ToggleTileInfoShow()
    {
        canShowTileInfo = !canShowTileInfo;

        if (canShowTileInfo)
        {
            //if can show tile info then activate text
            TileInfoText.gameObject.SetActive(true);
        }
        else
        {
            //if can not show tile info then deactivate text and reset selected Tile
            if (selectedTile != null)
            {
                selectedTile.ChangeVisual(TileType.None);
                selectedTile = null;
            }
            TileInfoText.gameObject.SetActive(false);
        }
    } 
}