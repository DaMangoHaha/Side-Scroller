using UnityEngine;

public class Alien : MonoBehaviour
{
    public float damage = 30f;   // how much energy it removes
    public float speed = 3f;     // scroll speed
    private bool hasHit = false; // prevent multiple hits
    private SpriteRenderer spriteRenderer;
    private Collider2D alienCollider;

    // Cached difficulty bonuses (applied on spawn)
    private float actualDamage;
    private float actualSpeed;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        alienCollider = GetComponent<Collider2D>();

        // Apply difficulty modifiers on spawn
        ApplyDifficultyModifiers();
    }

    /// <summary>
    /// Applies current difficulty bonuses to this enemy's stats.
    /// </summary>
    private void ApplyDifficultyModifiers()
    {
        actualDamage = damage;
        actualSpeed = speed;

        if (DifficultyManager.Instance != null)
        {
            actualDamage += DifficultyManager.Instance.bonusDamage;
            actualSpeed += DifficultyManager.Instance.bonusSpeed;
        }
    }

    void Update()
    {
        // Move left using modified speed
        transform.position += Vector3.left * actualSpeed * Time.deltaTime;

        // Destroy when off-screen
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasHit && collision.gameObject.CompareTag("Player"))
        {
            PlayerEnergy energy = collision.gameObject.GetComponent<PlayerEnergy>();
            if (energy == null) return;

            // Check if player is invulnerable before doing anything
            var invulnerableField = energy.GetType().GetField("isInvulnerable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            bool isInvulnerable = false;
            if (invulnerableField != null)
                isInvulnerable = (bool)invulnerableField.GetValue(energy);

            // If player is invulnerable, ignore hit entirely
            if (isInvulnerable)
            {
                Debug.Log("Alien hit ignored — player is invulnerable!");
                return;
            }

            hasHit = true;

            // Damage player using modified damage
            energy.TakeDamage(actualDamage);

            // Spike turns transparent and disables collider
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 0.3f;
                spriteRenderer.color = c;
            }

            if (alienCollider != null)
            {
                alienCollider.enabled = false;
            }
            SoundManager.Instance.PlaySound2D("Damage");
        }
    }
}


