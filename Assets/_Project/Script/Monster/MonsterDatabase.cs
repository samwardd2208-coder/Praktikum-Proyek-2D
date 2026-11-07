using System.Collections.Generic;
using UnityEngine;

// Daftar ke-12 MonsterData. Dibuat otomatis oleh menu Pulau Monster > 1.
[CreateAssetMenu(fileName = "MonsterDatabase",
        menuName = "Pulau Monster/Monster Database")]
public class MonsterDatabase : ScriptableObject
{
    [SerializeField] private List<MonsterData> monsters = new List<MonsterData>();

    public IReadOnlyList<MonsterData> Monsters => monsters;
    public int Count => monsters.Count;

    public MonsterData GetById(string id)
    {
        foreach (MonsterData m in monsters)
            if (m != null && m.Id == id) return m;
        return null;
    }

    public MonsterData GetByName(string displayName)
    {
        foreach (MonsterData m in monsters)
            if (m != null && m.DisplayName == displayName) return m;
        return null;
    }
}
