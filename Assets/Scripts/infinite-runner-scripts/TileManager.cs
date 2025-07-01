using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TileManager handles the infinite scrolling tile system for the 3-lane runner game.
/// It creates a pool of tiles that continuously scroll backward and reposition themselves
/// to create the illusion of endless forward movement.
/// </summary>
public class TileManager : InfiniteRunnerBase
{
    [Header("Tile Settings")]
    public GameObject tilePrefab;    // The prefab to use for creating tiles
    public GameObject collisionPrefab; // Optional: separate collision-only prefab
    public int numberOfTiles = 10;   // How many tiles to keep active at once
    private float tileLength; // Will be calculated from prefab bounds

    // List to keep track of all active tiles in the scene
    private List<GameObject> activeTiles = new List<GameObject>();
    private List<GameObject> collisionTiles = new List<GameObject>();

    /// Initialize the tile system by creating the initial set of tiles
    protected override void Start()
    {
        base.Start();
        
        // Calculate tile length from prefab bounds
        if (tilePrefab != null)
        {
            Renderer tileRenderer = tilePrefab.GetComponent<Renderer>();
            if (tileRenderer != null)
            {
                tileLength = tileRenderer.bounds.size.z/1.2f; // Get Z-dimension (depth)
            }
            else
            {
                // Fallback if no renderer found
                tileLength = 30f;
                Debug.LogWarning("No Renderer found on tilePrefab, using default length: " + tileLength);
            }
        }
        else
        {
            tileLength = 30f;
            Debug.LogError("tilePrefab is not assigned!");
        }
        
        // Create the initial tiles and position them in a line
        for (int i = 0; i < numberOfTiles; i++)
        {
            // Visual water tiles (low position for water effect)
            // Start further back to ensure coverage and add extra tile at the front
            Vector3 visualPos = new Vector3(0, 0, (i * tileLength) - (tileLength * 2)); // Start 2 tile-lengths back
            GameObject visualTile = Instantiate(tilePrefab, visualPos, Quaternion.identity);
            visualTile.name = $"WaterTile_{i}";
            activeTiles.Add(visualTile);
            
            // Collision tiles (higher position for player to stand on)
            Vector3 collisionPos = new Vector3(0, 2.4f, (i * tileLength) - (tileLength * 2)); // Same offset as visual
            GameObject collisionTile;
            
            if (collisionPrefab != null)
            {
                // Use separate collision prefab if provided
                collisionTile = Instantiate(collisionPrefab, collisionPos, Quaternion.identity);
            }
            else
            {
                // Use same prefab but make it invisible for collision only
                collisionTile = Instantiate(tilePrefab, collisionPos, Quaternion.identity);
                
                // Make collision tile invisible but keep collider
                Renderer renderer = collisionTile.GetComponent<Renderer>();
                if (renderer != null) renderer.enabled = false;
            }
            
            collisionTile.name = $"CollisionTile_{i}";
            collisionTiles.Add(collisionTile);
        }
    }

    /// <summary>
    /// Handle the continuous scrolling and tile recycling each frame
    /// </summary>
    protected override void Update()
    {
        base.Update();
        ScrollTiles();
    }

    // -------------------- Move tiles backward to create forward movement illusion --------------------
    void ScrollTiles()
    {
        float moveDistance = ForwardSpeed * Time.deltaTime;
        
        // Update the shared world distance using inherited method
        UpdateWorldDistance(moveDistance);

        // Move all tiles backward first
        for (int i = 0; i < activeTiles.Count; i++)
        {
            activeTiles[i].transform.position += Vector3.back * moveDistance;
        }
        
        for (int i = 0; i < collisionTiles.Count; i++)
        {
            collisionTiles[i].transform.position += Vector3.back * moveDistance;
        }

        // Then check for recycling - find the tile that's furthest back
        GameObject furthestVisualTile = null;
        GameObject furthestCollisionTile = null;
        float furthestVisualZ = float.MaxValue;
        float furthestCollisionZ = float.MaxValue;

        // Find furthest back tiles
        for (int i = 0; i < activeTiles.Count; i++)
        {
            if (activeTiles[i].transform.position.z < furthestVisualZ)
            {
                furthestVisualZ = activeTiles[i].transform.position.z;
                furthestVisualTile = activeTiles[i];
            }
        }

        for (int i = 0; i < collisionTiles.Count; i++)
        {
            if (collisionTiles[i].transform.position.z < furthestCollisionZ)
            {
                furthestCollisionZ = collisionTiles[i].transform.position.z;
                furthestCollisionTile = collisionTiles[i];
            }
        }

        // Recycle the furthest tiles if they're too far back
        if (furthestVisualTile != null && furthestVisualZ < -tileLength)
        {
            // Find the furthest forward tile to position after it
            float furthestForwardZ = float.MinValue;
            for (int i = 0; i < activeTiles.Count; i++)
            {
                if (activeTiles[i] != furthestVisualTile && activeTiles[i].transform.position.z > furthestForwardZ)
                {
                    furthestForwardZ = activeTiles[i].transform.position.z;
                }
            }
            
            Vector3 newPos = furthestVisualTile.transform.position;
            newPos.z = furthestForwardZ + tileLength;
            furthestVisualTile.transform.position = newPos;
        }

        if (furthestCollisionTile != null && furthestCollisionZ < -tileLength)
        {
            // Find the furthest forward collision tile to position after it
            float furthestForwardZ = float.MinValue;
            for (int i = 0; i < collisionTiles.Count; i++)
            {
                if (collisionTiles[i] != furthestCollisionTile && collisionTiles[i].transform.position.z > furthestForwardZ)
                {
                    furthestForwardZ = collisionTiles[i].transform.position.z;
                }
            }
            
            Vector3 newPos = furthestCollisionTile.transform.position;
            newPos.z = furthestForwardZ + tileLength;
            furthestCollisionTile.transform.position = newPos;
        }
    }
} 