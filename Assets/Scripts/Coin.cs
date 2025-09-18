using UnityEngine;

public class Coin : MonoBehaviour
{
    public enum CoinType { Bronze, Silver, Gold }
    public CoinType coinType;

    private int scoreValue;  // per-level score
    private int coinValue;   // global currency

    [Header("Audio")]
    public AudioClip coinPickupSFX;  // assign in Inspector
    private static AudioSource audioSource;

    void Start()
    {
        // Setup values for both score and global coins
        switch (coinType)
        {
            case CoinType.Bronze:
                scoreValue = 1;
                coinValue = 1;
                break;
            case CoinType.Silver:
                scoreValue = 3;
                coinValue = 2;
                break;
            case CoinType.Gold:
                scoreValue = 5;
                coinValue = 3;
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

            // Add to per-level score
            PlayerScore score = other.GetComponent<PlayerScore>();
            if (score != null)
                score.AddScore(scoreValue);

            // Play pickup sound
            if (coinPickupSFX != null && audioSource != null)
                audioSource.PlayOneShot(coinPickupSFX);

            // Destroy the coin after pickup
            Destroy(gameObject);
        }
    }
}

