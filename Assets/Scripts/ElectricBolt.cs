using UnityEngine;
using UnityEngine.SceneManagement;

public class ElectricBolt : MonoBehaviour
{
    [Header("Bolt Movement")]
    public float speed = 12f;
    public float destroyX = 15f;

    [Header("Scoring")]
    public int pointsPerKill = 50;

    void Update()
    {
        // Move bolt to the right
        transform.position += Vector3.right * speed * Time.deltaTime;

        // Destroy bolt when off-screen
        if (transform.position.x >= destroyX)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if it's an enemy/obstacle
        if (collision.CompareTag("Enemy") || collision.CompareTag("Obstacle"))
        {
            // Award score
            PlayerScore score = FindAnyObjectByType<PlayerScore>();
            if (score != null)
                score.AddScore(pointsPerKill);


            // Destroy the enemy/obstacle
            Destroy(collision.gameObject);
        }
    }
}

