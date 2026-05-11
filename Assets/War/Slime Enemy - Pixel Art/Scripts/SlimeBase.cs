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

    // Cached difficulty bonuses (applied on spawn)
    protected float actualDamage;
    protected float actualMoveSpeed;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
            Debug.LogError($"{name}: No Rigidbody2D found! Add one to the slime prefab.");

        // Apply difficulty modifiers on spawn
        ApplyDifficultyModifiers();
    }

    /// <summary>
    /// Applies current difficulty bonuses to this enemy's stats.
    /// </summary>
    protected virtual void ApplyDifficultyModifiers()
    {
        actualDamage = damage;
        actualMoveSpeed = moveSpeed;

        if (DifficultyManager.Instance != null)
        {
            actualDamage += DifficultyManager.Instance.bonusDamage;
            actualMoveSpeed += DifficultyManager.Instance.bonusSpeed;
        }
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
        // Use modified speed
        transform.position += Vector3.left * actualMoveSpeed * Time.deltaTime;
    }

    // ------------------------------
    // Slime-Specific Behavior (Override)
    // ------------------------------
    protected virtual void DoBehavior()
    {
        // Green slime = no behavior
        // Red slime = faster green slime
        // Blue slime = spawn from sky (handled in BlueSlime.cs)
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
                // Check for Selene's Charm buff first
                SeleneSkill selene = collision.gameObject.GetComponent<SeleneSkill>();
                if (selene != null && selene.TryCharmObstacle(gameObject))
                    return; // obstacle charmed — no damage

                // Use modified damage
                player.TakeDamage(actualDamage);
                hasHitPlayer = true;
                OnHitPlayer();  // Hook for derived classes
            }
        }
    }

    protected virtual void OnHitPlayer()
    {
        // Default: stun/die animation
        Die();
        SoundManager.Instance.PlaySound2D("Damage");
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

