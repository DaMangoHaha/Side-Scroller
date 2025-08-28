using UnityEngine;

public class PlayerSlide : MonoBehaviour
{
    [Header("Slide Settings")]
    public float slideDuration = 0.5f;
    public float slideYScale = 0.5f; // how "short" the player looks
    public Vector2 slideColliderSize = new Vector2(1f, 0.5f);
    public Vector2 slideColliderOffset = new Vector2(0f, -0.25f);

    private bool isSliding = false;
    private float originalYScale;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    private BoxCollider2D boxCollider;
    private Transform playerTransform;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.1f;
    public LayerMask groundLayer;
    private bool isGrounded;

    void Start()
    {
        playerTransform = transform;
        boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider != null)
        {
            originalColliderSize = boxCollider.size;
            originalColliderOffset = boxCollider.offset;
        }

        originalYScale = playerTransform.localScale.y;
    }

    void Update()
    {
        // check if grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // slide input
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded && !isSliding)
        {
            StartCoroutine(Slide());
        }
    }

    private System.Collections.IEnumerator Slide()
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



