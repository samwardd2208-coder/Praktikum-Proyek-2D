using System;
using System.Collections.Generic;
using UnityEngine;
 
public class MonsterInstance : IDamageable
{
    public MonsterData Data { get; private set; }
    public int Level { get; private set; }
    public int CurrentHP { get; private set; }
 
    private int exp;
    public int Exp => exp;
 
    public event Action Changed;
 
    private float Scale => 1f + (Level - 1) * 0.1f;
 
    public int MaxHP     => Mathf.RoundToInt(Data.BaseHP  * Scale) + Level * 2;
    public int Attack    => Mathf.RoundToInt(Data.BaseATK * Scale);
    public int Defense   => Mathf.RoundToInt(Data.BaseDEF * Scale);
    public int Speed     => Mathf.RoundToInt(Data.BaseSPD * Scale);
    public int ExpToNext => Level * 10;
 
    public MonsterInstance(MonsterData data, int level)
    {
        Data      = data;
        Level     = level;
        CurrentHP = MaxHP;
    }
 
    public void TakeDamage(int amount)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        Changed?.Invoke();
    }
 
    // Memulihkan HP, mengembalikan jumlah HP yang benar-benar pulih.
    public int Heal(int amount)
    {
        int before = CurrentHP;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + Mathf.Max(0, amount));
        Changed?.Invoke();
        return CurrentHP - before;
    }
 
    // Menambah EXP. Mengembalikan daftar kejadian (naik level / evolusi)
    // supaya battle bisa menampilkan animasinya satu per satu.
    public List<GrowthEvent> GainExp(int amount)
    {
        List<GrowthEvent> events = new List<GrowthEvent>();
        exp += amount;
 
        while (exp >= ExpToNext)
        {
            exp -= ExpToNext;
            int oldMax = MaxHP;
            MonsterData before = Data;
            Level++;
 
            string levelMessage = $"{before.DisplayName} naik ke Lv {Level}!";
            Debug.Log(levelMessage);
            events.Add(new GrowthEvent(GrowthType.LevelUp, before, before, levelMessage));
 
            if (Data.EvolvesTo != null && Level >= Data.EvolveLevel)
            {
                Data = Data.EvolvesTo;
                string evolveMessage = $"{before.DisplayName} berevolusi menjadi {Data.DisplayName}!";
                Debug.Log(evolveMessage);
                events.Add(new GrowthEvent(GrowthType.Evolution, before, Data, evolveMessage));
            }
 
            // HP sekarang ikut naik sebesar kenaikan HP maksimal
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + (MaxHP - oldMax));
        }
 
        Changed?.Invoke();
        return events;
    }
}
 