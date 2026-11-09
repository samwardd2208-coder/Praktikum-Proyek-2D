using System;
 
public class PlayerParty
{
    public const int StartingPotions = 3;
 
    public MonsterInstance Partner { get; private set; }
    public int Potions { get; private set; } = StartingPotions;
 
    public event Action PartnerChanged;
    public event Action PotionsChanged;
 
    public void SetPartner(MonsterInstance monster)
    {
        Partner = monster;
        PartnerChanged?.Invoke();
    }
 
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
}
 