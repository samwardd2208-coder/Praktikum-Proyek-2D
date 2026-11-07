using UnityEngine;

// Uji MonsterInstance tanpa battle: pasang di GameObject kosong (scene sandbox),
// isi Species + Level, Play, lalu pilih menu titik tiga komponen ini.
public class MonsterDebugger : MonoBehaviour
{
    [SerializeField] private MonsterData species;
    [SerializeField] private int level = 4;

    private MonsterInstance monster;

    [ContextMenu("1. Buat monster")]
    private void CreateMonster()
    {
        if (species == null) { Debug.LogWarning("Isi field Species dulu."); return; }
        monster = new MonsterInstance(species, level);
        Log("Dibuat");
    }

    [ContextMenu("2. Beri 10 damage")]
    private void Hit()
    {
        if (!Ready()) return;
        monster.TakeDamage(10);
        Log("Kena 10 damage");
    }

    [ContextMenu("3. Beri 100 EXP")]
    private void Give100Exp()
    {
        if (!Ready()) return;
        foreach (GrowthEvent e in monster.GainExp(100))
            Debug.Log(e.Message);
        Log("Setelah 100 EXP");
    }

    private bool Ready()
    {
        if (monster != null) return true;
        Debug.LogWarning("Pilih '1. Buat monster' dulu (saat Play).");
        return false;
    }

    private void Log(string title)
    {
        Debug.Log($"[{title}] {monster.Data.DisplayName} Lv{monster.Level} " +
                  $"HP {monster.CurrentHP}/{monster.MaxHP} ATK {monster.Attack} " +
                  $"EXP {monster.Exp}/{monster.ExpToNext}");
    }
}
