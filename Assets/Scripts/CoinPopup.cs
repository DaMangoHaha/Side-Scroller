using UnityEngine;
using TMPro;

/// <summary>
/// A floating text popup that drifts upward, fades out, and self-destructs.
/// Used for coin collection, damage taken, energy restored, etc.
/// </summary>
public class CoinPopup : MonoBehaviour
{
    public float floatSpeed = 1.5f;   // how fast the text rises
    public float lifetime = 0.8f;     // how long before it disappears
    public float fadeSpeed = 2f;      // how quickly alpha fades out

    private TextMeshPro textMesh;
    private Color textColor;
    private float timer;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    void Update()
    {
        // Float upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Fade out
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
        if (textMesh != null)
        {
            textColor = textMesh.color;
            textColor.a = alpha;
            textMesh.color = textColor;
        }

        // Destroy after lifetime
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Generic popup with custom text, color, and font size.
    /// </summary>
    public static void Create(Vector3 position, string text, Color color, float fontSize = 3f)
    {
        Vector3 spawnPos = position + new Vector3(0f, 0.5f, 0f);

        GameObject popupGO = new GameObject("FloatingPopup");
        popupGO.transform.position = spawnPos;

        TextMeshPro tmp = popupGO.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.sortingOrder = 100;

        RectTransform rt = popupGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(6f, 2f);

        popupGO.AddComponent<CoinPopup>();
    }

    /// <summary>
    /// Coin collection popup — gold colored "+X Coin(s)".
    /// </summary>
    public static void Create(Vector3 position, int coinValue)
    {
        string text = "+" + coinValue + " Coin" + (coinValue != 1 ? "s" : "");
        Create(position, text, new Color(1f, 0.85f, 0.1f, 1f)); // gold
    }

    /// <summary>
    /// Damage popup — red colored "-X".
    /// </summary>
    public static void CreateDamage(Vector3 position, float damageAmount)
    {
        string text = "-" + Mathf.RoundToInt(damageAmount) + " Energy";
        Create(position, text, new Color(1f, 0.2f, 0.2f, 1f), 4f); // red, slightly larger
    }

    /// <summary>
    /// Energy restore popup — green colored "+X Energy".
    /// </summary>
    public static void CreateEnergy(Vector3 position, float energyAmount)
    {
        string text = "+" + Mathf.RoundToInt(energyAmount) + " Energy";
        Create(position, text, new Color(0.2f, 1f, 0.4f, 1f)); // green
    }

    /// <summary>
    /// Status effect popup — displays a debuff/buff name with a matching color.
    /// Spawns slightly higher than damage popups so they don't overlap.
    /// </summary>
    public static void CreateStatusEffect(Vector3 position, string effectName, Color color)
    {
        // Offset upward a bit extra so it doesn't overlap with the damage popup
        Vector3 offset = new Vector3(0f, 1.2f, 0f);
        Create(position + offset, effectName, color, 3.5f);
    }

    /// <summary>
    /// Coin multiplier popup — cyan/blue colored "x2 Coins for Xs!".
    /// </summary>
    public static void CreateMultiplier(Vector3 position, float multiplier, float duration)
    {
        string text = $"x{multiplier} Coins for {duration}s!";
        Create(position, text, new Color(0.3f, 0.8f, 1f, 1f), 4f); // cyan, slightly larger
    }

    /// <summary>
    /// Bit Buff popup — blue colored text near the player.
    /// Spawns slightly higher so it doesn't overlap with other popups.
    /// </summary>
    public static void CreateBitBuff(Vector3 position, string message)
    {
        Vector3 offset = new Vector3(0f, 1.2f, 0f);
        Create(position + offset, message, new Color(0.3f, 0.5f, 1f, 1f), 3.5f); // blue
    }
}
