using UnityEngine;

public class WorldInstaller : SceneInstaller
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private PartnerFollower follower;
    [SerializeField] private Transform respawnPoint;            // depan Batu Simpan
    [SerializeField] private MonsterData debugStarter;          // sementara, dihapus P6

    private BattleSession battle;

    public override void Install(GameServices s)
    {
        player.Construct(s.Input);
        interactor.Construct(s.Input);
        follower.Construct(s.Party, player);

        foreach (WildMonster wild in FindInScene<WildMonster>())
            wild.Construct(s.Battle, s.Scenes, s.Party, player.transform);

        if (s.Party.Partner == null && debugStarter != null)
            s.Party.SetPartner(new MonsterInstance(debugStarter, 3));

        battle = s.Battle;
        battle.Ended += OnBattleEnded;
    }

    private void OnBattleEnded(BattleResult result, bool wasBoss)
    {
        if (result == BattleResult.Lose && respawnPoint != null)
            player.transform.position = respawnPoint.position;
    }

    void OnDestroy()
    {
        if (battle != null) battle.Ended -= OnBattleEnded;
    }
}
