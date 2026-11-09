using UnityEngine;
using System.Collections;

// Installer Scn_World: menyuntik service ke objek-objek dunia.
// Taruh di ROOT scene (SceneFlow hanya mencari installer di root).
public class WorldInstaller : SceneInstaller
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private PartnerFollower follower;
    [SerializeField] private Transform savePoint;          // Batu Simpan (tempat bangun setelah kalah)
    [SerializeField] private PartnerHud hud;               // P5: HUD partner + Ramuan

    private GameServices services;

    public override void Install(GameServices s)
    {
        services = s;
        s.Player = player.transform;                        // P6: dipakai SaveStone dan PauseMenu

        player.Construct(s.Input);
        interactor.Construct(s.Input);

        // P6: lanjutkan dari save. Dilakukan SEBELUM follower dibuat,
        // supaya partner langsung muncul di samping pemain, bukan di titik awal.
        bool resumed = false;
        if (s.Save.PendingLoad != null)
        {
            player.Teleport(s.Save.PendingLoad.playerPosition, Vector2.down);
            s.Save.PendingLoad = null;
            resumed = true;
        }

        // P6: dialogue dan flags ikut disuntik (auman boss + boss yang sudah kalah)
        foreach (WildMonster wild in FindInScene<WildMonster>())
            wild.Construct(s.Battle, s.Scenes, s.Party, player.transform, s.Dialogue, s.Flags);

        // P6: NPC, palang, dan Batu Simpan
        foreach (Npc npc in FindInScene<Npc>()) npc.Construct(s);
        foreach (FlagToggle toggle in FindInScene<FlagToggle>()) toggle.Construct(s.Flags);
        foreach (SaveStone stone in FindInScene<SaveStone>()) stone.Construct(s);

        if (follower != null)
            follower.Construct(s.Party, player);

        // (debugStarter dihapus: partner sekarang diberikan Tetua Rahmat)

        // P5: HUD
        if (hud != null)
        {
            hud.Bind(s.Party);
            AreaZone.AreaEntered += hud.SetArea;            // area
        }

        s.Battle.Ended += OnBattleEnded;

        // P6: toast Lanjutkan atau narasi pembuka
        if (resumed)
        {
            s.Toast.Show("Perjalanan dilanjutkan");
        }
        else if (!s.Flags.Has(StoryFlags.IntroSeen))
        {
            s.Scenes.HoldFade = true;                       // tahan fade-in
            StartCoroutine(Intro());
        }
    }

    void Update()
    {
        if (services == null) return;

        // P6: waktu bermain (disimpan di save)
        services.Playtime += Time.deltaTime;

        // P5: HUD dunia disembunyikan selama battle
        if (hud != null)
            hud.gameObject.SetActive(!services.Battle.IsActive);
    }

    void OnDestroy()
    {
        if (services != null) services.Battle.Ended -= OnBattleEnded;
        if (hud != null) AreaZone.AreaEntered -= hud.SetArea;   // area
    }

    // ---------- P6: narasi pembuka ----------
    private IEnumerator Intro()
    {
        services.Dialogue.Run(IntroLines());
        yield return null;
        while (services.Dialogue.IsRunning) yield return null;
        services.Flags.Set(StoryFlags.IntroSeen);
        yield return services.Scenes.ReleaseFade();         // fade-in
        services.Toast.Show("Temui Tetua Rahmat (kanan jalan desa)");
    }

    private IEnumerator IntroLines()
    {
        yield return services.Dialogue.Narrate("Di ujung Pulau Batam ada desa kecil yang hidup berdampingan dengan monster.");
        yield return services.Dialogue.Narrate("Hari ini, seorang petualang baru tiba di desa itu.");   // tebakan: ganti dengan teks demo
    }

    // Kalah -> partner pulih penuh, pemain dipindah ke depan Batu Simpan. (Boss kalah -> ending, P7.)
    private void OnBattleEnded(BattleResult result, bool wasBoss)
    {
        // P6: boss dikalahkan -> catat flag (disimpan di save)
        if (result == BattleResult.Win && wasBoss)
            services.Flags.Set(StoryFlags.BossDefeated);

        if (result != BattleResult.Lose) return;

        MonsterInstance partner = services.Party.Partner;
        if (partner != null) partner.Heal(partner.MaxHP);

        if (savePoint != null)
            player.transform.position = savePoint.position + Vector3.down;
    }
}