using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Menu teks yang dipakai ulang: panah/WASD, Enter/Spasi/E, klik mouse, dan hover.
/// Pilihan aktif ditandai ▶ emas. Pilihan redup (Disabled) tidak bisa dipilih.
/// Pasang di objek "Menu" (columns = 2 untuk menu battle 2 x 2); anak: Btn_0..Btn_3 [Text + Button].
/// </summary>
public class TextMenu : MonoBehaviour
{
    [SerializeField] Text[] rows;
    [SerializeField] int columns = 1;

    /// <summary>Dipanggil dengan indeks pilihan yang dikonfirmasi (keyboard maupun klik).</summary>
    public event System.Action<int> Picked;

    public int Selected { get; private set; }

    /// <summary>P6: diisi GameBootstrap supaya semua menu ikut bersuara.</summary>
    public static AudioService Audio;

    string[] labels = new string[0];
    bool[] usable = new bool[0];
    int count;
    bool acceptInput;
    bool wired;

    // ---------------------------------------------------------------- setup

    void Awake()
    {
        AutoWire();
        Wire();
    }

    void Reset() => AutoWire();

    [ContextMenu("Auto-wire baris dari anak")]
    void AutoWire()
    {
        if (rows == null || rows.Length == 0)
            rows = GetComponentsInChildren<Text>(true);   // urutan hierarki: Btn_0..Btn_3
    }

    // Klik & hover untuk setiap baris
    void Wire()
    {
        if (wired || rows == null) return;
        wired = true;

        for (int i = 0; i < rows.Length; i++)
        {
            int index = i;                    // SALIN i ke variabel lokal sebelum lambda,
                                              // kalau tidak semua baris memilih yang terakhir
            Text row = rows[i];
            row.raycastTarget = true;

            // Jangan pakai GetComponent<T>() ?? AddComponent<T>() untuk komponen Unity:
            // objek Unity yang sudah dihancurkan tidak benar-benar null, jadi ?? bisa salah.
            Button button = row.GetComponent<Button>();
            if (button == null) button = row.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => Click(index));

            EventTrigger trigger = row.GetComponent<EventTrigger>();
            if (trigger == null) trigger = row.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => Hover(index));
            trigger.triggers.Add(enter);
        }
    }

    // ---------------------------------------------------------------- API

    public void Open(string[] options, bool[] enabled = null, int start = 0)
    {
        AutoWire();
        Wire();

        labels = options;
        count = Mathf.Min(options.Length, rows.Length);
        if (count == 0) { Close(); return; }

        usable = new bool[count];
        for (int i = 0; i < count; i++)
            usable[i] = enabled == null || (i < enabled.Length && enabled[i]);

        for (int i = 0; i < rows.Length; i++) rows[i].gameObject.SetActive(i < count);

        Selected = Mathf.Clamp(start, 0, count - 1);
        if (!usable[Selected]) Selected = FirstUsable();

        gameObject.SetActive(true);
        acceptInput = false;              // abaikan tombol pembuka (Enter yang sama)
        Redraw();
    }

    public void Close()
    {
        count = 0;
        gameObject.SetActive(false);
    }

    // ---------------------------------------------------------------- input

    void Update()
    {
        if (count == 0) return;
        if (!acceptInput) { acceptInput = true; return; }

        if (MenuInput.Up)    Move(-1, 0);
        if (MenuInput.Down)  Move(+1, 0);
        if (MenuInput.Left)  Move(0, -1);
        if (MenuInput.Right) Move(0, +1);
        if (MenuInput.Confirm) Pick(Selected);
    }

    void Move(int dRow, int dCol)
    {
        int cols = Mathf.Max(1, columns);
        int row = Selected / cols;
        int col = Selected % cols;

        int newCol = col + dCol;
        if (newCol < 0 || newCol >= cols) return;

        int target = (row + dRow) * cols + newCol;
        if (target < 0 || target >= count) return;

        Selected = target;
        if (Audio != null) Audio.PlayUiMove();
        Redraw();
    }

    void Click(int index)
    {
        if (index < 0 || index >= count) return;
        Selected = index;
        Redraw();
        Pick(index);
    }

    void Hover(int index)
    {
        if (index < 0 || index >= count || index == Selected) return;
        Selected = index;            // pointer memindah ▶
        if (Audio != null) Audio.PlayUiMove();
        Redraw();
    }

    void Pick(int index)
    {
        if (index < 0 || index >= count) return;
        if (!usable[index])          // redup: tidak bisa dipilih, bunyi batal
        {
            if (Audio != null) Audio.PlayUiCancel();
            return;
        }
        if (Audio != null) Audio.PlayUiConfirm();
        Picked?.Invoke(index);
    }

    // ---------------------------------------------------------------- tampilan

    int FirstUsable()
    {
        for (int i = 0; i < count; i++) if (usable[i]) return i;
        return 0;
    }

    void Redraw()
    {
        for (int i = 0; i < count; i++)
        {
            bool on = i == Selected;
            rows[i].text = (on ? "▶ " : "  ") + labels[i];
            rows[i].color = !usable[i] ? UiTheme.Disabled : on ? UiTheme.Gold : UiTheme.Text;
        }
    }
}
