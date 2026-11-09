using UnityEngine;
 
public class BattleInstaller : SceneInstaller
{
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Sprite forestBackground;
    [SerializeField] private Sprite caveBackground;
    [SerializeField] private BattleSystem battle;
    [SerializeField] private BattleView view;
 
    [Header("Audio (P6)")]
    [SerializeField] private AudioClip bgmBattle;
    [SerializeField] private AudioClip bgmBoss;
    [SerializeField] private BattleAudio battleAudio;     // opsional, boleh kosong
 
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
 
        // P6: musik battle / boss
        if (s.Audio != null) s.Audio.PlayBattleMusic(s.Battle.IsBoss ? bgmBoss : bgmBattle);
        if (battleAudio != null) battleAudio.Init(s);
 
        battle.Construct(s.Party, s.Battle, s.Scenes);
        new BattlePresenter(battle, view);
        battle.Begin();
    }
}