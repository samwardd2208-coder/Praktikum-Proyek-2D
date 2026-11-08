using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tampilan battle (Canvas_Battle, 320 x 180 piksel demo, dikali 4 ke kanvas 1280 x 720).
/// Tidak punya logika battle: hanya menampilkan pesan, menu, efek layar, dan meneruskan pilihan lewat ActionClicked.
/// Semua referensi dicari otomatis dari nama anak (lihat hierarki di slide), atau bisa diisi manual.
/// </summary>
public class BattleView : MonoBehaviour
{
    [Header("Teks & menu")]
    [SerializeField] Text messageText;        // anak "Message"
    [SerializeField] Text hintText;           // anak "SkillHint"
    [SerializeField] TextMenu menu;           // anak "Menu"

    [Header("Panel monster")]
    [SerializeField] MonsterPanel enemyPanel;     // "EnemyPanel"
    [SerializeField] MonsterPanel playerPanel;    // "PlayerPanel"

    [Header("Efek layar")]
    [SerializeField] RectTransform wipeTop;       // "WipeTop"
    [SerializeField] RectTransform wipeBottom;    // "WipeBottom"
    [SerializeField] Image flash;                 // "Flash" (putih, alpha 0)
    [SerializeField] Image[] rings;               // "Ring_0".."Ring_2"
    [SerializeField] Vector2 ringCenter = new Vector2(88f, 100f);   // piksel demo (di atas PlayerMon)
    [SerializeField] float charsPerSecond = 55f;

    public MonsterPanel EnemyPanel => enemyPanel;
    public MonsterPanel PlayerPanel => playerPanel;

    /// <summary>Indeks aksi yang dipilih pemain: 0 Serang, 1 Jurus, 2 Ramuan, 3 Kabur.</summary>
    public event System.Action<int> ActionClicked;

    Coroutine typing;
    int lastPick;                // ingat pilihan terakhir

    // ---------------------------------------------------------------- setup

    void Reset() => AutoWire();

    [ContextMenu("Auto-wire anak")]
    void AutoWire()
    {
        if (messageText == null) messageText = Child<Text>("Message");
        if (hintText == null)    hintText    = Child<Text>("SkillHint");
        if (menu == null)        menu        = Child<TextMenu>("Menu");
        if (enemyPanel == null)  enemyPanel  = Child<MonsterPanel>("EnemyPanel");
        if (playerPanel == null) playerPanel = Child<MonsterPanel>("PlayerPanel");
        if (wipeTop == null)     wipeTop     = Child<RectTransform>("WipeTop");
        if (wipeBottom == null)  wipeBottom  = Child<RectTransform>("WipeBottom");
        if (flash == null)       flash       = Child<Image>("Flash");
        if (rings == null || rings.Length == 0)
        {
            List<Image> found = new List<Image>();
            for (int i = 0; i < 3; i++)
            {
                Image ring = Child<Image>("Ring_" + i);
                if (ring != null) found.Add(ring);
            }
            rings = found.ToArray();
        }
    }

    T Child<T>(string childName) where T : Component
    {
        Transform t = transform.Find(childName);
        return t != null ? t.GetComponent<T>() : null;
    }

    void Awake()
    {
        AutoWire();

        // Efek layar tidak boleh menghalangi klik
        if (flash != null) { flash.raycastTarget = false; SetAlpha(flash, 0f); }
        foreach (Image ring in rings) { ring.raycastTarget = false; ring.gameObject.SetActive(false); }
        SetupWipe(wipeTop, true);
        SetupWipe(wipeBottom, false);
        SetWipe(1f);                                  // layar tertutup sampai PlayIntro()

        if (hintText != null) hintText.enabled = false;
        if (menu != null)
        {
            menu.Picked += OnMenuPicked;
            menu.Close();
        }
    }

    void OnDestroy()
    {
        if (menu != null) menu.Picked -= OnMenuPicked;
    }

    void OnMenuPicked(int index)
    {
        lastPick = index;
        menu.Close();
        ActionClicked?.Invoke(index);
    }

    // ---------------------------------------------------------------- pesan & menu

    public void ShowMessage(string text)
    {
        hintText.enabled = false;
        if (menu != null) menu.Close();
        if (typing != null) StopCoroutine(typing);
        typing = StartCoroutine(Type(text));              // 55 huruf per detik
    }

    IEnumerator Type(string text)
    {
        messageText.text = "";
        float shown = 0f;
        while (shown < text.Length)
        {
            shown += Time.deltaTime * charsPerSecond;
            messageText.text = text.Substring(0, Mathf.Min(text.Length, Mathf.FloorToInt(shown)));
            yield return null;
        }
        messageText.text = text;
        typing = null;
    }

    public void ShowMenu(IReadOnlyList<string> labels, IReadOnlyList<bool> usable, string prompt, string hint)
    {
        if (typing != null) StopCoroutine(typing);
        typing = null;
        messageText.text = prompt;                        // "Apa yang akan Pyrit lakukan?"
        hintText.text = hint;                             // "Jurus elemen: Semburan Api"
        hintText.enabled = true;
        int start = lastPick < usable.Count && usable[lastPick] ? lastPick : 0;   // ingat pilihan terakhir
        menu.Open(new List<string>(labels).ToArray(), new List<bool>(usable).ToArray(), start);
    }

    // ---------------------------------------------------------------- efek layar

    /// <summary>Dua tirai gelap (atas & bawah) membuka ke tepi layar dalam 0,48 s.</summary>
    public void PlayIntro() => StartCoroutine(IntroRoutine());

    IEnumerator IntroRoutine()
    {
        const float duration = 0.48f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            SetWipe(1f - t / duration);
            yield return null;
        }
        SetWipe(0f);
    }

    static void SetupWipe(RectTransform rt, bool top)
    {
        if (rt == null) return;
        float edge = top ? 1f : 0f;
        rt.anchorMin = new Vector2(0f, edge);
        rt.anchorMax = new Vector2(1f, edge);
        rt.pivot = new Vector2(0.5f, edge);
        rt.anchoredPosition = Vector2.zero;
        Image img = rt.GetComponent<Image>();
        if (img != null) { img.raycastTarget = false; img.color = UiTheme.Ink; }
    }

    // k = 1: kedua tirai menutup separuh layar masing-masing; k = 0: terbuka
    void SetWipe(float k)
    {
        float half = ((RectTransform)transform).rect.height * 0.5f * Mathf.Clamp01(k);
        foreach (RectTransform rt in new[] { wipeTop, wipeBottom })
        {
            if (rt == null) continue;
            rt.sizeDelta = new Vector2(0f, half);
            rt.gameObject.SetActive(half > 0.5f);
        }
    }

    /// <summary>Kilat putih memudar 0,5 s.</summary>
    public void PlayFlash() => StartCoroutine(FlashRoutine());

    IEnumerator FlashRoutine()
    {
        const float duration = 0.5f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            SetAlpha(flash, 1f - t / duration);
            yield return null;
        }
        SetAlpha(flash, 0f);
    }

    /// <summary>Tiga cincin emas melebar bergantian (125 px demo / detik).</summary>
    public void PlayEvolveRings(float seconds)
    {
        for (int i = 0; i < rings.Length; i++)
            StartCoroutine(RingRoutine(rings[i], i * seconds * 0.2f));
    }

    IEnumerator RingRoutine(Image ring, float delay)
    {
        RectTransform rt = ring.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = UiTheme.Pos(ringCenter.x, ringCenter.y);

        yield return new WaitForSeconds(delay);
        ring.gameObject.SetActive(true);

        const float life = 0.9f;
        for (float t = 0f; t < life; t += Time.deltaTime)
        {
            float diameter = (6f + 125f * t) * UiTheme.Scale;
            rt.sizeDelta = new Vector2(diameter, diameter);
            Color c = UiTheme.Gold;
            c.a = 1f - t / life;
            ring.color = c;
            yield return null;
        }
        ring.gameObject.SetActive(false);
    }

    static void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
