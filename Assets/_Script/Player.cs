using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Action<Tile> OnMove;
    public GridManager gridManager;
    public Vector2Int spawnTileIndex;
    
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 0.5f;
    public float rotationSpeed = 720f;
    public LayerMask gridLayer;
    
    [Header("UI Elements")]
    public TextMeshProUGUI movingText;

    [HideInInspector] public Tile currentTile;
    Tile targetTile;
    List<Tile> path = new List<Tile>();
    Coroutine moveCoroutine;
    Coroutine movingMessageCoroutine;
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
        if (Input.GetMouseButtonDown(0)){
            if (isMoving){
                if(movingMessageCoroutine != null)
                    StopCoroutine(movingMessageCoroutine);
                movingMessageCoroutine = StartCoroutine(EnableMovingMessage());
            }
            else
                MovePlayer();
        }
    }

    private IEnumerator EnableMovingMessage()
    {
        movingText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(0.4f);
        
        movingText.gameObject.SetActive(false);
    }

    private void MovePlayer()
    {
        if(path != null && path.Count > 0){
            foreach (var pathTile in path){
                if (pathTile != null)
                    pathTile.ChangeVisual(TileType.None);
            }
        }
        
        //Cast a ray to get the tile to reach
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, gridLayer)){
            targetTile = hit.collider.GetComponent<Tile>();
            
            StartMovement();
        }
    }

    private void StartMovement()
    {
        if (targetTile == null) return;
        if(!targetTile.isWalkable) return;
            
        path = gridManager.FindPath(currentTile, targetTile);
        
        if(path == null || path.Count == 0) return;
        
        foreach (var pathTile in path){
            if (pathTile != null)
                pathTile.ChangeVisual(TileType.Path);
        }

        if(moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        // Start moving along the path
        moveCoroutine = StartCoroutine(MoveAlongPath());
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
            
            //update enemy for player position change
            int nextPathTileIndex = (i + 1 ) < path.Count ? (i + 1) : i;
            OnMove?.Invoke(path[nextPathTileIndex]);
            
            // Small pause between each tile
            yield return new WaitForSeconds(0.1f);
        }

        isMoving = false;
    }
}