using System.Collections;
using UnityEngine;

// Versi P4: tampilkan musuh dari BattleSession, menang otomatis setelah 2 detik.
// P5 mengganti isinya dengan BattleSystem penuh.
public class BattleInstaller : SceneInstaller
{
    [SerializeField] private SpriteRenderer enemyRenderer;
    [SerializeField] private float autoWinSeconds = 2f;

    public override void Install(GameServices s)
    {
        MonsterInstance enemy = s.Battle.Enemy;
        if (enemy != null) enemyRenderer.sprite = enemy.Data.Frames[0];
        StartCoroutine(AutoWin(s));
    }

    private IEnumerator AutoWin(GameServices s)
    {
        yield return new WaitForSeconds(autoWinSeconds);
        s.Scenes.EndBattle(BattleResult.Win);
    }
}
