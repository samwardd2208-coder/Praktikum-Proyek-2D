using UnityEngine;

public class MonsterDebugger : MonoBehaviour
{
    [SerializeField] private MonsterData species;
    [SerializeField] private int level = 4;

    private MonsterInstance myMonster;

    [ContextMenu("1. Buat monster")]
    public void CreateMonster()
    {
        if (species == null)
        {
            Debug.LogWarning("Species belum diisi di Inspector!");
            return;
        }

        myMonster = new MonsterInstance(species, level);
        Debug.Log($"Monster dibuat: {myMonster.Data.DisplayName} (Lv. {myMonster.Level}) | HP: {myMonster.CurrentHP}/{myMonster.MaxHP} | ATK: {myMonster.Attack}");
    }

    [ContextMenu("2. Beri 10 EXP")]
    public void Give10Exp()
    {
        if (myMonster == null)
        {
            Debug.LogWarning("Buat monster terlebih dahulu!");
            return;
        }

        Debug.Log("Memberikan 10 EXP...");
        myMonster.GainExp(10);
    }

    [ContextMenu("3. Beri 100 EXP")]
    public void Give100Exp()
    {
        if (myMonster == null)
        {
            Debug.LogWarning("Buat monster terlebih dahulu!");
            return;
        }

        Debug.Log("Memberikan 100 EXP...");
        myMonster.GainExp(100);
    }
}