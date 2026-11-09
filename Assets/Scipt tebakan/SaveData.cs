using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int version = SaveService.CurrentVersion;
    public string savedAt;
    public string scene = "Scn_World";
    public Vector2 playerPosition;
    public string partnerId;                 // id, bukan aset
    public int partnerLevel, partnerExp, partnerHp;
    public int potions;
    public float playtime;                   // layar SELESAI (P7)
    public List<string> flags = new List<string>();
}