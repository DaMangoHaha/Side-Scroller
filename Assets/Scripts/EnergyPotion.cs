using UnityEngine;

public class EnergyPotion : MonoBehaviour
{
    public float pauseDuration = 5f;    // how long the energy depletion pauses
    public float restoreAmount = 25f;   // how much energy is restored instantly

    [Header("Audio")]
    public AudioClip potionPickupSFX;   // assign in Inspector
    private static AudioSource audioSource;

    void Start()
    {
        // Ensure there is a shared AudioSource for potion sounds
        if (audioSource == null)
        {
            GameObject audioObj = new GameObject("PotionAudioSource");
            audioSource = audioObj.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerEnergy energy = other.GetComponent<PlayerEnergy>();
            if (energy != null)
            {
                // Restore energy immediately
                energy.currentEnergy = Mathf.Clamp(energy.currentEnergy + restoreAmount, 0, energy.maxEnergy);
                energy.UpdateUI();

                // Pause depletion
                energy.PauseDepletion(pauseDuration);
            }

            // Play sound
            if (potionPickupSFX != null && audioSource != null)
                audioSource.PlayOneShot(potionPickupSFX);

            Destroy(gameObject);
        }
    }
}



