using System.Collections;
using UnityEngine;

public class AttackAction : BattleAction
{
    public override int Priority => 0; // Prioritas paling rendah (jalan duluan jika speed sama)

    public override IEnumerator Execute(BattleSystem battle, 
        MonsterInstance user, MonsterInstance target)
    {
        string who = (user == battle.Enemy && !battle.IsBoss ? "Lawan " : "") 
            + user.Data.DisplayName;
        
        // 1. Tampilkan pesan
        yield return battle.Say($"{who} menyerang!", 0.7f);
        
        // 2. Panggil animasi menerjang
        battle.RaiseAttacked(user);
        yield return new WaitForSeconds(0.2f);
        
        // 3. Cek apakah serangan meleset (6% kemungkinan)
        if (Random.value < 0.06f)
        {
            battle.RaiseMissed();
            yield return battle.Say("Serangan meleset!", 0.8f);
            yield break; // Berhenti di sini, tidak ada damage
        }

        // 4. Hitung damage
        int damage = DamageFormula.Calculate(user, target, 14, 
            1f, out bool critical);
        
        // 5. Kurangi HP target
        target.TakeDamage(damage);
        battle.RaiseHit(target, damage, critical, null);
        
        yield return new WaitForSeconds(0.45f);
        
        // 6. Pesan tambahan
        if (critical) yield return battle.Say("Serangan kritis!", 0.7f);
    }
}