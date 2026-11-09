using System.Collections;
using UnityEngine;

public class PotionAction : BattleAction
{
    public override int Priority => 1;

    public override IEnumerator Execute(BattleSystem battle, 
        MonsterInstance user, MonsterInstance target)
    {
        // Cek apakah ramuan masih ada
        if (battle.Party.Potions <= 0)
        {
            yield return battle.Say("Ramuan habis!", 0.7f);
            yield break;
        }

        // Gunakan ramuan
        battle.Party.UsePotion();
        int healed = user.Heal(Mathf.RoundToInt(user.MaxHP * 0.4f)); // Pulih 40% MaxHP
        
        yield return battle.Say($"{user.Data.DisplayName} memakai Ramuan dan pulih {healed} HP!", 0.8f);
    }
}