using System.Collections;
using UnityEngine;

/// <summary>
/// Added to an obstacle (Spike or Alien) when Selene's Charm buff activates on it.
/// The obstacle reverses direction (moves right) and destroys the next obstacle it touches.
/// </summary>
public class CharmedObstacle : MonoBehaviour
{
    public float charmedSpeed = 5f;

    private bool activated = false;
    private Collider2D col;

    public void Activate()
    {
        activated = true;
        col = GetComponent<Collider2D>();

        // Tint pink to make charm visually obvious
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(1f, 0.4f, 0.85f);

        // Disable damage scripts so the charmed enemy no longer hurts the player
        Spike spike = GetComponent<Spike>();
        if (spike != null) spike.enabled = false;

        Alien alien = GetComponent<Alien>();
        if (alien != null) alien.enabled = false;

        // Re-enable the collider in case it was already disabled
        if (col != null)
            col.enabled = true;

        // Change layer to avoid hitting the player again
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    void Update()
    {
        if (!activated) return;

        // Move right (opposite of normal scroll direction)
        transform.position += Vector3.right * charmedSpeed * Time.deltaTime;

        // Destroy if off-screen to the right
        if (transform.position.x > 20f)
            Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!activated) return;

        // Ignore collisions with the player
        if (collision.gameObject.CompareTag("Player")) return;

        // Check if the collided object is an obstacle
        Spike spike = collision.gameObject.GetComponent<Spike>();
        Alien alien = collision.gameObject.GetComponent<Alien>();
        CharmedObstacle otherCharmed = collision.gameObject.GetComponent<CharmedObstacle>();

        if (spike != null || alien != null || otherCharmed != null)
        {
            Debug.Log("Charmed obstacle collided with another obstacle — both destroyed!");
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!activated) return;
        if (other.CompareTag("Player")) return;

        Spike spike = other.GetComponent<Spike>();
        Alien alien = other.GetComponent<Alien>();
        CharmedObstacle otherCharmed = other.GetComponent<CharmedObstacle>();

        if (spike != null || alien != null || otherCharmed != null)
        {
            Debug.Log("Charmed obstacle triggered on another obstacle — both destroyed!");
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
