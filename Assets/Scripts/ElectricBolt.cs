using UnityEngine;
using UnityEngine.SceneManagement;

public class ElectricBolt : MonoBehaviour
{
    [Header("Bolt Movement")]
    public float speed = 12f;
    public float destroyX = 15f;

    [Header("Scoring")]
    public int pointsPerKill = 50;

    [Header("Upgrade Support")]
    [HideInInspector] public NinjaSkill_ElectricBolt ownerSkill; // set by NinjaSkill on spawn
    [HideInInspector] public bool isLargeBolt = false;           // Tier 2 large bolt

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
            // Play electric zap sound only when destroying an obstacle
            if (collision.CompareTag("Obstacle") && SoundManager.Instance != null)
                SoundManager.Instance.PlaySound2D("ElectricZap");

            // Award score
            PlayerScore score = FindAnyObjectByType<PlayerScore>();
            if (score != null)
                score.AddScore(pointsPerKill);

            // Notify owner skill of the kill (Tier 1: cooldown reduction)
            if (ownerSkill != null)
                ownerSkill.OnBoltKill();

            // Destroy the enemy/obstacle
            Destroy(collision.gameObject);

            // Large bolt (Tier 2) does NOT destroy itself on collision
            if (!isLargeBolt)
            {
                Destroy(gameObject);
            }
        }
    }
}

