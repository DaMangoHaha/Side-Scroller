using UnityEngine;

/// <summary>
/// ScriptableObject that defines a single character costume/skin.
/// Create one of these for every skin variant in your game.
/// 
/// In Unity: Right-click in Project window ? Create ? Costumes ? Costume Data
/// </summary>
[CreateAssetMenu(fileName = "NewCostume", menuName = "Costumes/Costume Data")]
public class CostumeData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique ID for this costume, e.g. 'Bits_Valentines', 'Thief_Valentines'")]
    public string costumeID;

    [Tooltip("Which character this costume belongs to (must match CharacterSpawner names)")]
    public string characterID; // "Bits", "Thief", "Ninja", "WizKid", "Crystal", "Cubit"

    [Tooltip("Display name shown in the shop UI")]
    public string displayName; // e.g. "Cupid Bits"

    [Tooltip("Description shown in the shop or preview")]
    [TextArea]
    public string description; // e.g. "Bits spreads love this Valentine's Day!"

    [Header("Visuals")]
    [Tooltip("Shop/preview icon sprite for this costume")]
    public Sprite shopIcon;

    [Tooltip("Override Animator Controller for this costume. If null, the default animator is kept.")]
    public RuntimeAnimatorController animatorOverride;

    [Header("Unlock / Purchase")]
    [Tooltip("Cost in regular coins (0 = free / default skin)")]
    public int coinCost = 0;

    [Tooltip("Cost in Cupid Coins (seasonal currency, 0 = not applicable)")]
    public int cupidCoinCost = 0;

    [Tooltip("If true, this costume is available from the start without purchasing")]
    public bool isFreeDefault = false;

    [Header("Metadata")]
    [Tooltip("Optional tag for filtering, e.g. 'Valentines', 'Halloween', 'Default'")]
    public string themeTag = "Default";
}
