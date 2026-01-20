using UnityEngine;
using UnityEngine.InputSystem;

public class DoubleJump : MonoBehaviour
{
    public float jumpForce = 8f;
    public int maxJumps = 2;
    private int jumpsUsed = 0;

    public Transform groundCheck;
    public float groundRadius = 0.1f;
    public LayerMask groundLayer;
    private bool isGrounded;

    private Rigidbody2D rb;
    private Animator anim;

    [Header("Input (New Input System)")]
    public InputActionReference jumpActionRef; // optional: assign an action from an Input Actions asset
    private InputAction jumpAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Use provided InputActionReference if available, otherwise create a simple fallback action
        if (jumpActionRef != null && jumpActionRef.action != null)
        {
            jumpAction = jumpActionRef.action;
        }
        else
        {
            jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");
            jumpAction.AddBinding("<Gamepad>/buttonSouth");
        }
    }

    void OnEnable()
    {
        if (jumpAction != null)
        {
            jumpAction.performed += OnJumpPerformed;
            jumpAction.Enable();
        }
    }

    void OnDisable()
    {
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
            jumpAction.Disable();
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (jumpsUsed < maxJumps)
        {
            // reset vertical speed then apply impulse
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpsUsed++;
        }
        }

    void Update()
    {
        // Check if player is on the ground
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // Reset jump count when grounded
        if (isGrounded)
        {
            jumpsUsed = 0;
        }

        anim.SetBool("isGrounded", isGrounded);
        if (rb != null)
            anim.SetFloat("verticalVelocity", rb.linearVelocity.y);
    }
}
