using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TileManager handles the infinite scrolling tile system for the 3-lane runner game.
/// It creates a pool of tiles that continuously scroll backward and reposition themselves
/// to create the illusion of endless forward movement.
/// </summary>
public class WorldManager : InfiniteRunnerBase // Inherit from GameBase instead of MonoBehaviour
{
    [Header("Tile Settings")]
    public GameObject tilePrefab;    // The prefab to use for creating tiles
    public GameObject collisionPrefab; // Optional: separate collision-only prefab
    public int numberOfTiles = 6;   // How many tiles to keep active at once
    public float tileLength = 30f;  // Length of each tile (used for positioning)

    // List to keep track of all active tiles in the scene
    private List<GameObject> activeTiles = new List<GameObject>();
    private List<GameObject> collisionTiles = new List<GameObject>();


    // -------------------- World Movement --------------------
    // worldDistance is now inherited from GameBase

    // -------------------- Obstacle Spawning --------------------
    [Header("Obstacles")]
    public GameObject[] obstaclePrefabs; // Array of obstacle prefabs that can be randomly spawned
    public float distanceBetween = 15f; // Distance between each obstacle spawn
    private float spawnZ = 30f; // Z-position where the next obstacle will spawn
    private List<GameObject> spawnedObstacles = new List<GameObject>(); // Track spawned obstacles

    /// Initialize the tile system by creating the initial set of tiles
    protected override void Start() // Override the base Start method
    {
        base.Start(); // Call the parent's Start method first
        
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
    protected override void Update() // Override the base Update method
    {
        base.Update(); // Call the parent's Update method first
        
        ScrollTiles();
        SpawnObstacles();
        MoveObstacles();
    }

    // -------------------- Move tiles backward to create forward movement illusion --------------------
    void ScrollTiles()
    {
        float moveDistance = ForwardSpeed * Time.deltaTime;
        
        // Update the shared world distance using inherited method
        UpdateWorldDistance(moveDistance);

        // Move visual tiles backward
        for (int i = 0; i < activeTiles.Count; i++)
        {
            activeTiles[i].transform.position += Vector3.back * moveDistance;
            
            // If tile has moved too far back, move it to the front
            if (activeTiles[i].transform.position.z < -tileLength)
            {
                Vector3 newPos = activeTiles[i].transform.position;
                newPos.z += numberOfTiles * tileLength;
                activeTiles[i].transform.position = newPos;
            }
        }
        
        // Move collision tiles backward
        for (int i = 0; i < collisionTiles.Count; i++)
        {
            collisionTiles[i].transform.position += Vector3.back * moveDistance;
            
            // If tile has moved too far back, move it to the front
            if (collisionTiles[i].transform.position.z < -tileLength)
            {
                Vector3 newPos = collisionTiles[i].transform.position;
                newPos.z += numberOfTiles * tileLength;
                collisionTiles[i].transform.position = newPos;
            }
        }
    }

    // -------------------- Move obstacles towards the player --------------------
    void MoveObstacles()
    {
        float moveDistance = ForwardSpeed * Time.deltaTime;

        // Move all tracked obstacles towards the player
        for (int i = spawnedObstacles.Count - 1; i >= 0; i--)
        {
            GameObject obstacle = spawnedObstacles[i];
            
            if (obstacle != null)
            {
                obstacle.transform.position += Vector3.back * moveDistance;
                
                // Destroy obstacles that have passed the player
                if (obstacle.transform.position.z < -20f) // Use fixed position instead of player.position
                {
                    Destroy(obstacle);
                    spawnedObstacles.RemoveAt(i); // Remove from tracking list
                }
            }
            else
            {
                // Remove null references from the list
                spawnedObstacles.RemoveAt(i);
            }
        }
    }

    // -------------------- Spawn obstacles in front of the player --------------------
    void SpawnObstacles()
    {
        if (obstaclePrefabs.Length > 0 && WorldDistance + 30f > spawnZ) // Use inherited WorldDistance property
        {
            // Pick a random obstacle prefab
            GameObject obstacle = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            // Random lane X-position using inherited method, Y = same as player height, Z = ahead of player
            Vector3 spawnPos = new Vector3(RandomLaneX(), 3.4f, spawnZ);
            GameObject spawnedObstacle = Instantiate(obstacle, spawnPos, Quaternion.identity);
            
            // Track the spawned obstacle in our list
            spawnedObstacles.Add(spawnedObstacle);
            
            spawnZ += distanceBetween; // Move spawnZ forward for the next one
        }
    }

}
