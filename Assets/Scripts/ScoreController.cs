using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ScoreController handles ONLY UI and score display functionality.
/// This inherits from InfiniteRunnerBase so it can access shared game data.
/// Attach this to ONE GameObject in your scene to control the UI.
/// </summary>
public class ScoreController : InfiniteRunnerBase
{
    [Header("UI Settings")]
    public Text scoreText; // Reference to the UI text that displays the score
    public string scorePrefix = "Score: "; // Prefix for score display
    public bool showFloatScore = false; // Show decimal places or just integer
    
    [Header("Money Display")]
    public Text moneyText; // Reference to the UI text that displays the money
    public string moneyPrefix = "$: "; // Prefix for money display

    protected override void Start()
    {
        base.Start(); // Call base class Start
        
        if (scoreText == null)
        {
            Debug.LogError("ScoreController: Score Text not assigned!");
        }
        
        if (moneyText == null)
        {
            Debug.LogError("ScoreController: Money Text not assigned!");
        }
    }

    protected override void Update()
    {
        base.Update(); // Call base class Update
        
        UpdateScoreDisplay(); // Update the score display
        UpdateMoneyDisplay(); // Update the money display
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            if (showFloatScore)
            {
                scoreText.text = scorePrefix + GameScore.ToString("F1"); // Show 1 decimal place
            }
            else
            {
                scoreText.text = scorePrefix + Mathf.FloorToInt(GameScore).ToString(); // Show integer only
            }
        }
    }
    
    void UpdateMoneyDisplay()
    {
        if (moneyText != null)
        {
            moneyText.text = moneyPrefix + MoneyAmount.ToString();
        }
    }
} 