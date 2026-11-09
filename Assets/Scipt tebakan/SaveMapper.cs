using UnityEngine;

public static class SaveMapper
{
    public static SaveData Capture(GameServices s, Vector2 playerPosition)
    {
        var d = new SaveData
        {
            playerPosition = playerPosition,
            potions = s.Party.Potions,
            playtime = s.Playtime,
            flags = s.Flags.ToList()
        };
        var p = s.Party.Partner;
        if (p != null)
        {
            d.partnerId = p.Data.Id;
            d.partnerLevel = p.Level;
            d.partnerExp = p.Exp;
            d.partnerHp = p.CurrentHP;
        }
        return d;
    }

    public static void Apply(SaveData d, GameServices s)
    {
        s.Party.Reset();
        s.Party.Potions = d.potions;
        s.Flags.Load(d.flags);
        s.Playtime = d.playtime;

        if (string.IsNullOrEmpty(d.partnerId)) return;
        MonsterData data = s.Database.GetById(d.partnerId);
        if (data == null) return;
         var m = new MonsterInstance(data, d.partnerLevel);
        m.Restore(d.partnerExp, d.partnerHp);
        s.Party.SetPartner(m);
    }
}