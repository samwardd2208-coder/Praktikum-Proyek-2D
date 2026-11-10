using System.Collections;
using UnityEngine;

public class StarterGiver : Npc
{
    [SerializeField] private MonsterData[] starters; // Seret 3 monster data di Inspector (Pyrit, Blubi, Voltcat)
    [SerializeField] private int starterLevel = 3;
    
    // Opsional: Jika kamu sudah buat ToastView di Praktikum 4, seret di sini. Jika belum, biarkan null.
    [SerializeField] private ToastView toast; 

    protected override IEnumerator Talk()
    {
        // 1. Jika player sudah punya starter, beri pesan biasa
        if (flags.Has(StoryFlags.HasStarter))
        {
            yield return dialogue.Say(npcName, "Sudah pilih partnermu? Ayo jelajahi hutan di utara!");
            yield break; // Keluar dari method
        }

        // 2. Dialog Pembuka
        yield return dialogue.Say(npcName, "Ah, kamu datang juga! Hutan di utara penuh monster liar.");
        yield return dialogue.Say(npcName, "Tidak ada yang boleh masuk hutan tanpa partner. Pilih satu dari tiga ini.");

        // 3. Loop Pilihan (Ulangi sampai player memilih "Ya")
        MonsterData pick = null;
        while (pick == null)
        {
            int choice = 0;
            int sure = 0;

            // Tampilkan pilihan monster
            string[] options = new string[starters.Length];
            for (int i = 0; i < starters.Length; i++) options[i] = starters[i].DisplayName;

            yield return dialogue.Choose(npcName, "Siapa yang akan menjadi partnermu?", options, c => choice = c);
            
            MonsterData m = starters[choice];
            
            // Konfirmasi
            yield return dialogue.Choose(npcName, $"{m.DisplayName} tipe {m.Element}. Yakin?", 
                new string[] { "Ya", "Pilih lagi" }, c => sure = c);
                
            if (sure == 0) pick = m; // Jika pilih "Ya" (index 0), keluar loop
        }

        // 4. Berikan Monster & Set Flag
        // GANTI DENGAN INI:
        if (party != null)
        {
            party.SetPartner(new MonsterInstance(pick, starterLevel));
        }

        flags.Set(StoryFlags.HasStarter); // PENTING: Set flag agar Penjaga bergeser!

        yield return dialogue.Say("", $"Kamu mendapat {pick.DisplayName}!");
        yield return dialogue.Say(npcName, "Rawat dia baik-baik. Menang bertarung membuatnya naik level, lalu berevolusi.");
        
        // Jika ada ToastView, aktifkan baris ini:
        if (toast != null) toast.Show("Penjaga Batas sudah membuka jalan ke hutan");
    }
}