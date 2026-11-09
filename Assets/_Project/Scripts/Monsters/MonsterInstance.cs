using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterInstance : IDamageable
{
    public MonsterData Data { get; private set; }
    public int Level { get; private set; }
    public int CurrentHP { get; private set; }
    public event Action Changed;

    private float Scale => 1f + (Level - 1) * 0.1f;
    
    // --- STATISTIK MONSTER ---
    public int MaxHP  => Mathf.RoundToInt(Data.BaseHP * Scale) + Level * 2;
    public int Attack => Mathf.RoundToInt(Data.BaseATK * Scale);
    public int Defense => Mathf.RoundToInt(Data.BaseDEF * Scale); // TAMBAHAN (Biar DamageFormula tidak merah)
    public int Speed  => Mathf.RoundToInt(Data.BaseSPD * Scale);  // TAMBAHAN (Untuk urutan giliran)
    public int ExpToNext => Level * 10;

    public MonsterInstance(MonsterData data, int level)
    {
        Data = data;
        Level = level;
        CurrentHP = MaxHP;
    }

    public void TakeDamage(int amount)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        Changed?.Invoke();
    }

    // --- TAMBAHAN: Untuk memulihkan HP (Ramuan/Istirahat) ---
    public int Heal(int amount)
    {
        int before = CurrentHP;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        return CurrentHP - before;
    }

    // --- TAMBAHAN: Untuk naik level dan evolusi ---
    public IEnumerable<GrowthEvent> GainExp(int amount)
    {   
        Level++;
        CurrentHP = MaxHP;
        
        var events = new List<GrowthEvent>();
        events.Add(new GrowthEvent(GrowthType.LevelUp, this, this, 
            $"{Data.DisplayName} naik ke Lv {Level}!"));

        if (Data.EvolvesTo != null && Level >= Data.EvolveLevel)
        {
            var oldData = Data;
            Data = Data.EvolvesTo;
            events.Add(new GrowthEvent(GrowthType.Evolve, 
                new MonsterInstance(oldData, Level - 1), this,
                $"{oldData.DisplayName} berevolusi menjadi {Data.DisplayName}!"));
        }

        return events;
    }
}