using System;
using System.Collections.Generic;

public class StoryFlags
{
    // Kunci-kunci flag (hanya ditulis 1x saja)
    public const string HasStarter = "has_starter";
    public const string HasGift = "has_gift";       // Tambahan untuk Pak Dagang
    public const string IntroSeen = "intro_seen";
    public const string BossDefeated = "boss_defeated";

    private HashSet<string> activeFlags = new HashSet<string>();

    // Event untuk memberi tahu objek lain (seperti FlagToggle) saat flag berubah
    public event Action<string> Changed;

    public bool Has(string key) => activeFlags.Contains(key);

    public void Set(string key)
    {
        if (!activeFlags.Contains(key))
        {
            activeFlags.Add(key);
            Changed?.Invoke(key); // Beri tahu dunia bahwa flag ini aktif
        }
    }

    public void Load(List<string> savedFlags)
    {
        activeFlags.Clear();
        if (savedFlags != null)
        {
            foreach (string f in savedFlags) activeFlags.Add(f);
        }
    }

    public List<string> GetAll() => new List<string>(activeFlags);
}