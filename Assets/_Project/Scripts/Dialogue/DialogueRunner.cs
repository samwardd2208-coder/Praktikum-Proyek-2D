using System;
using System.Collections;
using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    [SerializeField] private DialogueView view;
    
    private InputGate gate;
    private bool isRunning;
    private int picked;
    private bool clicked;

    public bool IsRunning => isRunning;

    public void Construct(InputGate inputGate)
    {
        gate = inputGate;
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
        gate.Lock(); // Kunci pemain agar tidak bisa jalan
        
        yield return null; // Lewati frame pertama (abaikan tombol pembuka)
        clicked = false;
        
        yield return sequence; // Jalankan urutan dialog
        
        view.Hide();
        yield return null; // Cegah dialog terbuka lagi di frame yang sama
        
        gate.Unlock(); // Buka kunci pemain
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
        // Ketik prompt, tapi TIDAK menampilkan tanda panah (false) karena akan ada pilihan
        yield return TypeLine(speaker, prompt, false); 
        
        picked = -1;
        view.ShowChoices(options); // Munculkan TextMenu/Buttons pilihan
        
        // Tunggu sampai pemain memilih salah satu
        yield return new WaitUntil(() => picked >= 0); 
        
        view.HideChoices();
        result?.Invoke(picked); // Kirim hasil pilihan ke pemanggil
    }

    // --- HELPER: EFEK MENGETIK & SKIP ---
    private IEnumerator TypeLine(string speaker, string text, bool showContinueHint)
    {
        view.SetSpeaker(speaker);
        view.SetBodyText("");
        view.ShowContinueHint(false);
        
        bool textComplete = false;
        
        // Mulai animasi mengetik
        Coroutine typing = StartCoroutine(TypeText(text, () => 
        {
            textComplete = true;
            if (showContinueHint) view.ShowContinueHint(true);
        }));
        
        // Loop untuk mendeteksi tombol SKIP
        while (!textComplete)
        {
            if (AdvancePressed())
            {
                if (typing != null) StopCoroutine(typing); // Hentikan animasi ketik
                view.SetBodyText(text); // Tampilkan teks penuh
                textComplete = true;
                if (showContinueHint) view.ShowContinueHint(true);
                break;
            }
            yield return null;
        }
        
        // Tunggu pemain menekan tombol lagi untuk lanjut ke dialog berikutnya
        clicked = false; 
        yield return new WaitUntil(() => AdvancePressed());
    }

    private IEnumerator TypeText(string text, Action onComplete)
    {
        for (int i = 0; i < text.Length; i++)
        {
            view.SetBodyText(view.GetBodyText() + text[i]);
            yield return new WaitForSeconds(0.018f); // Kecepatan ketik
        }
        onComplete?.Invoke();
    }

    // Cek apakah pemain menekan tombol lanjut (Klik mouse, E, Spasi, atau Enter)
    private bool AdvancePressed()
    {
        // Kita pakai MenuInput.Confirm dari P5 agar lebih rapi!
        bool pressed = clicked || MenuInput.Confirm; 
        clicked = false;
        return pressed;
    }

    // Dipanggil oleh Button UI saat pilihan diklik
    public void OnChoicePicked(int index) { picked = index; }
    
    // Dipanggil oleh ClickCatcher (Button) saat layar diklik
    public void OnClick() { clicked = true; }
}