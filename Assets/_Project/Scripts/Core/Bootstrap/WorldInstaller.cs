using UnityEngine;
using System.Collections;

public class WorldInstaller : SceneInstaller
{
    [Header("Referensi Scene")]
    [SerializeField] private PlayerMovement player;
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private PartnerFollower follower;
    [SerializeField] private Transform respawnPoint;      // depan Batu Simpan
    [SerializeField] private MonsterData debugStarter;    // sisa P5, tidak dipakai lagi
    [SerializeField] private PartnerHud partnerHud;

    [Header("Katalog Species (untuk Load partner)")]
    [SerializeField] private MonsterData[] speciesCatalog;

    [Header("BGM Area Awal (opsional)")]
    [SerializeField] private AudioClip areaStartMusic;    // mis. BGM_Village

    private BattleSession battle;
    private DialogueRunner dialogueRunner;
    private ScreenFader screenFader;

    public override void Install(GameServices s)
    {
        // 1. Player & partner
        player.Construct(s.Input);
        interactor.Construct(s.Input);
        follower.Construct(s.Party, player);

        // 2. Monster liar
        foreach (WildMonster wild in FindObjectsByType<WildMonster>(FindObjectsSortMode.None))
            wild.Construct(s.Battle, s.Scenes, s.Party, player.transform);

        // 3. Ambil data save DULU (sebelum NPC dibangun),
        //    supaya GuardNpc langsung menepi saat OnConstructed.
        SaveData load = (s.Save != null) ? s.Save.PendingLoad : null;
        if (load != null)
        {
            s.Flags.Load(load.flags);
            s.Party.SetPotions(load.potions);
            RestorePartner(s, load);
        }

        // 4. Suntikkan dependencies ke DialogueRunner, NPC, SaveStone, AreaZone
        dialogueRunner = FindAnyObjectByType<DialogueRunner>();
        if (dialogueRunner != null)
        {
            // Berikan AudioService ke DialogueRunner (untuk SFX blip/menu)
            dialogueRunner.Construct(s.Input, s.Audio);
            s.Dialogue = dialogueRunner;
        }
        s.Player = player.transform;

        foreach (Npc npc in FindObjectsByType<Npc>(FindObjectsSortMode.None))
            npc.Construct(dialogueRunner, s.Flags, s.Party, s.Audio);

        foreach (SaveStone stone in FindObjectsByType<SaveStone>(FindObjectsSortMode.None))
            stone.Construct(s, player.transform);

        // BARU: suntik AreaZone untuk BGM per area
        foreach (AreaZone zone in FindObjectsByType<AreaZone>(FindObjectsSortMode.None))
            zone.Construct(s);


        // 5. HUD
        if (partnerHud != null)
        {
            partnerHud.Construct(s.Party);
            partnerHud.SetAreaName("Desa Awal");
        }

        // 6. Daftarkan Toast global
        ToastView toast = FindAnyObjectByType<ToastView>(FindObjectsInactive.Include);
        if (toast != null) s.Toast = toast;

        // 7. Battle
        battle = s.Battle;
        battle.Ended += OnBattleEnded;

        screenFader = FindAnyObjectByType<ScreenFader>();

        // 8. LOAD GAME atau INTRO
        if (load != null)
        {
            // --- LOAD: pulihkan posisi, lewati intro ---
            player.transform.position = new Vector3(load.playerX, load.playerY, 0);
            s.Save.PendingLoad = null; // hapus agar tidak load terus-menerus
            if (s.Toast != null) s.Toast.Show("Perjalanan dilanjutkan");

            // Putar BGM area sesuai posisi load (jika ada)
            if (s.Audio != null && areaStartMusic != null)
                s.Audio.PlayWorldMusic(areaStartMusic);
        }
        else if (!s.Flags.Has(StoryFlags.IntroSeen))
        {
            // --- MAIN BARU: tahan layar hitam, jalankan narasi ---
            s.Scenes.HoldFade = true;
            StartCoroutine(Intro(s));
        }
        else
        {
            // Intro sudah pernah dilihat -> putar BGM area awal
            if (s.Audio != null && areaStartMusic != null)
                s.Audio.PlayWorldMusic(areaStartMusic);
        }
    }

    // Pulihkan partner dari save (species + level + exp + hp)
    private void RestorePartner(GameServices s, SaveData load)
    {
        if (string.IsNullOrEmpty(load.partnerSpecies)) return;
        MonsterData data = FindSpecies(load.partnerSpecies);
        if (data == null)
        {
            Debug.LogWarning("WorldInstaller: species tidak ada di catalog: " + load.partnerSpecies);
            return;
        }
        MonsterInstance mon = new MonsterInstance(data, 1);
        mon.Restore(load.partnerLevel, load.partnerExp, load.partnerHp);
        s.Party.SetPartner(mon);
    }

    private MonsterData FindSpecies(string key)
    {
        if (speciesCatalog == null) return null;
        foreach (MonsterData d in speciesCatalog)
            if (d != null && (d.name == key || d.DisplayName == key)) return d;
        return null;
    }

    // --- Narasi pembuka di atas layar hitam ---
    private IEnumerator Intro(GameServices s)
    {
        s.Input.Lock();
        yield return null; // beri 1 frame supaya semua komponen siap

        if (dialogueRunner != null)
        {
            dialogueRunner.Run(IntroLines());
            yield return null;
            while (dialogueRunner.IsRunning) yield return null;
        }
        else
        {
            Debug.LogWarning("WorldInstaller: DialogueRunner tidak ditemukan, narasi dilewati.");
        }

        s.Flags.Set(StoryFlags.IntroSeen);

        // Layar hitam memudar ke dunia, lalu input dibuka
        yield return s.Scenes.ReleaseFade();

        // Setelah intro selesai, putar BGM area awal
        if (s.Audio != null && areaStartMusic != null)
            s.Audio.PlayWorldMusic(areaStartMusic);
    }

    private IEnumerator IntroLines()
    {
        yield return dialogueRunner.Narrate("Di ujung Pulau Batam, terdapat desa kecil yang damai...");
        yield return dialogueRunner.Narrate("Namun, hutan di utara penuh dengan monster liar yang mengancam.");
    }

    // --- Kalah battle -> kembali ke depan Batu Simpan ---
    private void OnBattleEnded(BattleResult result, bool wasBoss)
    {
        if (result == BattleResult.Lose && respawnPoint != null)
            player.transform.position = respawnPoint.position;
    }

    private void OnDestroy()
    {
        if (battle != null) battle.Ended -= OnBattleEnded;
    }
}