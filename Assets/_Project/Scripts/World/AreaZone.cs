using UnityEngine;

public class AreaZone : MonoBehaviour
{
    [Header("Info Area")]
    [SerializeField] private string areaName;       // Misal: "Desa Awal", "Hutan Utara"
    [SerializeField] private AudioClip music;       // BGM khusus area ini

    [Header("UI (Opsional)")]
    [SerializeField] private ToastView toast;       // Untuk menampilkan nama area

    private GameServices services;
    private bool hasPlayed; // Mencegah musik diputar berulang-ulang saat player mondar-mandir di perbatasan

    // Dipanggil oleh WorldInstaller untuk menyuntikkan Services
    public void Construct(GameServices s)
    {
        services = s;
        // Cari Toast otomatis jika belum diset di Inspector
        if (toast == null) 
            toast = FindAnyObjectByType<ToastView>(FindObjectsInactive.Include);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah yang masuk adalah Player (lebih aman daripada pakai Tag)
        if (other.GetComponent<PlayerMovement>() == null) return;
        if (hasPlayed) return; 
        
        hasPlayed = true;

        // 1. Putar Musik Area (Crossfade 1 detik otomatis ditangani AudioService)
        if (music != null && services?.Audio != null)
        {
            services.Audio.PlayWorldMusic(music);
        }

        // 2. Tampilkan Nama Area di Toast (muncul sebentar lalu hilang)
        if (!string.IsNullOrEmpty(areaName) && toast != null)
        {
            toast.Show(areaName);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null) return;
        
        // Reset status saat player keluar dari area, 
        // supaya musik bisa diputar lagi jika player masuk kembali
        hasPlayed = false; 
    }
}