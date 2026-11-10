using System.Collections;
using UnityEngine;

public class GuardNpc : Npc
{
    [SerializeField] private Vector2 asidePosition = new Vector2(22.5f, 10.4f); // Posisi dia menepi

    protected override void OnConstructed()
    {
        // Dengarkan perubahan flag
        if (flags != null)
        {
            flags.Changed += OnFlagChanged;
            // Jika saat game mulai flag sudah ada (misal dari Load Game), langsung menepi
            if (flags.Has(StoryFlags.HasStarter)) StepAside();
        }
    }

    private void OnFlagChanged(string key)
    {
        if (key == StoryFlags.HasStarter) StepAside();
    }

    private void StepAside()
    {
        // Geser 1.5 unit ke kanan dari posisi asal
        transform.position = new Vector3(transform.position.x + 1.5f, transform.position.y, transform.position.z);
        
        // Opsional: Balik badan agar terlihat seperti sedang menyandar/menoleh
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = true; 
    }

    protected override IEnumerator Talk()
    {
        if (!flags.Has(StoryFlags.HasStarter))
        {
            yield return dialogue.Say(npcName, "Berhenti! Hutan terlalu berbahaya tanpa monster partner. Temui Tetua dulu.");
        }
        else
        {
            yield return dialogue.Say(npcName, "Jalan terbuka. Monster di rumput tinggi akan mengejarmu kalau terlalu dekat.");
        }
    }

    void OnDestroy()
    {
        if (flags != null) flags.Changed -= OnFlagChanged;
    }
}