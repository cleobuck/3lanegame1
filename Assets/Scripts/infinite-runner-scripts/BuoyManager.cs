using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// BuoyManager handles spawning and managing buoys in the 3-lane runner game.
/// It creates lines of buoys that mark the lanes and continuously spawns them ahead.
/// </summary>
public class BuoyManager : InfiniteRunnerBase
{
    [Header("Buoys")]
    public GameObject buoyPrefab; // Drag your buoy prefab here
    public float buoySpacing = 5f; // 5 units apart
    public float buoyOffset = 2f; // 2 units to the side of each lane
    private List<GameObject> spawnedBuoys = new List<GameObject>(); // Track spawned buoys
    private float lastBuoyZ = 0f; // Track where last buoys were spawned

    protected override void Start()
    {
        base.Start();
        SpawnInitialBuoys();
    }

    protected override void Update()
    {
        base.Update();
        ManageBuoys();
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
        
        lastBuoyZ = endZ; // Don't add buoySpacing here - next buoy should be at endZ + buoySpacing
        // Debug.Log($"Initial buoys spawned from {startZ} to {endZ}. Next spawn at: {lastBuoyZ + buoySpacing}");
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
            Vector3 buoyPos = new Vector3(x, 5f, zPosition); // Y=5 to be above collision tiles (2.4f)
            // Debug.Log($"Spawning buoy at position: {buoyPos}");
            GameObject buoy = Instantiate(buoyPrefab, buoyPos, Quaternion.identity);
            buoy.name = $"Buoy_Z{zPosition}_X{x}";
            spawnedBuoys.Add(buoy);
        }
    }

    void ManageBuoys()
    {
        // Spawn multiple buoys ahead to ensure no gaps
        while (lastBuoyZ < WorldDistance + 90f) // Keep buoys 90 units ahead
        {
            lastBuoyZ += buoySpacing;
            SpawnBuoyLine(lastBuoyZ);
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