using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerFreeMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // Left & Right movement
        float moveX = Input.GetAxisRaw("Horizontal");
        moveInput = new Vector2(moveX, 0f).normalized;

        // Update animator (optional)
        if (anim != null)
        {
            anim.SetBool("isGrounded", true); // always grounded in town
            anim.SetFloat("horizontalSpeed", Mathf.Abs(moveX));
        }
        // Interactions keys
        if (Input.GetKeyDown(KeyCode.W))
        {
            TryInteract(1); // up interaction
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            TryInteract(-1); // down interaction
        }
        void FixedUpdate()
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }
        void TryInteract(int direction)
        {
            // Cast a small ray forward/upward for interaction detection
            //RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up * direction, 1f, LayerMask.GetMask("NPC"));
           // if (hit.collider != null)
           //     hit.collider.GetComponent<NPCDialogue>()?.TriggerDialogue();
        }
    }
}
