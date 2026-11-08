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

    private float Scale => 1f + (Level - 1) * 0.1f;
    public int MaxHP => Mathf.RoundToInt(Data.BaseHP * Scale) + Level * 2;
    public int Attack => Mathf.RoundToInt(Data.BaseATK * Scale);
    public int Defense => Mathf.RoundToInt(Data.BaseDEF * Scale);
    public int Speed => Mathf.RoundToInt(Data.BaseSPD * Scale);
    public int ExpToNext => Level * 10;
    public bool IsFainted => CurrentHP <= 0;

    public MonsterInstance(MonsterData data, int level)
    {
        Data = data;
        Level = Mathf.Max(1, level);
        CurrentHP = MaxHP;
    }

    public void TakeDamage(int amount)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        Changed?.Invoke();
    }

    public int Heal(int amount)                       
    {
        int before = CurrentHP;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        Changed?.Invoke();
        return CurrentHP - before;
    }

    public void FullHeal()
    {
        CurrentHP = MaxHP;
        Changed?.Invoke();
    }

    public void Restore(int exp, int hp)           
    {
        Exp = Mathf.Max(0, exp);
        CurrentHP = Mathf.Clamp(hp, 1, MaxHP);
        Changed?.Invoke();
    }

    public List<GrowthEvent> GainExp(int amount)
    {
        var events = new List<GrowthEvent>();
        Exp += amount;

        while (Exp >= ExpToNext)
        {
            Exp -= ExpToNext;
            MonsterData before = Data;
            int oldMax = MaxHP;
            Level++;
            CurrentHP += MaxHP - oldMax;
            events.Add(new GrowthEvent(GrowthType.LevelUp, before, before,
                $"{before.DisplayName} naik ke Lv {Level}!"));

            if (Data.EvolvesTo != null && Data.EvolveLevel > 0 && Level >= Data.EvolveLevel)
            {
                oldMax = MaxHP;
                Data = Data.EvolvesTo;
                CurrentHP += MaxHP - oldMax;
                events.Add(new GrowthEvent(GrowthType.Evolved, before, Data,
                    $"{before.DisplayName} berevolusi menjadi {Data.DisplayName}!"));
            }
        }

        Changed?.Invoke();
        return events;
    }
}