using System.Collections;
using UnityEngine;

public class CoinPotion : MonoBehaviour
{
    [Header("Coin Multiplier Settings")]
    public float multiplierDuration = 7f;   // how long the x2 multiplier lasts
    public float coinMultiplier = 2f;       // the multiplier value (x2)

    [Header("Audio")]
    public AudioClip potionPickupSFX;       // assign in Inspector
    private static AudioSource audioSource;

    void Start()
    {
        // Ensure there is a shared AudioSource for potion sounds
        if (audioSource == null)
        {
            GameObject audioObj = new GameObject("CoinPotionAudioSource");
            audioSource = audioObj.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (CoinsManager.Instance != null)
            {
                // Show floating popup indicating multiplier active
                CoinPopup.CreateMultiplier(other.transform.position, coinMultiplier, multiplierDuration);

                // Apply the coin multiplier buff
                CoinsManager.Instance.ApplyCoinMultiplier(coinMultiplier, multiplierDuration);
            }
            else
            {
                Debug.LogWarning("CoinPotion: CoinsManager.Instance is null! Coin multiplier not applied.");
            }

            // Play sound
            if (potionPickupSFX != null && audioSource != null)
                audioSource.PlayOneShot(potionPickupSFX);

            Destroy(gameObject);
        }
    }
}
