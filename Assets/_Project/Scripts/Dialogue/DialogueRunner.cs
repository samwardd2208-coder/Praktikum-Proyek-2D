using System;
using System.Collections;
using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    [SerializeField] private DialogueView view;
    
    // BARU: Audio SFX
    [SerializeField] private AudioClip blipClip;
    [SerializeField] private AudioClip uiMoveClip;
    [SerializeField] private AudioClip confirmClip;
    [SerializeField] private AudioClip cancelClip;

    private InputGate gate;
    private AudioService audio;  // BARU
    private bool isRunning;
    private int picked;
    private bool clicked;

    public bool IsRunning => isRunning;

    // DIPERBARUI: Tambah parameter AudioService
    public void Construct(InputGate inputGate, AudioService audio = null)
    {
        gate = inputGate;
        this.audio = audio;
    }

    // --- LANGKAH 1: RUN & RUN ROUTINE ---
    public void Run(IEnumerator sequence)
    {
        if (!isRunning && sequence != null)
        {
            StartCoroutine(RunRoutine(sequence));
        }
    }

    private IEnumerator RunRoutine(IEnumerator sequence)
    {
        isRunning = true;
        gate.Lock();
        yield return null;
        clicked = false;
        yield return sequence;
        view.Hide();
        yield return null;
        gate.Unlock();
        isRunning = false;
    }

    // --- LANGKAH 2: SAY, NARRATE, CHOOSE ---
    public IEnumerator Say(string speaker, string text)
    {
        view.Show(false);
        yield return TypeLine(speaker, text, true);
    }

    public IEnumerator Narrate(string text)
    {
        view.Show(true);
        yield return TypeLine("", text, true);
    }

    public IEnumerator Choose(string speaker, string prompt, string[] options, Action<int> result)
    {
        view.Show(false);
        yield return TypeLine(speaker, prompt, false);
        picked = -1;
        view.ShowChoices(options);
        yield return new WaitUntil(() => picked >= 0);
        view.HideChoices();
        result?.Invoke(picked);
    }

    // --- HELPER: EFEK MENGETIK & SKIP ---
    private IEnumerator TypeLine(string speaker, string text, bool showContinueHint)
    {
        view.SetSpeaker(speaker);
        view.SetBodyText("");
        view.ShowContinueHint(false);

        bool textComplete = false;
        Coroutine typing = StartCoroutine(TypeText(text, () =>
        {
            textComplete = true;
            if (showContinueHint) view.ShowContinueHint(true);
        }));

        while (!textComplete)
        {
            if (AdvancePressed())
            {
                if (typing != null) StopCoroutine(typing);
                view.SetBodyText(text);
                textComplete = true;
                if (showContinueHint) view.ShowContinueHint(true);
                break;
            }
            yield return null;
        }

        clicked = false;
        yield return new WaitUntil(() => AdvancePressed());
    }

    private IEnumerator TypeText(string text, Action onComplete)
    {
        for (int i = 0; i < text.Length; i++)
        {
            view.SetBodyText(view.GetBodyText() + text[i]);
            
            // BARU: Mainkan SFX blip saat mengetik
            if (!char.IsWhiteSpace(text[i]) && !char.IsPunctuation(text[i]))
            {
                PlayBlip();
            }
            
            yield return new WaitForSeconds(0.018f);
        }
        onComplete?.Invoke();
    }

    private bool AdvancePressed()
    {
        bool pressed = clicked || MenuInput.Confirm;
        clicked = false;
        return pressed;
    }

    public void OnChoicePicked(int index) { picked = index; }
    public void OnClick() { clicked = true; }

    // --- BARU: Audio Helpers ---
    private void PlayBlip()
    {
        if (audio != null && blipClip != null) audio.PlaySfx(blipClip, 0.1f);
    }

    public void PlayUiMove()
    {
        if (audio != null && uiMoveClip != null) audio.PlaySfx(uiMoveClip);
    }

    public void PlayConfirm()
    {
        if (audio != null && confirmClip != null) audio.PlaySfx(confirmClip);
    }

    public void PlayCancel()
    {
        if (audio != null && cancelClip != null) audio.PlaySfx(cancelClip);
    }
}