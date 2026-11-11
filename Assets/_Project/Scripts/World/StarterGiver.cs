using System.Collections;
using UnityEngine;

// PERHATIKAN: Sekarang StarterGiver mewarisi Npc, bukan MonoBehaviour!
public class StarterGiver : Npc 
{
    [Header("Data Starter")]
    [SerializeField] private MonsterData[] starterOptions;
    [SerializeField] private int starterLevel = 3;

    [Header("Audio SFX")]
    [SerializeField] private AudioClip levelUpClip;

    // Override method Talk() dari class induk Npc
    protected override IEnumerator Talk()
    {
        // Jika player sudah punya partner, beri dialog biasa
        if (flags.Has(StoryFlags.HasStarter))
        {
            yield return dialogue.Say(npcName, "Kamu sudah punya partner. Ayo jelajahi hutan di utara!");
            yield break;
        }

        yield return dialogue.Say(npcName, "Ini dia partner pertamamu. Pilih dengan bijak!");

        // Siapkan pilihan monster
        string[] options = new string[starterOptions.Length];
        for (int i = 0; i < starterOptions.Length; i++)
            options[i] = starterOptions[i].DisplayName;

        int choice = -1;
        yield return dialogue.Choose(npcName, "Siapa yang akan menjadi partnermu?", options, (index) => choice = index);

        if (choice >= 0 && choice < starterOptions.Length)
        {
            MonsterData chosenData = starterOptions[choice];
            MonsterInstance monster = new MonsterInstance(chosenData, starterLevel);
            
            // 1. Berikan Partner
            party.SetPartner(monster);
            flags.Set(StoryFlags.HasStarter);

            // 2. Mainkan SFX_LevelUp
            if (audio != null && levelUpClip != null)
            {
                audio.PlaySfx(levelUpClip);
            }

            yield return dialogue.Say(npcName, $"{chosenData.DisplayName} tipe {chosenData.Element}. Yakin?");
            yield return dialogue.Say(npcName, "Ayo jelajahi hutan di utara!");
        }
    }
}