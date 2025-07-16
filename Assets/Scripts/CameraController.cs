using UnityEngine;

/// <summary>
/// CameraController handles ONLY camera functionality.
/// This inherits from InfiniteRunnerBase so it can access shared game data.
/// Attach this to ONE GameObject in your scene to control the camera.
/// </summary>
public class CameraController : InfiniteRunnerBase
{
    [Header("Camera Settings")]
    public Transform mainCamera; // Reference to the main camera in the scene
    public float cameraHeight = 10f; // Height above the player (easily adjustable!)
    public float cameraDistance = 10f; // Distance behind the player
    
    // Private variables
    private Vector3 camOffset; // The camera's offset from the player
    private PlayerController playerController;
    private bool cameraSetup = false; // Track if camera has been set up

    protected override void Start()
    {
        base.Start(); // Call base class Start
        
        
        // Find the PlayerController in the scene
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            return;
        }
        
        if (mainCamera == null)
        {
            return;
        }
       
    }

    protected override void Update()
    {
        base.Update(); // Call base class Update
        
        // Only run if we have a valid player controller
        if (playerController == null || mainCamera == null) return;
        
        // Try to set up camera if not done yet
        if (!cameraSetup)
        {
            SetupCamera();
        }
        else
        {
            CameraFollow(); // Make camera follow player
        }
    }

    void SetupCamera()
    {
        // Wait for player to be spawned by PlayerController
        if (mainCamera != null && playerController != null)
        {
            Transform player = playerController.GetPlayerTransform();
            if (player != null)
            {
                // Use the adjustable height and distance values
                Vector3 newCameraPos = player.position + new Vector3(0, cameraHeight, -cameraDistance);
                
             
                mainCamera.position = newCameraPos;
                
            
                
                mainCamera.LookAt(player); // Make the camera look at the player
                camOffset = mainCamera.position - player.position; // Store this offset
                
                cameraSetup = true; // Mark as set up
                
            
            }
          
        }
    }

    void CameraFollow()
    {
        if (mainCamera != null && playerController != null)
        {
            Transform player = playerController.GetPlayerTransform();
            if (player != null)
            {
                // Only follow the player's lane switching (X movement), keep fixed Y and Z
                Vector3 targetPosition = player.position + camOffset;
                Vector3 oldPos = mainCamera.position;
                mainCamera.position = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
                
            
            }
        }
    }
} 