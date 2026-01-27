using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CubitPassive : MonoBehaviour
{
    [Header("Protection Protocol Settings")]
    public float energyThreshold = 20f;           // Trigger when energy is below this percentage
    public float protectionDuration = 5f;         // Duration of energy pause
    public float protectionCooldown = 45f;        // Cooldown before can trigger again
    public float damageReduction = 0.75f;         // Player takes 75% damage
    public float damageStorage = 0.25f;           // Store 25% of damage
    public float energyConversionRate = 0.10f;    // Convert 10% of stored damage to energy

    [Header("UI")]
    public Image cubitIcon; // drag the CubitIcon here in Inspector
    private Color inactiveColor;
    private Color activeColor;

    [Header("Visual Feedback")]
    public GameObject protectionEffectPrefab;     // Optional shield VFX
    
    private PlayerEnergy playerEnergy;
    private bool isProtectionReady = true;
    private bool isProtectionActive = false;
    private float storedDamage = 0f;
    private GameObject activeEffect;

    void Start()
    {
        playerEnergy = GetComponent<PlayerEnergy>();
        
        if (playerEnergy == null)
        {
            Debug.LogError("CubitPassive requires PlayerEnergy component!");
            enabled = false;
            return;
        }

        if (cubitIcon != null)
        {
            activeColor = cubitIcon.color;
            inactiveColor = cubitIcon.color;
            inactiveColor.a = 0.2f; // faded look
            cubitIcon.color = inactiveColor;
        }
    }

    void Update()
    {
        if (playerEnergy == null) return;

        // Check if energy drops below threshold and protocol is ready
        float energyPercent = (playerEnergy.currentEnergy / playerEnergy.maxEnergy) * 100f;
        
        if (energyPercent < energyThreshold && isProtectionReady && !isProtectionActive)
        {
            StartCoroutine(ActivateProtectionProtocol());
        }
    }

    private IEnumerator ActivateProtectionProtocol()
    {
        isProtectionActive = true;
        isProtectionReady = false;
        storedDamage = 0f;

        Debug.Log("Protection Protocol ACTIVATED!");

        // Pause energy depletion
        playerEnergy.PauseDepletion(protectionDuration);

        // Keep icon at active color during protection
        if (cubitIcon != null)
            cubitIcon.color = activeColor;

        // Spawn protection effect
        if (protectionEffectPrefab != null)
            activeEffect = Instantiate(protectionEffectPrefab, transform.position, Quaternion.identity, transform);

        // Play sound effect if available
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("ProtectionProtocol");

        // Wait for protection duration
        yield return new WaitForSeconds(protectionDuration);

        // Convert stored damage to energy
        if (storedDamage > 0)
        {
            float energyToRestore = storedDamage * energyConversionRate;
            playerEnergy.RestoreEnergy(energyToRestore);
            Debug.Log($"Protection Protocol ended. Converted {storedDamage:F1} stored damage to {energyToRestore:F1} energy!");
        }
        else
        {
            Debug.Log("Protection Protocol ended. No damage was stored.");
        }

        // Clean up effect
        if (activeEffect != null)
            Destroy(activeEffect);

        isProtectionActive = false;

        // Set icon to inactive (faded) state during cooldown
        if (cubitIcon != null)
            cubitIcon.color = inactiveColor;

        // Start cooldown
        yield return new WaitForSeconds(protectionCooldown);

        // Ready again - set to active color
        isProtectionReady = true;
        if (cubitIcon != null)
            cubitIcon.color = activeColor;

        Debug.Log("Protection Protocol is ready again!");
    }

    // This method intercepts damage and applies Protection Protocol logic
    public float ProcessDamage(float incomingDamage)
    {
        if (!isProtectionActive)
            return incomingDamage; // No protection active, take full damage

        // Store 25% of the damage
        float damageToStore = incomingDamage * damageStorage;
        storedDamage += damageToStore;

        // Take 75% of the damage
        float damageToTake = incomingDamage * damageReduction;

        Debug.Log($"Protection Protocol: Reduced {incomingDamage:F1} damage to {damageToTake:F1}, stored {damageToStore:F1}");

        return damageToTake;
    }

    public bool IsProtectionActive()
    {
        return isProtectionActive;
    }

    public bool IsProtectionReady()
    {
        return isProtectionReady;
    }
}