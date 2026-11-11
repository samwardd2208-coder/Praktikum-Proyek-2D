using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaveStone : MonoBehaviour, IInteractable
{
    [Header("Audio & Efek")]
    [SerializeField] private AudioClip saveClip; // ← INI SLOT YANG KAMU CARI
    // [SerializeField] private PixelBurst burst; // (Opsional, biarkan dikomentari dulu jika belum ada script PixelBurst)

    [Header("Deteksi Fallback")]
    [SerializeField] private float interactRadius = 1.6f;

    private GameServices services;
    private Transform player;
    private DialogueRunner dialogue;
    private ToastView toast;

    public void Construct(GameServices s, Transform playerTransform)
    {
        services = s;
        player = playerTransform;
        dialogue = FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
        toast = FindAnyObjectByType<ToastView>(FindObjectsInactive.Include);
    }

    // Dipanggil oleh PlayerInteractor (jika terdeteksi)
    public void Interact(PlayerInteractor who) => TryUse();

    // Fallback: deteksi tombol E sendiri
    void Update()
    {
        if (services == null || player == null) return;
        if (Keyboard.current == null) return;
        if (!Keyboard.current[Key.E].wasPressedThisFrame) return;
        if (Vector2.Distance(player.position, transform.position) > interactRadius) return;
        TryUse();
    }

    private void TryUse()
    {
        if (dialogue == null)
            dialogue = FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
        if (dialogue == null || dialogue.IsRunning) return;
        
        dialogue.Run(Use());
    }

    private IEnumerator Use()
    {
        // 1. Pulihkan HP Partner
        if (services.Party.Partner != null) services.Party.Partner.HealFull();

        // 2. Kumpulkan Data
        Vector3 pos = player != null ? player.position : transform.position;
        SaveData data = SaveMapper.Capture(services, pos);

        // 3. Mainkan SFX Save (BARU)
        if (services.Audio != null && saveClip != null) 
        {
            services.Audio.PlaySfx(saveClip);
        }

        // 4. Simpan Game
        bool saved = services.Save.Save(data);
        if (toast != null) toast.Show(saved ? "Game Tersimpan!" : "Gagal menyimpan!");

        // 5. Tampilkan Dialog
        yield return dialogue.Say("", saved
            ? "Batu Simpan bersinar. HP partner pulih dan perjalanan tersimpan."
            : "Batu Simpan bersinar, tetapi penyimpanan gagal.");
    }
}