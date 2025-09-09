using UnityEngine;

public class Coin : MonoBehaviour
{
    public enum CoinType { Bronze, Silver, Gold }
    public CoinType coinType;

    private int scoreValue;

    [Header("Audio")]
    public AudioClip coinPickupSFX;  // assign in Inspector
    private static AudioSource audioSource;

    void Start()
    {
        // Setup score values
        switch (coinType)
        {
            case CoinType.Bronze:
                scoreValue = 1;
                break;
            case CoinType.Silver:
                scoreValue = 3;
                break;
            case CoinType.Gold:
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
            // Add score
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


