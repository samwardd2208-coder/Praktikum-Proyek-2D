using UnityEngine;

// Installer Scn_World: menyuntik service ke objek-objek dunia.
// Taruh di ROOT scene (SceneFlow hanya mencari installer di root).
public class WorldInstaller : SceneInstaller
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private PartnerFollower follower;
    [SerializeField] private Transform savePoint;          // Batu Simpan (tempat bangun setelah kalah)
    [SerializeField] private MonsterData debugStarter;     // sementara, dihapus P6
    [SerializeField] private PartnerHud hud;               // P5: HUD partner + Ramuan

    private GameServices services;

    public override void Install(GameServices s)
    {
        services = s;

        player.Construct(s.Input);
        interactor.Construct(s.Input);

        foreach (WildMonster wild in FindInScene<WildMonster>())
            wild.Construct(s.Battle, s.Scenes, s.Party, player.transform);

        if (follower != null)
            follower.Construct(s.Party, player);

        if (s.Party.Partner == null && debugStarter != null)
            s.Party.SetPartner(new MonsterInstance(debugStarter, 3));

        // P5: HUD
        if (hud != null)
        {
            hud.Bind(s.Party);
            AreaZone.AreaEntered += hud.SetArea;            // area
        }

        s.Battle.Ended += OnBattleEnded;
    }
    void Update()
    {
        // P5: HUD dunia disembunyikan selama battle
        if (hud != null && services != null)
            hud.gameObject.SetActive(!services.Battle.IsActive);
    }
    void OnDestroy()
    {
        if (services != null) services.Battle.Ended -= OnBattleEnded;
        if (hud != null) AreaZone.AreaEntered -= hud.SetArea;   // area
    }

    // Kalah -> partner pulih penuh, pemain dipindah ke depan Batu Simpan. (Boss kalah -> ending, P7.)
    private void OnBattleEnded(BattleResult result, bool wasBoss)
    {
        if (result != BattleResult.Lose) return;

        MonsterInstance partner = services.Party.Partner;
        if (partner != null) partner.Heal(partner.MaxHP);

        if (savePoint != null)
            player.transform.position = savePoint.position + Vector3.down;
    }
}