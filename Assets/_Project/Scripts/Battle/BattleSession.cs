using System;
using UnityEngine;

public enum BattleResult { Win, Lose, Flee }

public class BattleSession
{
    public const float SafeSeconds = 2.2f;
    private Action<BattleResult> onFinished;
    private float safeUntil;

    public MonsterInstance Enemy { get; private set; }
    public bool IsBoss { get; private set; }
    public bool IsSafe => Time.time < safeUntil;
    
    public event Action<BattleResult, bool> Ended;

    public void Begin(MonsterInstance enemy, bool isBoss, Action<BattleResult> onFinish)
    {
        Enemy = enemy;
        IsBoss = isBoss;
        onFinished = onFinish;
    }

    public void Finish(BattleResult result)
    {
        Action<BattleResult> callback = onFinished;
        bool wasBoss = IsBoss;
        
        onFinished = null; 
        Enemy = null; 
        IsBoss = false;
        safeUntil = Time.time + SafeSeconds;
        
        callback?.Invoke(result);
        Ended?.Invoke(result, wasBoss);
    }
}