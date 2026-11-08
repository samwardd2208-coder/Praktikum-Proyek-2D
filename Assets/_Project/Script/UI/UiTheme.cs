using UnityEngine;

/// <summary>
/// Satu tempat untuk warna + konversi koordinat desain (layar demo 320 x 180) ke uGUI (kanvas 1280 x 720).
/// 1 piksel demo = 4 unit kanvas.
/// </summary>
public static class UiTheme
{
    public const float Scale = 4f;

    // --- Warna ---
    public static readonly Color Ink      = Hex(0x121027);
    public static readonly Color Panel    = Hex(0x1C1540);
    public static readonly Color Gold     = Hex(0xFFC61A);
    public static readonly Color Text     = Hex(0xEDE9F7);
    public static readonly Color Muted    = Hex(0xA9A3C9);
    public static readonly Color Disabled = Hex(0x5C5680);
    public static readonly Color Exp      = Hex(0x6F9BFF);
    public static readonly Color BarBack  = Hex(0x0B0918);   // latar bar HP / EXP
    public static readonly Color Shadow   = Hex(0x120F26);   // bayangan teks

    static Color Hex(int rgb) => new Color(
        ((rgb >> 16) & 0xFF) / 255f,
        ((rgb >> 8) & 0xFF) / 255f,
        (rgb & 0xFF) / 255f,
        1f);

    // --- Koordinat: x, y dari kiri-atas -> anchor & pivot (0, 1) ---
    public static Vector2 Pos(float x, float y)  => new Vector2(x * Scale, -y * Scale);
    public static Vector2 Size(float w, float h) => new Vector2(w * Scale, h * Scale);
    public static int FontSize(float px)         => Mathf.RoundToInt(px * Scale);   // teks 8 px -> fontSize 32

    /// <summary>Tempatkan elemen biasa pada koordinat demo.</summary>
    public static void Place(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = Pos(x, y);
        rt.sizeDelta = Size(w, h);
    }

    /// <summary>panel(x, y, w, h): tepi gelap 1 px berada di luar, jadi rect = (x-1, y-1, w+2, h+2).</summary>
    public static void PlacePanel(RectTransform rt, float x, float y, float w, float h)
        => Place(rt, x - 1f, y - 1f, w + 2f, h + 2f);
}
