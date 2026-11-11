using UnityEditor;
using UnityEngine;

public static class SaveDebugMenu
{
    [MenuItem("Tools/Hapus Save (Pulau Monster)")]
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey("PulauMonster_SaveSlot1");
        PlayerPrefs.Save();
        Debug.Log("Save dihapus. Intro & pilihan starter akan muncul lagi.");
    }
}