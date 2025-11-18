using UnityEngine;

public class RedSlime : SlimeBase
{
    [Header("Red Slime Settings")]
    public float jumpForce = 6f;      // upward jump strength
    public float jumpInterval = 2.5f; // time between jumps
    private float jumpTimer = 0f;

    private bool isJumping = false;

    protected override void Awake()
    {
        base.Awake();
        anim.SetTrigger("Idle");
    }

    protected override void DoBehavior()
    {
        jumpTimer += Time.deltaTime;

        // Ready to jump again?
        if (!isJumping && jumpTimer >= jumpInterval)
        {
            Jump();
            jumpTimer = 0f;
        }
    }

    void Jump()
{
    isJumping = true;

    // Trigger jump animation
    anim.SetTrigger("Jump");

    // Apply upward force
    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
    rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

    StartCoroutine(ResetJumpState());
}


    private System.Collections.IEnumerator ResetJumpState()
    {
        yield return new WaitForSeconds(0.6f);
        isJumping = false;
        anim.SetTrigger("Idle"); // go back to idle between jumps
    }

    protected override void OnHitPlayer()
    {
        // Red slime dies on hit
        Die();
    }
}

