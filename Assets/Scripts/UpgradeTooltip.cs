using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to a character's upgrade icon in the shop.
/// When the player hovers over (or long-presses on mobile) the icon,
/// a tooltip panel appears showing the skill's upgrade tier information.
/// </summary>
public class UpgradeTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Skill Info")]
    [Tooltip("Display name shown at the top of the tooltip (e.g. 'Bit Buff Upgrades')")]
    public string skillTitle = "Skill Upgrades";

    [Tooltip("Color of the title text")]
    public Color titleColor = Color.white;

    [Tooltip("Cost per upgrade tier (shown next to locked tiers)")]
    public int upgradeCost = 300;

    [Header("Tier Descriptions (set 3)")]
    [TextArea] public string tier1Description = "Tier 1: ...";
    [TextArea] public string tier2Description = "Tier 2: ...";
    [TextArea] public string tier3Description = "Tier 3: ...";

    [Header("Current Tier Source")]
    [Tooltip("Drag the character's upgrade panel here so the tooltip can read the current tier. " +
             "Supports BitSkillUpgradePanel, CrystalSkillUpgradePanel, CubitSkillUpgradePanel, " +
             "NinjaSkillUpgradePanel, ThiefSkillUpgradePanel, WizKidSkillUpgradePanel.")]
    public MonoBehaviour upgradePanel;

    [Header("Tooltip Settings")]
    [Tooltip("Offset from the icon position (in canvas pixels)")]
    public Vector2 tooltipOffset = new Vector2(0f, 120f);

    // Runtime
    private GameObject tooltipRoot;
    private Canvas tooltipCanvas;

    // -------------------------------------------------------
    // Pointer Events
    // -------------------------------------------------------

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    // -------------------------------------------------------
    // Show / Hide
    // -------------------------------------------------------

    public void ShowTooltip()
    {
        if (tooltipRoot != null) return; // already visible

        int currentTier = GetCurrentTier();
        string[] descriptions = { tier1Description, tier2Description, tier3Description };

        Canvas canvas = GetOrCreateTooltipCanvas();

        // --- Tooltip root ---
        tooltipRoot = new GameObject("UpgradeTooltip");
        tooltipRoot.transform.SetParent(canvas.transform, false);

        RectTransform rootRT = tooltipRoot.AddComponent<RectTransform>();

        // Position near the icon
        Vector2 screenPos;
        RectTransform iconRT = GetComponent<RectTransform>();
        if (iconRT != null)
        {
            // Convert icon world position to screen space, then to canvas local
            Vector3 worldPos = iconRT.position;
            screenPos = RectTransformUtility.WorldToScreenPoint(null, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, screenPos, null, out Vector2 localPos);
            rootRT.anchoredPosition = localPos + tooltipOffset;
        }
        else
        {
            rootRT.anchoredPosition = tooltipOffset;
        }

        rootRT.anchorMin = new Vector2(0.5f, 0.5f);
        rootRT.anchorMax = new Vector2(0.5f, 0.5f);
        rootRT.sizeDelta = new Vector2(520f, 260f);

        // Background
        Image bg = tooltipRoot.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        bg.raycastTarget = false;

        // --- Title ---
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(tooltipRoot.transform, false);

        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -20f);
        titleRT.sizeDelta = new Vector2(0f, 36f);

        TextMeshProUGUI titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = skillTitle;
        titleTMP.fontSize = 26;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = titleColor;
        titleTMP.raycastTarget = false;

        // --- Tier rows ---
        for (int i = 0; i < 3; i++)
        {
            int tierNumber = i + 1;

            GameObject rowGO = new GameObject("Tier" + tierNumber);
            rowGO.transform.SetParent(tooltipRoot.transform, false);

            RectTransform rowRT = rowGO.AddComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0f, 1f);
            rowRT.anchorMax = new Vector2(1f, 1f);
            rowRT.anchoredPosition = new Vector2(0f, -58f - (i * 55f));
            rowRT.sizeDelta = new Vector2(-20f, 50f);

            // Row background tint
            Image rowBG = rowGO.AddComponent<Image>();
            rowBG.raycastTarget = false;

            if (currentTier >= tierNumber)
            {
                // Owned
                rowBG.color = new Color(0.12f, 0.35f, 0.12f, 0.7f); // green tint
            }
            else if (currentTier == tierNumber - 1)
            {
                // Next available
                rowBG.color = new Color(0.15f, 0.15f, 0.35f, 0.7f); // blue tint
            }
            else
            {
                // Locked
                rowBG.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); // grey
            }

            // Row text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(rowGO.transform, false);

            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(8f, 2f);
            textRT.offsetMax = new Vector2(-8f, -2f);

            TextMeshProUGUI textTMP = textGO.AddComponent<TextMeshProUGUI>();
            textTMP.fontSize = 18;
            textTMP.alignment = TextAlignmentOptions.MidlineLeft;
            textTMP.color = Color.white;
            textTMP.raycastTarget = false;
            textTMP.enableWordWrapping = true;

            // Build label
            string status;
            if (currentTier >= tierNumber)
                status = "  <color=#66FF66>[OWNED]</color>";
            else if (currentTier == tierNumber - 1)
                status = "  <color=#6699FF>[" + upgradeCost + " Coins]</color>";
            else
                status = "  <color=#999999>[LOCKED]</color>";

            textTMP.text = descriptions[i] + status;
        }

        // --- "Hover to close" hint at bottom ---
        GameObject hintGO = new GameObject("Hint");
        hintGO.transform.SetParent(tooltipRoot.transform, false);

        RectTransform hintRT = hintGO.AddComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(0f, 0f);
        hintRT.anchorMax = new Vector2(1f, 0f);
        hintRT.anchoredPosition = new Vector2(0f, 12f);
        hintRT.sizeDelta = new Vector2(0f, 20f);

        TextMeshProUGUI hintTMP = hintGO.AddComponent<TextMeshProUGUI>();
        hintTMP.text = "Move cursor away to close";
        hintTMP.fontSize = 14;
        hintTMP.fontStyle = FontStyles.Italic;
        hintTMP.alignment = TextAlignmentOptions.Center;
        hintTMP.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
        hintTMP.raycastTarget = false;
    }

    public void HideTooltip()
    {
        if (tooltipRoot != null)
        {
            Destroy(tooltipRoot);
            tooltipRoot = null;
        }
    }

    private void OnDisable()
    {
        HideTooltip();
    }

    // -------------------------------------------------------
    // Read current tier from whichever upgrade panel is assigned
    // -------------------------------------------------------

    private int GetCurrentTier()
    {
        if (upgradePanel == null) return 0;

        // Use reflection-free approach: check known types
        if (upgradePanel is BitSkillUpgradePanel bit && bit.bitSkill != null)
            return bit.bitSkill.GetUpgradeTier();
        if (upgradePanel is CrystalSkillUpgradePanel crystal && crystal.crystalAbility != null)
            return crystal.crystalAbility.GetUpgradeTier();
        if (upgradePanel is CubitSkillUpgradePanel cubit && cubit.cubitPassive != null)
            return cubit.cubitPassive.GetUpgradeTier();
        if (upgradePanel is NinjaSkillUpgradePanel ninja && ninja.ninjaSkill != null)
            return ninja.ninjaSkill.GetUpgradeTier();
        if (upgradePanel is ThiefSkillUpgradePanel thief && thief.thiefSkill != null)
            return thief.thiefSkill.GetUpgradeTier();
        if (upgradePanel is WizKidSkillUpgradePanel wiz && wiz.wizKidSkill != null)
            return wiz.wizKidSkill.GetUpgradeTier();

        return 0;
    }

    // -------------------------------------------------------
    // Shared tooltip canvas (high sort order so it's on top)
    // -------------------------------------------------------

    private Canvas GetOrCreateTooltipCanvas()
    {
        if (tooltipCanvas != null) return tooltipCanvas;

        GameObject existing = GameObject.Find("UpgradeTooltipCanvas");
        if (existing != null)
        {
            tooltipCanvas = existing.GetComponent<Canvas>();
            if (tooltipCanvas != null) return tooltipCanvas;
        }

        GameObject canvasGO = new GameObject("UpgradeTooltipCanvas");
        tooltipCanvas = canvasGO.AddComponent<Canvas>();
        tooltipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tooltipCanvas.sortingOrder = 1100; // above upgrade panels (999) and popups (1000)

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        return tooltipCanvas;
    }
}
