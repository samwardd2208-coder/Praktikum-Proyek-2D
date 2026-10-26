using System;
using UnityEngine;

public class MonsterInstance // Tambahkan ": IDamageable" jika interface-nya sudah ada
{
    public MonsterData Data { get; private set; }
    public int Level { get; private set; }
    public int CurrentHP { get; private set; }
    public event Action Changed;                // P5: HUD

    private float Scale => 1f + (Level - 1) * 0.1f;
    public int MaxHP => Mathf.RoundToInt(Data.BaseHP * Scale) + Level * 2;
    public int Attack => Mathf.RoundToInt(Data.BaseATK * Scale);
    public int ExpToNext => Level * 10;

    // Constructor untuk inisialisasi monster
    public MonsterInstance(MonsterData data, int level = 1)
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

    public void GainExp(int amount)
    {
        // Tambah exp atau langsung simulasi naik level berdasarkan jumlah exp
        // Sesuai modul: jika diberi 100 EXP pada Lv 4 (ExpToNext = 40), maka bisa naik beberapa level
        int remainingExp = amount;
    
        while (remainingExp >= ExpToNext)
        {
            remainingExp -= ExpToNext;
            Level++;
            Debug.Log($"{Data.DisplayName} naik ke Lv {Level}!");

            // Cek apakah level memenuhi syarat evolusi
            // Pastikan MonsterData punya properti EvolveLevel dan EvolvesTo
            if (Data != null && Data.EvolvesTo != null && Level >= 5) // Sesuaikan syarat level evolusi jika ada di MonsterData
            {
                string oldName = Data.DisplayName;
                Data = Data.EvolvesTo;
                Debug.Log($"{oldName} berevolusi menjadi {Data.DisplayName}!");
            }
        }
    }
}