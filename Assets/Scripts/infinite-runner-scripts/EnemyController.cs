using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple EnemyController that handles player collision.
/// If player is sliding, enemy disappears. Otherwise, game over.
/// </summary>
public class EnemyController : MonoBehaviour
{
    
    private void Start()
    {
  
 
 
        transform.rotation = Quaternion.Euler(0, 180, 0); // Try 180 degrees first
        
       
    }
    
    private void OnTriggerEnter(Collider other)
    {
     


        if (other.CompareTag("Player")) // Using standard "Player" tag
        {
            // Get the player's animator to check if sliding
            Animator playerAnimator = other.GetComponent<Animator>();
            
            if (playerAnimator != null && playerAnimator.GetBool("IsSliding"))
            {
            
                Destroy(gameObject);
            }
            else
            {
               
                SceneManager.LoadScene("GameOverUi");
            }
        }
    }
    
   
} 