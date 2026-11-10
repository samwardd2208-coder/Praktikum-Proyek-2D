#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Taruh di Assets/_Project/Editor/. Jalankan lewat menu:
// Tools > Pulau Monster > Rapikan Canvas_Hud   (Scn_World terbuka, jangan Play)
//
// Koordinat dari slide "PartnerHud versi demo" (layar 320x180 piksel, dikali 4).
// panel(x, y, w, h) -> rect (x-1, y-1, w+2, h+2) karena tepi gelap 1 px di luar.
public static class RapikanCanvasHud
{
    const float S = 4f;
    const float PanelMultiplier = 1.5625f;

    [MenuItem("Tools/Pulau Monster/Rapikan Canvas_Hud")]
    static void Run()
    {
        GameObject canvasGo = GameObject.Find("Canvas_Hud");
        if (canvasGo == null)
        {
            Debug.LogError("Canvas_Hud tidak ditemukan. Buka Scn_World dan jangan dalam mode Play.");
            return;
        }

        // ---------- Canvas Scaler: 1280 x 720, Match 0,5 ----------
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            Undo.RecordObject(scaler, "Rapikan");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        Transform c = canvasGo.transform;

        // ---------- HudContainer: penuh layar, tanpa layout group ----------
        Transform hud = FindDeep(c, "HudContainer");
        if (hud == null) { Debug.LogWarning("HudContainer tidak ditemukan"); return; }
        RectTransform hrt = (RectTransform)hud;
        Undo.RecordObject(hrt, "Rapikan");
        hrt.anchorMin = Vector2.zero;
        hrt.anchorMax = Vector2.one;
        hrt.offsetMin = Vector2.zero;
        hrt.offsetMax = Vector2.zero;
        hrt.localScale = Vector3.one;
        DisableLayout(hud);

        // ---------- Panel partner: (4, 4, 112, 34) ----------
        RectTransform partner = Panel(hud, "Panel_Background", 4, 4, 112, 34);
        if (partner != null)
        {
            float ox = 3f, oy = 3f;     // pojok kiri-atas rect panel (x-1, y-1)

            SetPortrait(PlaceAbs(partner, "Img_Portrait", ox, oy, 8, 9, 24, 24), true);
            Style(PlaceAbs(partner, "Txt_Name", ox, oy, 36, 6, 50, 10), 32, TextAnchor.UpperLeft, "#EDE9F7");
            Style(PlaceAbs(partner, "Txt_Level", ox, oy, 80, 6, 30, 10), 32, TextAnchor.UpperRight, "#FFC61A");
            Bar(partner, "Bar_HP_BG", ox, oy, 36, 18, 74, 6, null);
            Style(PlaceAbs(partner, "Txt_HP", ox, oy, 70, 24, 40, 8), 28, TextAnchor.UpperRight, "#A9A3C9");
            Bar(partner, "Bar_EXP_BG", ox, oy, 36, 32, 74, 3, "#6F9BFF");
        }

        // ---------- Panel area: (226, 4, 90, 24) ----------
        RectTransform area = Panel(hud, "Panel_Area", 226, 4, 90, 24);
        if (area != null)
        {
            float ox = 225f, oy = 3f;
            Style(PlaceAbs(area, "Txt_AreaName", ox, oy, 232, 7, 80, 10), 32, TextAnchor.UpperLeft, "#FFC61A");
            Style(PlaceAbs(area, "Txt_Potions", ox, oy, 232, 16, 80, 9), 28, TextAnchor.UpperLeft, "#EDE9F7");
        }

        EditorSceneManager.MarkSceneDirty(canvasGo.scene);
        Debug.Log("Canvas_Hud dirapikan. Ctrl+S untuk menyimpan, Ctrl+Z untuk membatalkan.");
    }

    // ---------------- helper ----------------

    static RectTransform Panel(Transform parent, string name, float x, float y, float w, float h)
    {
        Transform t = FindDeep(parent, name);
        if (t == null) { Debug.LogWarning("Tidak ditemukan: " + name); return null; }
        RectTransform rt = (RectTransform)t;
        Undo.RecordObject(rt, "Rapikan");
        TopLeft(rt);
        rt.anchoredPosition = new Vector2((x - 1f) * S, -(y - 1f) * S);
        rt.sizeDelta = new Vector2((w + 2f) * S, (h + 2f) * S);
        rt.localScale = Vector3.one;
        DisableLayout(rt);

        Image img = rt.GetComponent<Image>();
        if (img != null)
        {
            Undo.RecordObject(img, "Rapikan");
            img.type = UnityEngine.UI.Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = PanelMultiplier;
        }
        return rt;
    }

    // x, y, w, h = koordinat layar demo; (ox, oy) = pojok kiri-atas parent dalam koordinat layar demo
    static RectTransform PlaceAbs(Transform parent, string name, float ox, float oy,
                                  float x, float y, float w, float h)
    {
        Transform t = FindDeep(parent, name);
        if (t == null) { Debug.LogWarning("Tidak ditemukan: " + name + " di " + parent.name); return null; }
        RectTransform rt = (RectTransform)t;
        Undo.RecordObject(rt, "Rapikan");
        TopLeft(rt);
        rt.anchoredPosition = new Vector2((x - ox) * S, -(y - oy) * S);
        rt.sizeDelta = new Vector2(w * S, h * S);
        rt.localScale = Vector3.one;
        return rt;
    }

    static void Bar(Transform panel, string bgName, float ox, float oy,
                    float x, float y, float w, float h, string fillHex)
    {
        RectTransform bg = PlaceAbs(panel, bgName, ox, oy, x, y, w, h);
        if (bg == null) return;

        Image bgImg = bg.GetComponent<Image>();
        if (bgImg != null && ColorUtility.TryParseHtmlString("#0B0918", out Color c))
        {
            Undo.RecordObject(bgImg, "Rapikan");
            bgImg.color = c;
        }

        if (bg.childCount == 0) return;
        RectTransform fill = (RectTransform)bg.GetChild(0);       // anak pertama = Fill
        Undo.RecordObject(fill, "Rapikan");
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.pivot = new Vector2(0f, 0.5f);
        fill.offsetMin = new Vector2(S * 0.5f, S * 0.5f);         // inset setengah piksel
        fill.offsetMax = new Vector2(-S * 0.5f, -S * 0.5f);

        Image fi = fill.GetComponent<Image>();
        if (fi != null)
        {
            Undo.RecordObject(fi, "Rapikan");
            fi.type = UnityEngine.UI.Image.Type.Filled;
            fi.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            fi.fillOrigin = 0;
            if (fillHex != null && ColorUtility.TryParseHtmlString(fillHex, out Color col)) fi.color = col;
        }
    }

    static void SetPortrait(RectTransform rt, bool preserveAspect)
    {
        if (rt == null) return;
        Image img = rt.GetComponent<Image>();
        if (img == null) return;
        Undo.RecordObject(img, "Rapikan");
        img.preserveAspect = preserveAspect;
    }

    static void Style(RectTransform rt, int size, TextAnchor anchor, string hex)
    {
        if (rt == null) return;
        Text t = rt.GetComponent<Text>();
        if (t == null) return;
        Undo.RecordObject(t, "Rapikan");
        t.fontSize = size;
        t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.resizeTextForBestFit = false;
        if (ColorUtility.TryParseHtmlString(hex, out Color col)) t.color = col;
    }

    // Layout group / fitter menimpa posisi manual -> matikan.
    static void DisableLayout(Transform t)
    {
        foreach (LayoutGroup lg in t.GetComponents<LayoutGroup>())
        {
            Undo.RecordObject(lg, "Rapikan");
            lg.enabled = false;
            Debug.Log("Layout group dimatikan di " + t.name);
        }
        foreach (ContentSizeFitter f in t.GetComponents<ContentSizeFitter>())
        {
            Undo.RecordObject(f, "Rapikan");
            f.enabled = false;
        }
    }

    static void TopLeft(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
    }

    static Transform FindDeep(Transform parent, string name)
    {
        foreach (Transform t in parent)
        {
            if (t.name == name) return t;
            Transform r = FindDeep(t, name);
            if (r != null) return r;
        }
        return null;
    }
}
#endif