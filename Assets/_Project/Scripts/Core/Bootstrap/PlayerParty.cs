using System;

public class PlayerParty
{
    public const int StartingPotions = 3;

    public MonsterInstance Partner { get; private set; }
    public int Potions { get; set; } = StartingPotions;

    // Event untuk perubahan Partner/HP
    public event Action PartyChanged;

    // Event khusus untuk Ramuan (Sesuai modul P5 Slide 40)
    public event Action PotionsChanged;

    public void SetPartner(MonsterInstance monster)
    {
        // DIPERBAIKI: lepas langganan partner lama supaya tidak menumpuk saat load/reset
        if (Partner != null) Partner.Changed -= OnPartnerChanged;

        Partner = monster;

        // Agar HUD HP otomatis update saat monster terkena damage
        if (Partner != null) Partner.Changed += OnPartnerChanged;

        PartyChanged?.Invoke();
    }

    private void OnPartnerChanged() => PartyChanged?.Invoke();

    public bool UsePotion()
    {
        if (Potions <= 0) return false;

        Potions--;
        PotionsChanged?.Invoke();
        return true;
    }

    public void AddPotions(int amount)
    {
        Potions += amount;
        PotionsChanged?.Invoke();
    }

    // BARU: dipakai SaveMapper.Apply
    public void SetPotions(int amount)
    {
        Potions = amount < 0 ? 0 : amount;
        PotionsChanged?.Invoke();
    }

    // BARU: dipakai "Mulai baru" di layar judul (Ramuan 3, tanpa partner)
    public void Reset()
    {
        SetPartner(null);
        SetPotions(StartingPotions);
    }
}
