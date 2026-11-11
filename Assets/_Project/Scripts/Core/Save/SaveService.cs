using UnityEngine;

public class SaveService
{
    private const string SAVE_KEY = "PulauMonster_SaveSlot1";
    public SaveData PendingLoad { get; set; }

    public bool HasSave() => PlayerPrefs.HasKey(SAVE_KEY);

    public bool Save(SaveData data)
    {
        try
        {
            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
            Debug.Log("Game Saved!");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Gagal menyimpan: " + e.Message);
            return false;
        }
    }

    public SaveData Load()
    {
        PendingLoad = HasSave()
            ? JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SAVE_KEY))
            : null;
        return PendingLoad;
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        PendingLoad = null;
    }
}