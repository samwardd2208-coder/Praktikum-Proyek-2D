using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel info monster (nama, Lv, elemen, bar HP, angka HP, bar EXP) + animasi sprite di dunia tanpa Animator.
/// Pasang di EnemyPanel dan PlayerPanel (Image Sliced). Referensi anak dicari otomatis berdasarkan nama:
///   Name, Level, Element, HpBar/Fill, HpText, ExpBar/Fill
/// Sprite dunia (SpriteRenderer) dicari dengan nama objek "EnemyMon" / "PlayerMon" kalau slot Body kosong.
///
/// ADAPTASI P4: MonsterData.Frames[0..1] = idle, Frames[2] = frame putih (hurt/evolusi);
///              MonsterInstance.Exp dan ExpToNext untuk bar EXP.
/// </summary>
public class MonsterPanel : MonoBehaviour
{
    [Header("Sprite di dunia")]
    [SerializeField] SpriteRenderer body;
    [SerializeField] bool isPlayerSide;           // true = menghadap kanan (flipX); otomatis kalau nama diawali "Player"

    [Header("HUD (otomatis dicari dari nama anak)")]
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text elementText;            // hanya panel lawan
    [SerializeField] Text hpText;                 // hanya panel partner
    [SerializeField] Image hpFill;
    [SerializeField] Image expFill;               // hanya panel partner

    /// <summary>Tampilkan ★ di depan nama (boss).</summary>
    public bool BossMark { get; set; }

    MonsterInstance shown;
    MonsterData shownData;
    Vector3 home;
    Vector3 faintOffset;
    float hpShow, expShow;
    float lungeStart = -10f;
    bool hurting, evolving, fainted;
    Coroutine hurtRoutine;

    // ---------------------------------------------------------------- setup

    void Reset() => AutoWire();

    [ContextMenu("Auto-wire anak")]
    void AutoWire()
    {
        if (nameText == null)    nameText    = Child<Text>("Name");
        if (levelText == null)   levelText   = Child<Text>("Level");
        if (elementText == null) elementText = Child<Text>("Element");
        if (hpText == null)      hpText      = Child<Text>("HpText");
        if (hpFill == null)      hpFill      = Child<Image>("HpBar/Fill");
        if (expFill == null)     expFill     = Child<Image>("ExpBar/Fill");
        if (!isPlayerSide)       isPlayerSide = name.StartsWith("Player");
    }

    T Child<T>(string path) where T : Component
    {
        Transform t = transform.Find(path);
        return t != null ? t.GetComponent<T>() : null;
    }

    void Awake()
    {
        AutoWire();
        isPlayerSide |= name.StartsWith("Player");

        if (body == null)
        {
            GameObject go = GameObject.Find(isPlayerSide ? "PlayerMon" : "EnemyMon");
            if (go != null) body = go.GetComponent<SpriteRenderer>();
        }
        if (body != null)
        {
            body.flipX = isPlayerSide;            // PlayerMon: flipX (menghadap lawan)
            home = body.transform.localPosition;
        }

        if (nameText != null)    nameText.color = UiTheme.Text;
        if (levelText != null)   levelText.color = UiTheme.Gold;
        if (elementText != null) elementText.color = UiTheme.Muted;
        if (hpText != null)      hpText.color = UiTheme.Muted;
        if (expFill != null)     expFill.color = UiTheme.Exp;
    }

    // ---------------------------------------------------------------- tampilan

    /// <summary>
    /// Isi panel dari monster m. snap = true: bar langsung ke nilai sekarang; false: bar bergerak pelan.
    /// displayAs: nama/sprite yang ditampilkan (dipakai agar nama tetap "Pyrit" sampai animasi evolusi selesai).
    /// </summary>
    public void Show(MonsterInstance m, bool snap = true, MonsterData displayAs = null)
    {
        shown = m;
        shownData = displayAs != null ? displayAs : m.Data;

        if (nameText != null)    nameText.text = (BossMark ? "★ " : "") + shownData.DisplayName;
        if (levelText != null)   levelText.text = "Lv" + m.Level;
        if (elementText != null) elementText.text = shownData.Element.ToString();

        if (snap)
        {
            hpShow = m.CurrentHP;
            expShow = ExpFraction(m);
        }

        if (body != null)
        {
            if (!hurting && !evolving && !fainted) body.sprite = Frame(shownData, 0);
            if (!isPlayerSide)                                        // skala lawan: boss 3 / Lv 9+ 2,4 / biasa 2
                body.transform.localScale = Vector3.one * (BossMark ? 3f : m.Level >= 9 ? 2.4f : 2f);
        }
    }

    static Sprite Frame(MonsterData data, int index)
        => data.Frames[Mathf.Clamp(index, 0, data.Frames.Length - 1)];

    static float ExpFraction(MonsterInstance m)
        => m.ExpToNext > 0 ? Mathf.Clamp01((float)m.Exp / m.ExpToNext) : 0f;

    void Update()
    {
        if (shown == null || body == null) return;

        if (!hurting && !evolving && !fainted)
            body.sprite = Frame(shownData, Mathf.FloorToInt(Time.time / 0.35f) % 2);   // idle 2 frame

        float k = 1f - Mathf.Pow(0.85f, Time.deltaTime * 60f);      // HP mendekat 15 % per frame
        hpShow = Mathf.Lerp(hpShow, shown.CurrentHP, k);
        float hpFraction = shown.CurrentHP <= 0 ? 0f : Mathf.Clamp01(hpShow / shown.MaxHP);
        if (hpFill != null)
        {
            hpFill.fillAmount = hpFraction;
            hpFill.color = HpColors.For(hpFill.fillAmount);         // hijau -> emas -> merah
        }
        if (hpText != null) hpText.text = $"{Mathf.RoundToInt(hpShow)}/{shown.MaxHP}";

        if (expFill != null)
        {
            expShow = Mathf.Lerp(expShow, ExpFraction(shown), k);
            expFill.fillAmount = expShow;
        }

        float p = (Time.time - lungeStart) / 0.26f;                 // menerjang 14 px (14 / 16 = 0,875 unit)
        float dx = p < 1f ? Mathf.Sin(p * Mathf.PI) * 0.875f * (isPlayerSide ? 1 : -1) : 0f;
        body.transform.localPosition = home + new Vector3(dx, 0f, 0f) + faintOffset;
    }

    // ---------------------------------------------------------------- coroutine animasi

    public void PlayLunge() => lungeStart = Time.time;

    // Hurt: frame 3 (putih) berkedip 0,4 s
    public void PlayHurt()
    {
        if (body == null || shownData == null) return;
        if (hurtRoutine != null) StopCoroutine(hurtRoutine);
        hurtRoutine = StartCoroutine(HurtRoutine());
    }

    IEnumerator HurtRoutine()
    {
        hurting = true;
        bool white = false;
        for (float t = 0f; t < 0.4f; t += 0.05f)
        {
            white = !white;
            body.sprite = Frame(shownData, white ? 2 : 0);
            yield return new WaitForSeconds(0.05f);
        }
        hurting = false;
        hurtRoutine = null;
    }

    // Faint: turun 20 px + memudar 0,5 s
    public void PlayFaint()
    {
        if (body == null) return;
        StartCoroutine(FaintRoutine());
    }

    IEnumerator FaintRoutine()
    {
        fainted = true;
        Color start = body.color;
        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            float k = t / 0.5f;
            faintOffset = new Vector3(0f, -1.25f * k, 0f);          // 20 / 16 = 1,25 unit
            Color c = start; c.a = 1f - k;
            body.color = c;
            yield return null;
        }
        faintOffset = new Vector3(0f, -1.25f, 0f);
        Color end = start; end.a = 0f;
        body.color = end;
    }

    // Evolve: sprite lama <-> baru, makin cepat, sesekali putih
    public void PlayEvolve(MonsterData from, MonsterData to, float seconds)
    {
        if (body == null) return;
        StartCoroutine(EvolveRoutine(from, to, seconds));
    }

    IEnumerator EvolveRoutine(MonsterData from, MonsterData to, float seconds)
    {
        evolving = true;
        bool showNew = false;
        float t = 0f, timer = 0f;
        while (t < seconds)
        {
            float k = t / seconds;
            float interval = Mathf.Lerp(0.35f, 0.06f, k * k);       // makin cepat
            timer += Time.deltaTime;
            t += Time.deltaTime;
            if (timer >= interval)
            {
                timer = 0f;
                showNew = !showNew;
                int frame = UnityEngine.Random.value < 0.25f ? 2 : 0;   // sesekali putih
                body.sprite = Frame(showNew ? to : from, frame);
            }
            yield return null;
        }
        body.sprite = Frame(to, 0);
        evolving = false;
    }
}
