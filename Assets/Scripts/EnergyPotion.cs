using UnityEngine;

public class EnergyPotion : MonoBehaviour
{
    public float pauseDuration = 5f;    // how long the energy depletion pauses
    public float restoreAmount = 25f;   // how much energy is restored instantly

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerEnergy energy = other.GetComponent<PlayerEnergy>();
            if (energy != null)
            {
                // Restore energy immediately
                energy.currentEnergy = Mathf.Clamp(energy.currentEnergy + restoreAmount, 0, energy.maxEnergy);
                energy.UpdateUI(); // make sure slider updates

                // Pause depletion
                energy.PauseDepletion(pauseDuration);
            }

            Destroy(gameObject); // remove potion after collection
        }
    }
}


