using System;
using System.IO;
using UnityEngine;

public class SaveService
{
    public const int CurrentVersion = 1;

    // Diisi TitleInstaller.Continue, dibaca dan dikosongkan WorldInstaller.
    public SaveData PendingLoad;

    private static string FilePath => Path.Combine(Application.persistentDataPath, "save_0.json");

    public bool HasSave() => File.Exists(FilePath);

    public bool Save(SaveData data)
    {
        try
        {
            data.savedAt = DateTime.Now.ToString("s");
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Save gagal: " + e.Message);
            return false;
        }
    }

    public SaveData Load()
    {
        if (!HasSave()) return null;
        try
        {
            var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
            return (data != null && data.version <= CurrentVersion) ? data : null;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Load gagal: " + e.Message);
            return null;
        }
    }
}