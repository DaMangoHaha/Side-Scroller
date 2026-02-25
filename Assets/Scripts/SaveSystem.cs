using System.IO;
using UnityEngine;

public static class SaveSystem
{
    /// <summary>
    /// The full file path where save data is stored (persistentDataPath/savedata.json).
    /// </summary>
    private static string SavePath => Path.Combine(Application.persistentDataPath, "savedata.json");

    /// <summary>
    /// In-memory cache of the current save data to avoid redundant file reads.
    /// </summary>
    private static SaveData cachedData = null;

    /// <summary>
    /// Loads save data from disk. Returns the cached copy if already loaded,
    /// otherwise reads and deserializes the JSON file. If no save file exists
    /// or an error occurs, a fresh <see cref="SaveData"/> instance is returned.
    /// </summary>
    public static SaveData LoadData()
    {
        // Return cached data if it has already been loaded this session
        if (cachedData != null)
            return cachedData;

        if (File.Exists(SavePath))
        {
            try
            {
                // Read the JSON file and deserialize it into a SaveData object
                string json = File.ReadAllText(SavePath);
                cachedData = JsonUtility.FromJson<SaveData>(json);
                // Rebuild runtime dictionaries from the serialized lists
                cachedData.RebuildDictionaries();
                return cachedData;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading save data: " + e.Message);
                // Fall back to default data on read/parse failure
                cachedData = new SaveData();
                return cachedData;
            }
        }

        // No save file found — start with default data
        cachedData = new SaveData();
        return cachedData;
    }

    /// <summary>
    /// Serializes the provided <see cref="SaveData"/> to JSON and writes it
    /// to disk, also updating the in-memory cache.
    /// </summary>
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

    /// <summary>
    /// Deletes the save file from disk and clears the in-memory cache,
    /// effectively resetting all persisted progress.
    /// </summary>
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