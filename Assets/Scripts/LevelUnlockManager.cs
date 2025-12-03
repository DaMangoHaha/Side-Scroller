using UnityEngine;

public class LevelUnlockManager : MonoBehaviour
{
    public static LevelUnlockManager Instance;

    // 0 = locked, 1 = unlocked
    public bool level1Unlocked = true;
    public bool level2Unlocked = true;
    public bool level3Unlocked = true;
    public bool level4Unlocked = true;

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

    public void UnlockLevel(int levelNumber)
    {
        if (levelNumber == 1)  level1Unlocked = true;
        if (levelNumber == 2) level2Unlocked = true;
        if (levelNumber == 3) level3Unlocked = true;
        if (levelNumber == 4) level4Unlocked = true;
        // Later, extend this with more levels
    }
}

