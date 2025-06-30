using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ObstacleManager handles spawning and managing obstacles in the 3-lane runner game.
/// It spawns obstacles at random lanes and moves them toward the player.
/// </summary>
public class ObstacleManager : InfiniteRunnerBase
{
    [Header("Obstacles")]
    public GameObject[] obstaclePrefabs; // Array of obstacle prefabs that can be randomly spawned
    public float distanceBetween = 15f; // Distance between each obstacle spawn
    private float spawnZ = 30f; // Z-position where the next obstacle will spawn
    private List<GameObject> spawnedObstacles = new List<GameObject>(); // Track spawned obstacles

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        SpawnObstacles();
        MoveObstacles();
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