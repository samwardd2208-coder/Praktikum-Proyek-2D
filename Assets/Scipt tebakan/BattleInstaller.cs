using System.Collections;
using UnityEngine;

// Installer Scn_Battle versi P4: bukti bahwa DATA ikut pindah scene.
// Menampilkan musuh dari BattleSession.Enemy + partner, lalu menang otomatis setelah 2 detik.
// (P5 mengganti isinya dengan sistem battle penuh.)
public class BattleInstaller : SceneInstaller
{
    [SerializeField] private SpriteRenderer enemyRenderer;
    [SerializeField] private SpriteRenderer partnerRenderer;
    [SerializeField] private float autoWinSeconds = 2f;

    public override void Install(GameServices s)
    {
        MonsterInstance enemy = s.Battle.Enemy;
        MonsterInstance partner = s.Party.Partner;

        if (enemy == null)
        {
            Debug.LogWarning("BattleSession.Enemy kosong, battle dibatalkan.");
            s.Scenes.EndBattle(BattleResult.Fled);
            return;
        }

        Show(enemyRenderer, enemy);
        Show(partnerRenderer, partner);

        string info = $"[Battle] Musuh: {enemy.Data.DisplayName} Lv{enemy.Level} ({enemy.Data.Element}) HP {enemy.CurrentHP}/{enemy.MaxHP}";
        if (partner != null)
        {
            float mult = ElementChart.Multiplier(partner.Data.Element, enemy.Data.Element);
            info += $" | Partner: {partner.Data.DisplayName} Lv{partner.Level} ({partner.Data.Element}), serangan partner x{mult}";
        }
        Debug.Log(info);

        StartCoroutine(AutoWin(s));
    }

    private IEnumerator AutoWin(GameServices s)
    {
        yield return new WaitForSeconds(autoWinSeconds);
        s.Scenes.EndBattle(BattleResult.Won);
    }

    private static void Show(SpriteRenderer target, MonsterInstance monster)
    {
        if (target == null) return;
        Sprite[] frames = monster?.Data.Frames;
        target.enabled = frames != null && frames.Length > 0;
        if (target.enabled) target.sprite = frames[0];
    }
}
