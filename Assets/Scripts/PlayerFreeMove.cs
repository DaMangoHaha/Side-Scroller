using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerFreeMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private bool facingRight = true;

    [Header("Interaction")]
    public float interactRange = 1.5f; // how close player must be
    public LayerMask npcLayer;         // assign "NPC" layer in Inspector

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
            anim.SetFloat("horizontalSpeed", Mathf.Abs(moveX));

        // Flip sprite if direction changes
        if (moveX > 0 && !facingRight)
            Flip();
        else if (moveX < 0 && facingRight)
            Flip();

        // Interaction input
        if (Input.GetKeyDown(KeyCode.W))
            TryInteract();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void TryInteract()
    {
        // Checks for any NPC within a small radius in front of player
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Vector2 origin = transform.position;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, interactRange, npcLayer);
        if (hit.collider != null)
        {
            NPC npc = hit.collider.GetComponent<NPC>();
            if (npc != null)
            {
                npc.Interact();
                Debug.Log("Interacted with " + npc.name);
            }
        }
        else
        {
            Debug.Log("No NPC nearby to interact with.");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize interaction ray in Scene view with a yellow line. Select the player in the hierarchy, then enable Gizmos, then start the game to see it.
        Gizmos.color = Color.yellow;
        Vector3 dir = facingRight ? Vector3.right : Vector3.left;
        Gizmos.DrawLine(transform.position, transform.position + dir * interactRange);
    }
}
