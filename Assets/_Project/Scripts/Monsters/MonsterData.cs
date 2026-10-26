using UnityEngine;

public enum Element { Pyro, Aqua, Sprout, Volt }

[CreateAssetMenu(fileName = "Mon_", menuName = "Pulau Monster/Monster Data")]
public class MonsterData : ScriptableObject
{
    [SerializeField] private string id; // Contoh: "MON_001"
    [SerializeField] private string displayName;
    [SerializeField] private Element element;
    [SerializeField] private Sprite[] frames; // [0] Idle0, [1] Idle1, [2] Attack, [3] Hurt
    [SerializeField] private int baseHP, baseATK, baseDEF, baseSPD;
    [SerializeField] private string skillName = "Semburan Api";
    [SerializeField] private MonsterData evolvesTo;
    [SerializeField] private int evolveLevel;

    // Property read-only untuk mengakses field private dari luar
    public string Id => id;
    public string DisplayName => displayName;
    public Element Element => element;
    public Sprite[] Frames => frames;
    public int BaseHP => baseHP;
    public int BaseATK => baseATK;
    public int BaseDEF => baseDEF;
    public int BaseSPD => baseSPD;
    public string SkillName => skillName;
    public MonsterData EvolvesTo => evolvesTo;
    public int EvolveLevel => evolveLevel;
}