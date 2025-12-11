using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [Header("Character Objects")]
    public GameObject bits;
    public GameObject thief;
    public GameObject ninja;
    public GameObject wizKid;
    public GameObject crystal;

    [Header("Skill Icon Objects")]
    public GameObject bitsIcon;
    public GameObject thiefIcon;
    public GameObject ninjaIcon;
    public GameObject wizKidIcon;
    public GameObject crystalIcon;

    void Start()
    {
        ApplySelection();
    }

    void ApplySelection()
    {
        string selected = CharacterManager.Instance.selectedCharacter;

        // --- PLAYER GAMEOBJECTS ---
        bits.SetActive(selected == "Bits");
        thief.SetActive(selected == "Thief");
        ninja.SetActive(selected == "Ninja");
        wizKid.SetActive(selected == "Wiz Kid");
        crystal.SetActive(selected == "Crystal");

        // --- SKILL ICONS ---
        bitsIcon.SetActive(selected == "Bits");
        thiefIcon.SetActive(selected == "Thief");
        ninjaIcon.SetActive(selected == "Ninja");
        wizKidIcon.SetActive(selected == "Wiz Kid");
        crystalIcon.SetActive(selected == "Crystal");
    }
}

