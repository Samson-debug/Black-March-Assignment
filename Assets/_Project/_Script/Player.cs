using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Action OnMove;
    public GridManager gridManager;
    public Vector2Int spawnTileIndex;
    
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 0.5f;
    public float rotationSpeed = 720f;

    [HideInInspector] public Tile currentTile;
    Tile destinationTile;
    List<Tile> path = new List<Tile>();
    bool isMoving = false;
    
    
    private void Awake()
    {
        SetPosition();
    }

    private void SetPosition()
    {
        Tile tile = gridManager.GetTile(spawnTileIndex.x, spawnTileIndex.y);
        currentTile = tile;
        currentTile.hasEntity = true;
        transform.position = tile.transform.position + new Vector3(0, 0.5f, 0);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && !isMoving)
            MovePlayer();
    }

    private void MovePlayer()
    {
        if(path != null && path.Count > 0){
            foreach (var pathTile in path)
            {
                if (pathTile != null)
                    pathTile.ChangeVisual(TileType.None);
            }
        }
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile == null) return;
            if(!tile.isWalkable) return;
            
            path = gridManager.FindPath(currentTile, tile, gridManager);
            
            if(path == null || path.Count == 0) return;
            foreach (var pathTile in path)
            {
                pathTile.ChangeVisual(TileType.Path);
            }

            // Start moving along the path
            StartCoroutine(MoveAlongPath());
        }
    }

    private IEnumerator MoveAlongPath()
    {
        isMoving = true;

        for(int i = 0; i < path.Count; i++)
        {
            Tile targetTile = path[i];
            Vector3 startPos = transform.position;
            Vector3 endPos = targetTile.transform.position + new Vector3(0, 0.5f, 0);
            Vector3 direction = (endPos - startPos).normalized;
            
            //Rotation logic
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            float rotationProgress = 0f;
            while (rotationProgress < 1f)
            {
                rotationProgress += Time.deltaTime * (rotationSpeed / 360f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationProgress);
                yield return null;
            }

            // Jump logic
            float moveProgress = 0f;
            while (moveProgress < 1f)
            {
                moveProgress += Time.deltaTime * moveSpeed;
                float normalizedProgress = Mathf.Clamp01(moveProgress);
                
                // Calculate jump height
                float height = Mathf.Sin(normalizedProgress * Mathf.PI) * jumpHeight;
                Vector3 currentPos = Vector3.Lerp(startPos, endPos, normalizedProgress);
                currentPos.y += height;
                
                transform.position = currentPos;
                yield return null;
            }

            // Doulble check landing position
            transform.position = endPos;
            Tile previousTile = currentTile;
            currentTile = targetTile;

            previousTile.hasEntity = false;
            currentTile.hasEntity = true;
            
            //Reset Path visual
            previousTile.ChangeVisual(TileType.None);
            
            // Small pause between each tile
            yield return new WaitForSeconds(0.1f);
        }

        isMoving = false;
        OnMove?.Invoke();
    }
}