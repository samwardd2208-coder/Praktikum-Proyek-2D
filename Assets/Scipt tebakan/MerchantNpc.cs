using System.Collections;
using UnityEngine;

public class MerchantNpc : Npc
{
    [SerializeField] private int giftAmount = 3;
    [SerializeField] private AudioClip healClip;       // SFX_Heal

    protected override IEnumerator Talk()
    {
        if (!Flags.Has(StoryFlags.Gift))
        {
            Services.Party.Potions += giftAmount;       // HUD Ramuan naik
            Flags.Set(StoryFlags.Gift);
            if (Services.Audio != null) Services.Audio.PlaySfx(healClip, 0f);
            yield return Dialogue.Say(NpcName, "Petualang baru? Bawa 3 Ramuan ini. Gratis untuk hari pertama!");
        }
        else
        {
            yield return Dialogue.Say(NpcName, "Hadiahnya sudah kuberikan tadi. Semoga beruntung di hutan!");
        }
    }
}