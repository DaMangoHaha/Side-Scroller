using UnityEngine;

public class Coin : MonoBehaviour
{
    public enum CoinType { Bronze, Silver, Gold }
    public CoinType coinType;

    private int coinValue;   // how many coins to add
    private int scoreValue;  // how many score points to add

    [Header("Audio")]
    public AudioClip coinPickupSFX;  // assign in Inspector
    private static AudioSource audioSource;

    void Start()
    {
        // Set coin + score values
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

        // Shared AudioSource for all coins
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
            // Add coins to counter
            if (CoinsManager.Instance != null)
                CoinsManager.Instance.AddCoins(coinValue);

            // Add to per-level score
            PlayerScore score = other.GetComponent<PlayerScore>();
            if (score != null)
                score.AddScore(scoreValue);

            // Play pickup sound
            if (coinPickupSFX != null && audioSource != null)
                audioSource.PlayOneShot(coinPickupSFX);

            // Remove coin
            Destroy(gameObject);
        }
    }
}
