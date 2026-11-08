using UnityEditor;
using UnityEngine;

public static class P5LayoutTool
{
    static readonly (string path, float x, float y, float w, float h)[] Rects =
    {
        ("EnemyPanel", 28, -28, 520, 128),
        ("EnemyPanel/Name", 28, -16, 304, 40),
        ("EnemyPanel/Level", 348, -16, 144, 40),
        ("EnemyPanel/Element", 28, -64, 104, 32),
        ("EnemyPanel/HpBar", 136, -68, 352, 24),
        ("PlayerPanel", 740, -364, 520, 152),
        ("PlayerPanel/Name", 28, -16, 280, 40),
        ("PlayerPanel/Level", 316, -16, 176, 40),
        ("PlayerPanel/HpBar", 28, -60, 464, 24),
        ("PlayerPanel/HpText", 28, -88, 464, 32),
        ("PlayerPanel/ExpBar", 28, -124, 464, 12),
        ("MessagePanel", -4, -524, 792, 200),
        ("MenuPanel", 780, -524, 504, 200),
        ("Message", 36, -552, 720, 80),
        ("SkillHint", 36, -632, 720, 36),
        ("Menu", 784, -528, 496, 192),
        ("Menu/Btn_0", 32, -32, 216, 64),
        ("Menu/Btn_1", 264, -32, 216, 64),
        ("Menu/Btn_2", 32, -112, 216, 64),
        ("Menu/Btn_3", 264, -112, 216, 64),
    };

    static readonly string[] Fills =
    {
        "EnemyPanel/HpBar/Fill", "PlayerPanel/HpBar/Fill", "PlayerPanel/ExpBar/Fill"
    };

    [MenuItem("Tools/P5/Terapkan Layout Canvas_Battle")]
    static void Apply()
    {
        GameObject canvas = GameObject.Find("Canvas_Battle");
        if (canvas == null)
        {
            Debug.LogError("P5 layout: Canvas_Battle tidak ditemukan. Buka Scn_Battle dulu.");
            return;
        }

        Transform root = canvas.transform;
        int done = 0;

        foreach (var r in Rects)
        {
            RectTransform rt = root.Find(r.path) as RectTransform;
            if (rt == null) { Debug.LogWarning("P5 layout: tidak ketemu / bukan RectTransform: " + r.path); continue; }

            Undo.RecordObject(rt, "P5 layout");
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(r.x, r.y);
            rt.sizeDelta = new Vector2(r.w, r.h);
            done++;
        }

        foreach (string p in Fills) Stretch(root.Find(p) as RectTransform, 4f);
        Stretch(root.Find("Flash") as RectTransform, 0f);

        Debug.Log("P5 layout: " + done + " dari " + Rects.Length + " objek diatur.");
    }

    static void Stretch(RectTransform rt, float inset)
    {
        if (rt == null) return;
        Undo.RecordObject(rt, "P5 layout");
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
    }
}