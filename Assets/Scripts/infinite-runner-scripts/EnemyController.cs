using UnityEngine;
using System.Collections;

/// <summary>
/// EnemyController handles basic enemy behavior including animations.
/// When used as an obstacle, movement is handled by the ObstacleManager.
/// This script should be attached to enemy prefabs.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;
    
    [Header("Behavior")]
    public bool isObstacle = true; // Set to true when used as an obstacle
    public float moveSpeed = 2f; // Only used if not an obstacle
    public bool shouldMove = true; // Only used if not an obstacle
    
    [Header("Animation States")]
    public string idleAnimationName = "Idle";
    public string walkAnimationName = "Walk";
    public string attackAnimationName = "Attack";
    
    [Header("Obstacle Behavior")]
    public bool playWalkAnimationAsObstacle = true; // Play walk animation when moving as obstacle
    
    [Header("Rotation Settings")]
    public bool autoRotateWhenSpawned = true; // Automatically rotate to face player
    public Vector3 obstacleRotation = new Vector3(0, 180, 0); // Rotation when spawned as obstacle
    
    [Header("Position Settings")]
    public bool adjustHeightWhenSpawned = true; // Adjust Y position when spawned as obstacle
    public float obstacleGroundHeight = 2.4f; // Y position when spawned as obstacle (same as collision tiles)
    public bool lockYPosition = true; // Lock Y position to prevent sinking
    
    private float lockedYPosition; // Store the locked Y position
    
    private void Start()
    {
        // Get animator if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();
            
        // Start with appropriate animation
        if (animator != null)
        {
            if (isObstacle && playWalkAnimationAsObstacle && !string.IsNullOrEmpty(walkAnimationName))
            {
                animator.Play(walkAnimationName);
            }
            else if (!string.IsNullOrEmpty(idleAnimationName))
            {
                animator.Play(idleAnimationName);
            }
        }
    }
    
    private void Update()
    {
        // Only handle movement if not an obstacle (ObstacleManager handles obstacle movement)
        if (!isObstacle)
        {
            HandleNonObstacleMovement();
        }
        // If it's an obstacle, just maintain animation - movement is handled by ObstacleManager
        
        // Lock Y position if enabled and we're an obstacle
        if (isObstacle && lockYPosition)
        {
            Vector3 pos = transform.position;
            if (Mathf.Abs(pos.y - lockedYPosition) > 0.01f) // Only adjust if it's moved significantly
            {
                pos.y = lockedYPosition;
                transform.position = pos;
            }
        }
    }
    
    private void HandleNonObstacleMovement()
    {
        // Handle movement for non-obstacle enemies
        if (shouldMove)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            
            // Play walk animation if moving
            if (animator != null && !string.IsNullOrEmpty(walkAnimationName))
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName(walkAnimationName))
                {
                    animator.Play(walkAnimationName);
                }
            }
        }
        else
        {
            // Play idle animation if not moving
            if (animator != null && !string.IsNullOrEmpty(idleAnimationName))
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName(idleAnimationName))
                {
                    animator.Play(idleAnimationName);
                }
            }
        }
    }
    
    /// <summary>
    /// Play a specific animation
    /// </summary>
    public void PlayAnimation(string animationName)
    {
        if (animator != null && !string.IsNullOrEmpty(animationName))
        {
            animator.Play(animationName);
        }
    }
    
    /// <summary>
    /// Play attack animation
    /// </summary>
    public void Attack()
    {
        if (animator != null && !string.IsNullOrEmpty(attackAnimationName))
        {
            animator.Play(attackAnimationName);
        }
    }
    
    /// <summary>
    /// Called when this enemy is spawned as an obstacle
    /// </summary>
    public void OnSpawnedAsObstacle()
    {
        isObstacle = true;
        
        // Handle physics to prevent falling through collision tiles
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Disable gravity and physics movement for obstacles
            rb.useGravity = false;
            rb.isKinematic = true; // Make it kinematic so it doesn't fall
        }
        
        // Adjust height to match collision tiles if enabled
        if (adjustHeightWhenSpawned)
        {
            Vector3 pos = transform.position;
            pos.y = obstacleGroundHeight;
            transform.position = pos;
        }
        
        // Store the locked Y position
        if (lockYPosition)
        {
            lockedYPosition = transform.position.y;
        }

        // Face toward the player if auto-rotate is enabled
        if (autoRotateWhenSpawned)
        {
            // Try different rotation methods
            Vector3 originalRotation = transform.rotation.eulerAngles;
            
            // Method 1: Direct euler rotation
            transform.rotation = Quaternion.Euler(obstacleRotation);
        }
        
        // Start appropriate animation for obstacle
        if (animator != null && playWalkAnimationAsObstacle && !string.IsNullOrEmpty(walkAnimationName))
        {
            animator.Play(walkAnimationName);
        }
    }
    

} 