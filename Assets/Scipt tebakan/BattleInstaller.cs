using UnityEngine;

public class BattleInstaller : SceneInstaller
{
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Sprite forestBackground;
    [SerializeField] private Sprite caveBackground;
    [SerializeField] private BattleSystem battle;
    [SerializeField] private BattleView view;

    public override void Install(GameServices s)
    {
        background.sprite = s.Battle.IsBoss ? caveBackground : forestBackground;
        
        // DEBUG: Cek apakah Enemy null sebelum construct
        if (s.Battle.Enemy == null)
        {
            Debug.LogError("[BattleInstaller] ERROR: BattleSession.Enemy adalah NULL!");
            Debug.LogError("[BattleInstaller] Pastikan Enemy di-assign sebelum Install() dipanggil.");
            return; // Stop agar tidak crash
        }
        
        Debug.Log($"[BattleInstaller] Battle dimulai vs {s.Battle.Enemy.Data.DisplayName} Lv{s.Battle.Enemy.Level}");
        
        battle.Construct(s.Party, s.Battle, s.Scenes);
        new BattlePresenter(battle, view);
        battle.Begin();
    }
}