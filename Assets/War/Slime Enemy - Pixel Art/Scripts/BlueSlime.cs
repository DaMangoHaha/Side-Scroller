using UnityEngine;

public class BlueSlime : SlimeBase
{
    [Header("Blue Slime Settings")]
    public float fallSpeed = 3f;      // diagonal downward movement speed
    public float horizontalFallSpeed = 2f; // slight left movement while falling
    public LayerMask groundLayer;     // assign "Ground" layer
    public float groundCheckDistance = 0.2f;

    private bool hasLanded = false;

    protected override void Awake()
    {
        base.Awake();
        anim.SetTrigger("Idle"); // starts as floating idle
        rb.gravityScale = 0;     // floating, so no gravity until landing
    }

    protected override void DoBehavior()
    {
        if (!hasLanded)
        {
            FallFromSky();
        }
        else
        {
            // Once landed, SlimeBase handles scrolling left
            // Optionally: Add walking animation here if you have one
        }
    }

    void FallFromSky()
    {
        // Move diagonally down-left
        Vector3 fallDirection = new Vector3(-horizontalFallSpeed, -fallSpeed, 0f);
        transform.position += fallDirection * Time.deltaTime;

        // Check if grounded
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        if (hit.collider != null)
        {
            Land();
        }
    }

    void Land()
    {
        hasLanded = true;

        // Snap to ground so it doesn't hover slightly above
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z);

        rb.gravityScale = 0; // keep controlled movement
        anim.SetTrigger("Idle"); // grounded idle state
    }

    protected override void OnHitPlayer()
    {
        Die();
        SoundManager.Instance.PlaySound2D("Damage");
    }
}
