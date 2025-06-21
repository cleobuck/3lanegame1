using UnityEngine;
using UnityEngine.UI; // For working with the UI (Text score display)
using System.Collections.Generic;

public class EndlessRunnerGame : MonoBehaviour
{
    // -------------------- Player Settings --------------------
    [Header("Player")]
    public GameObject playerPrefab; // The prefab (template) used to spawn the player at runtime
    private CharacterController controller; // Controls player movement and handles collisions (filled at runtime)
    private Transform player; // Reference to the player's position in the scene (filled at runtime)

    public float forwardSpeed = 10f; // How fast the world moves towards the player
    public float laneDistance = 4f; // Distance between lanes (used for switching left/right)
    private int desiredLane = 1; // The lane the player wants to be in: 0 = left, 1 = center, 2 = right
    public float laneChangeSpeed = 10f; // How fast the player switches lanes
    public float jumpForce = 8f; // How high the player jumps
    public float gravity = -20f; // Gravity applied when falling
    private Vector3 direction; // Stores the player's movement direction

    // -------------------- Camera Settings --------------------
    [Header("Camera")]
    public Transform mainCamera; // Reference to the main camera in the scene
    private Vector3 camOffset; // The camera's offset from the player (used to follow smoothly)

    // -------------------- World Movement --------------------
    [Header("World Movement")]
    public Transform ground; // Reference to the ground plane that will move
    public float groundLength = 20f; // Length of the ground plane (for repositioning)
    private float worldDistance = 0f; // How far the world has "moved" (for score)
    private List<GameObject> groundTiles = new List<GameObject>(); // Track multiple ground tiles
    private int numberOfGroundTiles = 5; // How many ground tiles to maintain

    // -------------------- Obstacle Spawning --------------------
    [Header("Obstacles")]
    public GameObject[] obstaclePrefabs; // Array of obstacle prefabs that can be randomly spawned
    public float distanceBetween = 15f; // Distance between each obstacle spawn
    private float spawnZ = 30f; // Z-position where the next obstacle will spawn
    private List<GameObject> spawnedObstacles = new List<GameObject>(); // Track spawned obstacles

    // -------------------- UI and Score --------------------
    [Header("UI")]
    public Text scoreText; // Reference to the UI text that displays the score
    private float score; // The current score (based on distance run)

    // -------------------- Start() is called when the game begins --------------------
    void Start()
    {
        
        // If no player exists yet, spawn the player from the prefab
        if (player == null && playerPrefab != null)
        {
            
            // Spawn player at a fixed position (they won't move forward)
            Vector3 playerPosition = new Vector3(0, 1, 0); // Slightly above ground
            GameObject spawned = Instantiate(playerPrefab, playerPosition, Quaternion.identity);
            
            // Immediately check state after instantiation            
            // Force activation multiple times to ensure it sticks
            spawned.SetActive(true);
            
            player = spawned.transform; // Store reference to the player's Transform
            controller = spawned.GetComponent<CharacterController>(); // Get the CharacterController attached to the player
                        
            // Double-check activation after getting components
            if (!spawned.activeInHierarchy)  spawned.SetActive(true);
            
            // Ensure the controller is enabled
            if (controller != null) controller.enabled = true;
           
        }
  
        // Set up the camera to be behind and above the player (fixed position)
        if (mainCamera != null && player != null)
        {
          
            mainCamera.position = player.position + new Vector3(0, 5, -10); // Fixed offset from player
            mainCamera.LookAt(player); // Make the camera look at the player
            camOffset = mainCamera.position - player.position; // Store this offset
        }
        
        // Set up infinite ground system
        SetupInfiniteGround();
    }
    
    // -------------------- Setup infinite ground tiles --------------------
    void SetupInfiniteGround()
    {
        if (ground != null)
        {
            // Try multiple methods to get the correct ground size
            Renderer groundRenderer = ground.GetComponent<Renderer>();
            MeshFilter meshFilter = ground.GetComponent<MeshFilter>();
            
            if (groundRenderer != null)
            {
                groundLength = groundRenderer.bounds.size.z; // Use actual ground size
                Debug.Log($"Auto-detected ground length: {groundLength}");
                Debug.Log($"Ground bounds: {groundRenderer.bounds}");
                Debug.Log($"Ground scale: {ground.transform.localScale}");
                Debug.Log($"Ground position: {ground.transform.position}");
            }
            
            // Unity's default plane is 10x10 units, check if we need to override
            if (meshFilter != null && meshFilter.sharedMesh.name.Contains("Plane"))
            {
                // Unity default plane is 10 units, but scaled by transform
                float actualSize = 10f * ground.transform.localScale.z;
                Debug.Log($"Detected Unity plane, calculated size: {actualSize}");
                groundLength = actualSize;
            }
            
            // Slightly overlap tiles to prevent gaps
            float tileSpacing = groundLength * 0.99f; // 1% overlap
            
            // Add the original ground to our tracking list
            groundTiles.Add(ground.gameObject);
            
            // Create additional ground tiles extending forward (with slight overlap)
            for (int i = 1; i < numberOfGroundTiles; i++)
            {
                GameObject newTile = Instantiate(ground.gameObject);
                // Position tiles with slight overlap to prevent gaps
                Vector3 newPosition = ground.position + new Vector3(0, 0, i * tileSpacing);
                newTile.transform.position = newPosition;
                newTile.name = $"GroundTile_{i}"; // Name them for easier debugging
                groundTiles.Add(newTile);
                
                Debug.Log($"Created tile {i} at position: {newPosition} (spacing: {tileSpacing})");
            }
            
            Debug.Log($"Created {numberOfGroundTiles} ground tiles, each {groundLength} units long, spaced {tileSpacing} apart");
            
            // Debug: Show all tile positions
            for (int i = 0; i < groundTiles.Count; i++)
            {
                Debug.Log($"Tile {i} final position: {groundTiles[i].transform.position}");
            }
        }
    }
    
    // -------------------- Update() runs every frame --------------------
    void Update()
    {
        // If something went wrong and player/controller aren't set, stop running logic
        if (player == null || controller == null || !controller.enabled ) 
        {
            return;
        }
        
        RunLogic();       // Handle movement and jumping
        CameraFollow();   // Make camera follow player
        ScoreUpdate();    // Update the score based on distance
        SpawnObstacles(); // Spawn obstacles ahead of the player
    }

    // -------------------- Player movement and jumping --------------------
    void RunLogic()
    {
        // Move the world towards the player (creating forward movement illusion)
        MoveWorld();
        
        // Handle lane switching with left/right arrow keys
        if (Input.GetKeyDown(KeyCode.RightArrow)) desiredLane = Mathf.Min(desiredLane + 1, 2);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) desiredLane = Mathf.Max(desiredLane - 1, 0);

        // Calculate target X position based on desired lane
        float targetX = (desiredLane - 1) * laneDistance; // -1 * 4 = -4 (left), 0 * 4 = 0 (center), 1 * 4 = 4 (right)
        
        // Smoothly move towards target lane (only X movement)
        Vector3 currentPos = player.position;
        currentPos.x = Mathf.MoveTowards(currentPos.x, targetX, laneChangeSpeed * Time.deltaTime);

        // Handle jumping and gravity
        if (controller.isGrounded)
        {
            direction.y = -1f; // Stick to the ground
            if (Input.GetKeyDown(KeyCode.Space))
                direction.y = jumpForce; // Jump!
        }
        else
        {
            direction.y += gravity * Time.deltaTime; // Apply gravity over time
        }

        // Apply movement: lane switching + jumping/falling, no forward movement
        Vector3 moveVector = new Vector3(
            currentPos.x - player.position.x, // Lane switching movement
            direction.y * Time.deltaTime,      // Jumping/falling
            0                                  // No forward movement!
        );

        // Move the character
        if (controller != null && controller.enabled && controller.gameObject.activeInHierarchy)
        {
            controller.Move(moveVector);
        }
    }

    // -------------------- Move the world towards the player --------------------
    void MoveWorld()
    {
        float moveDistance = forwardSpeed * Time.deltaTime;
        worldDistance += moveDistance;

        // Move all tracked obstacles towards the player
        for (int i = spawnedObstacles.Count - 1; i >= 0; i--)
        {
            GameObject obstacle = spawnedObstacles[i];
            
            if (obstacle != null)
            {
                obstacle.transform.position += Vector3.back * moveDistance;
                
                // Destroy obstacles that have passed the player
                if (obstacle.transform.position.z < player.position.z - 20f)
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

    // -------------------- Make the camera follow the player --------------------
    void CameraFollow()
    {
        if (mainCamera != null && player != null)
        {
            // Only follow the player's lane switching (X movement), keep fixed Y and Z
            Vector3 targetPosition = player.position + camOffset;
            mainCamera.position = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
        }
    }

    // -------------------- Update and display the score --------------------
    void ScoreUpdate()
    {
        score = worldDistance; // Score based on how far the world has moved
        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.FloorToInt(score).ToString(); // Update UI text
    }

    // -------------------- Spawn obstacles in front of the player --------------------
    void SpawnObstacles()
    {
        if (obstaclePrefabs.Length > 0 && worldDistance + 30f > spawnZ) // Spawn based on world distance
        {
            // Pick a random obstacle prefab
            GameObject obstacle = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            // Random lane X-position, Y = same as player height, Z = ahead of player
            Vector3 spawnPos = new Vector3(RandomLaneX(), 1, spawnZ);
            GameObject spawnedObstacle = Instantiate(obstacle, spawnPos, Quaternion.identity);
            
            // Track the spawned obstacle in our list
            spawnedObstacles.Add(spawnedObstacle);
            
            spawnZ += distanceBetween; // Move spawnZ forward for the next one
        }
    }

    // -------------------- Pick a random lane (-1, 0, 1) for X-position --------------------
    float RandomLaneX()
    {
        int lane = Random.Range(0, 3); // Random lane index: 0 = left, 1 = center, 2 = right
        return (lane - 1) * laneDistance; // Convert to X-position: -laneDistance, 0, +laneDistance
    }
}

