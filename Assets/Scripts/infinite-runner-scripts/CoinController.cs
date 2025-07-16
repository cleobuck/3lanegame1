using UnityEngine;

/// <summary>
/// CoinController handles coin collection in the 3-lane runner game.
/// When player touches a coin, it increases the money amount and disappears.
/// </summary>
public class CoinController : InfiniteRunnerBase
{
    [Header("Coin Settings")]
    public int coinValue = 1; // How much money this coin is worth
    public float coinHeight = 5f; // Height above ground (higher than obstacles)
    
    private void Start()
    {
        // Override the Y position to make coins appear higher than obstacles
        Vector3 currentPos = transform.position;
        transform.position = new Vector3(currentPos.x, coinHeight, currentPos.z);
        
        // Optional: Add any other coin-specific initialization here
        // For example, you could add a rotation animation or glow effect
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Increase the money amount using the inherited property
            MoneyAmount += coinValue;
            
            // Optional: Play a coin collection sound effect here
            // AudioSource.PlayClipAtPoint(coinSound, transform.position);
            
            // Optional: Create a particle effect or animation
            // Instantiate(coinCollectEffect, transform.position, Quaternion.identity);
            
            // Destroy the coin
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        // Optional: Add coin rotation animation for visual appeal
        transform.Rotate(0, 90f * Time.deltaTime, 0);
    }
} 