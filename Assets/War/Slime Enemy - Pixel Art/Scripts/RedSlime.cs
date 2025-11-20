using UnityEngine;

public class RedSlime : SlimeBase
{
    [Header("Red Slime Settings")]
    public float jumpForce = 6f;
    public float jumpInterval = 2f; // time between jumps
    public float jumpCooldown = 0f;

    protected override void Awake()
    {
        base.Awake();

        if (rb == null)
            Debug.LogError($"{name}: Rigidbody2D missing!");

        // Recommended in Inspector:
        // rb.gravityScale = 1
        // Freeze rotation Z
    }

    protected override void DoBehavior()
    {
        jumpCooldown -= Time.deltaTime;

        if (jumpCooldown <= 0f)
        {
            Jump();
            jumpCooldown = jumpInterval; // reset timer
        }
    }

    void Jump()
    {
        if (isDead) return;

        // Play jump animation
        if (anim != null)
            anim.SetTrigger("Jump");

        // Apply upward force
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // reset vertical speed
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}

