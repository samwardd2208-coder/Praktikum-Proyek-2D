using System.Collections;
using UnityEngine;
 
// Versi P4 (placeholder): menampilkan musuh dari BattleSession,
// lalu menang otomatis. P5 mengganti isinya dengan battle penuh.
public class BattleInstaller : SceneInstaller
{
    [SerializeField] private SpriteRenderer enemyRenderer;
    [SerializeField] private float autoWinSeconds = 2f;
 
    public override void Install(GameServices s)
    {
        MonsterInstance enemy = s.Battle.Enemy;
 
        if (enemy == null)
        {
            Debug.LogWarning("BattleSession.Enemy kosong. Musuh tidak dikirim dari dunia.");
            StartCoroutine(AutoWin(s, 0.5f));
            return;
        }
 
        Debug.Log($"Battle: {enemy.Data.DisplayName} Lv{enemy.Level}");
        ShowEnemy(enemy.Data);
        StartCoroutine(AutoWin(s, autoWinSeconds));
    }
 
    private void ShowEnemy(MonsterData data)
    {
        if (enemyRenderer == null || data.Frames == null || data.Frames.Length == 0) return;
 
        // Monster tahap 1 diperbesar 3x, tahap 2 = 2.4x, tahap 3 = 2x (ukuran sama di layar)
        float width = data.Frames[0].rect.width;
        float scale = width <= 16f ? 3f : width <= 24f ? 2.4f : 2f;
        enemyRenderer.transform.localScale = new Vector3(scale, scale, 1f);
        enemyRenderer.sprite = data.Frames[0];
 
        StartCoroutine(IdleAnimation(data));
    }
 
    // Animasi diam: bergantian Idle0 dan Idle1
    private IEnumerator IdleAnimation(MonsterData data)
    {
        int frame = 0;
        while (true)
        {
            enemyRenderer.sprite = data.Frames[frame];
            frame = (frame + 1) % 2;
            yield return new WaitForSeconds(0.35f);
        }
    }
 
    private IEnumerator AutoWin(GameServices s, float delay)
    {
        yield return new WaitForSeconds(delay);
        s.Scenes.EndBattle(BattleResult.Win);
    }
}
 