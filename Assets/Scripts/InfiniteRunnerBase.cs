using UnityEngine;
using UnityEngine.UI; // For working with the UI (Text score display)

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
    protected static int moneyAmount = 0; // Money collected by the player
    
    // -------------------- Shared Settings --------------------
    [Header("Shared Settings")]
    protected float laneDistance = 4f;
    
    [Header("World Speed")]
    [Range(1f, 50f)] // Creates a slider in the Inspector!
    public float forwardSpeed = 5f; // PUBLIC so it shows in Inspector (but only for base class via custom editor)
    
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

    public static int MoneyAmount 
    { 
        get { return moneyAmount; } 
        set { moneyAmount = value; } 
    }

    public static float ForwardSpeed 
    { 
        get { return instance != null ? instance.forwardSpeed : 5f; } 
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
    
    /// Check if this is the base InfiniteRunnerBase class (used by custom editor)
    public bool IsBaseClass()
    {
        return this.GetType() == typeof(InfiniteRunnerBase);
    }
    
    // -------------------- Unity Events --------------------
    protected virtual void Awake()
    {
        // Only set the static instance if this is the BASE InfiniteRunnerBase class
        // This ensures forwardSpeed comes from the correct component
        if (instance == null && this.GetType() == typeof(InfiniteRunnerBase))
        {
            instance = this;
        }
    }
    
    protected virtual void Start()
    {
        // Base functionality - can be overridden by child classes
    }
    
    protected virtual void Update()
    {
        // Base functionality - can be overridden by child classes
    }
} 