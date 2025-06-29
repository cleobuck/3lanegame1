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
    private float tileLength; // Will be calculated from prefab bounds

    // List to keep track of all active tiles in the scene
    private List<GameObject> activeTiles = new List<GameObject>();
    private List<GameObject> collisionTiles = new List<GameObject>();


   [Header("Buoys")]
    public GameObject buoyPrefab; // Drag your buoy prefab here
    public float buoySpacing = 5f; // 5 units apart
    public float buoyOffset = 2f; // 2 units to the side of each lane
    private List<GameObject> spawnedBuoys = new List<GameObject>(); // Track spawned buoys
    private float lastBuoyZ = 0f; // Track where last buoys were spawned

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
        
        // Calculate tile length from prefab bounds
        if (tilePrefab != null)
        {
            Renderer tileRenderer = tilePrefab.GetComponent<Renderer>();
            if (tileRenderer != null)
            {
                tileLength = tileRenderer.bounds.size.z; // Get Z-dimension (depth)
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
            Vector3 visualPos = new Vector3(0, 0, (i * tileLength) - 60f); // Start tiles even further back
            GameObject visualTile = Instantiate(tilePrefab, visualPos, Quaternion.identity);
            visualTile.name = $"WaterTile_{i}";
            activeTiles.Add(visualTile);
            
            // Collision tiles (higher position for player to stand on)
            Vector3 collisionPos = new Vector3(0, 2.4f, (i * tileLength) - 30f); // Start collision tiles further back too
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
            Debug.Log($"Created collision tile {i} at position: {collisionPos}");
        }



          SpawnInitialBuoys();
    }

    void SpawnInitialBuoys()
    {
        // Spawn buoys starting from behind the player to just ahead
        float startZ = -60f; // Start from same position as tiles
        float endZ = 60f;    // End just ahead of player
        
        for (float z = startZ; z <= endZ; z += buoySpacing)
        {
            SpawnBuoyLine(z);
        }
        
        lastBuoyZ = endZ + buoySpacing; // Set next spawn position
        // Debug.Log($"Initial buoys spawned from {startZ} to {endZ}. Next spawn at: {lastBuoyZ}");
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
        ManageBuoys(); // ADD THIS LINE

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
            if (activeTiles[i].transform.position.z < -(tileLength + 10f)) // Even further back for visual tiles
            {
                Vector3 newPos = activeTiles[i].transform.position;
                newPos.z += numberOfTiles * tileLength; // Move to front
                activeTiles[i].transform.position = newPos;
            }
        }
        
        // Move collision tiles backward
        for (int i = 0; i < collisionTiles.Count; i++)
        {
            
            collisionTiles[i].transform.position += Vector3.back * moveDistance;
            
         
            
            if (collisionTiles[i].transform.position.z < -tileLength)
            {
                Vector3 prevPos = collisionTiles[i].transform.position;
                Vector3 newPos = collisionTiles[i].transform.position;
                newPos.z += numberOfTiles * tileLength; // Move to front
                collisionTiles[i].transform.position = newPos;
                Debug.Log($"Recycled collision tile from Z={prevPos.z} to Z={newPos.z}");
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


  
        // -------------------- Spawn and manage buoys --------------------
    void SpawnBuoyLine(float zPosition)
    {
        // Calculate the 4 buoy X positions (creating 4 vertical lines)
        float[] xPositions = {
            LaneToWorldX(0) - buoyOffset, // Left of lane 0 (-6)
            LaneToWorldX(0) + buoyOffset, // Right of lane 0 (-2) 
            LaneToWorldX(1) + buoyOffset, // Right of lane 1 (2)
            LaneToWorldX(2) + buoyOffset  // Right of lane 2 (6)
        };
        
        // Debug.Log($"Spawning buoys at Z={zPosition}. Lane positions: {LaneToWorldX(0)}, {LaneToWorldX(1)}, {LaneToWorldX(2)}");
        
        // Spawn a buoy at each X position for this Z position
        foreach (float x in xPositions)
        {
            Vector3 buoyPos = new Vector3(x, 5f, zPosition); // Y=3.5 to be above collision tiles (2.4f)
            // Debug.Log($"Spawning buoy at position: {buoyPos}");
            GameObject buoy = Instantiate(buoyPrefab, buoyPos, Quaternion.identity);
            buoy.name = $"Buoy_Z{zPosition}_X{x}";
            spawnedBuoys.Add(buoy);
        }
    }
    

    void ManageBuoys()
    {
        // Spawn new buoys ahead when player gets close
        if (WorldDistance + 30f > lastBuoyZ) // Spawn when player is 30 units away
        {

            SpawnBuoyLine(lastBuoyZ);
            lastBuoyZ += buoySpacing;
        }
        
        // Move buoys backward and clean up old ones
        float moveDistance = ForwardSpeed * Time.deltaTime;
        
        for (int i = spawnedBuoys.Count - 1; i >= 0; i--)
        {
            GameObject buoy = spawnedBuoys[i];
            
            if (buoy != null)
            {
                Vector3 oldPos = buoy.transform.position;
                buoy.transform.position += Vector3.back * moveDistance;
                
                // Remove buoys that have passed too far behind
                if (buoy.transform.position.z < -60f) // Increased cleanup distance
                {
                    // Debug.Log($"Destroying buoy at Z={buoy.transform.position.z}");
                    Destroy(buoy);
                    spawnedBuoys.RemoveAt(i);
                }
            }
            else
            {
                spawnedBuoys.RemoveAt(i);
            }
        }
    }

}
