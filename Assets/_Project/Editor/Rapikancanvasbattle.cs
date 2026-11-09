#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Taruh di Assets/_Project/Editor/. Jalankan lewat menu:
// Tools > Pulau Monster > Rapikan Canvas_Battle   (Scn_Battle terbuka, jangan Play)
//
// Aturan slide 29: koordinat desain layar 320x180 piksel, dikali 4 untuk uGUI.
// panel(x, y, w, h) -> rect (x-1, y-1, w+2, h+2) karena tepi gelap 1 px di luar.
public static class RapikanCanvasBattle
{
    const float S = 4f;                // 1 piksel demo = 4 unit kanvas
    const float PanelMultiplier = 1.5625f;

    [MenuItem("Tools/Pulau Monster/Rapikan Canvas_Battle")]
    static void Run()
    {
        GameObject canvasGo = GameObject.Find("Canvas_Battle");
        if (canvasGo == null)
        {
            Debug.LogError("Canvas_Battle tidak ditemukan. Buka Scn_Battle dan jangan dalam mode Play.");
            return;
        }
        Transform c = canvasGo.transform;

        // ---------- Panel utama ----------
        RectTransform enemy = Panel(c, "EnemyPanel", 8, 8, 128, 30);
        RectTransform player = Panel(c, "PlayerPanel", 186, 92, 128, 36);
        RectTransform message = Panel(c, "MessagePanel", 0, 132, 196, 48);
        RectTransform menuPanel = Panel(c, "MenuPanel", 196, 132, 124, 48);

        // ---------- Isi EnemyPanel (posisi relatif ke bagian dalam panel) ----------
        if (enemy != null)
        {
            Style(Place(enemy, "Txt_Name", 6, 4, 80, 10), 32, TextAnchor.UpperLeft, "#EDE9F7");
            Style(Place(enemy, "Txt_Level", 94, 4, 30, 10), 32, TextAnchor.UpperRight, "#FFC61A");
            Style(Place(enemy, "Txt_Element", 6, 17, 26, 9), 28, TextAnchor.UpperLeft, "#A9A3C9");
            Bar(enemy, "Bar_HP_BG", 34, 17, 89, 6);
        }

        // ---------- Isi PlayerPanel ----------
        if (player != null)
        {
            Style(Place(player, "Txt_Name", 6, 4, 80, 10), 32, TextAnchor.UpperLeft, "#EDE9F7");
            Style(Place(player, "Txt_Level", 94, 4, 30, 10), 32, TextAnchor.UpperRight, "#FFC61A");
            Bar(player, "Bar_HP_BG", 6, 17, 117, 6);
            Style(Place(player, "Txt_HP", 60, 25, 63, 8), 28, TextAnchor.UpperRight, "#A9A3C9");
            Bar(player, "Bar_EXP_BG", 6, 32, 117, 3);
        }

        // ---------- Pesan + jurus elemen ----------
        if (message != null)
        {
            Style(Place(message, "Txt_Message", 8, 7, 180, 16), 32, TextAnchor.UpperLeft, "#EDE9F7");
            Style(Place(message, "Txt_Hint", 8, 25, 180, 14), 28, TextAnchor.UpperLeft, "#A9A3C9");
        }

        // ---------- Menu 2 x 2 : kolom 204 + col*58, baris 140 + row*20 ----------
        if (menuPanel != null)
        {
            Transform menu = FindDeep(menuPanel, "Menu");
            if (menu != null)
            {
                RectTransform mrt = (RectTransform)menu;
                Undo.RecordObject(mrt, "Rapikan");
                TopLeft(mrt);
                mrt.anchoredPosition = Vector2.zero;
                mrt.sizeDelta = menuPanel.sizeDelta;      // sama dengan MenuPanel

                for (int i = 0; i < 4; i++)
                {
                    int col = i % 2, row = i / 2;
                    RectTransform btn = Place(mrt, "Btn_" + i, 8 + col * 58, 8 + row * 20, 56, 16);
                    if (btn != null)
                    {
                        Text t = btn.GetComponent<Text>();
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
            }
            else Debug.LogWarning("Objek 'Menu' tidak ditemukan di bawah MenuPanel.");
        }

        // ---------- Monster di dunia (kaki sprite = posisi) ----------
        SetWorld("EnemyMonster", new Vector3(4.5f, -0.125f, 0f), false);
        SetWorld("PlayerMonster", new Vector3(-4.5f, -2.375f, 0f), true);

        EditorSceneManager.MarkSceneDirty(canvasGo.scene);
        Debug.Log("Canvas_Battle dirapikan. Tekan Ctrl+S untuk menyimpan, Ctrl+Z untuk membatalkan.");
    }

    // ---------------- helper ----------------

    static RectTransform Panel(Transform canvas, string name, float x, float y, float w, float h)
    {
        Transform t = FindDeep(canvas, name);
        if (t == null) { Debug.LogWarning("Tidak ditemukan: " + name); return null; }
        RectTransform rt = (RectTransform)t;
        Undo.RecordObject(rt, "Rapikan");
        TopLeft(rt);
        rt.anchoredPosition = new Vector2((x - 1f) * S, -(y - 1f) * S);
        rt.sizeDelta = new Vector2((w + 2f) * S, (h + 2f) * S);

        Image img = rt.GetComponent<Image>();
        if (img != null)
        {
            Undo.RecordObject(img, "Rapikan");
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = PanelMultiplier;
        }
        return rt;
    }

    // ix, iy, w, h dalam piksel demo, relatif ke bagian dalam parent (+1 px tepi)
    static RectTransform Place(Transform parent, string name, float ix, float iy, float w, float h)
    {
        Transform t = FindDeep(parent, name);
        if (t == null) { Debug.LogWarning("Tidak ditemukan: " + name + " di " + parent.name); return null; }
        RectTransform rt = (RectTransform)t;
        Undo.RecordObject(rt, "Rapikan");
        TopLeft(rt);
        rt.anchoredPosition = new Vector2((ix + 1f) * S, -(iy + 1f) * S);
        rt.sizeDelta = new Vector2(w * S, h * S);
        return rt;
    }

    static void Bar(Transform panel, string bgName, float ix, float iy, float w, float h)
    {
        RectTransform bg = Place(panel, bgName, ix, iy, w, h);
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
        fill.offsetMin = new Vector2(S, S);                       // inset 1 px
        fill.offsetMax = new Vector2(-S, -S);

        Image fi = fill.GetComponent<Image>();
        if (fi != null)
        {
            Undo.RecordObject(fi, "Rapikan");
            fi.type = Image.Type.Filled;
            fi.fillMethod = Image.FillMethod.Horizontal;
            fi.fillOrigin = 0;
            if (bgName.Contains("EXP") && ColorUtility.TryParseHtmlString("#6F9BFF", out Color exp))
                fi.color = exp;
        }
    }

    static void Style(RectTransform rt, int size, TextAnchor anchor, string hex)
    {
        if (rt == null) return;
        Text t = rt.GetComponent<Text>();
        if (t == null) return;
        Undo.RecordObject(t, "Rapikan");
        t.fontSize = size;
        t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.resizeTextForBestFit = false;
        if (ColorUtility.TryParseHtmlString(hex, out Color col)) t.color = col;
    }

    static void SetWorld(string name, Vector3 pos, bool flip)
    {
        GameObject go = GameObject.Find(name);
        if (go == null) { Debug.LogWarning("Tidak ditemukan: " + name); return; }
        Undo.RecordObject(go.transform, "Rapikan");
        go.transform.position = pos;
        SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr != null && flip) { Undo.RecordObject(sr, "Rapikan"); sr.flipX = true; }
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