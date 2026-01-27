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
            SaveData data = SaveSystem.LoadData();
            equippedCharacter = data.equippedCharacter;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCharacter(string characterName)
    {
        equippedCharacter = characterName;
        
        SaveData data = SaveSystem.LoadData();
        data.equippedCharacter = characterName;
        SaveSystem.SaveData(data);
    }
}
