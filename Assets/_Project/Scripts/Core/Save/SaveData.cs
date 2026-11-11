using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public float playerX;
    public float playerY;
    public List<string> flags = new List<string>();
    public int potions;
    public string partnerSpecies;
    public int partnerLevel;
    public int partnerHp;
    public int partnerExp;
}