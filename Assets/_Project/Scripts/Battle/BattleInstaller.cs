using UnityEngine;
using System.Collections;

public class BattleInstaller : SceneInstaller
{
    [SerializeField] private BattleView view;
    [SerializeField] private Sprite forestBackground;
    [SerializeField] private Sprite caveBackground;
    [SerializeField] private SpriteRenderer background;

    public override void Install(GameServices s)
    {
        // 1. Ganti latar belakang battle
        if (background != null)
        {
            background.sprite = s.Battle.IsBoss ? caveBackground : forestBackground;
        }

        // 2. Inisialisasi BattleSystem
        var battle = new BattleSystem();
        battle.Construct(s.Party, s.Battle, s.Scenes);
        
        // 3. Hubungkan dengan View
        new BattlePresenter(battle, view);
        
        // 4. PENTING: Dengarkan event 'Ended' untuk kembali ke dunia
        battle.Ended += (result) =>
        {
            // Saat battle selesai (Menang/Kalah/Kabur), kembali ke Scn_World
            if (s.Scenes is SceneFlow sceneFlow)
            {
                sceneFlow.EndBattle(result);
            }
        };
        
        // 5. Mulai battle
        StartCoroutine(battle.Begin());
    }
}