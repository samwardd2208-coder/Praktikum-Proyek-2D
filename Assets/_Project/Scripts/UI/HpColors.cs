using UnityEngine;

public static class HpColors
{
    public static Color For(float ratio)
    {
        if (ratio > 0.5f) return new Color32(76, 217, 100, 255);   // Hijau
        if (ratio > 0.2f) return new Color32(255, 204, 0, 255);    // Emas
        return new Color32(255, 59, 48, 255);                      // Merah
    }
}