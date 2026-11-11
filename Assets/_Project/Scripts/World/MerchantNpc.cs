using System.Collections;
using UnityEngine;

public class MerchantNpc : Npc
{
    [Header("UI & Hadiah")]
    [SerializeField] private ToastView toast;
    [SerializeField] private int giftAmount = 3;

    [Header("Audio SFX")]
    [SerializeField] private AudioClip healClip; // ← Slot untuk SFX_Heal

    protected override IEnumerator Talk()
    {
        // Cek apakah player sudah pernah dapat hadiah
        if (!flags.Has(StoryFlags.HasGift))
        {
            yield return dialogue.Say(npcName, "Petualang baru? Bawa 3 Ramuan ini. Gratis untuk hari pertama!");
            
            // Berikan Ramuan
            if (party != null)
            {
                party.AddPotions(giftAmount); 
            }

            // BARU: Putar suara SFX_Heal saat dapat ramuan
            if (audio != null && healClip != null)
            {
                audio.PlaySfx(healClip);
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