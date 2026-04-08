using TMPro;
using UnityEngine;

/// <summary>
/// Displays a cooldown timer, "Skill Ready!", or "Using Skill..." text
/// next to a character's skill icon. Attach to the same GameObject as
/// the TextMeshProUGUI component or assign it in the Inspector.
///
/// Each character skill script sets the state every frame via the
/// public helper methods.
/// </summary>
public class SkillCooldownUI : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("The TMP text element that shows the cooldown / status.")]
    public TextMeshProUGUI cooldownText;

    // Cached so we only update when the displayed string actually changes.
    private string lastText = "";

    void Awake()
    {
        if (cooldownText == null)
            cooldownText = GetComponent<TextMeshProUGUI>();
    }

    // -------------------------------------------------------
    //  Public API – called by each character's skill script
    // -------------------------------------------------------

    /// <summary>Shows a countdown number (rounded up to whole seconds).</summary>
    public void ShowCooldown(float secondsRemaining)
    {
        int display = Mathf.CeilToInt(secondsRemaining);
        if (display < 0) display = 0;
        SetText(display.ToString());
    }

    /// <summary>Shows "Skill Ready!" text.</summary>
    public void ShowReady()
    {
        SetText("Skill Ready!");
    }

    /// <summary>Shows "Using Skill..." text.</summary>
    public void ShowUsing()
    {
        SetText("Using Skill...");
    }

    /// <summary>Shows Crystal's snowflake count (e.g. "3 / 5").</summary>
    public void ShowSnowflakeCount(int current, int needed)
    {
        SetText($"{current} / {needed}");
    }

    /// <summary>Shows an arbitrary string (fallback).</summary>
    public void ShowCustom(string message)
    {
        SetText(message);
    }

    // -------------------------------------------------------
    //  Internal
    // -------------------------------------------------------

    private void SetText(string value)
    {
        if (cooldownText == null) return;
        if (value == lastText) return;   // skip redundant updates
        lastText = value;
        cooldownText.text = value;
    }
}
