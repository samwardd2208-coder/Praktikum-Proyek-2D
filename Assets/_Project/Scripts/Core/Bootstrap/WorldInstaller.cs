using UnityEngine;

public class WorldInstaller : SceneInstaller
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private PartnerFollower follower;
    [SerializeField] private Transform respawnPoint;            // depan Batu Simpan
    [SerializeField] private MonsterData debugStarter;          // sementara, dihapus P6
    [SerializeField] private PartnerHud partnerHud;  // TAMBAHKAN INI

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
            if (partnerHud != null)
            {
                partnerHud.Construct(s.Party);
                partnerHud.SetAreaName("Desa Awal");
            }
            else
            {
                Debug.LogWarning("PartnerHud tidak tersambung di Inspector WorldInstaller!");
            }

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

    void Start()
    {
    // Cek apakah player baru saja kalah battle
        if (PlayerPrefs.GetInt("LostBattle", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat("LoseSpawnX");
            float y = PlayerPrefs.GetFloat("LoseSpawnY");
            
            // Pindahkan player ke posisi Batu Simpan
            if (player != null)
            {
                player.transform.position = new Vector3(x, y, 0);
            }
            
            // Reset flag
            PlayerPrefs.SetInt("LostBattle", 0);
            PlayerPrefs.Save(); // PENTING: Simpan ke disk
        }
    }
}
