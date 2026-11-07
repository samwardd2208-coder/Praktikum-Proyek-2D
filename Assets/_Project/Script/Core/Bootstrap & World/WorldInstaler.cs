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

        s.Battle.Ended += OnBattleEnded;
    }

    void OnDestroy()
    {
        if (services != null) services.Battle.Ended -= OnBattleEnded;
    }

    // Kalah -> pemain dipindah ke depan Batu Simpan. (Boss kalah -> ending, P7.)
    private void OnBattleEnded(BattleResult result, bool wasBoss)
    {
        if (result == BattleResult.Lost && savePoint != null)
            player.transform.position = savePoint.position + Vector3.down;
    }
}