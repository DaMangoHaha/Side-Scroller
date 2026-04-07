using UnityEngine;

/// <summary>
/// Charged Magic Bolt — A deceptive projectile for Level 6.
/// Flies from right to left on a random Y position within a configurable range.
/// Accelerates the closer it gets to the player, creating a fake-out effect.
/// Deals flat damage on hit with no secondary effects.
/// </summary>
public class ChargedMagicBolt : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Initial speed of the bolt")]
    public float baseSpeed = 5f;

    [Tooltip("Maximum speed the bolt can reach when close to the player")]
    public float maxSpeed = 18f;

    [Tooltip("Distance at which the bolt begins accelerating toward the player")]
    public float accelerationRange = 8f;

    [Header("Damage")]
    public float damage = 20f;

    [Header("Spawn Y Range")]
    [Tooltip("Minimum Y position this bolt can spawn at")]
    public float minY = -2f;
    [Tooltip("Maximum Y position this bolt can spawn at")]
    public float maxY = 2f;

    // Internal
    private float actualDamage;
    private float actualBaseSpeed;
    private float actualMaxSpeed;
    private bool hasHit = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D boltCollider;
    private Transform playerTransform;

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

        // Cache player reference
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void ApplyDifficultyModifiers()
    {
        actualDamage = damage;
        actualBaseSpeed = baseSpeed;
        actualMaxSpeed = maxSpeed;

        if (DifficultyManager.Instance != null)
        {
            actualDamage += DifficultyManager.Instance.bonusDamage;
            actualBaseSpeed += DifficultyManager.Instance.bonusSpeed;
            actualMaxSpeed += DifficultyManager.Instance.bonusSpeed;
        }
    }

    void Update()
    {
        float currentSpeed = actualBaseSpeed;

        // Accelerate when within range of the player
        if (playerTransform != null)
        {
            float distToPlayer = Mathf.Abs(transform.position.x - playerTransform.position.x);

            if (distToPlayer < accelerationRange)
            {
                // Lerp from base to max speed as distance shrinks
                float t = 1f - (distToPlayer / accelerationRange);
                currentSpeed = Mathf.Lerp(actualBaseSpeed, actualMaxSpeed, t);
            }
        }

        // Move left
        transform.position += Vector3.left * currentSpeed * Time.deltaTime;

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

            var invulnerableField = energy.GetType().GetField("isInvulnerable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            bool isInvulnerable = false;
            if (invulnerableField != null)
                isInvulnerable = (bool)invulnerableField.GetValue(energy);

            if (isInvulnerable)
            {
                Debug.Log("Charged Magic Bolt hit ignored — player is invulnerable!");
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
