using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAi : MonoBehaviour, AI
{
    public GridManager gridManager;
    public Vector2Int spawnTileIndex;

    [Header("AI Settings")] 
    public Player player;
    
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 0.5f;
    public float rotationSpeed = 720f;
    
    Tile currentTile;
    Tile targetTile;
    List<Tile> path = new List<Tile>();
    Coroutine moveCoroutine;
    bool isMoving = false;
    
    #region Subscribing/Unsubscribing Events
    private void OnEnable()
    {
        player.OnMove += Chase;
    }

    private void OnDisable()
    {
        player.OnMove -= Chase;
    }

    #endregion

    private void Start()
    {
        SetPosition();
        Chase(player.currentTile);
    }

    private void SetPosition()
    {
        Tile tile = gridManager.GetTile(spawnTileIndex.x, spawnTileIndex.y);
        currentTile = tile;
        currentTile.hasEntity = true;
        transform.position = tile.transform.position + new Vector3(0, 0.5f, 0);
    }

    public void Chase(Tile _targetTile)
    {
        if (path != null && path.Count > 0)
        {
            foreach (var pathTile in path){
                if (pathTile != null)
                    pathTile.ChangeVisual(TileType.None);
            }
        }
        
        targetTile = _targetTile;
        if (targetTile == null || targetTile == currentTile) return;
        
        path = gridManager.FindPath(currentTile, targetTile);
        
        if(path == null || path.Count == 0) return;
    
        foreach (var pathTile in path){
            if (pathTile != null)
                pathTile.ChangeVisual(TileType.Path);
        }

        if(moveCoroutine != null)
            StopCoroutine(moveCoroutine); // Stop current movement
        // Start moving
        moveCoroutine = StartCoroutine(MoveAlongPath());
    }

    private IEnumerator MoveAlongPath()
    {
        isMoving = true;

        for (int i = 0; i < path.Count; i++)
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
            float movementProgressValue = 0f;
            while (movementProgressValue < 1f)
            {
                if (targetTile == path[path.Count - 1]) break;
                movementProgressValue += Time.deltaTime * moveSpeed;
                float normalizedProgress = Mathf.Clamp01(movementProgressValue);

                // Creating jump arc
                float height = Mathf.Sin(normalizedProgress * Mathf.PI) * jumpHeight;
                Vector3 currentPos = Vector3.Lerp(startPos, endPos, normalizedProgress);
                currentPos.y += height;

                transform.position = currentPos;
                
                yield return null;
            }

            // Doulble check landing position
            if (targetTile != path[path.Count - 1])
            {
                transform.position = endPos;
                Tile previousTile = currentTile;
                currentTile = targetTile;

                previousTile.hasEntity = false;
                previousTile.isWalkable = true;
                currentTile.hasEntity = true;
                currentTile.isWalkable = false;
                
                //Reset Path visualS
                previousTile.ChangeVisual(TileType.None);
            }

            // Small pause between each tile
            yield return new WaitForSeconds(0.1f);
        }

        isMoving = false;
    }
}