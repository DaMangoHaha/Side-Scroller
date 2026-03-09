using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlide : MonoBehaviour
{
    [Header("Slide Settings")]
    public float slideDuration = 0.5f;
    public float slideYScale = 0.5f; // how "short" the player looks
    public Vector2 slideColliderSize = new Vector2(1f, 0.5f);
    public Vector2 slideColliderOffset = new Vector2(0f, -0.25f);

    [Header("Input (New Input System)")]
    public InputActionReference slideActionRef; // optional: assign an action from an Input Actions asset

    private InputAction slideAction;
    private bool createdLocalAction = false;

    private bool isSliding = false;
    private float originalYScale;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    private BoxCollider2D boxCollider;
    private Transform playerTransform;
    private StatusEffectManager statusEffects;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.1f;
    public LayerMask groundLayer;
    private bool isGrounded;

    void Start()
    {
        playerTransform = transform;
        boxCollider = GetComponent<BoxCollider2D>();
        statusEffects = GetComponent<StatusEffectManager>();

        if (boxCollider != null)
        {
            originalColliderSize = boxCollider.size;
            originalColliderOffset = boxCollider.offset;
        }

        originalYScale = playerTransform.localScale.y;
    }

    void OnEnable()
    {
        // Use provided InputActionReference if available, otherwise create a simple fallback action
        if (slideActionRef != null && slideActionRef.action != null)
        {
            slideAction = slideActionRef.action;
        }
        else
        {
            slideAction = new InputAction("Slide", InputActionType.Button);
            slideAction.AddBinding("<Keyboard>/leftCtrl");
            slideAction.AddBinding("<Keyboard>/s");
            slideAction.AddBinding("<Gamepad>/buttonWest");
            createdLocalAction = true;
        }

        if (slideAction != null)
        {
            slideAction.performed += OnSlidePerformed;
            slideAction.Enable();
        }
    }

    void OnDisable()
    {
        if (slideAction != null)
        {
            slideAction.performed -= OnSlidePerformed;
            slideAction.Disable();
        }

        if (createdLocalAction && slideAction != null)
        {
            slideAction.Dispose();
            slideAction = null;
            createdLocalAction = false;
        }
    }

    private void OnSlidePerformed(InputAction.CallbackContext ctx)
    {
        // Apply Soggy input delay if active
        if (statusEffects != null && statusEffects.isSoggy)
        {
            StartCoroutine(DelayedSlide(statusEffects.GetInputDelay()));
            return;
        }

        TrySlide();
    }

    // Called by mobile UI virtual button via UICanvasControllerInput2
    public void SlideInput(bool pressed)
    {
        if (pressed)
        {
            // Apply Soggy input delay if active
            if (statusEffects != null && statusEffects.isSoggy)
            {
                StartCoroutine(DelayedSlide(statusEffects.GetInputDelay()));
                return;
            }

            TrySlide();
        }
    }

    private IEnumerator DelayedSlide(float delay)
    {
        yield return new WaitForSeconds(delay);
        TrySlide();
    }

    private void TrySlide()
    {
        if (isGrounded && !isSliding)
        {
            StartCoroutine(Slide());
        }
    }

    void Update()
    {
        // check if grounded (guard null)
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        else
            isGrounded = false;

        // legacy input fallback (kept for convenience if new system isn't wired in the editor)
        // This can be removed once all builds use the new input system.
        if (Keyboard.current != null && Keyboard.current.leftCtrlKey.wasPressedThisFrame && isGrounded && !isSliding)
        {
            StartCoroutine(Slide());
        }
    }

    private IEnumerator Slide()
    {
        isSliding = true;

        // shrink player vertically
        Vector3 scale = playerTransform.localScale;
        playerTransform.localScale = new Vector3(scale.x, slideYScale, scale.z);

        // adjust collider
        if (boxCollider != null)
        {
            boxCollider.size = slideColliderSize;
            boxCollider.offset = slideColliderOffset;
        }

        yield return new WaitForSeconds(slideDuration);

        // restore size
        playerTransform.localScale = new Vector3(scale.x, originalYScale, scale.z);

        if (boxCollider != null)
        {
            boxCollider.size = originalColliderSize;
            boxCollider.offset = originalColliderOffset;
        }

        isSliding = false;
    }
}



