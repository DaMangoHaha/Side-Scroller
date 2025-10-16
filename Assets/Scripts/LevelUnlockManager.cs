using UnityEngine;

public class LevelUnlockManager : MonoBehaviour
{
    public static LevelUnlockManager Instance;

    // 0 = locked, 1 = unlocked
    public bool level1Unlocked = true;
    public bool level2Unlocked = false;

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
        if (levelNumber == 2) level2Unlocked = true;
        // Later, extend this with more levels
    }
}

