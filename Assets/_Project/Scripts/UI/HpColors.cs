using UnityEngine;

public static class HpColors
{
    static readonly Color Green = new Color(0.44f, 0.82f, 0.55f);
    static readonly Color Gold  = new Color(1f, 0.78f, 0.10f);
    static readonly Color Red   = new Color(0.88f, 0.25f, 0.29f);

    // fill = 0..1 (sisa HP dibagi MaxHP)
    public static Color For(float fill)
    {
        if (fill > 0.5f) return Green;
        if (fill > 0.2f) return Gold;
        return Red;
    }
}