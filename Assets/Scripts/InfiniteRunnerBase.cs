using UnityEngine;

/// Base class for all game-related scripts to inherit from.
/// Contains shared variables and common functionality.
public class InfiniteRunnerBase : MonoBehaviour
{
    // -------------------- Shared Game State --------------------
    [Header("Shared Game Data")]
    protected static float worldDistance = 0f; // Protected = accessible to child classes
    protected static float gameScore = 0f;
    protected static bool isGameRunning = true;
    protected static int currentLane = 1; // 0=left, 1=center, 2=right
    
    // -------------------- Shared Settings --------------------
    [Header("Shared Settings")]
    protected float laneDistance = 4f;
    
    [Header("World Speed")]
    [Range(1f, 50f)] // Creates a slider in the Inspector!
    public float forwardSpeed = 19f; // PUBLIC so it shows in Inspector
    
    // Static reference to access the speed from anywhere
    protected static InfiniteRunnerBase instance;
    
    // -------------------- Properties (Getters/Setters) --------------------
    public static float WorldDistance 
    { 
        get { return worldDistance; } 
        set { worldDistance = value; } 
    }
    
    public static float GameScore 
    { 
        get { return gameScore; } 
        set { gameScore = value; } 
    }

    public static float ForwardSpeed 
{ 
    get { return instance != null ? instance.forwardSpeed : 10f; } 
}
    
    public static bool IsGameRunning 
    { 
        get { return isGameRunning; } 
        set { isGameRunning = value; } 
    }
    
    public static int CurrentLane 
    { 
        get { return currentLane; } 
        set { currentLane = Mathf.Clamp(value, 0, 2); } // Keep in valid range
    }
    

    
    // -------------------- Shared Helper Methods --------------------
    
    /// Convert lane index to world X position
    protected float LaneToWorldX(int lane)
    {
        return (lane - 1) * laneDistance; // -4, 0, 4
    }
    
    /// Get a random lane X position
    protected float RandomLaneX()
    {
        int lane = Random.Range(0, 3);
        return LaneToWorldX(lane);
    }
    
    /// Update the world distance (called by WorldManager)
    protected void UpdateWorldDistance(float deltaDistance)
    {
        worldDistance += deltaDistance;
        gameScore = worldDistance; // Score = distance traveled
    }
    
    // -------------------- Unity Events --------------------
    protected virtual void Awake()
    {
        // Set up the static instance reference
        if (instance == null)
        {
            instance = this;
        }
    }
    
    protected virtual void Start()
    {
        // Override in child classes if needed
    }
    
    protected virtual void Update()
    {
        // Override in child classes if needed
    }
} 