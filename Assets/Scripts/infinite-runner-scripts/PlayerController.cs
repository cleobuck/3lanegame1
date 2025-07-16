using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerController handles all player-specific logic including movement, jumping, sliding, and input.
/// Uses the new Unity Input System for modern input handling.
/// </summary>
public class PlayerController : InfiniteRunnerBase
{
    [Header("Player Settings")]
    public GameObject playerPrefab; // The prefab used to spawn the player
    public float laneChangeSpeed = 20f; // How fast the player switches lanes
    public float jumpForce = 25f; // How high the player jumps
    public float gravity = -40f; // Gravity applied when falling
    public float slideDuration = 1.533f; // How long the slide lasts

    [Header("Input Actions")]
    public InputActionAsset inputActions;

    // Player components and references
    private CharacterController controller;
    private Transform player;
    private Animator playerAnimator;
    
    // Movement state
    private int desiredLane = 1; // 0 = left, 1 = center, 2 = right
    private Vector3 direction; // Movement direction vector
    
    // Sliding state
    private bool isSliding = false;
    private float slideTimer = 0f;
    
    // Input actions
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction slideAction;

    protected override void Awake()
    {
        base.Awake();
        SetupInputActions();
    }

    protected override void Start()
    {
        base.Start();
        SpawnPlayer();
    }

    protected override void Update()
    {
        base.Update();
        
        // Only run player logic if player exists and is properly set up
        if (player == null || controller == null || !controller.enabled)
            return;
            
        HandlePlayerMovement();
        UpdateAnimations();
    }

    void SetupInputActions()
    {
        if (inputActions == null)
        {
            Debug.LogError("InputActions asset not assigned to PlayerController!");
            return;
        }

        // Get the input actions from the Player action map
        var playerMap = inputActions.FindActionMap("Player");
        moveAction = playerMap.FindAction("Move");
        jumpAction = playerMap.FindAction("Jump");
        slideAction = playerMap.FindAction("Crouch");
    }

    void OnEnable()
    {
        if (moveAction != null) moveAction.Enable();
        if (jumpAction != null) jumpAction.Enable();
        if (slideAction != null) slideAction.Enable();

        // Subscribe to input events
        if (moveAction != null) moveAction.performed += OnMove;
        if (jumpAction != null) jumpAction.performed += OnJump;
        if (slideAction != null) slideAction.performed += OnSlide;
    }

    void OnDisable()
    {
        if (moveAction != null) moveAction.Disable();
        if (jumpAction != null) jumpAction.Disable();
        if (slideAction != null) slideAction.Disable();

        // Unsubscribe from input events
        if (moveAction != null) moveAction.performed -= OnMove;
        if (jumpAction != null) jumpAction.performed -= OnJump;
        if (slideAction != null) slideAction.performed -= OnSlide;
    }

    void SpawnPlayer()
    {
        if (player == null && playerPrefab != null)
        {
            // Spawn player at correct height to match collision tiles
            Vector3 playerPosition = new Vector3(0, 3.4f, 0); // Y=3.4f to be above collision tiles (2.4f)
            GameObject spawned = Instantiate(playerPrefab, playerPosition, Quaternion.identity);
            
            // Ensure player is active
            spawned.SetActive(true);
            
            // Get required components
            player = spawned.transform;
            controller = spawned.GetComponent<CharacterController>();
            playerAnimator = spawned.GetComponent<Animator>();
            
            // Ensure controller is enabled
            if (controller != null) controller.enabled = true;
            
            // Verify setup
            if (controller == null)
                Debug.LogError("CharacterController component not found on player prefab!");
        }
    }

    void HandlePlayerMovement()
    {
        // Update slide timer
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f)
            {
                isSliding = false;
            }
        }

        // Update the shared current lane
        CurrentLane = desiredLane;

        // Calculate target X position based on desired lane
        float targetX = LaneToWorldX(desiredLane);
        
        // Smoothly move towards target lane (only X movement)
        Vector3 currentPos = player.position;
        currentPos.x = Mathf.MoveTowards(currentPos.x, targetX, laneChangeSpeed * Time.deltaTime);

        // Handle jumping and gravity
        if (controller.isGrounded)
        {
            // Only stick to ground if we're not jumping
            if (direction.y <= 0)
            {
                direction.y = -1f; // Stick to the ground
            }
        
        }
        else
        {
            direction.y += gravity * Time.deltaTime; // Apply gravity over time
        }

        // Apply movement: lane switching + jumping/falling, no forward movement
        Vector3 moveVector = new Vector3(
            currentPos.x - player.position.x, // Lane switching movement
            direction.y * Time.deltaTime,      // Jumping/falling
            0                                  // No forward movement (world moves instead)
        );


        // Move the character
        if (controller != null && controller.enabled && controller.gameObject.activeInHierarchy)
        {
            controller.Move(moveVector);
        }
    }

    void UpdateAnimations()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsJumping", !controller.isGrounded);
            playerAnimator.SetBool("IsSliding", isSliding);
        }
    }

    // Input event handlers
    void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        
        // Handle horizontal movement (lane switching)
        if (input.x > 0.5f) // Right
        {
            desiredLane = Mathf.Min(desiredLane + 1, 2);
        }
        else if (input.x < -0.5f) // Left
        {
            desiredLane = Mathf.Max(desiredLane - 1, 0);
        }
    }

    void OnJump(InputAction.CallbackContext context)
    {
        
        if (controller != null && controller.isGrounded)
        {
            direction.y = jumpForce;
        }
        else
        {
            Debug.LogError($"[PlayerController] Jump blocked - Controller null: {controller == null}, Not grounded: {controller != null && !controller.isGrounded}");
        }
    }

    void OnSlide(InputAction.CallbackContext context)
    {
        if (controller != null && controller.isGrounded && !isSliding)
        {
            isSliding = true;
            slideTimer = slideDuration;
        }
    }

    // Public getters for other scripts
    public Transform GetPlayerTransform() => player;
    public bool IsPlayerSliding() => isSliding;
    public CharacterController GetCharacterController() => controller;
} 