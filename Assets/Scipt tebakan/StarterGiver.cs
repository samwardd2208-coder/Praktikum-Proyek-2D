using System.Collections;
using System.Linq;
using UnityEngine;

public class StarterGiver : Npc
{
    [SerializeField] private MonsterData[] starters;     // Pyrit, Blubi, Sprig
    [SerializeField] private int starterLevel = 3;
    [SerializeField] private AudioClip getClip;          // SFX_LevelUp

    private string[] options;

    protected override void OnConstructed()
    {
        options = starters.Select(m => $"{m.DisplayName} ({m.Element})").ToArray();
    }

    protected override IEnumerator Talk()
    {
        if (Flags.Has(StoryFlags.HasStarter))
        {
            yield return Dialogue.Say(NpcName, "Rawat partnermu baik-baik. Batu Simpan di sebelahku siap membantu.");
            yield break;
        }

        yield return Dialogue.Say(NpcName, "Ah, kamu datang juga! Hutan di utara penuh monster liar.");
        yield return Dialogue.Say(NpcName,
            "Tidak ada yang boleh masuk hutan tanpa partner. Pilih satu dari tiga ini.");

        MonsterData pick = null;
        while (pick == null)          // ulangi sampai "Ya"
        {
            int choice = 0, sure = 0;
            yield return Dialogue.Choose(NpcName, "Siapa yang akan menjadi partnermu?", options, c => choice = c);
            MonsterData m = starters[choice];
            yield return Dialogue.Choose(NpcName, $"{m.DisplayName} tipe {m.Element}. Yakin?",
                                         new[] { "Ya", "Pilih lagi" }, c => sure = c);
            if (sure == 0) pick = m;
        }

        Services.Party.SetPartner(new MonsterInstance(pick, starterLevel));   // Lv 3
        Flags.Set(StoryFlags.HasStarter);                                      // Penjaga bergeser
        if (Services.Audio != null) Services.Audio.PlaySfx(getClip, 0f);

        yield return Dialogue.Say("", $"Kamu mendapat {pick.DisplayName}!");
        yield return Dialogue.Say(NpcName,
            "Rawat dia baik-baik. Menang bertarung membuatnya naik level, lalu berevolusi.");
        yield return Dialogue.Say(NpcName,
            "Batu Simpan di sebelahku menyimpan perjalananmu dan memulihkan partnermu.");
        Services.Toast.Show("Penjaga Batas sudah membuka jalan ke hutan");
    }
}