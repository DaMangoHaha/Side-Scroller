using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    // The currently selected character name
    public string equippedCharacter = "Bits"; // default

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load saved preference (if exists)
            equippedCharacter = PlayerPrefs.GetString("EquippedCharacter", "Bits");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCharacter(string characterName)
    {
        equippedCharacter = characterName;
        PlayerPrefs.SetString("EquippedCharacter", characterName);
        PlayerPrefs.Save();
    }
}
