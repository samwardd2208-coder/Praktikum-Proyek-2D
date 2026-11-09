using UnityEngine;


public class MonsterDebugger : MonoBehaviour
{
    [SerializeField] private MonsterData species;   
    [SerializeField] private int level = 4;        

    private MonsterInstance monster;                

    [ContextMenu("1. Buat monster")]
    void BuildMonster()
    {
        if (species == null) { Debug.LogWarning("Species kosong! Seret Mon_Pyrit dulu."); return; }
        monster = new MonsterInstance(species, level);
        PrintStatus("Baru dibuat");
    }

    [ContextMenu("2. Cetak status")]
    void PrintStatusMenu()
    {
        if (monster == null) { Debug.LogWarning("Belum ada monster, klik no.1 dulu."); return; }
        PrintStatus("Status sekarang");
    }

    [ContextMenu("3. Beri 100 EXP")]
    void GiveExp()
    {
        if (monster == null) { Debug.LogWarning("Belum ada monster, klik no.1 dulu."); return; }
        monster.GainExp(100);
        PrintStatus("Setelah +100 EXP");
    }

    void PrintStatus(string tag)
    {
        Debug.Log($"[{tag}] {monster.Data.DisplayName} Lv{monster.Level} | " +
                  $"MaxHP {monster.MaxHP} | ATK {monster.Attack} | " +
                  $"ExpToNext {monster.ExpToNext} | EXP {monster.Exp}");
    }
}