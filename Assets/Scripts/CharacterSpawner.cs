using TMPro;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [Header("Character Objects")]
    public GameObject bits;
    public GameObject thief;
    public GameObject ninja;
    public GameObject wizKid;
    public GameObject crystal;
    public GameObject cubit;
    public GameObject selene;

    [Header("Skill Icon Objects")]
    public GameObject bitsIcon;
    public GameObject thiefIcon;
    public GameObject ninjaIcon;
    public GameObject wizKidIcon;
    public GameObject crystalIcon;
    public GameObject cubitIcon;
    public GameObject seleneIcon;

    [Header("Upgrade Icon Objects")]
    public GameObject bitsUpgradeIcon; // Bit's Skill Chip upgrade icon (shown underneath his Energy Bar)
    public GameObject thiefUpgradeIcon; // Thief's Sticky Fingers upgrade icon (shown underneath his Energy Bar)
    public GameObject ninjaUpgradeIcon; // Ninja's Electric Bolt upgrade icon (shown underneath her Energy Bar)
    public GameObject wizKidUpgradeIcon; // WizKid's Sprouting Sorcery upgrade icon (shown underneath his Energy Bar)
    public GameObject crystalUpgradeIcon; // Crystal's Glaciate upgrade icon (shown underneath her Energy Bar)
    public GameObject cubitUpgradeIcon; // Cubit's Protection Protocol upgrade icon (shown underneath his Energy Bar)
    public GameObject seleneUpgradeIcon; // Selene's Eclipse upgrade icon (shown underneath her Energy Bar)

    [Header("Self-Buff Icons")] // These icons appear in the top-left corner, below the energy bar, when the corresponding buff is active. Crystal is the only character with a self-buff (Chill Wind).
    public GameObject chillWindIcon; // Crystal's Chill Wind buff icon (shown in the top-left corner of the screen when active)

    [Header("Energy Bars")]
    public GameObject bitsEnergyBar;
    public GameObject thiefEnergyBar;
    public GameObject ninjaEnergyBar;
    public GameObject wizKidEnergyBar;
    public GameObject crystalEnergyBar;
    public GameObject cubitEnergyBar;
    public GameObject seleneEnergyBar;

    [Header("Skill Cooldown UI")]
    public TextMeshProUGUI bitsCooldownText;
    public TextMeshProUGUI thiefCooldownText;
    public TextMeshProUGUI ninjaCooldownText;
    public TextMeshProUGUI wizKidCooldownText;
    public TextMeshProUGUI crystalCooldownText;
    public TextMeshProUGUI cubitCooldownText;
    public TextMeshProUGUI seleneCooldownText;

    void Awake()
    {
        ApplySelection();
    }

    void ApplySelection()
    {
        string equipped = CharacterEquipManager.Instance.GetEquippedCharacter();


        // --- PLAYER GAMEOBJECTS ---
        bits.SetActive(equipped == "Bits");
        thief.SetActive(equipped == "Thief");
        ninja.SetActive(equipped == "Ninja");
        wizKid.SetActive(equipped == "WizKid");
        crystal.SetActive(equipped == "Crystal");
        cubit.SetActive(equipped == "Cubit");
        if (selene != null) selene.SetActive(equipped == "Selene");

        // --- SKILL ICONS ---
        bitsIcon.SetActive(equipped == "Bits");
        thiefIcon.SetActive(equipped == "Thief");
        ninjaIcon.SetActive(equipped == "Ninja");
        wizKidIcon.SetActive(equipped == "WizKid");
        crystalIcon.SetActive(equipped == "Crystal");
        cubitIcon.SetActive(equipped == "Cubit");
        if (seleneIcon != null) seleneIcon.SetActive(equipped == "Selene");

        // --- UPGRADE ICONS ---
        if (bitsUpgradeIcon != null) bitsUpgradeIcon.SetActive(equipped == "Bits");
        if (thiefUpgradeIcon != null) thiefUpgradeIcon.SetActive(equipped == "Thief");
        if (ninjaUpgradeIcon != null) ninjaUpgradeIcon.SetActive(equipped == "Ninja");
        if (wizKidUpgradeIcon != null) wizKidUpgradeIcon.SetActive(equipped == "WizKid");
        if (crystalUpgradeIcon != null) crystalUpgradeIcon.SetActive(equipped == "Crystal");
        if (cubitUpgradeIcon != null) cubitUpgradeIcon.SetActive(equipped == "Cubit");
        if (seleneUpgradeIcon != null) seleneUpgradeIcon.SetActive(equipped == "Selene");

        // --- ENERGY BARS ---
        bitsEnergyBar.SetActive(equipped == "Bits");
        thiefEnergyBar.SetActive(equipped == "Thief");
        ninjaEnergyBar.SetActive(equipped == "Ninja");
        wizKidEnergyBar.SetActive(equipped == "WizKid");
        crystalEnergyBar.SetActive(equipped == "Crystal");
        cubitEnergyBar.SetActive(equipped == "Cubit");
        if (seleneEnergyBar != null) seleneEnergyBar.SetActive(equipped == "Selene");

        // --- SKILL COOLDOWN UI ---
        bitsCooldownText.gameObject.SetActive(equipped == "Bits");
        thiefCooldownText.gameObject.SetActive(equipped == "Thief");
        ninjaCooldownText.gameObject.SetActive(equipped == "Ninja");
        wizKidCooldownText.gameObject.SetActive(equipped == "WizKid");
        crystalCooldownText.gameObject.SetActive(equipped == "Crystal");
        cubitCooldownText.gameObject.SetActive(equipped == "Cubit");
        if (seleneCooldownText != null) seleneCooldownText.gameObject.SetActive(equipped == "Selene");
    }
}

