using UnityEngine;

/// <summary>
/// Twin Magic Bolt — A two-in-one projectile for Level 6.
/// Spawns at a random Y position within a configurable range to throw the player off.
/// On hit it rolls a 50/50 chance: either double damage or half damage.
/// </summary>
public class TwinMagicBolt : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;

    [Header("Damage")]
    [Tooltip("Base damage before the 50/50 roll")]
    public float damage = 15f;

    [Header("Spawn Y Range")]
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

            // Check for Selene's Charm buff first
            SeleneSkill selene = collision.gameObject.GetComponent<SeleneSkill>();
            if (selene != null && selene.TryCharmObstacle(gameObject))
                return; // obstacle charmed — no damage

            var invulnerableField = energy.GetType().GetField("isInvulnerable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            bool isInvulnerable = false;
            if (invulnerableField != null)
                isInvulnerable = (bool)invulnerableField.GetValue(energy);

            if (isInvulnerable)
            {
                Debug.Log("Twin Magic Bolt hit ignored — player is invulnerable!");
                return;
            }

            hasHit = true;

            // 50/50 roll: double damage or half damage
            float finalDamage;
            if (Random.value < 0.5f)
            {
                finalDamage = actualDamage * 2f;
                Debug.Log("Twin Magic Bolt — Double damage! (" + finalDamage + ")");
                CoinPopup.CreateStatusEffect(transform.position, "TWIN STRIKE!", new Color(1f, 0.3f, 0.9f));
            }
            else
            {
                finalDamage = actualDamage * 0.5f;
                Debug.Log("Twin Magic Bolt — Half damage! (" + finalDamage + ")");
                CoinPopup.CreateStatusEffect(transform.position, "Weak Twin...", new Color(0.6f, 0.6f, 1f));
            }

            energy.TakeDamage(finalDamage);

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
