using UnityEngine;

public class CupidCoin : MonoBehaviour
{
    private int coinValue = 1;   // how many cupid coins to add
    private int scoreValue = 1;  // how many score points to add

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Add cupid coins to counter
            if (CupidCoinsManager.Instance != null)
                CupidCoinsManager.Instance.AddCoins(coinValue);
            SoundManager.Instance.PlaySound2D("Coin");

            // Add to per-level score
            PlayerScore score = other.GetComponent<PlayerScore>();
            if (score != null)
                score.AddScore(scoreValue);

            // Reduce cooldown if ThiefSkill is active on the player
            ThiefSkill thiefSkill = other.GetComponent<ThiefSkill>();
            if (thiefSkill != null)
            {
                thiefSkill.ReduceCooldown(1f); // -1s cooldown per coin collected
            }

            // Remove coin
            Destroy(gameObject);
        }
    }
}