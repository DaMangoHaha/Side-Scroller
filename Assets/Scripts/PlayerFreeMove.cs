using UnityEngine;
using UnityEngine.InputSystem;

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
    public float interactRange = 1.5f;
    // how close player must be
    public LayerMask npcLayer;
    // assign "NPC" layer in Inspector

    [Header("Input (New Input System)")]
    public InputActionReference moveActionRef;
    // optional: assign a Vector2 action (left stick / WASD)
    public InputActionReference interactActionRef;
    // optional: assign a Button action (interact)

    private InputAction moveAction;
    private InputAction interactAction;
    private bool createdLocalMoveAction = false;
    private bool createdLocalInteractAction = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        // Setup move action (prefer assigned reference)
        if (moveActionRef != null && moveActionRef.action != null)
        {
            moveAction = moveActionRef.action;
        }
        else
        {
            moveAction = new InputAction("Move", InputActionType.Value);
            // composite for WASD/arrow keys
            moveAction.AddCompositeBinding("2DVector")
                .With("up", "<Keyboard>/w")
                .With("down", "<Keyboard>/s")
                .With("left", "<Keyboard>/a")
                .With("right", "<Keyboard>/d");
            // gamepad left stick
            moveAction.AddBinding("<Gamepad>/leftStick");
            createdLocalMoveAction = true;
        }

        if (moveAction != null)
        {
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
            moveAction.Enable();
        }

        // Setup interact action (prefer assigned reference)
        if (interactActionRef != null && interactActionRef.action != null)
        {
            interactAction = interactActionRef.action;
        }
        else
        {
            interactAction = new InputAction("Interact", InputActionType.Button);
            interactAction.AddBinding("<Keyboard>/w"); // keep legacy behaviour: W to interact
            interactAction.AddBinding("<Gamepad>/buttonSouth");
            createdLocalInteractAction = true;
        }

        if (interactAction != null)
        {
            interactAction.performed += OnInteractPerformed;
            interactAction.Enable();
        }
    }

    void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
            moveAction.Disable();
        }

        if (interactAction != null)
        {
            interactAction.performed -= OnInteractPerformed;
            interactAction.Disable();
        }

        if (createdLocalMoveAction && moveAction != null)
        {
            moveAction.Dispose();
            moveAction = null;
            createdLocalMoveAction = false;
        }

        if (createdLocalInteractAction && interactAction != null)
        {
            interactAction.Dispose();
            interactAction = null;
            createdLocalInteractAction = false;
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        Vector2 v = ctx.ReadValue<Vector2>();
        moveInput = new Vector2(v.x, 0f).normalized;
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        TryInteract();
    }

    void Update()
    {
        // If new input action is not assigned or enabled, fall back to legacy input
        if (moveAction == null || !moveAction.enabled)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            moveInput = new Vector2(moveX, 0f).normalized;

            if (anim != null)
                anim.SetFloat("horizontalSpeed", Mathf.Abs(moveX));

            if (moveX > 0 && !facingRight)
                Flip();
            else if (moveX < 0 && facingRight)
                Flip();
        }
        else
        {
            // Animation updates based on moveInput
            if (anim != null)
                anim.SetFloat("horizontalSpeed", Mathf.Abs(moveInput.x));

            if (moveInput.x > 0 && !facingRight)
                Flip();
            else if (moveInput.x < 0 && facingRight)
                Flip();
        }

        // Interaction legacy fallback (if action not enabled)
        if ((interactAction == null || !interactAction.enabled) && Keyboard.current != null)
        {
            if (Keyboard.current.wKey.wasPressedThisFrame)
                TryInteract();
        }
    }

    void FixedUpdate()
    {
        // Apply horizontal movement
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
            // Check for normal NPCs first
            NPC npcDialogue = hit.collider.GetComponent<NPC>();
            if (npcDialogue != null)
            {
                npcDialogue.StartConversation();
                Debug.Log("Talked to NPC: " + npcDialogue.name);
                return;
            }

            SpecialNPC special = hit.collider.GetComponent<SpecialNPC>();
            if (special != null)
            {
                special.StartSpecialConversation();
                Debug.Log("Interacted with SPECIAL NPC: " + special.name);
                return;
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

    // Added for UI virtual controls for mobile
    public void MoveInput(Vector2 direction)
    {
        // update movement input from virtual joystick/buttons
        moveInput = direction;

        // keep facing direction consistent with horizontal input
        if (direction.x > 0f && !facingRight) Flip();
        else if (direction.x < 0f && facingRight) Flip();
    }

    public void InteractInput(bool pressed)
    {
        // trigger interaction when UI button is pressed
        if (pressed)
        {
            TryInteract();
        }
    }
}
