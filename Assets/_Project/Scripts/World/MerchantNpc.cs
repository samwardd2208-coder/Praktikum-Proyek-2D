using System.Collections;
using UnityEngine;

public class MerchantNpc : Npc
{
    [SerializeField] private ToastView toast;
    [SerializeField] private int giftAmount = 3;

    protected override IEnumerator Talk()
    {
        // Cek apakah player sudah pernah dapat hadiah
        if (!flags.Has(StoryFlags.HasGift))
        {
            yield return dialogue.Say(npcName, "Petualang baru? Bawa 3 Ramuan ini. Gratis untuk hari pertama!");

            // Berikan Ramuan
            if (party != null)
            {
                // Pastikan method AddPotions ada di PlayerParty kamu (dari P5)
                // Jika error, coba gunakan: party.Potions += giftAmount; (jika setter-nya public)
                party.AddPotions(giftAmount); 
            }

            // Tandai sudah dapat hadiah
            flags.Set(StoryFlags.HasGift);

            // Munculkan Toast
            if (toast != null) toast.Show($"Kamu mendapat {giftAmount} Ramuan!");
        }
        else
        {
            yield return dialogue.Say(npcName, "Sudah dapat hadiah? Ayo belanja yang lain!");
        }
    }
}