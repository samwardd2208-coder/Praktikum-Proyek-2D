using UnityEngine;
using System.Collections;

public class BattleInstaller : SceneInstaller
{
    [Header("UI & Latar")]
    [SerializeField] private BattleView view;
    [SerializeField] private Sprite forestBackground;
    [SerializeField] private Sprite caveBackground;
    [SerializeField] private SpriteRenderer background;

    // BARU: Slot untuk BGM Battle
    [Header("Audio BGM Battle")]
    [SerializeField] private AudioClip battleMusic;
    [SerializeField] private AudioClip bossMusic;

    public override void Install(GameServices s)
    {
        // 1. Ganti latar belakang battle
        if (background != null)
        {
            background.sprite = s.Battle.IsBoss ? caveBackground : forestBackground;
        }

        // 2. BARU: Putar BGM battle (boss atau biasa)
        if (s.Audio != null)
        {
            AudioClip clip = s.Battle.IsBoss ? bossMusic : battleMusic;
            if (clip != null) 
            {
                s.Audio.PlayBattleMusic(clip);
            }
        }

        // 3. Inisialisasi BattleSystem
        var battle = new BattleSystem();
        battle.Construct(s.Party, s.Battle, s.Scenes);

        // 4. Hubungkan dengan View
        new BattlePresenter(battle, view);

        // 5. PENTING: Dengarkan event 'Ended' untuk kembali ke dunia
        battle.Ended += (result) =>
        {
            // Saat battle selesai (Menang/Kalah/Kabur), kembali ke Scn_World
            if (s.Scenes is SceneFlow sceneFlow)
            {
                sceneFlow.EndBattle(result);
            }
        };

        // 6. Mulai battle
        StartCoroutine(battle.Begin());
    }
}