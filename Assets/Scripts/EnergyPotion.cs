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
        // Also re-create if the old one was destroyed during a scene change
        if (audioSource == null)
        {
            GameObject audioObj = new GameObject("PotionAudioSource");
            audioSource = audioObj.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[EnergyPotion] OnTriggerEnter2D hit by: '{other.gameObject.name}', Tag: '{other.tag}', " +
                  $"HasPlayerEnergy (self): {other.GetComponent<PlayerEnergy>() != null}, " +
                  $"HasPlayerEnergy (parent): {other.GetComponentInParent<PlayerEnergy>() != null}");

        if (other.CompareTag("Player"))
        {
            PlayerEnergy energy = other.GetComponentInParent<PlayerEnergy>();
            if (energy == null)
                energy = other.GetComponentInChildren<PlayerEnergy>();

            if (energy != null)
            {
                // Show floating energy popup
                CoinPopup.CreateEnergy(other.transform.position, restoreAmount);

                // Restore energy immediately
                energy.currentEnergy = Mathf.Clamp(energy.currentEnergy + restoreAmount, 0, energy.maxEnergy);
                energy.UpdateUI();

                // Pause depletion
                energy.PauseDepletion(pauseDuration);

                Debug.Log($"[EnergyPotion] SUCCESS — restored {restoreAmount} energy to '{other.gameObject.name}'");
            }
            else
            {
                Debug.LogWarning($"[EnergyPotion] FAILED — PlayerEnergy not found on '{other.gameObject.name}' or any parent/child! " +
                                 $"Root object: '{other.transform.root.gameObject.name}'");
            }

            // Play sound
            if (potionPickupSFX != null && audioSource != null)
                audioSource.PlayOneShot(potionPickupSFX);

            Destroy(gameObject);
        }
    }
}



