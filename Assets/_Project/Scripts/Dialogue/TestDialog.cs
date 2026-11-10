using System.Collections;
using UnityEngine;

public class TestDialog : MonoBehaviour
{
    [SerializeField] private DialogueRunner runner;

    void Update()
    {
        // Tekan tombol 'E' atau 'Enter' untuk memulai dialog tes
        if (MenuInput.Confirm && runner != null && !runner.IsRunning)
        {
            runner.Run(TestSequence());
        }
    }

    private IEnumerator TestSequence()
    {
        yield return runner.Say("Tetua Rahmat", "Halo! Selamat datang di Pulau Monster.");
        
        // --- FITUR CHOOSE (PILIHAN) ---
        int choice = 0; // Variabel untuk menyimpan hasil pilihan (0 = indeks pertama, 1 = indeks kedua)
        
        yield return runner.Choose(
            "Tetua Rahmat", 
            "Apakah kamu siap untuk bertualang?", 
            new string[] { "Siap banget!", "Belum, nanti aja" }, 
            (hasil) => choice = hasil // Callback: simpan hasil pilihan ke variabel 'choice'
        );

        // Logika bercabang berdasarkan pilihan
        if (choice == 0)
        {
            yield return runner.Say("Tetua Rahmat", "Bagus! Pergilah ke hutan di utara.");
        }
        else
        {
            yield return runner.Say("Tetua Rahmat", "Tidak apa-apa, istirahat dulu di desa.");
        }
        
        yield return runner.Narrate("Akhir dari dialog percobaan.");
    }
}