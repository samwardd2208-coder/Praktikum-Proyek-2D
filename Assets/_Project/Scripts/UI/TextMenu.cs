using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TextMenu : MonoBehaviour
{
    [SerializeField] Text[] rows;          // Btn_0..Btn_3 (komponen Text)
    [SerializeField] int columns = 1;      // 2 untuk menu 2 x 2
    [SerializeField] Color normal   = new Color(0.93f, 0.91f, 0.97f); // EDE9F7
    [SerializeField] Color selected = new Color(1f, 0.78f, 0.10f);    // FFC61A
    [SerializeField] Color disabled = new Color(0.36f, 0.34f, 0.50f); // 5C5680

    public event Action<int> Picked;
    public int Selected { get; private set; }

    string[] labels;
    bool[] usable;
    int count;
    bool acceptInput;

    void Awake()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            int index = i;   // salin ke variabel lokal sebelum lambda!

            var button = rows[i].GetComponent<Button>();
            if (button != null) button.onClick.AddListener(() => Click(index));

            var trigger = rows[i].GetComponent<EventTrigger>();
            if (trigger == null) trigger = rows[i].gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entry.callback.AddListener(_ => Hover(index));
            trigger.triggers.Add(entry);
        }
    }

    public void Open(string[] options, bool[] enabled = null, int start = 0)
    {
        labels = options;
        count = Mathf.Min(options.Length, rows.Length);
        usable = enabled ?? new bool[count];
        if (enabled == null) for (int i = 0; i < count; i++) usable[i] = true;
        for (int i = 0; i < rows.Length; i++) rows[i].gameObject.SetActive(i < count);
        Selected = Mathf.Clamp(start, 0, count - 1);
        if (!usable[Selected]) Selected = FirstUsable();
        gameObject.SetActive(true);
        acceptInput = false;   // abaikan tombol pembuka
        Redraw();
    }

    public void Close()
    {
        count = 0;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (count == 0) return;
        if (!acceptInput) { acceptInput = true; return; }

        if (MenuInput.Down)  Move(0, +1);
        if (MenuInput.Up)    Move(0, -1);
        if (MenuInput.Right) Move(+1, 0);
        if (MenuInput.Left)  Move(-1, 0);
        if (MenuInput.Confirm) Pick(Selected);
    }

    // dx = geser kolom, dy = geser baris
    void Move(int dx, int dy)
    {
        int cols = Mathf.Max(1, columns);
        int target = Selected;

        if (cols == 1)
        {
            target = Selected + dx + dy;
        }
        else
        {
            int row = Selected / cols, col = Selected % cols;
            col += dx; row += dy;
            if (col < 0 || col >= cols || row < 0) return;
            target = row * cols + col;
        }

        if (target < 0 || target >= count) return;
        if (!usable[target]) return;

        Selected = target;
        Redraw();
    }

    void Hover(int i)
    {
        if (count == 0 || i >= count || !usable[i]) return;
        Selected = i;
        Redraw();
    }

    void Click(int i)
    {
        if (count == 0 || i >= count) return;
        Selected = i;
        Pick(i);
    }

    void Pick(int i)
    {
        if (!usable[i]) return;      // pilihan redup tidak bisa dipilih
        Picked?.Invoke(i);
    }

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
            rows[i].color = !usable[i] ? disabled : on ? selected : normal;
        }
    }
}