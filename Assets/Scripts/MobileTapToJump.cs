using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to any GameObject in a level scene.
/// On mobile, tapping the screen (outside of UI elements) triggers a jump
/// via the DoubleJump component on the player.
/// On non-mobile platforms this component destroys itself automatically.
/// </summary>
public class MobileTapToJump : MonoBehaviour
{
#if UNITY_IOS || UNITY_ANDROID

    [Header("Player Reference")]
    [Tooltip("Assign the player's DoubleJump component. If left empty, it will be found automatically at Start.")]
    public DoubleJump doubleJump;

    [Header("Settings")]
    [Tooltip("Minimum swipe distance (in pixels) to ignore — prevents jumping when the player is dragging the joystick.")]
    public float swipeThreshold = 30f;

    private Vector2 touchStartPos;

    void Start()
    {
        if (doubleJump == null)
        {
            // Try to find the player's DoubleJump component in the scene
            doubleJump = Object.FindFirstObjectByType<DoubleJump>();
        }

        if (doubleJump == null)
        {
            Debug.LogWarning("MobileTapToJump: No DoubleJump component found. Disabling.");
            enabled = false;
        }
    }

    void Update()
    {
        if (doubleJump == null) return;

        var touchscreen = Touchscreen.current;
        if (touchscreen == null) return;

        var primaryTouch = touchscreen.primaryTouch;

        // Detect the moment a touch begins
        if (primaryTouch.press.wasPressedThisFrame)
        {
            touchStartPos = primaryTouch.position.ReadValue();
        }

        // Detect the moment a touch ends (finger lifted)
        if (primaryTouch.press.wasReleasedThisFrame)
        {
            Vector2 touchEndPos = primaryTouch.position.ReadValue();

            // Ignore if the finger moved too far (likely a joystick drag)
            if (Vector2.Distance(touchStartPos, touchEndPos) > swipeThreshold)
                return;

            // Ignore taps that land on UI elements (buttons, joystick, etc.)
            if (IsPointerOverUI(touchEndPos))
                return;

            doubleJump.JumpInput(true);
        }
    }

    /// <summary>
    /// Returns true if the given screen position is over a UI element.
    /// </summary>
    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }

#else

    // Non-mobile platforms — remove this component automatically
    void Start()
    {
        Destroy(this);
    }

#endif
}
