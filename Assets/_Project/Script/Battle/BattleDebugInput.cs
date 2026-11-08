using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Tombol uji cepat untuk checklist pertemuan 5. Pasang di objek yang sama dengan BattleSystem.
/// Hanya aktif di Editor / Development Build.
///   F1 = HP lawan jadi 1      -> serang sekali, tes menang + EXP
///   F2 = HP partner jadi 1    -> tes kalah -> kembali ke desa
///   F3 = partner tinggal 1 EXP dari Lv 5 -> menangkan battle untuk tes evolusi
///   F4 = tambah 3 Ramuan
///   F5 = pulihkan HP partner penuh
/// </summary>
public class BattleDebugInput : MonoBehaviour
{
    [SerializeField] BattleSystem battle;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    void Awake()
    {
        if (battle == null) battle = GetComponent<BattleSystem>();
    }

    void Update()
    {
        Keyboard k = Keyboard.current;
        if (k == null || battle == null || battle.Player == null || battle.Enemy == null) return;

        if (k.f1Key.wasPressedThisFrame) { battle.Enemy.TakeDamage(battle.Enemy.CurrentHP - 1); Debug.Log("[Debug] HP lawan = 1"); }
        if (k.f2Key.wasPressedThisFrame) { battle.Player.TakeDamage(battle.Player.CurrentHP - 1); Debug.Log("[Debug] HP partner = 1"); }
        if (k.f3Key.wasPressedThisFrame) PrepareEvolveTest();
        if (k.f4Key.wasPressedThisFrame) { battle.AddPotions(3); Debug.Log("[Debug] +3 Ramuan"); }
        if (k.f5Key.wasPressedThisFrame) { battle.Player.Heal(battle.Player.MaxHP); Debug.Log("[Debug] HP partner penuh"); }
    }

    void PrepareEvolveTest()
    {
        MonsterInstance p = battle.Player;
        for (int guard = 0; guard < 20 && p.Level < 4; guard++) Feed(p, p.ExpToNext - p.Exp);   // naik sampai Lv 4
        Feed(p, p.ExpToNext - p.Exp - 1);                                                       // 1 EXP sebelum Lv 5
        Debug.Log("[Debug] Partner Lv " + p.Level + ", tinggal 1 EXP lagi. Menangkan battle ini untuk evolusi.");
    }

    static void Feed(MonsterInstance p, int exp)
    {
        if (exp <= 0) return;
        foreach (GrowthEvent ignored in p.GainExp(exp)) { }       // habiskan enumerator agar EXP benar-benar masuk
    }
#endif
}
