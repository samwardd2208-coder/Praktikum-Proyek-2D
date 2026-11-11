using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterInstance : IDamageable
{
    public MonsterData Data { get; private set; }
    public int Level { get; private set; }
    public int CurrentHP { get; private set; }
    public int Exp { get; private set; }                 // BARU: dibutuhkan untuk save/load & bar EXP
    public event Action Changed;

    private float Scale => 1f + (Level - 1) * 0.1f;

    // --- STATISTIK MONSTER ---
    public int MaxHP  => Mathf.RoundToInt(Data.BaseHP * Scale) + Level * 2;
    public int Attack => Mathf.RoundToInt(Data.BaseATK * Scale);
    public int Defense => Mathf.RoundToInt(Data.BaseDEF * Scale);
    public int Speed  => Mathf.RoundToInt(Data.BaseSPD * Scale);
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

    // Memulihkan HP (Ramuan/Istirahat). Sekarang memanggil Changed agar HUD ikut berubah.
    public int Heal(int amount)
    {
        int before = CurrentHP;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        Changed?.Invoke();
        return CurrentHP - before;
    }

    // BARU: dipakai Batu Simpan
    public void HealFull()
    {
        CurrentHP = MaxHP;
        Changed?.Invoke();
    }

    // BARU: memulihkan keadaan dari file save
    public void Restore(int level, int exp, int hp)
    {
        Level = Mathf.Max(1, level);
        Exp = Mathf.Max(0, exp);
        CurrentHP = Mathf.Clamp(hp, 0, MaxHP);
        Changed?.Invoke();
    }

    // DIPERBAIKI: sekarang benar-benar memakai jumlah EXP.
    // Naik level selama Exp >= ExpToNext; evolusi bila Level >= EvolveLevel.
    // Contoh uji: Pyrit Lv 4 diberi 100 EXP -> Lv 5 -> berevolusi Pyrago -> Lv 6.
    public IEnumerable<GrowthEvent> GainExp(int amount)
    {
        var events = new List<GrowthEvent>();
        Exp += amount;

        while (Exp >= ExpToNext)
        {
            Exp -= ExpToNext;            // hitung sebelum Level bertambah
            Level++;
            CurrentHP = MaxHP;

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
        }

        Changed?.Invoke();
        return events;
    }
}
