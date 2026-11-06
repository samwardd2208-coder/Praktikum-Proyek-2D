using UnityEngine;

public class WorldInstaller : SceneInstaller
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private MonsterData debugStarter;

    public override void Install(GameServices s)
    {
        player.Construct(s.Input);
        interactor.Construct(s.Input);

        // Inject service ke semua WildMonster di scene
        foreach (WildMonster wild in FindObjectsByType<WildMonster>())
        {
            wild.Construct(s.Battle, s.Scenes, s.Party, player.transform);
        }

        // Cheat: kasih partner starter kalau belum ada
        if (s.Party.Partner == null && debugStarter != null)
        {
            s.Party.SetPartner(new MonsterInstance(debugStarter, 3));
        }
    }
}