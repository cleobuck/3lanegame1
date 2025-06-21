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
    public int numberOfTiles = 6;   // How many tiles to keep active at once
    public float tileLength = 30f;  // Length of each tile (used for positioning)
    public float scrollSpeed = 10f; // How fast tiles move backward (player's forward speed)

    // List to keep track of all active tiles in the scene
    private List<GameObject> activeTiles = new List<GameObject>();

    /// <summary>
    /// Initialize the tile system by creating the initial set of tiles
    /// </summary>
    void Start()
    {
        // Create the initial tiles and position them in a line
        for (int i = 0; i < numberOfTiles; i++)
        {
            // Calculate spawn position: each tile is placed one tile-length ahead of the previous
            // i=0: position (0,0,0), i=1: position (0,0,30), i=2: position (0,0,60), etc.
            Vector3 spawnPos = new Vector3(0, 0, i * tileLength);
            
            // Create the tile at the calculated position with no rotation
            GameObject tile = Instantiate(tilePrefab, spawnPos, Quaternion.identity);
            
            // Add the new tile to our tracking list
            activeTiles.Add(tile);
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
            // Time.deltaTime ensures smooth movement regardless of framerate
            // Space.World means we move in world coordinates, not relative to the tile's rotation
            tile.transform.Translate(Vector3.back * scrollSpeed * Time.deltaTime, Space.World);
        }

        // STEP 2: Check if we need to recycle the frontmost tile
        // Get the first tile in our list (the one that's been moving the longest)
        GameObject firstTile = activeTiles[0];
        
        // Check if this tile has moved far enough behind the player to be out of view
        // If its Z position is less than negative tile length, it's behind the player
        if (firstTile.transform.position.z < -tileLength)
        {
            // STEP 3: Recycle the old tile by moving it to the end of the track
            
            // Remove the old tile from the front of our list
            activeTiles.RemoveAt(0);

            // Find the position where we should place the recycled tile
            // Get the last tile in our list (the furthest ahead)
            GameObject lastTile = activeTiles[activeTiles.Count - 1];
            
            // Calculate new position: one tile-length ahead of the last tile
            Vector3 newPos = lastTile.transform.position + new Vector3(0, 0, tileLength);

            // Move the recycled tile to its new position at the end of the track
            firstTile.transform.position = newPos;

            // Add the recycled tile to the end of our list
            // Now it becomes the "last" tile and will be the last to be recycled again
            activeTiles.Add(firstTile);
        }
    }
}
