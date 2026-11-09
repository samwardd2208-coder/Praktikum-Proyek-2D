using System.Collections;
using UnityEngine;

// Kelas dasar: semua aksi battle punya Priority dan Execute.
public abstract class BattleAction
{
    // makin besar, makin duluan (Ramuan 1, Kabur 2)
    public virtual int Priority => 0;

    public abstract IEnumerator Execute(BattleSystem battle,
        MonsterInstance user, MonsterInstance target);

    protected static string WhoName(BattleSystem battle, MonsterInstance user)
    {
        bool wildEnemy = user == battle.Enemy && !battle.IsBoss;
        return (wildEnemy ? "Lawan " : "") + user.Data.DisplayName;
    }
}

// SERANG: power 14, meleset 6%
public class AttackAction : BattleAction
{
    protected virtual int Power => 14;
    protected virtual string Move(MonsterInstance user) => null;   // null = serang biasa
    protected virtual float ElementBonus(MonsterInstance user, MonsterInstance target) => 1f;

    public override IEnumerator Execute(BattleSystem battle,
        MonsterInstance user, MonsterInstance target)
    {
        string who = WhoName(battle, user);
        string move = Move(user);

        yield return battle.Say(move == null
            ? $"{who} menyerang!"
            : $"{who} memakai {move}!", 0.7f);

        battle.RaiseAttacked(user);                  // animasi menerjang
        yield return new WaitForSeconds(0.2f);

        if (Random.value < 0.06f)
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

// JURUS: power 22 x elemen, dibatasi 3x per battle
public class SkillAction : AttackAction
{
    protected override int Power => 22;
    protected override string Move(MonsterInstance user) => user.Data.SkillName;
    protected override float ElementBonus(MonsterInstance user, MonsterInstance target)
        => ElementChart.Multiplier(user.Data.Element, target.Data.Element);
}

// RAMUAN: pulih 40% MaxHP, prioritas 1
public class PotionAction : BattleAction
{
    public override int Priority => 1;

    public override IEnumerator Execute(BattleSystem battle,
        MonsterInstance user, MonsterInstance target)
    {
        if (!battle.Party.UsePotion())
        {
            yield return battle.Say("Ramuan habis!", 0.7f);
            yield break;
        }

        int healed = user.Heal(Mathf.RoundToInt(user.MaxHP * 0.4f));
        yield return battle.Say(
            $"{user.Data.DisplayName} memakai Ramuan dan pulih {healed} HP.", 0.9f);
    }
}

// KABUR: 70%, boss tidak bisa, prioritas 2
public class RunAction : BattleAction
{
    public override int Priority => 2;

    public override IEnumerator Execute(BattleSystem battle,
        MonsterInstance user, MonsterInstance target)
    {
        if (battle.IsBoss)
        {
            yield return battle.Say("Tidak bisa kabur dari penguasa Gua Petir!", 0.9f);
            yield break;
        }

        if (Random.value < 0.7f)
        {
            yield return battle.Say("Kamu berhasil kabur!", 0.8f);
            battle.Escape();
        }
        else
        {
            yield return battle.Say("Gagal kabur!", 0.8f);
        }
    }
}