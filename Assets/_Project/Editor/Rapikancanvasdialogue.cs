#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Taruh di Assets/_Project/Editor/. Jalankan lewat menu:
// Tools > Pulau Monster > Rapikan Canvas_Dialogue   (Scn_Bootstrap terbuka, jangan Play)
//
// Koordinat dari slide P6 "Canvas_Dialogue di Scn_Bootstrap" (layar 320x180 piksel, dikali 4).
// panel(x, y, w, h) -> rect (x-1, y-1, w+2, h+2) karena tepi gelap 1 px di luar.
public static class RapikanCanvasDialogue
{
    const float S = 4f;
    const float PanelMultiplier = 1.5625f;

    [MenuItem("Tools/Pulau Monster/Rapikan Canvas_Dialogue")]
    static void Run()
    {
        GameObject canvasGo = GameObject.Find("Canvas_Dialogue");
        if (canvasGo == null)
        {
            Debug.LogError("Canvas_Dialogue tidak ditemukan. Buka Scn_Bootstrap dan jangan dalam mode Play.");
            return;
        }

        // ---------- Canvas: order 20, Scale With Screen Size 1280 x 720, Match 0,5 ----------
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        if (canvas != null)
        {
            Undo.RecordObject(canvas, "Rapikan");
            canvas.sortingOrder = 20;
        }
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

        // ---------- DialogueRoot & ClickCatcher: layar penuh ----------
        Stretch(Find(c, "DialogueRoot"));
        RectTransform catcher = Find(c, "ClickCatcher");
        Stretch(catcher);
        if (catcher != null)
        {
            Image ci = catcher.GetComponent<Image>();
            if (ci != null)
            {
                Undo.RecordObject(ci, "Rapikan");
                ci.color = new Color(0f, 0f, 0f, 0f);      // alpha 0
                ci.raycastTarget = true;                    // tetap menangkap klik
            }
            Button cb = catcher.GetComponent<Button>();
            if (cb != null)
            {
                Undo.RecordObject(cb, "Rapikan");
                cb.transition = Selectable.Transition.None;
                Navigation nav = cb.navigation;
                nav.mode = Navigation.Mode.None;
                cb.navigation = nav;
            }
        }

        // ---------- Box: (6, 128, 308, 46) -> rect (5, 127, 310, 48) ----------
        RectTransform box = PlaceRect(c, "Box", 5, 127, 310, 48);
        Sliced(box);

        // Body: teks di dalam kotak
        Style(PlaceRect(c, "Body", 14, 137, 292, 30), 32, TextAnchor.UpperLeft, "#EDE9F7", false, true);

        // SpeakerTag: (12, 117) -> rect (11, 116), lebar default 76 px demo (= 304 unit untuk 12 huruf)
        RectTransform tag = PlaceRect(c, "SpeakerTag", 11, 116, 76, 14);
        Sliced(tag);
        RectTransform speaker = Find(c, "Speaker");
        Stretch(speaker);
        Style(speaker, 32, TextAnchor.MiddleCenter, "#121027", true, false);

        // ContinueHint "▼" di (302, 162)
        Style(PlaceRect(c, "ContinueHint", 302, 162, 10, 10), 32, TextAnchor.MiddleCenter, "#FFC61A", false, false);

        // ---------- Choices: kanan, 4 px di atas kotak (tinggi untuk 3 pilihan) ----------
        RectTransform choices = PlaceRect(c, "Choices", 203, 73, 112, 50);
        Sliced(choices);
        for (int i = 0; i < 3; i++)
        {
            RectTransform ch = PlaceRect(c, "Choice_" + i, 213, 79 + i * 13, 96, 12);
            if (ch == null && i == 2)
                Debug.LogWarning("Choice_2 belum ada: duplikat Choice_1 (Ctrl+D) lalu ganti nama jadi Choice_2, jalankan ulang menu ini.");
            if (ch != null)
            {
                Text t = ch.GetComponent<Text>();
                if (t != null)
                {
                    Undo.RecordObject(t, "Rapikan");
                    t.fontSize = 32;
                    t.alignment = TextAnchor.MiddleLeft;
                    t.horizontalOverflow = HorizontalWrapMode.Overflow;
                    t.verticalOverflow = VerticalWrapMode.Overflow;
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(canvasGo.scene);
        Debug.Log("Canvas_Dialogue dirapikan. Ctrl+S untuk menyimpan, Ctrl+Z untuk membatalkan.");
    }

    // ---------------- helper ----------------

    // x, y, w, h = rect dalam piksel demo di koordinat layar.
    // Posisi dihitung relatif ke parent (parent yang sudah ditempatkan ikut dihitung).
    static RectTransform PlaceRect(Transform root, string name, float x, float y, float w, float h)
    {
        Transform t = FindDeep(root, name);
        if (t == null) { Debug.LogWarning("Tidak ditemukan: " + name); return null; }
        RectTransform rt = (RectTransform)t;

        Vector2 origin = Vector2.zero;                       // pojok kiri-atas parent (piksel demo)
        RectTransform p = rt.parent as RectTransform;
        if (p != null && !(p.anchorMin == Vector2.zero && p.anchorMax == Vector2.one))
            origin = new Vector2(p.anchoredPosition.x / S, -p.anchoredPosition.y / S);

        Undo.RecordObject(rt, "Rapikan");
        TopLeft(rt);
        rt.anchoredPosition = new Vector2((x - origin.x) * S, -(y - origin.y) * S);
        rt.sizeDelta = new Vector2(w * S, h * S);
        rt.localScale = Vector3.one;
        DisableLayout(rt);
        return rt;
    }

    static RectTransform Find(Transform root, string name)
    {
        Transform t = FindDeep(root, name);
        if (t == null) { Debug.LogWarning("Tidak ditemukan: " + name); return null; }
        return (RectTransform)t;
    }

    static void Stretch(RectTransform rt)
    {
        if (rt == null) return;
        Undo.RecordObject(rt, "Rapikan");
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        DisableLayout(rt);
    }

    static void Sliced(RectTransform rt)
    {
        if (rt == null) return;
        Image img = rt.GetComponent<Image>();
        if (img == null) return;
        Undo.RecordObject(img, "Rapikan");
        img.type = UnityEngine.UI.Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = PanelMultiplier;
    }

    static void Style(RectTransform rt, int size, TextAnchor anchor, string hex, bool bold, bool wrap)
    {
        if (rt == null) return;
        Text t = rt.GetComponent<Text>();
        if (t == null) return;
        Undo.RecordObject(t, "Rapikan");
        t.fontSize = size;
        t.alignment = anchor;
        t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        t.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.resizeTextForBestFit = false;
        if (ColorUtility.TryParseHtmlString(hex, out Color col)) t.color = col;

        // Body diberi bayangan #120F26 (2, -2) seperti di demo
        if (rt.name == "Body")
        {
            Shadow sh = rt.GetComponent<Shadow>();
            if (sh == null) sh = Undo.AddComponent<Shadow>(rt.gameObject);
            Undo.RecordObject(sh, "Rapikan");
            sh.effectDistance = new Vector2(2f, -2f);
            if (ColorUtility.TryParseHtmlString("#120F26", out Color sc)) sh.effectColor = sc;
        }
    }

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