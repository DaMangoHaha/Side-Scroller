using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerFreeMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private bool facingRight = true; // track which way the player is facing

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Left/right input
        float moveX = Input.GetAxisRaw("Horizontal");
        moveInput = new Vector2(moveX, 0f).normalized;

        // Animation updates
        if (anim != null)
        {
            anim.SetFloat("horizontalSpeed", Mathf.Abs(moveX));
        }

        // Flip sprite if direction changes
        if (moveX > 0 && !facingRight)
            Flip();
        else if (moveX < 0 && facingRight)
            Flip();

        // Interaction placeholders
        if (Input.GetKeyDown(KeyCode.W))
            Debug.Log("Interact Up");
        else if (Input.GetKeyDown(KeyCode.S))
            Debug.Log("Interact Down");
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1; // flip the X axis
        transform.localScale = scale;
    }
}

