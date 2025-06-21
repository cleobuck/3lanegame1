using UnityEngine;
using UnityEngine.UI; // For working with the UI (Text score display)

public class EndlessRunnerGame : MonoBehaviour
{
    // -------------------- Player Settings --------------------
    [Header("Player")]
    public GameObject playerPrefab; // The prefab (template) used to spawn the player at runtime
    public CharacterController controller; // Controls player movement and handles collisions
    public Transform player; // Reference to the player's position in the scene

    public float forwardSpeed = 10f; // How fast the player moves forward
    public float laneDistance = 4f; // Distance between lanes (used for switching left/right)
    private int desiredLane = 1; // The lane the player wants to be in: 0 = left, 1 = center, 2 = right
    public float jumpForce = 8f; // How high the player jumps
    public float gravity = -20f; // Gravity applied when falling
    private Vector3 direction; // Stores the player's movement direction

    // -------------------- Camera Settings --------------------
    [Header("Camera")]
    public Transform mainCamera; // Reference to the main camera in the scene
    private Vector3 camOffset; // The camera's offset from the player (used to follow smoothly)

    // -------------------- Obstacle Spawning --------------------
    [Header("Obstacles")]
    public GameObject[] obstaclePrefabs; // Array of obstacle prefabs that can be randomly spawned
    public float distanceBetween = 15f; // Distance between each obstacle spawn
    private float spawnZ = 30f; // Z-position where the next obstacle will spawn

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
            GameObject spawned = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity); // Spawn at position (0,0,0)
            spawned.SetActive(true); // <-- Force it to be active, just in case
            player = spawned.transform; // Store reference to the player's Transform
            controller = spawned.GetComponent<CharacterController>(); // Get the CharacterController attached to the player
        }

        // Set up the camera to be behind and above the player, and look at the player
        if (mainCamera != null && player != null)
        {
            mainCamera.position = player.position + new Vector3(0, 5, -10); // Offset camera
            mainCamera.LookAt(player); // Make the camera look at the player
            camOffset = mainCamera.position - player.position; // Store this offset for following
        }
    }
    


    // -------------------- Update() runs every frame --------------------
    void Update()
    {
        // If something went wrong and player/controller aren't set, stop running logic
        if (player == null || controller == null) return;
        
        RunLogic();       // Handle movement and jumping
        CameraFollow();   // Make camera follow player
        ScoreUpdate();    // Update the score based on distance
        SpawnObstacles(); // Spawn obstacles ahead of the player
    }

    // -------------------- Player movement and jumping --------------------
    void RunLogic()
    {
        direction.z = forwardSpeed; // Always move forward on the Z axis

        // Handle lane switching with left/right arrow keys
        if (Input.GetKeyDown(KeyCode.RightArrow)) desiredLane = Mathf.Min(desiredLane + 1, 2);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) desiredLane = Mathf.Max(desiredLane - 1, 0);

        // Determine the target lane position based on desired lane
        Vector3 targetPosition = player.position.z * Vector3.forward;
        if (desiredLane == 0) targetPosition += Vector3.left * laneDistance;
        else if (desiredLane == 2) targetPosition += Vector3.right * laneDistance;

        // Move the player horizontally toward the desired lane smoothly
        Vector3 moveVector = Vector3.zero;
        moveVector.x = (targetPosition - player.position).x * 10f; // Smooth side movement
        moveVector.y = direction.y; // Use Y direction for jumping/falling
        moveVector.z = direction.z; // Move forward

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

        // Move the character based on calculated movement vector
        controller.Move(moveVector * Time.deltaTime);
    }

    // -------------------- Make the camera follow the player --------------------
    void CameraFollow()
    {
        if (mainCamera != null && player != null)
        {
            mainCamera.position = player.position + camOffset; // Keep same offset from player
        }
    }

    // -------------------- Update and display the score --------------------
    void ScoreUpdate()
    {
        score = player.position.z; // Score based on how far the player has moved
        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.FloorToInt(score).ToString(); // Update UI text
    }

    // -------------------- Spawn obstacles in front of the player --------------------
    void SpawnObstacles()
    {
        if (player.position.z + 60f > spawnZ) // If player is getting close to spawnZ
        {
            // Pick a random obstacle prefab
            GameObject obstacle = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            // Random lane X-position, Y = ground level, Z = spawn position
            Vector3 spawnPos = new Vector3(RandomLaneX(), 0, spawnZ);
            Instantiate(obstacle, spawnPos, Quaternion.identity); // Spawn it
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

