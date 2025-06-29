using UnityEngine;
using UnityEngine.UI; // For working with the UI (Text score display)
using System.Collections.Generic;

public class EndlessRunnerGame : InfiniteRunnerBase // Inherit from GameBase instead of MonoBehaviour
{
    // -------------------- Player Settings --------------------
    [Header("Player")]
    public GameObject playerPrefab; // The prefab (template) used to spawn the player at runtime
    private CharacterController controller; // Controls player movement and handles collisions (filled at runtime)
    private Transform player; // Reference to the player's position in the scene (filled at runtime)

    private Animator playerAnimator; // Add this variable
    // laneDistance is now inherited from GameBase
    private int desiredLane = 1; // The lane the player wants to be in: 0 = left, 1 = center, 2 = right
    public float laneChangeSpeed = 20f; // How fast the player switches lanes
    public float jumpForce = 25f; // How high the player jumps
    public float gravity = -40f; // Gravity applied when falling
    private Vector3 direction; // Stores the player's movement direction


    private bool isSliding = false; // Track if we're currently sliding
    public float slideDuration = 1.533f; // How long the slide lasts
    private float slideTimer = 0f; // Timer to track slide duration
    // -------------------- Camera Settings --------------------
    [Header("Camera")]
    public Transform mainCamera; // Reference to the main camera in the scene
    private Vector3 camOffset; // The camera's offset from the player (used to follow smoothly)

    // -------------------- UI and Score --------------------
    [Header("UI")]
    public Text scoreText; // Reference to the UI text that displays the score
    // score is now inherited as gameScore from GameBase

    // -------------------- Start() is called when the game begins --------------------
    protected override void Start() // Override the base Start method
    {
        base.Start(); // Call the parent's Start method first
        
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


            playerAnimator = spawned.GetComponent<Animator>();
        }
  
        // Set up the camera to be behind and above the player (fixed position)
        if (mainCamera != null && player != null)
        {
            mainCamera.position = player.position + new Vector3(0, 5, -10); // Fixed offset from player
            mainCamera.LookAt(player); // Make the camera look at the player
            camOffset = mainCamera.position - player.position; // Store this offset
        }
    }

    // -------------------- Update() runs every frame --------------------
    protected override void Update() // Override the base Update method
    {
        base.Update(); // Call the parent's Update method first
        
        // If something went wrong and player/controller aren't set, stop running logic
        if (player == null || controller == null || !controller.enabled ) 
        {
            return;
        }
        
        RunLogic();       // Handle movement and jumping
        CameraFollow();   // Make camera follow player
        ScoreUpdate();    // Update the score based on distance
    }

    // -------------------- Player movement and jumping --------------------
    void RunLogic()
    {


        // Handle lane switching with left/right arrow keys
        if (Input.GetKeyDown(KeyCode.RightArrow)) desiredLane = Mathf.Min(desiredLane + 1, 2);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) desiredLane = Mathf.Max(desiredLane - 1, 0);


    // Handle sliding with down arrow key
    if (Input.GetKeyDown(KeyCode.DownArrow) && controller.isGrounded && !isSliding)
    {
        isSliding = true;
        slideTimer = slideDuration; // Start the slide timer
    }

    // Update slide timer
    if (isSliding)
    {
        slideTimer -= Time.deltaTime;
        if (slideTimer <= 0f)
        {
            isSliding = false; // End the slide
        }
    }

        // Update the shared current lane
        CurrentLane = desiredLane;

        // Calculate target X position based on desired lane using inherited method
        float targetX = LaneToWorldX(desiredLane); // Use inherited helper method
        
        // Smoothly move towards target lane (only X movement)
        Vector3 currentPos = player.position;
        currentPos.x = Mathf.MoveTowards(currentPos.x, targetX, laneChangeSpeed * Time.deltaTime);

        // Handle jumping and gravity
        if (controller.isGrounded)
        {
            direction.y = -1f; // Stick to the ground
            if (Input.GetKeyDown(KeyCode.UpArrow))
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


        // Set animation based on grounded state
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsJumping", !controller.isGrounded);
            playerAnimator.SetBool("IsSliding", isSliding); 
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
        // Use inherited GameScore property instead of local score variable
        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.FloorToInt(GameScore).ToString(); // Update UI text
    }

    // RandomLaneX method is now inherited from GameBase
} 