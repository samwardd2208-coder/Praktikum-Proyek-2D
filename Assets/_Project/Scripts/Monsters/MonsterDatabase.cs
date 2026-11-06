using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Mon_Database", menuName = "Pulau Monster/Monster Database")]
public class MonsterDatabase : ScriptableObject
{
    [SerializeField] private List<MonsterData> monsters = new List<MonsterData>();

    public MonsterData GetById(string id)
    {
        foreach (var m in monsters)
        {
            if (m.Id == id) return m;
        }
        return null;
    }

    public List<MonsterData> GetAll() => monsters;
}