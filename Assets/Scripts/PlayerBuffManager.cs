using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Periodically grants the player a random buff during a level.
/// Every 90 seconds there is a 50% chance one of three buffs is applied:
///   1. Max Energy increased by 5%
///   2. Defense increased by 10%
///   3. Restore 25% of Max Energy
/// A green notification popup is shown when a buff is granted.
/// </summary>
public class PlayerBuffManager : MonoBehaviour
{
    public static PlayerBuffManager Instance { get; private set; }

    [Header("Buff Timing")]
    [Tooltip("Seconds between each potential buff roll")]
    public float buffInterval = 90f;

    [Tooltip("Chance (0-1) that the player actually receives a buff each interval")]
    [Range(0f, 1f)]
    public float buffChance = 0.5f;

    [Header("Buff Values")]
    [Tooltip("Percentage increase to max energy per buff (0.05 = 5%)")]
    public float maxEnergyBonus = 0.05f;

    [Tooltip("Percentage increase to defense per buff (0.10 = 10%)")]
    public float defenseBonus = 0.10f;

    [Tooltip("Percentage of max energy restored per buff (0.25 = 25%)")]
    public float restorePercent = 0.25f;

    [Header("Notification Settings")]
    [Tooltip("UI Text element for center-screen notifications (optional)")]
    public TextMeshProUGUI notificationText;

    [Tooltip("How long the notification stays on screen")]
    public float notificationDuration = 3f;

    [Tooltip("Color for player buff notifications")]
    public Color notificationColor = new Color(0.2f, 1f, 0.2f, 1f); // Green

    // Internal tracking
    private int buffLevel = 0;
    private Coroutine notificationCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (LevelTimer.Instance == null) return;

        float elapsed = LevelTimer.Instance.GetElapsedTime();
        float nextBuffTime = (buffLevel + 1) * buffInterval;

        if (elapsed >= nextBuffTime)
        {
            buffLevel++;
            TryGrantBuff();
        }
    }

    /// <summary>
    /// Rolls the 50/50 chance and, if successful, applies a random buff.
    /// </summary>
    private void TryGrantBuff()
    {
        // 50% chance to receive a buff
        if (Random.value > buffChance)
        {
            Debug.Log("[PlayerBuffManager] Buff roll failed — no buff this interval.");
            return;
        }

        ApplyRandomBuff();
    }

    /// <summary>
    /// Picks one of three buffs at random and applies it to the player.
    /// </summary>
    private void ApplyRandomBuff()
    {
        PlayerEnergy energy = FindPlayerEnergy();
        if (energy == null)
        {
            Debug.LogWarning("[PlayerBuffManager] Could not find PlayerEnergy on Player!");
            return;
        }

        int choice = Random.Range(0, 3);
        string message = "";

        switch (choice)
        {
            case 0: // Max Energy +5%
                energy.maxEnergyMultiplier += maxEnergyBonus;
                energy.UpdateUI();
                message = "Max Energy increased by 5%!";
                Debug.Log($"[PlayerBuffManager] Max Energy multiplier now {energy.maxEnergyMultiplier:F2}");
                break;

            case 1: // Defense +10%
                energy.playerBuffDamageReduction += defenseBonus;
                // Cap at 90% to avoid full invincibility
                energy.playerBuffDamageReduction = Mathf.Min(energy.playerBuffDamageReduction, 0.9f);
                message = "Defense increased by 10%!";
                Debug.Log($"[PlayerBuffManager] Player buff damage reduction now {energy.playerBuffDamageReduction:F2}");
                break;

            case 2: // Restore 25% of Max Energy
                float restoreAmount = energy.maxEnergy * energy.maxEnergyMultiplier * restorePercent;
                energy.RestoreEnergy(restoreAmount);
                message = "Restored 25% of Max Energy!";
                Debug.Log($"[PlayerBuffManager] Restored {restoreAmount:F1} energy to player.");
                break;
        }

        ShowNotification(message);
    }

    /// <summary>
    /// Finds the PlayerEnergy component on the Player-tagged object.
    /// </summary>
    private PlayerEnergy FindPlayerEnergy()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            return player.GetComponent<PlayerEnergy>();
        return null;
    }

    /// <summary>
    /// Shows a green center-screen notification to the player.
    /// </summary>
    private void ShowNotification(string message)
    {
        if (notificationText == null)
        {
            Vector3 popupPos = Camera.main != null
                ? Camera.main.transform.position + new Vector3(0, 1f, 10f)
                : Vector3.zero;

            CreateBuffPopup(popupPos, message);
            return;
        }

        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }

        notificationCoroutine = StartCoroutine(ShowNotificationRoutine(message));
    }

    private IEnumerator ShowNotificationRoutine(string message)
    {
        notificationText.text = message;
        notificationText.color = notificationColor;
        notificationText.gameObject.SetActive(true);

        yield return new WaitForSeconds(notificationDuration);

        notificationText.gameObject.SetActive(false);
        notificationCoroutine = null;
    }

    /// <summary>
    /// Creates a floating green popup in world space (mirrors DifficultyPopup style).
    /// </summary>
    private void CreateBuffPopup(Vector3 position, string message)
    {
        GameObject popupGO = new GameObject("PlayerBuffPopup");
        popupGO.transform.position = position;

        TextMeshPro tmp = popupGO.AddComponent<TextMeshPro>();
        tmp.text = "BUFF: " + message;
        tmp.fontSize = 5f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = notificationColor;
        tmp.sortingOrder = 200;

        RectTransform rt = popupGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(12f, 3f);

        // Reuse DifficultyPopup for the float / pulse / fade behaviour
        DifficultyPopup popup = popupGO.AddComponent<DifficultyPopup>();
        popup.lifetime = notificationDuration;
    }

    /// <summary>
    /// Resets buff tracking. Call when starting a new run.
    /// </summary>
    public void ResetBuffs()
    {
        buffLevel = 0;
        Debug.Log("[PlayerBuffManager] Buff tracking reset.");
    }
}
