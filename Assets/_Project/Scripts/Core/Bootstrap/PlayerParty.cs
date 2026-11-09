using System;

public class PlayerParty
{
    public MonsterInstance Partner { get; private set; }
    public int Potions { get; private set; } = 3;

    // Event untuk perubahan Partner/HP
    public event Action PartyChanged;
    
    // Event khusus untuk Ramuan (Sesuai modul P5 Slide 40)
    public event Action PotionsChanged;

    public void SetPartner(MonsterInstance monster)
    {
        Partner = monster;
        
        // Agar HUD HP otomatis update saat monster terkena damage
        if (Partner != null) 
        {
            Partner.Changed += () => PartyChanged?.Invoke();
        }
        
        PartyChanged?.Invoke();
    }

    public bool UsePotion()
    {
        if (Potions <= 0) return false;
        
        Potions--;
        PotionsChanged?.Invoke(); // Panggil event khusus ramuan
        return true;
    }

    public void AddPotions(int amount)
    {
        Potions += amount;
        PotionsChanged?.Invoke();
    }
}