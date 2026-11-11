using System;
using UnityEngine;
using UnityEngine.UI;

public class TextMenu : MonoBehaviour
{
    [SerializeField] private Text[] rows;
    // Tambahkan:
    private AudioService audio;
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip confirmClip;
    [SerializeField] private AudioClip cancelClip;

    public void Construct(AudioService audio) { this.audio = audio; }

// Saat navigasi panah:
    private void OnNavigate() => audio?.PlaySfx(moveClip);

// Saat pilih (Picked event):
    private void OnSelect() => audio?.PlaySfx(confirmClip);
    
    private string[] labels;
    private bool[] usable;
    private int count;
    private int selected;
    private bool acceptInput;
    
    public event Action<int> Picked;
    
    public int Selected => selected;

    public void Open(string[] options, bool[] enabled = null, int start = 0)
    {
        labels = options;
        count = Mathf.Min(options.Length, rows.Length);
        usable = enabled ?? new bool[count];
        if (enabled == null)
        {
            for (int i = 0; i < count; i++) usable[i] = true;
        }
        
        for (int i = 0; i < rows.Length; i++)
        {
            rows[i].gameObject.SetActive(i < count);
        }
        
        selected = Mathf.Clamp(start, 0, count - 1);
        if (!usable[selected]) selected = FirstUsable();
        
        gameObject.SetActive(true);
        acceptInput = false;
        Redraw();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private int FirstUsable()
    {
        for (int i = 0; i < count; i++)
        {
            if (usable[i]) return i;
        }
        return 0;
    }

    private void Redraw()
    {
        for (int i = 0; i < count; i++)
        {
            bool on = i == selected;
            rows[i].text = (on ? "▶ " : "  ") + labels[i];
            rows[i].color = !usable[i] ? UiTheme.Disabled : on ? UiTheme.Gold : UiTheme.Text;
        }
    }

    void Update()
    {
        if (count == 0) return;
        if (!acceptInput) { acceptInput = true; return; }
        
        if (MenuInput.Up) Move(-1, 0);
        if (MenuInput.Down) Move(1, 0);
        if (MenuInput.Left) Move(0, -1);
        if (MenuInput.Right) Move(0, 1);
        if (MenuInput.Confirm) Pick(selected);
    }

    private void Move(int rowDelta, int colDelta)
    {
        int next = selected + rowDelta;
        if (next < 0 || next >= count) return;
        if (!usable[next]) return;
        selected = next;
        Redraw();
    }

    private void Pick(int index)
    {
        if (!usable[index]) return;
        Picked?.Invoke(index);
    }

    // Untuk klik mouse
    public void Click(int index)
    {
        if (index >= 0 && index < count && usable[index])
        {
            selected = index;
            Redraw();
            Pick(index);
        }
    }

    public void Hover(int index)
    {
        if (index >= 0 && index < count && usable[index])
        {
            selected = index;
            Redraw();
        }
    }
}