using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "savedata.json");
    private static SaveData cachedData = null;

    public static SaveData LoadData()
    {
        if (cachedData != null)
            return cachedData;

        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                cachedData = JsonUtility.FromJson<SaveData>(json);
                // Rebuild runtime dictionaries from the serialized lists
                cachedData.RebuildDictionaries();
                return cachedData;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading save data: " + e.Message);
                cachedData = new SaveData();
                return cachedData;
            }
        }

        cachedData = new SaveData();
        return cachedData;
    }

    public static void SaveData(SaveData data)
    {
        cachedData = data;
        try
        {
            // Sync runtime dictionaries back into serializable lists before writing
            data.SyncListsFromDictionaries();
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error saving data: " + e.Message);
        }
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            cachedData = null;
            Debug.Log("Save file deleted.");
        }
    }
}