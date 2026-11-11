using UnityEngine;

public static class SaveMapper
{
    public static SaveData Capture(GameServices s, Vector3 playerPosition)
    {
        SaveData data = new SaveData
        {
            playerX = playerPosition.x,
            playerY = playerPosition.y,
            flags = s.Flags.GetAll(),
            potions = s.Party.Potions
        };
        MonsterInstance p = s.Party.Partner;
        if (p != null)
        {
            data.partnerSpecies = p.Data.name;
            data.partnerLevel   = p.Level;
            data.partnerHp      = p.CurrentHP;
            data.partnerExp     = p.Exp;
        }
        return data;
    }

    // Dipakai TitleInstaller.Continue & PauseMenu
    public static void Apply(SaveData data, GameServices s)
    {
        s.Flags.Load(data.flags);
        s.Party.SetPotions(data.potions);
        // Partner dipulihkan WorldInstaller lewat PendingLoad + speciesCatalog
    }
}