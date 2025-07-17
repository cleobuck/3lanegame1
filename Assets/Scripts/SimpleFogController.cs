using UnityEngine;

/// <summary>
/// Enhanced atmospheric fog controller that creates natural aerial perspective
/// Objects naturally fade and become mistier as they get further away from the camera
/// This creates realistic atmospheric perspective effects for better immersion
/// Works with Built-in Render Pipeline - no URP required!
/// </summary>
public class SimpleFogController : MonoBehaviour
{
    [Header("Aerial Perspective Settings")]
    public bool enableFogOnStart = true;
    public Color fogColor = new Color(0.5f, 0.6f, 0.7f, 1f); // Atmospheric blue-gray
    public FogMode fogMode = FogMode.ExponentialSquared; // Best for aerial perspective
    
    [Header("Atmospheric Density")]
    [Range(0.001f, 0.1f)]
    public float atmosphericDensity = 0.015f; // How thick the atmosphere is
    [Range(0.5f, 3f)]
    public float perspectiveIntensity = 1.2f; // How strong the aerial perspective effect is
    
    [Header("Distance-Based Fading")]
    public bool useDistanceBasedFading = true;
    [Range(5f, 30f)]
    public float fadeStartDistance = 8f; // When objects start to fade
    [Range(20f, 60f)]
    public float fadeEndDistance = 35f; // When objects are fully faded
    
    [Header("Dynamic Atmospheric Effects")]
    public bool adjustWithGameSpeed = true; // Atmosphere changes with game speed
    public float speedInfluence = 0.2f; // How much speed affects the atmosphere
    
    [Header("Atmospheric Themes")]
    public AtmosphericTheme currentTheme = AtmosphericTheme.Ocean;
    
    [Header("Advanced Settings")]
    public bool enableHeightBasedFog = true; // Fog gets thicker closer to ground
    public float groundFogHeight = 3f; // Height where fog is thickest
    public float heightFogIntensity = 0.5f; // How much height affects fog
    
    private Camera playerCamera;
    private float baseDensity;
    private Color baseColor;
    
    public enum AtmosphericTheme
    {
        Ocean,      // Blue-gray oceanic atmosphere
        Misty,      // Light gray misty morning
        Stormy,     // Dark atmospheric storm
        Sunset,     // Warm orange-pink atmosphere
        Arctic,     // Cold blue-white atmosphere
        Desert      // Warm yellow-brown atmosphere
    }

    void Start()
    {
        // Find the main camera
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindFirstObjectByType<Camera>();
        }

        // Store base values
        baseDensity = atmosphericDensity;
        baseColor = fogColor;

        if (enableFogOnStart)
        {
            ApplyAtmosphericTheme(currentTheme);
            EnableAerialPerspective();
        }
    }

    void Update()
    {
        if (RenderSettings.fog)
        {
            UpdateAtmosphericEffects();
        }
    }

    void UpdateAtmosphericEffects()
    {
        // Dynamic atmosphere based on game speed
        if (adjustWithGameSpeed)
        {
            float currentSpeed = InfiniteRunnerBase.ForwardSpeed;
            float speedMultiplier = 1f + (currentSpeed * speedInfluence * 0.1f);
            
            // Adjust fog density based on speed (faster = more atmospheric effect)
            float dynamicDensity = baseDensity * speedMultiplier;
            dynamicDensity = Mathf.Clamp(dynamicDensity, 0.005f, 0.08f);
            
            RenderSettings.fogDensity = dynamicDensity * perspectiveIntensity;
        }
        
        // Height-based fog adjustment
        if (enableHeightBasedFog && playerCamera != null)
        {
            float cameraHeight = playerCamera.transform.position.y;
            float heightFactor = Mathf.Clamp01((groundFogHeight - cameraHeight) / groundFogHeight);
            float heightAdjustedDensity = RenderSettings.fogDensity * (1f + heightFactor * heightFogIntensity);
            RenderSettings.fogDensity = heightAdjustedDensity;
        }
        
        // Distance-based linear fog for additional control
        if (useDistanceBasedFading && RenderSettings.fogMode == FogMode.Linear)
        {
            RenderSettings.fogStartDistance = fadeStartDistance;
            RenderSettings.fogEndDistance = fadeEndDistance;
        }
    }

    public void EnableAerialPerspective()
    {
        // Enable Unity's built-in fog system for aerial perspective
        RenderSettings.fog = true;
        RenderSettings.fogColor = fogColor;
        
        if (useDistanceBasedFading)
        {
            // Use linear fog for precise distance control
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = fadeStartDistance;
            RenderSettings.fogEndDistance = fadeEndDistance;
        }
        else
        {
            // Use exponential fog for more natural atmospheric perspective
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogDensity = atmosphericDensity * perspectiveIntensity;
        }

        Debug.Log($"Aerial Perspective enabled - Theme: {currentTheme}, Mode: {RenderSettings.fogMode}");
        Debug.Log($"Atmospheric settings - Density: {RenderSettings.fogDensity:F4}, Color: {fogColor}");
    }

    public void DisableAerialPerspective()
    {
        RenderSettings.fog = false;
        Debug.Log("Aerial Perspective disabled");
    }

    public void ApplyAtmosphericTheme(AtmosphericTheme theme)
    {
        currentTheme = theme;
        
        switch (theme)
        {
            case AtmosphericTheme.Ocean:
                SetOceanAtmosphere();
                break;
            case AtmosphericTheme.Misty:
                SetMistyAtmosphere();
                break;
            case AtmosphericTheme.Stormy:
                SetStormyAtmosphere();
                break;
            case AtmosphericTheme.Sunset:
                SetSunsetAtmosphere();
                break;
            case AtmosphericTheme.Arctic:
                SetArcticAtmosphere();
                break;
            case AtmosphericTheme.Desert:
                SetDesertAtmosphere();
                break;
        }
        
        EnableAerialPerspective();
    }

    // -------------------- Atmospheric Themes --------------------
    
    void SetOceanAtmosphere()
    {
        fogColor = new Color(0.4f, 0.6f, 0.8f, 1f); // Ocean blue
        atmosphericDensity = 0.018f; // Moderate density
        perspectiveIntensity = 1.2f;
        fadeStartDistance = 10f;
        fadeEndDistance = 30f;
        useDistanceBasedFading = false; // Use exponential for natural ocean haze
        fogMode = FogMode.ExponentialSquared;
        
        Debug.Log("Ocean atmosphere applied - Natural oceanic haze with aerial perspective");
    }

    void SetMistyAtmosphere()
    {
        fogColor = new Color(0.8f, 0.8f, 0.85f, 1f); // Light gray-blue
        atmosphericDensity = 0.025f; // Thicker atmosphere
        perspectiveIntensity = 1.5f;
        fadeStartDistance = 6f;
        fadeEndDistance = 20f;
        useDistanceBasedFading = true; // Linear for misty morning effect
        fogMode = FogMode.Linear;
        
        Debug.Log("Misty atmosphere applied - Soft morning mist with distance fading");
    }

    void SetStormyAtmosphere()
    {
        fogColor = new Color(0.3f, 0.3f, 0.4f, 1f); // Dark gray
        atmosphericDensity = 0.035f; // Very thick atmosphere
        perspectiveIntensity = 1.8f;
        fadeStartDistance = 5f;
        fadeEndDistance = 15f;
        useDistanceBasedFading = true; // Heavy fog with limited visibility
        fogMode = FogMode.Linear;
        
        Debug.Log("Stormy atmosphere applied - Heavy storm fog with dramatic perspective");
    }

    void SetSunsetAtmosphere()
    {
        fogColor = new Color(0.9f, 0.6f, 0.4f, 1f); // Warm orange-pink
        atmosphericDensity = 0.012f; // Light atmospheric haze
        perspectiveIntensity = 1.0f;
        fadeStartDistance = 12f;
        fadeEndDistance = 40f;
        useDistanceBasedFading = false; // Exponential for warm glow
        fogMode = FogMode.Exponential;
        
        Debug.Log("Sunset atmosphere applied - Warm atmospheric glow with soft perspective");
    }

    void SetArcticAtmosphere()
    {
        fogColor = new Color(0.7f, 0.8f, 0.9f, 1f); // Cold blue-white
        atmosphericDensity = 0.008f; // Very clear but cold atmosphere
        perspectiveIntensity = 0.8f;
        fadeStartDistance = 15f;
        fadeEndDistance = 50f;
        useDistanceBasedFading = true; // Linear for crisp arctic visibility
        fogMode = FogMode.Linear;
        
        Debug.Log("Arctic atmosphere applied - Clear cold air with distant perspective");
    }

    void SetDesertAtmosphere()
    {
        fogColor = new Color(0.8f, 0.7f, 0.5f, 1f); // Warm yellow-brown
        atmosphericDensity = 0.020f; // Heat haze effect
        perspectiveIntensity = 1.3f;
        fadeStartDistance = 8f;
        fadeEndDistance = 25f;
        useDistanceBasedFading = false; // Exponential for heat shimmer
        fogMode = FogMode.ExponentialSquared;
        
        Debug.Log("Desert atmosphere applied - Heat haze with warm atmospheric perspective");
    }

    // -------------------- Public Controls --------------------
    
    public void SetAtmosphericDensity(float density)
    {
        atmosphericDensity = Mathf.Clamp(density, 0.001f, 0.1f);
        baseDensity = atmosphericDensity;
        if (RenderSettings.fog && RenderSettings.fogMode != FogMode.Linear)
        {
            RenderSettings.fogDensity = atmosphericDensity * perspectiveIntensity;
        }
    }

    public void SetPerspectiveIntensity(float intensity)
    {
        perspectiveIntensity = Mathf.Clamp(intensity, 0.5f, 3f);
        if (RenderSettings.fog && RenderSettings.fogMode != FogMode.Linear)
        {
            RenderSettings.fogDensity = atmosphericDensity * perspectiveIntensity;
        }
    }

    public void SetFogColor(Color color)
    {
        fogColor = color;
        baseColor = color;
        if (RenderSettings.fog)
        {
            RenderSettings.fogColor = fogColor;
        }
    }

    public void SetFadeDistances(float startDistance, float endDistance)
    {
        fadeStartDistance = Mathf.Clamp(startDistance, 1f, 50f);
        fadeEndDistance = Mathf.Clamp(endDistance, fadeStartDistance + 5f, 100f);
        
        if (RenderSettings.fog && RenderSettings.fogMode == FogMode.Linear)
        {
            RenderSettings.fogStartDistance = fadeStartDistance;
            RenderSettings.fogEndDistance = fadeEndDistance;
        }
    }

    // -------------------- Inspector Helpers --------------------
    
    [System.Serializable]
    public class AtmosphericPreset
    {
        public string name;
        public Color color;
        public float density;
        public float intensity;
        public FogMode mode;
    }
} 