using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple EnemyController that handles player collision.
/// If player is sliding, enemy disappears. Otherwise, game over.
/// </summary>
public class EnemyController : MonoBehaviour
{
    private PlayerController playerController;
    
    private void Start()
    {
        // Find the PlayerController in the scene
        playerController = FindFirstObjectByType<PlayerController>();
        
        // Set enemy rotation to face the player
        transform.rotation = Quaternion.Euler(0, 180, 0);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Check if player is sliding using the PlayerController
            if (playerController != null && playerController.IsPlayerSliding())
            {
                // Player is sliding - destroy the enemy
                Destroy(gameObject);
            }
            else
            {
                // Player is not sliding - game over
                SceneManager.LoadScene("GameOverUi");
            }
        }
    }
} 