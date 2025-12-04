using UnityEngine;

public class Spike : MonoBehaviour
{
    public float damage = 20f;   // how much energy it removes
    public float speed = 5f;     // scroll speed
    private bool hasHit = false; // prevent multiple hits
    private SpriteRenderer spriteRenderer;
    private Collider2D spikeCollider;

    [Header("Audio")]
    public AudioClip hitSoundSFX;  // assign in Inspector
    private static AudioSource audioSource;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spikeCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        // Move left
        transform.position += Vector3.left * speed * Time.deltaTime;

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
                Debug.Log("Spike hit ignored — player is invulnerable!");
                return;
            }

            hasHit = true;

            // Damage player
            energy.TakeDamage(damage);

            // Spike turns transparent and disables collider
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 0.3f;
                spriteRenderer.color = c;
            }

            if (spikeCollider != null)
            {
                spikeCollider.enabled = false;
            }

            // Play hit sound
            if (audioSource == null)
            {
                GameObject audioObj = new GameObject("HitAudioSource");
                audioSource = audioObj.AddComponent<AudioSource>();
            }

            if (hitSoundSFX != null && audioSource != null)
                audioSource.PlayOneShot(hitSoundSFX);
        }
    }
}
