using UnityEngine;

public class Coin : MonoBehaviour
{
    public enum CoinType { Bronze, Silver, Gold }
    public CoinType coinType;

    private int coinValue;   // adds to global CoinsManager
    private int scoreValue;  // adds to in-level score

    [Header("Audio")]
    public AudioClip coinPickupSFX;  // assign in Inspector
    private static AudioSource audioSource;

    void Start()
    {
        // Assign values
        switch (coinType)
        {
            case CoinType.Bronze:
                coinValue = 1;
                scoreValue = 1;
                break;
            case CoinType.Silver:
                coinValue = 2;
                scoreValue = 3;
                break;
            case CoinType.Gold:
                coinValue = 3;
                scoreValue = 5;
                break;
        }

        // Reuse one AudioSource for all coins
        if (audioSource == null)
        {
            GameObject audioObj = new GameObject("CoinAudioSource");
            audioSource = audioObj.AddComponent<AudioSource>();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Add to global coin counter
            if (CoinsManager.Instance != null)
                CoinsManager.Instance.AddCoins(coinValue);

            // Add to score
            PlayerScore score = other.GetComponent<PlayerScore>();
            if (score != null)
                score.AddScore(scoreValue);

            // Play sound
            if (coinPickupSFX != null && audioSource != null)
                audioSource.PlayOneShot(coinPickupSFX);

            // Destroy coin
            Destroy(gameObject);
        }
    }
}

