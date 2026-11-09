using System;
using System.Collections.Generic;

public class StoryFlags
{
    public const string HasStarter = "has_starter";
    public const string Gift = "gift";
    public const string IntroSeen = "intro_seen";
    public const string BossDefeated = "boss_defeated";

    private readonly HashSet<string> set = new HashSet<string>();
    public event Action<string> Changed;

    public bool Has(string key) => set.Contains(key);

    public void Set(string key)
    {
        if (set.Add(key)) Changed?.Invoke(key);
    }

    // Dipakai Load Game dan Mulai Baru. Tidak memicu Changed:
    // objek scene membaca flag sendiri saat Construct.
    public void Load(IEnumerable<string> keys)
    {
        set.Clear();
        if (keys == null) return;
        foreach (var k in keys) set.Add(k);
    }

    public List<string> ToList() => new List<string>(set);
}