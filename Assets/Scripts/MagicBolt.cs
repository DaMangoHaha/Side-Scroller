using UnityEngine;

/// <summary>
/// Magic Bolt — The most standard projectile in Level 6.
/// Flies from the right side of the screen to the left on a random Y position
/// within a configurable range, forcing the player to either jump or slide.
/// Deals flat damage on hit with no secondary effects.
/// </summary>
public class MagicBolt : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 7f;

    [Header("Damage")]
    public float damage = 15f;

    [Header("Spawn Y Range (set by spawner or inspector)")]
    [Tooltip("Minimum Y position this bolt can spawn at")]
    public float minY = -2f;
    [Tooltip("Maximum Y position this bolt can spawn at")]
    public float maxY = 2f;

    // Internal
    private float actualDamage;
    private float actualSpeed;
    private bool hasHit = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D boltCollider;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boltCollider = GetComponent<Collider2D>();
        ApplyDifficultyModifiers();
    }

    void Start()
    {
        // Randomise Y position within the configured range
        Vector3 pos = transform.position;
        pos.y = Random.Range(minY, maxY);
        transform.position = pos;
    }

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
        // Move left
        transform.position += Vector3.left * actualSpeed * Time.deltaTime;

        // Destroy when off-screen left
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

            // Check for Selene's Charm buff first
            SeleneSkill selene = collision.gameObject.GetComponent<SeleneSkill>();
            if (selene != null && selene.TryCharmObstacle(gameObject))
                return; // obstacle charmed — no damage

            // Check invulnerability via reflection (matches existing pattern)
            var invulnerableField = energy.GetType().GetField("isInvulnerable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            bool isInvulnerable = false;
            if (invulnerableField != null)
                isInvulnerable = (bool)invulnerableField.GetValue(energy);

            if (isInvulnerable)
            {
                Debug.Log("Magic Bolt hit ignored — player is invulnerable!");
                return;
            }

            hasHit = true;
            energy.TakeDamage(actualDamage);

            // 25% chance to inflict Cursed debuff
            StatusEffectManager statusEffects = collision.gameObject.GetComponent<StatusEffectManager>();
            if (statusEffects != null)
            {
                statusEffects.TryApplyCursed(collision.transform.position);
            }

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 0.3f;
                spriteRenderer.color = c;
            }

            if (boltCollider != null)
                boltCollider.enabled = false;

            SoundManager.Instance.PlaySound2D("Damage");
        }
    }
}
