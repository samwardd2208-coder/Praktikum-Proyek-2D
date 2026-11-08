using UnityEngine;

/// <summary>Warna bar HP: hijau > 50 %, emas > 20 %, merah selebihnya.</summary>
public static class HpColors
{
    public static readonly Color Green = new Color32(0x6B, 0xD9, 0x7E, 0xFF);
    public static readonly Color Red   = new Color32(0xE8, 0x4A, 0x3C, 0xFF);

    public static Color For(float fraction)
    {
        if (fraction > 0.5f) return Green;
        if (fraction > 0.2f) return UiTheme.Gold;
        return Red;
    }
}
