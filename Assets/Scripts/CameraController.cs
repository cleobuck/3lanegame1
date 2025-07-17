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
    public float cameraHeight = 20f; // Height above the player (easily adjustable!)
    public float cameraDistance = 15f; // Distance behind the player
    
    [Header("Camera Lighting")]
    public bool addCameraLight = true; // Enable/disable the camera-relative light
    public Color lightColor = Color.white; // Color of the directional light
    [Range(0.5f, 3f)]
    public float lightIntensity = 1.5f; // Intensity of the directional light
    [Range(-90f, 90f)]
    public float lightAngleFromRight = 45f; // Angle from the right side of camera (0 = directly right)
    [Range(-45f, 45f)]
    public float lightVerticalAngle = -30f; // Vertical angle (negative = from above)
    public bool castShadows = true; // Whether the light should cast shadows
    
    // Private variables
    private Vector3 camOffset; // The camera's offset from the player
    private PlayerController playerController;
    private bool cameraSetup = false; // Track if camera has been set up
    
    // Lighting variables
    private Light cameraDirectionalLight;
    private GameObject lightObject;

    protected override void Start()
    {
        base.Start(); // Call base class Start
        
        // Find the PlayerController in the scene
        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController == null)
        {
            return;
        }
        
        if (mainCamera == null)
        {
            return;
        }
        
        // Setup camera light if enabled
        if (addCameraLight)
        {
            SetupCameraLight();
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
        
        // Update light position relative to camera
        if (addCameraLight && cameraDirectionalLight != null && mainCamera != null)
        {
            UpdateCameraLight();
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
    
    void SetupCameraLight()
    {
        // Create a new GameObject for the directional light
        lightObject = new GameObject("Camera Directional Light");
        cameraDirectionalLight = lightObject.AddComponent<Light>();
        
        // Configure the light
        cameraDirectionalLight.type = LightType.Directional;
        cameraDirectionalLight.color = lightColor;
        cameraDirectionalLight.intensity = lightIntensity;
        cameraDirectionalLight.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
        
        // Position the light relative to camera
        UpdateCameraLight();
        
        Debug.Log("CameraController: Created directional light from right side of camera");
    }
    
    void UpdateCameraLight()
    {
        if (cameraDirectionalLight == null || mainCamera == null) return;
        
        // Calculate the direction from the right side of the camera
        Vector3 cameraForward = mainCamera.forward;
        Vector3 cameraRight = mainCamera.right;
        Vector3 cameraUp = mainCamera.up;
        
        // Create rotation based on angles from camera's right side
        Quaternion horizontalRotation = Quaternion.AngleAxis(lightAngleFromRight, cameraUp);
        Quaternion verticalRotation = Quaternion.AngleAxis(lightVerticalAngle, cameraRight);
        
        // Combine rotations: first rotate around camera's up axis, then around right axis
        Vector3 lightDirection = horizontalRotation * verticalRotation * cameraForward;
        
        // Set the light's rotation to point in the calculated direction
        lightObject.transform.rotation = Quaternion.LookRotation(lightDirection);
        
        // Position the light at the camera's position (directional lights don't need specific positioning)
        lightObject.transform.position = mainCamera.position;
        
        // Update light properties in case they were changed in inspector
        cameraDirectionalLight.color = lightColor;
        cameraDirectionalLight.intensity = lightIntensity;
        cameraDirectionalLight.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
    }
    
    // Public methods to control the light
    public void SetLightIntensity(float intensity)
    {
        lightIntensity = intensity;
        if (cameraDirectionalLight != null)
        {
            cameraDirectionalLight.intensity = intensity;
        }
    }
    
    public void SetLightColor(Color color)
    {
        lightColor = color;
        if (cameraDirectionalLight != null)
        {
            cameraDirectionalLight.color = color;
        }
    }
    
    public void ToggleCameraLight()
    {
        addCameraLight = !addCameraLight;
        
        if (addCameraLight && cameraDirectionalLight == null)
        {
            SetupCameraLight();
        }
        else if (!addCameraLight && lightObject != null)
        {
            DestroyImmediate(lightObject);
            cameraDirectionalLight = null;
        }
    }
    
    public void SetLightAngle(float horizontalAngle, float verticalAngle)
    {
        lightAngleFromRight = horizontalAngle;
        lightVerticalAngle = verticalAngle;
        UpdateCameraLight();
    }
} 