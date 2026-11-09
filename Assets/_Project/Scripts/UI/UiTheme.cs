using UnityEngine;

public static class UiTheme
{
    public static readonly Color Ink      = Hex("121027");
    public static readonly Color Panel    = Hex("1C1540");
    public static readonly Color Gold     = Hex("FFC61A");
    public static readonly Color Text     = Hex("EDE9F7");
    public static readonly Color Muted    = Hex("A9A3C9");
    public static readonly Color Disabled = Hex("5C5680");
    public static readonly Color Exp      = Hex("6F9BFF");
    public static readonly Color BarBack  = Hex("0B0918");
    public static readonly Color Shadow   = Hex("120F26");

    public static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color c);
        return c;
    }
}
