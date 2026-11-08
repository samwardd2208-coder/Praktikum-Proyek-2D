using System;

// Isi tas pemain: satu partner + 3 Ramuan.
public class PlayerParty
{
    public const int StartingPotions = 3;
    public event System.Action PotionsChanged;
public bool NeedsRespawn { get; set; }
public void AddPotions(int amount) { }

    public MonsterInstance Partner { get; private set; }
    public int Potions { get; private set; } = StartingPotions;
    public event Action Changed;

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
}
