using UnityEngine;

public class Spike : MonoBehaviour
{
    public float damage = 20f;   // how much energy it removes
    public float speed = 5f;     // scroll speed
    private bool hasHit = false; // prevent multiple hits
    private SpriteRenderer spriteRenderer;
    private Collider2D spikeCollider;

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
            hasHit = true;

            // Damage player
            PlayerEnergy energy = collision.gameObject.GetComponent<PlayerEnergy>();
            if (energy != null)
            {
                energy.TakeDamage(damage); // 👈 calls the flashing + energy loss
            }

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
        }
    }
}


