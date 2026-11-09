using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterInstance : IDamageable
{
    public MonsterData Data { get; private set; }
    public int Level { get; private set; }
    public int Exp { get; private set; }
    public int CurrentHP { get; private set; }
    public event Action Changed;  
                 // P5: HUD

    public MonsterInstance(MonsterData data, int level)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        Data = data;
        Level = Mathf.Max(1, level);
        CurrentHP = MaxHP;
    }

    private float Scale => 1f + (Level - 1) * 0.1f;
    public int MaxHP => Mathf.RoundToInt(Data.BaseHP * Scale) + Level * 2;
    public int Attack => Mathf.RoundToInt(Data.BaseATK * Scale);
    public int Defense => Mathf.RoundToInt(Data.BaseDEF * Scale);
    public int Speed => Mathf.RoundToInt(Data.BaseSPD * Scale);
    public int ExpToNext => Level * 10;
    public bool IsFainted => CurrentHP <= 0;

    public void TakeDamage(int amount)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        Changed?.Invoke();
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        Changed?.Invoke();
    }

    public void HealFull()
    {
        CurrentHP = MaxHP;
        Changed?.Invoke();
    }
    public void Restore(int exp, int hp)
{
    Exp = exp;
    CurrentHP = Mathf.Max(0, hp);
    Changed?.Invoke();      // HUD ikut refresh
}

    public List<GrowthEvent> GainExp(int amount)
    {
        var events = new List<GrowthEvent>();
        Exp += Mathf.Max(0, amount);

        while (Exp >= ExpToNext)
        {
            int maxBefore = MaxHP;
            Exp -= ExpToNext;
            Level++;
            events.Add(new GrowthEvent(GrowthEventType.LevelUp,
                $"{Data.DisplayName} naik ke Lv {Level}!"));

            if (Data.EvolvesTo != null && Data.EvolveLevel > 0 && Level >= Data.EvolveLevel)
            {
                string oldName = Data.DisplayName;
                Data = Data.EvolvesTo;
                events.Add(new GrowthEvent(GrowthEventType.Evolved,
                    $"{oldName} berevolusi menjadi {Data.DisplayName}!"));
            }

            CurrentHP = Mathf.Min(MaxHP, CurrentHP + (MaxHP - maxBefore));
        }

        Changed?.Invoke();
        return events;
    }
}