using UnityEngine;

public class SlimeBase : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Combat")]
    public float damage = 20f;
    protected bool hasHitPlayer = false;
    protected bool isDead = false;

    [Header("Components")]
    protected Animator anim;
    protected Collider2D col;
    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rb;

    [Header("Death Settings")]
    public float fadeAfterDeathDelay = 0.5f;
    public float destroyDelay = 1.2f;
    public AudioClip hitSoundSFX;  // assign in Inspector
    public static AudioSource audioSource;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void Update()
    {
        if (!isDead)
        {
            MoveLeft();
            DoBehavior(); // overridden by child slimes
        }

        // Destroy when off screen
        if (transform.position.x < -15f)
            Destroy(gameObject);
    }

    // ------------------------------
    // Movement
    // ------------------------------
    protected virtual void MoveLeft()
    {
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;
    }

    // ------------------------------
    // Slime-Specific Behavior (Override)
    // ------------------------------
    protected virtual void DoBehavior()
    {
        // Green slime = no behavior
        // Red slime = jump cycle
        // Blue slime = random jump / idle
    }

    // ------------------------------
    // Collision Logic
    // ------------------------------
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || hasHitPlayer)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerEnergy player = collision.gameObject.GetComponent<PlayerEnergy>();
            if (player != null)
            {
                player.TakeDamage(damage);
                hasHitPlayer = true;
                OnHitPlayer();  // Hook for derived classes
            }
        }
    }

    protected virtual void OnHitPlayer()
    {
        // Default: stun/die animation
        Die();
    }

    // ------------------------------
    // Death Logic
    // ------------------------------
    public virtual void Die()
    {
        if (isDead) return;

        isDead = true;

        // Disable collider so player passes through afterward
        if (col != null)
            col.enabled = false;

        // Play death animation if available
        if (anim != null)
            anim.SetTrigger("Death");

        // Fade out and destroy
        StartCoroutine(FadeAndDestroy());
    }

    private System.Collections.IEnumerator FadeAndDestroy()
    {
        yield return new WaitForSeconds(fadeAfterDeathDelay);

        float fadeTime = 0.5f;
        float t = 0;

        Color startColor = spriteRenderer.color;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}

