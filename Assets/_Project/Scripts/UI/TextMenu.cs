using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Menu teks ▶ yang bisa dipakai lewat keyboard / gamepad (MenuInput) DAN mouse.
//  - Arah / WASD         : pindah pilihan
//  - Enter/Spasi/E/Z/klik : pilih
//  - Esc/X/Backspace      : batal (event Cancelled)
//  - Hover mouse          : memindahkan ▶ ke baris itu
//  - Suara                : pindah, pilih, batal (bunyi batal juga saat memilih opsi redup)
//  - Efek                 : baris aktif berwarna emas & sedikit lebih besar, klik memberi "denyut"
public class TextMenu : MonoBehaviour
{
    [Header("Baris (Text). Tiap baris otomatis diberi Button + hover.")]
    [SerializeField] private Text[] rows;

    [Tooltip("1 = menu vertikal. 2 = menu 2 x 2 (battle).")]
    [SerializeField] private int columns = 1;

    [Header("Suara")]
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip confirmClip;
    [SerializeField] private AudioClip cancelClip;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    [Header("Warna")]
    [SerializeField] private Color normal = new Color32(0xED, 0xE9, 0xF7, 255);     // Text
    [SerializeField] private Color selected = new Color32(0xFF, 0xC6, 0x1A, 255);   // Gold
    [SerializeField] private Color disabled = new Color32(0x5C, 0x56, 0x80, 255);   // Disabled
    [SerializeField] private Color flash = Color.white;                             // kilat saat dipilih

    [Header("Efek")]
    [Tooltip("Tambahan ukuran huruf untuk baris aktif.")]
    [SerializeField] private int selectedFontBonus = 4;
    [Tooltip("Tambahan ukuran huruf sesaat saat baris dipilih (denyut).")]
    [SerializeField] private int punchFontBonus = 8;
    [Tooltip("Jeda sebelum event Picked dikirim, agar efek klik terlihat. 0 = langsung.")]
    [SerializeField] private float pickDelay = 0.12f;

    public event Action<int> Picked;
    public event Action Cancelled;

    public int Selected { get; private set; }
    public bool IsOpen => gameObject.activeSelf && count > 0;

    private string[] labels = new string[0];
    private bool[] usable = new bool[0];
    private int count;
    private bool acceptInput;
    private bool busy;                      // sedang menunggu pickDelay

    private int[] baseFont;
    private float[] punch;
    private float[] flashAmount;
    private AudioSource sfx;

    // ---------------------------------------------------------------- setup

    private void Awake()
    {
        if (rows == null) rows = new Text[0];

        baseFont = new int[rows.Length];
        punch = new float[rows.Length];
        flashAmount = new float[rows.Length];

        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] == null) continue;
            baseFont[i] = rows[i].fontSize;
            rows[i].raycastTarget = true;                      // wajib agar bisa diklik / di-hover

            // Button (Transition None, Navigation None) -> klik
            Button button = rows[i].GetComponent<Button>();
            if (button == null) button = rows[i].gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;
            button.targetGraphic = rows[i];

            int index = i;                                      // salin i sebelum lambda
            button.onClick.AddListener(() => Click(index));

            // EventTrigger PointerEnter -> hover
            EventTrigger trigger = rows[i].GetComponent<EventTrigger>();
            if (trigger == null) trigger = rows[i].gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entry.callback.AddListener(_ => Hover(index));
            trigger.triggers.Add(entry);
        }
    }

    // ---------------------------------------------------------------- API

    public void Open(string[] options, bool[] enabled = null, int start = 0)
    {
        labels = options ?? new string[0];
        count = Mathf.Min(labels.Length, rows.Length);

        usable = new bool[Mathf.Max(count, 1)];
        for (int i = 0; i < count; i++)
            usable[i] = enabled == null || (i < enabled.Length && enabled[i]);

        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] == null) continue;
            rows[i].gameObject.SetActive(i < count);
            rows[i].fontSize = baseFont[i];
            punch[i] = 0f;
            flashAmount[i] = 0f;
        }

        Selected = count > 0 ? Mathf.Clamp(start, 0, count - 1) : 0;
        if (count > 0 && !usable[Selected]) Selected = FirstUsable();

        busy = false;
        gameObject.SetActive(true);
        acceptInput = false;                // abaikan tombol/klik yang membuka menu
        Redraw();
    }

    public void Close()
    {
        count = 0;
        busy = false;
        gameObject.SetActive(false);
    }

    // ---------------------------------------------------------------- input

    private void Update()
    {
        if (count == 0) return;

        if (!acceptInput) { acceptInput = true; return; }

        if (!busy)
        {
            if (MenuInput.Down) Move(+columns, false);
            if (MenuInput.Up) Move(-columns, false);
            if (MenuInput.Right && columns > 1) Move(+1, true);
            if (MenuInput.Left && columns > 1) Move(-1, true);

            if (MenuInput.Confirm) Pick(Selected);
            if (MenuInput.Cancel)
            {
                PlaySfx(cancelClip);
                Cancelled?.Invoke();
            }
        }

        AnimateRows();
    }

    private void Hover(int index)
    {
        if (!acceptInput || busy || index >= count) return;
        if (!usable[index]) return;
        if (Selected == index) return;

        Selected = index;
        PlaySfx(moveClip, 0.04f);
        Redraw();
    }

    private void Click(int index)
    {
        if (!acceptInput || busy || index >= count) return;

        if (!usable[index])
        {
            PlaySfx(cancelClip);            // pilihan redup: bunyi batal
            return;
        }

        if (Selected != index) { Selected = index; Redraw(); }
        Pick(index);
    }

    private void Move(int step, bool sameRow)
    {
        int candidate = Selected;
        for (int guard = 0; guard < count + 1; guard++)
        {
            candidate += step;

            // menu satu kolom: lompat memutar
            if (columns == 1)
            {
                if (candidate < 0) candidate = count - 1;
                if (candidate >= count) candidate = 0;
            }
            else
            {
                if (candidate < 0 || candidate >= count) return;
                if (sameRow && candidate / columns != Selected / columns) return;
            }

            if (usable[candidate]) break;
            if (guard == count) return;
        }

        if (candidate == Selected) return;
        Selected = candidate;
        PlaySfx(moveClip, 0.04f);
        Redraw();
    }

    private void Pick(int index)
    {
        if (busy) return;
        if (index < 0 || index >= count || !usable[index])
        {
            PlaySfx(cancelClip);
            return;
        }

        PlaySfx(confirmClip);
        punch[index] = 1f;                  // denyut ukuran huruf
        flashAmount[index] = 1f;            // kilat warna

        if (pickDelay <= 0f) Picked?.Invoke(index);
        else StartCoroutine(PickAfterDelay(index));
    }

    private IEnumerator PickAfterDelay(int index)
    {
        busy = true;
        yield return new WaitForSecondsRealtime(pickDelay);   // tetap jalan saat Time.timeScale = 0
        busy = false;
        Picked?.Invoke(index);
    }

    // ---------------------------------------------------------------- tampilan

    private void Redraw()
    {
        for (int i = 0; i < count; i++)
        {
            bool on = i == Selected;
            rows[i].text = (on ? "▶ " : "  ") + labels[i];
        }
        ApplyColors();
    }

    private void ApplyColors()
    {
        for (int i = 0; i < count; i++)
        {
            Color baseColor = !usable[i] ? disabled : (i == Selected ? selected : normal);
            rows[i].color = Color.Lerp(baseColor, flash, flashAmount[i]);
        }
    }

    // Efek halus: ukuran huruf baris aktif membesar, denyut & kilat memudar.
    private void AnimateRows()
    {
        float dt = Time.unscaledDeltaTime;
        for (int i = 0; i < count; i++)
        {
            punch[i] = Mathf.MoveTowards(punch[i], 0f, dt * 5f);
            flashAmount[i] = Mathf.MoveTowards(flashAmount[i], 0f, dt * 6f);

            float bonus = (i == Selected ? selectedFontBonus : 0) + punch[i] * punchFontBonus;
            int size = baseFont[i] + Mathf.RoundToInt(bonus);
            if (rows[i].fontSize != size) rows[i].fontSize = size;
        }
        ApplyColors();
    }

    // ---------------------------------------------------------------- bantu

    private int FirstUsable()
    {
        for (int i = 0; i < count; i++) if (usable[i]) return i;
        return 0;
    }

    private void PlaySfx(AudioClip clip, float pitchJitter = 0f)
    {
        if (clip == null) return;
        if (sfx == null)
        {
            sfx = GetComponent<AudioSource>();
            if (sfx == null) sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.spatialBlend = 0f;                              // 2D
        }
        sfx.pitch = 1f + UnityEngine.Random.Range(-pitchJitter, pitchJitter);
        sfx.PlayOneShot(clip, sfxVolume);
    }
}