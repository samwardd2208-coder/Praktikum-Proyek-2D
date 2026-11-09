using System;
using UnityEngine;

public enum BattleResult { Win, Lose, Flee }

// Pembawa data antar-scene (versi sederhana dari BattleSession di modul).
// Dunia -> isi Enemy -> Scn_Battle membaca Enemy -> Finish(hasil) -> dunia lanjut.
public static class BattleBridge
{
    public const float SafeSeconds = 2.2f;

    private static Action<BattleResult> onFinished;
    private static float safeUntil;

    public static MonsterInstance Enemy { get; private set; }
    public static bool IsBoss { get; private set; }
    public static bool InBattle => Enemy != null;
    public static bool IsSafe => Time.time < safeUntil;   // waktu aman setelah battle

    public static event Action Started;
    public static event Action<BattleResult, bool> Ended; // (hasil, boss?)

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        onFinished = null; safeUntil = 0f; Enemy = null; IsBoss = false;
        Started = null; Ended = null;
    }

    public static void Begin(MonsterInstance enemy, bool isBoss, Action<BattleResult> callback)
    {
        Enemy = enemy;
        IsBoss = isBoss;
        onFinished = callback;
        Started?.Invoke();
    }

    public static void Finish(BattleResult result)
    {
        Action<BattleResult> callback = onFinished;
        bool wasBoss = IsBoss;
        onFinished = null; Enemy = null; IsBoss = false;
        safeUntil = Time.time + SafeSeconds;
        callback?.Invoke(result);       // monster yang memulai battle
        Ended?.Invoke(result, wasBoss); // siapa saja yang peduli
    }
}
