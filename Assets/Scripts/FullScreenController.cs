using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles fullscreen toggle using the new Input System.
/// Press ESC to toggle between fullscreen and windowed mode.
/// </summary>
public class FullScreenController : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionAsset inputActions;
    
    private InputAction escapeAction;

    void Awake()
    {
        if (inputActions != null)
        {
            // Try to find an existing ESC action, or create a reference to keyboard escape
            var uiMap = inputActions.FindActionMap("UI");
            if (uiMap != null)
            {
                escapeAction = uiMap.FindAction("Cancel");
            }
        }
    }

    void OnEnable()
    {
        if (escapeAction != null)
        {
            escapeAction.Enable();
            escapeAction.performed += OnEscapePressed;
        }
    }

    void OnDisable()
    {
        if (escapeAction != null)
        {
            escapeAction.performed -= OnEscapePressed;
            escapeAction.Disable();
        }
    }

    void OnEscapePressed(InputAction.CallbackContext context)
    {
        // Toggle fullscreen mode
        Screen.fullScreen = !Screen.fullScreen;
    }
    
    // Alternative Update method if Input Actions aren't set up
    void Update()
    {
        // Fallback to keyboard check if Input Actions aren't configured
        if (escapeAction == null && Keyboard.current?.escapeKey.wasPressedThisFrame == true)
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }
}