using System;
using UnityEngine;

public enum BattleResult { None, Win, Lose, Flee }

public class BattleSession
{
    public const float SafeSeconds = 2.2f;
    private Action<BattleResult> onFinished;
    private float safeUntil;

    public MonsterInstance Enemy { get; private set; }
    public bool IsBoss { get; private set; }
    public bool IsSafe => Time.time < safeUntil;
    public event Action<BattleResult, bool> Ended;   // (hasil, boss?)

    public void Begin(MonsterInstance enemy, bool isBoss, Action<BattleResult> callback)
    {
        Enemy = enemy;
        IsBoss = isBoss;
        onFinished = callback;
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
