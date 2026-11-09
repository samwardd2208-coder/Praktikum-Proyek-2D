using System.Collections.Generic;   
using UnityEngine;                  

[CreateAssetMenu(fileName = "MonsterDatabase", menuName = "Pulau Monster/Monster Database")]
public class MonsterDatabase : ScriptableObject
{
   
    [SerializeField] private List<MonsterData> monsters = new List<MonsterData>();

    
    public List<MonsterData> Monsters => monsters;

    public int Count => monsters.Count;
}