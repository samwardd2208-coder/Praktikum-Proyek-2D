using System.Collections;
using UnityEngine;

public class RunAction : BattleAction
{
    public override int Priority => 2; // Prioritas tinggi, selalu jalan duluan

    public override IEnumerator Execute(BattleSystem battle, 
        MonsterInstance user, MonsterInstance target)
    {
        // Boss tidak bisa dikaburi
        if (battle.IsBoss)
        {
            yield return battle.Say("Tidak bisa kabur dari boss!", 0.8f);
            yield break;
        }

        yield return battle.Say($"{user.Data.DisplayName} mencoba kabur...", 0.6f);
        
        // 70% kemungkinan berhasil kabur
        if (Random.value < 0.7f)
        {
            yield return battle.Say("Berhasil kabur!", 0.7f);
            battle.ForceFlee(); // Memaksa battle selesai dengan status Flee
        }
        else
        {
            yield return battle.Say("Gagal kabur!", 0.7f);
        }
    }
}