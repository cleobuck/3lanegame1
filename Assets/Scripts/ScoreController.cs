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

    protected override void Start()
    {
        base.Start(); // Call base class Start
        
        // Auto-find MoneyText if not assigned
        if (moneyText == null)
        {
            GameObject moneyTextObj = GameObject.Find("MoneyText");
            if (moneyTextObj != null)
            {
                moneyText = moneyTextObj.GetComponent<Text>();
                Debug.Log("ScoreController: Auto-found MoneyText component");
            }
            else
            {
                Debug.LogError("ScoreController: MoneyText GameObject not found! Please create a Text UI element named 'MoneyText'");
            }
        }
        
        // Always setup MoneyText position if it exists (whether auto-found or pre-assigned)
        if (moneyText != null)
        {
            SetupMoneyTextPosition();
        }
        
        if (scoreText == null)
        {
            // Debug.LogError("ScoreController: Score Text not assigned!");
        }
        
        if (moneyText == null)
        {
            Debug.LogError("ScoreController: Money Text not assigned or found!");
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
            string newText = MoneyAmount.ToString() + "$";
            moneyText.text = newText;
            
        }
        else
        {
            Debug.LogError("ScoreController: moneyText is null in UpdateMoneyDisplay!");
        }
    }
    
    void SetupMoneyTextPosition()
    {
        if (moneyText != null)
        {
            // Ensure Canvas is properly set up
            Canvas canvas = moneyText.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                // Force Canvas to Screen Space - Overlay for UI independence
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    Debug.Log($"ScoreController: Changing Canvas render mode from {canvas.renderMode} to ScreenSpaceOverlay");
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
            }
            
            RectTransform rectTransform = moneyText.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Set anchors to top-right
                rectTransform.anchorMin = new Vector2(1f, 1f);
                rectTransform.anchorMax = new Vector2(1f, 1f);
                rectTransform.pivot = new Vector2(1f, 1f);
                
                // Position 20 pixels from top-right corner
                rectTransform.anchoredPosition = new Vector2(-20f, -20f);
                rectTransform.sizeDelta = new Vector2(200f, 60f); // Make it bigger for testing
                
                // Make it more visible for testing
                moneyText.color = Color.white; // Bright yellow
                moneyText.fontSize = 45; // Larger font
                moneyText.text = "TEST 99$"; // Test text
                
               
            }
        }
    }
} 