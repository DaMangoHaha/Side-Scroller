using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 30f;   // how much energy it removes
    public float speed = 3f;     // scroll speed
    private bool hasHit = false; // prevent multiple hits
    private SpriteRenderer spriteRenderer;
    private Collider2D bulletCollider;

    [Header("Audio")]
    public AudioClip hitSoundSFX;  // assign in Inspector
    private static AudioSource audioSource;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bulletCollider = GetComponent<Collider2D>();
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
            hasHit = true;

            // Damage player
            PlayerEnergy energy = collision.gameObject.GetComponent<PlayerEnergy>();
            if (energy != null)
            {
                energy.TakeDamage(damage); // calls the flashing + energy loss
            }

            // Bullet turns transparent and disables collider
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 0.3f;
                spriteRenderer.color = c;
            }
            if (bulletCollider != null)
            {
                bulletCollider.enabled = false;
            }

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



