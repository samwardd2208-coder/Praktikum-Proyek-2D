using UnityEngine;

/// <summary>Rumus damage. static: tanpa state, mudah diuji.</summary>
public static class DamageFormula
{
    public const float CriticalChance = 0.08f;

    // multiplier = elemen x 0,8 untuk monster liar biasa (lihat BattleSystem.DamageScale)
    public static int Calculate(MonsterInstance user, MonsterInstance target, int power,
                                float multiplier, out bool critical)
    {
        critical = Random.value < CriticalChance;
        float raw = user.Attack * power / (target.Defense * 0.5f + 10f);
        raw *= multiplier * (critical ? 1.5f : 1f) * Random.Range(0.9f, 1.1f);
        return Mathf.Max(1, Mathf.FloorToInt(raw));
    }
}
