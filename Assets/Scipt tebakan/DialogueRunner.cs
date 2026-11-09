using System;
using System.Collections;
using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    [SerializeField] private DialogueView view;
    [SerializeField] private AudioClip blipClip;       // SFX_Dialog_Blip
    [SerializeField] private float charsPerSecond = 55f;
    [SerializeField] private int blipEvery = 3;

    private InputGate gate;
    private AudioService audio;
    private bool clicked;
    private int picked = -1;

    public bool IsRunning { get; private set; }

    // Dipanggil GameBootstrap. Boleh tidak dipanggil (untuk smoke test).
    public void Init(GameServices s) { gate = s.Input; audio = s.Audio; }

    private void OnEnable()
    {
        view.Clicked += OnClicked;
        view.ChoicePicked += OnChoice;
    }

    private void OnDisable()
    {
        view.Clicked -= OnClicked;
        view.ChoicePicked -= OnChoice;
    }

    private void OnClicked() => clicked = true;
    private void OnChoice(int i) => picked = i;

    // ---------- jalankan urutan ----------
    public void Run(IEnumerator sequence)
    {
        if (!IsRunning && sequence != null)
            StartCoroutine(RunRoutine(sequence));
    }

    private IEnumerator RunRoutine(IEnumerator sequence)
    {
        IsRunning = true;
        if (gate != null) gate.Lock();      // pemain berhenti
        yield return null;                  // lewati tombol pembuka
        clicked = false;
        yield return sequence;              // urutan milik NPC / cutscene
        view.Hide();
        yield return null;                  // cegah dialog terbuka lagi
        if (gate != null) gate.Unlock();
        IsRunning = false;
    }

    // ---------- API urutan ----------
    public IEnumerator Say(string speaker, string text)
    { view.Show(false); yield return TypeLine(speaker, text, true); }

    public IEnumerator Narrate(string text)   // kotak biru
    { view.Show(true); yield return TypeLine("", text, true); }

    public IEnumerator Choose(string speaker, string prompt, string[] options, Action<int> result)
    {
        view.Show(false);
        yield return TypeLine(speaker, prompt, false);   // tanpa ▼
        picked = -1;
        yield return null;                               // jangan pakai Enter yang sama untuk memilih
        view.ShowChoices(options);                       // TextMenu
        yield return new WaitUntil(() => picked >= 0);
        view.HideChoices();
        result?.Invoke(picked);
    }

    // Versi lama (DialogueData) tetap ada, dipakai Sari.
    public IEnumerator PlayData(DialogueData data)
    {
        if (data == null || data.lines == null) yield break;
        foreach (string line in data.lines)
            yield return Say(data.speaker, line);
    }

    public void Play(DialogueData data) => Run(PlayData(data));

    // ---------- internal ----------
    private IEnumerator TypeLine(string speaker, string text, bool showHint)
    {
        yield return null;          // satu frame jeda agar tombol sebelumnya tidak ikut terhitung
        clicked = false;

        view.SetSpeaker(speaker);
        view.SetBody("");
        view.ShowHint(false);

        int shown = 0;
        float t = 0f;
        while (shown < text.Length)
        {
            if (AdvancePressed())
            {
                shown = text.Length;                 // lewati ketikan
            }
            else
            {
                t += Time.unscaledDeltaTime * charsPerSecond;
                int target = Mathf.Min(text.Length, (int)t);
                while (shown < target)
                {
                    shown++;
                    if (shown % blipEvery == 0 && text[shown - 1] != ' ' && audio != null)
                        audio.PlaySfx(blipClip, 0.03f);
                }
            }
            view.SetBody(text.Substring(0, shown));
            yield return null;
        }

        view.SetBody(text);
        if (!showHint) yield break;

        view.ShowHint(true);
        yield return null;
        while (!AdvancePressed()) yield return null;
        view.ShowHint(false);
    }

    private bool AdvancePressed()
    {
        bool pressed = clicked || MenuInput.Confirm;   // klik / E / Spasi / Enter
        clicked = false;
        return pressed;
    }
}