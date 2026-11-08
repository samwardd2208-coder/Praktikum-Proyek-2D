using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================================
//  BattleAction: satu class abstrak, empat aksi turunan (inheritance + polymorphism)
//  Aksi      Efek                        Prio
//  Serang    power 14, meleset 6 %       0
//  Jurus     power 22 x elemen, 3x       0
//  Ramuan    pulih 40 % MaxHP            1
//  Kabur     70 %, boss: tidak bisa      2
// ============================================================================

public abstract class BattleAction
{
    public abstract string Label { get; }
    public virtual int Priority => 0;

    /// <summary>Coroutine satu giliran aksi. user = pelaku, target = lawan.</summary>
    public abstract IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target);
}

// ---------------------------------------------------------------- Serang (dan basis Jurus)

public class AttackAction : BattleAction
{
    public override string Label => "Serang";
    public virtual int Power => 14;
    public virtual float MissChance => 0.06f;

    /// <summary>Nama jurus; null = Serang biasa.</summary>
    protected virtual string Move(MonsterInstance user) => null;

    protected float ElementBonus(MonsterInstance user, MonsterInstance target)
        => Move(user) == null ? 1f : ElementChart.Multiplier(user.Data.Element, target.Data.Element);

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        string who = (user == battle.Enemy && !battle.IsBoss ? "Lawan " : "") + user.Data.DisplayName;
        string move = Move(user);                                    // null = Serang biasa
        if (move != null) battle.ConsumeSkill(user);

        yield return battle.Say(move == null ? $"{who} menyerang!" : $"{who} memakai {move}!", 0.7f);
        battle.RaiseAttacked(user);                                  // animasi menerjang
        yield return new WaitForSeconds(0.2f);

        if (UnityEngine.Random.value < MissChance)
        {
            battle.RaiseMissed();
            yield return battle.Say("Serangan meleset!", 0.8f);
            yield break;
        }

        float bonus = ElementBonus(user, target);
        int damage = DamageFormula.Calculate(user, target, Power,
                bonus * battle.DamageScale(user), out bool critical);
        target.TakeDamage(damage);
        battle.RaiseHit(target, damage, critical,
                move != null ? user.Data.Element : (Element?)null);
        yield return new WaitForSeconds(0.45f);

        if (critical) yield return battle.Say("Serangan kritis!", 0.7f);
        if (bonus > 1f) yield return battle.Say("Sangat efektif!", 0.7f);
        else if (bonus < 1f) yield return battle.Say("Kurang efektif...", 0.7f);
    }
}

// ---------------------------------------------------------------- Jurus

public class SkillAction : AttackAction
{
    public override string Label => "Jurus";
    public override int Power => 22;

    protected override string Move(MonsterInstance user) => MoveName(user);

    static readonly Dictionary<string, string> names = new Dictionary<string, string>
    {
        { "Pyro", "Semburan Api" },
        { "Volt", "Sambaran Petir" },
    };

    /// <summary>Nama jurus elemen untuk monster ini (dipakai juga oleh hint menu).</summary>
    public static string MoveName(MonsterInstance user)
    {
        string element = user.Data.Element.ToString();
        return names.TryGetValue(element, out string name) ? name : "Jurus " + element;
    }
}

// ---------------------------------------------------------------- Ramuan

public class PotionAction : BattleAction
{
    public const float HealFraction = 0.4f;

    public override string Label => "Ramuan";
    public override int Priority => 1;

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        if (!battle.UsePotion())
        {
            yield return battle.Say("Ramuan sudah habis!", 0.7f);
            yield break;
        }

        yield return battle.Say($"{user.Data.DisplayName} memakai Ramuan!", 0.7f);
        int healed = Mathf.RoundToInt(user.MaxHP * HealFraction);
user.Heal(healed);
        yield return new WaitForSeconds(0.5f);                       // beri waktu bar HP naik pelan
        yield return battle.Say(healed > 0 ? $"{user.Data.DisplayName} pulih {healed} HP."
                                           : "HP sudah penuh.", 0.7f);
    }
}

// ---------------------------------------------------------------- Kabur

public class RunAction : BattleAction
{
    public const float EscapeChance = 0.7f;

    public override string Label => "Kabur";
    public override int Priority => 2;

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        if (battle.IsBoss)                                           // boss: giliran tidak hilang (lihat Run())
        {
            yield return battle.Say("Tidak bisa kabur dari boss!", 0.8f);
            yield break;
        }

        yield return battle.Say("Kamu mencoba kabur...", 0.7f);
        if (UnityEngine.Random.value < EscapeChance)
        {
            yield return battle.Say("Berhasil kabur!", 0.7f);
            battle.Flee();
        }
        else
        {
            yield return battle.Say("Gagal kabur!", 0.7f);
        }
    }
}

// ---------------------------------------------------------------- Tabel elemen

/// <summary>
/// ADAPTASI P4: kalau proyekmu sudah punya tabel kelemahan elemen, ganti isi Multiplier() dengan panggilan ke sana.
/// Versi bawaan ini netral terhadap nama enum: elemen disusun melingkar, tiap elemen unggul atas elemen
/// berikutnya (x1,5) dan lemah terhadap elemen sebelumnya (x0,5).
/// </summary>
// public static class ElementChart
// {
//     public static float Multiplier(Element attacker, Element defender)
//     {
//         int n = System.Enum.GetValues(typeof(Element)).Length;
//         if (n < 3) return 1f;
//         int a = (int)attacker, d = (int)defender;
//         if ((a + 1) % n == d) return 1.5f;
//         if ((d + 1) % n == a) return 0.5f;
//         return 1f;
//     }
// }
