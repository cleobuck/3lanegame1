using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TileManager handles the infinite scrolling tile system for the 3-lane runner game.
/// It creates a pool of tiles that continuously scroll backward and reposition themselves
/// to create the illusion of endless forward movement.
/// </summary>
public class TileManager : MonoBehaviour
{
    [Header("Tile Settings")]
    public GameObject tilePrefab;    // The prefab to use for creating tiles
    public GameObject collisionPrefab; // Optional: separate collision-only prefab
    public int numberOfTiles = 6;   // How many tiles to keep active at once
    public float tileLength = 30f;  // Length of each tile (used for positioning)
    public float scrollSpeed = 10f; // How fast tiles move backward (player's forward speed)

    // List to keep track of all active tiles in the scene
    private List<GameObject> activeTiles = new List<GameObject>();
    private List<GameObject> collisionTiles = new List<GameObject>();

    /// <summary>
    /// Initialize the tile system by creating the initial set of tiles
    /// </summary>
    void Start()
    {
        // Create the initial tiles and position them in a line
        for (int i = 0; i < numberOfTiles; i++)
        {
            // Visual water tiles (low position for water effect)
            Vector3 visualPos = new Vector3(0, 0, i * tileLength);
            GameObject visualTile = Instantiate(tilePrefab, visualPos, Quaternion.identity);
            visualTile.name = $"WaterTile_{i}";
            activeTiles.Add(visualTile);
            
            // Collision tiles (higher position for player to stand on)
            Vector3 collisionPos = new Vector3(0, 2.4f, i * tileLength);
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
    void Update()
    {
        // STEP 1: Move all tiles backward to simulate forward player movement
        foreach (GameObject tile in activeTiles)
        {
            // Move each tile backward (negative Z direction) at the scroll speed
            tile.transform.Translate(Vector3.back * scrollSpeed * Time.deltaTime, Space.World);
        }
        
        foreach (GameObject tile in collisionTiles)
        {
            // Move collision tiles at the same speed
            tile.transform.Translate(Vector3.back * scrollSpeed * Time.deltaTime, Space.World);
        }

        // STEP 2: Check if we need to recycle the frontmost tile
        // Get the first tile in our list (the one that's been moving the longest)
        GameObject firstTile = activeTiles[0];
        GameObject firstCollisionTile = collisionTiles[0];
        
        // Check if this tile has moved far enough behind the player to be out of view
        if (firstTile.transform.position.z < -tileLength)
        {
            // STEP 3: Recycle both visual and collision tiles
            
            // Remove the old tiles from the front of our lists
            activeTiles.RemoveAt(0);
            collisionTiles.RemoveAt(0);

            // Find the position where we should place the recycled tiles
            GameObject lastTile = activeTiles[activeTiles.Count - 1];
            GameObject lastCollisionTile = collisionTiles[collisionTiles.Count - 1];
            
            // Calculate new positions: one tile-length ahead of the last tiles
            Vector3 newVisualPos = lastTile.transform.position + new Vector3(0, 0, tileLength);
            Vector3 newCollisionPos = lastCollisionTile.transform.position + new Vector3(0, 0, tileLength);

            // Move the recycled tiles to their new positions
            firstTile.transform.position = newVisualPos;
            firstCollisionTile.transform.position = newCollisionPos;

            // Add the recycled tiles to the end of our lists
            activeTiles.Add(firstTile);
            collisionTiles.Add(firstCollisionTile);
        }
    }
}
