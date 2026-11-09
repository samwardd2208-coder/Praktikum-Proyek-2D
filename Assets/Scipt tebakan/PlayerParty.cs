using System;
 
// Isi tas pemain: satu partner + 3 Ramuan.
public class PlayerParty
{
    public const int StartingPotions = 3;
 
    public event Action Changed;            // partner berganti / Ramuan dipakai
    public event Action PotionsChanged;     // jumlah Ramuan berubah (dipakai HUD)
 
    public bool NeedsRespawn { get; set; }
 
    public MonsterInstance Partner { get; private set; }
 
    private int potions = StartingPotions;
    public int Potions
    {
        get => potions;
        set
        {
            potions = Math.Max(0, value);
            PotionsChanged?.Invoke();       // HUD "Ramuan ×N" ikut berubah
        }
    }
 
    public void SetPartner(MonsterInstance partner)
    {
        Partner = partner;
        Changed?.Invoke();
    }
 
    public bool UsePotion()
    {
        if (Potions <= 0) return false;
        Potions--;
        Changed?.Invoke();
        return true;
    }
 
    // P6: dipakai Pak Dagang (beli Ramuan) dan hadiah
    public void AddPotions(int amount)
    {
        Potions += amount;
    }
 
    // P6: Mulai baru -> Ramuan 3, tanpa partner
    public void Reset()
    {
        Partner = null;
        NeedsRespawn = false;
        Potions = StartingPotions;
        Changed?.Invoke();                  // HUD partner dan follower ikut menyegarkan diri
    }
}