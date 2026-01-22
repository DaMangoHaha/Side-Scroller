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

    [Header("Skill Icon Objects")]
    public GameObject bitsIcon;
    public GameObject thiefIcon;
    public GameObject ninjaIcon;
    public GameObject wizKidIcon;
    public GameObject crystalIcon;
    public GameObject cubitIcon;

    [Header("Energy Bars")]
    public GameObject bitsEnergyBar;
    public GameObject thiefEnergyBar;
    public GameObject ninjaEnergyBar;
    public GameObject wizKidEnergyBar;
    public GameObject crystalEnergyBar;
    public GameObject cubitEnergyBar;


    void Start()
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

        // --- SKILL ICONS ---
        bitsIcon.SetActive(equipped == "Bits");
        thiefIcon.SetActive(equipped == "Thief");
        ninjaIcon.SetActive(equipped == "Ninja");
        wizKidIcon.SetActive(equipped == "WizKid");
        crystalIcon.SetActive(equipped == "Crystal");
        cubitIcon.SetActive(equipped == "Cubit");

        // --- ENERGY BARS ---
        bitsEnergyBar.SetActive(equipped == "Bits");
        thiefEnergyBar.SetActive(equipped == "Thief");
        ninjaEnergyBar.SetActive(equipped == "Ninja");
        wizKidEnergyBar.SetActive(equipped == "WizKid");
        crystalEnergyBar.SetActive(equipped == "Crystal");
        cubitEnergyBar.SetActive(equipped == "Cubit");

    }
}

