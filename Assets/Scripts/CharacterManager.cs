using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    public CharacterData[] characters;
    public int selectedCharacterIndex = 0; // Bit is default

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SelectCharacter(int index)
    {
        if (characters[index].isUnlocked)
        {
            selectedCharacterIndex = index;
            Debug.Log("Equipped: " + characters[index].characterName);
        }
        else
        {
            Debug.Log("Character is locked!");
        }
    }

    public void UnlockCharacter(int index)
    {
        characters[index].isUnlocked = true;
    }
}

