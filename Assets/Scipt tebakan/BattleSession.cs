using System;
using UnityEngine;

// Penghubung dunia <-> battle: siapa musuhnya, siapa yang harus dikabari saat selesai,
// dan "waktu aman" setelah battle.
public class BattleSession
{
    public const float SafeSeconds = 2.2f;
    private Action<BattleResult> onFinished;
    private float safeUntil;

    public MonsterInstance Enemy { get; private set; }
    public bool IsBoss { get; private set; }
    public bool IsActive => Enemy != null;
    public bool IsSafe => Time.time < safeUntil;
    public event Action<BattleResult, bool> Ended;  // (hasil, boss?)
    
    
    public MonsterData EnemyData;
    public int EnemyLevel;

    public void Begin(MonsterInstance enemy, bool isBoss, Action<BattleResult> finished)
    {
        Enemy = enemy;
        IsBoss = isBoss;
        onFinished = finished;
    }

    public void Finish(BattleResult result)
    {
        Action<BattleResult> callback = onFinished;
        bool wasBoss = IsBoss;
        onFinished = null; Enemy = null; IsBoss = false;
        safeUntil = Time.time + SafeSeconds;
        callback?.Invoke(result);          // monster yang memulai
        Ended?.Invoke(result, wasBoss);    // dunia (P7: ending)
    }
}
