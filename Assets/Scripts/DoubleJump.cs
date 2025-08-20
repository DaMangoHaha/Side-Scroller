using UnityEngine;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Check if player is on the ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // Reset jump count when grounded
        if (isGrounded)
        {
            jumpsUsed = 0;
        }

        // Jump or double jump
        if (Input.GetKeyDown(KeyCode.Space) && jumpsUsed < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // reset vertical speed
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpsUsed++;
        }

        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("verticalVelocity", rb.linearVelocity.y);

    }
}


