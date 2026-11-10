using System.Collections;
using UnityEngine;

public abstract class Npc : MonoBehaviour, IInteractable
{
    [SerializeField] protected string npcName = "NPC";
    
    // KEMBALIKAN VARIABEL INI (PENTING UNTUK TOP-DOWN)
    [SerializeField] protected SpriteRenderer body;
    [SerializeField] protected Sprite faceDown, faceLeft, faceRight, faceUp;

    protected DialogueRunner dialogue;
    protected StoryFlags flags;
    protected PlayerParty party;

    public virtual void Construct(DialogueRunner dialogue, StoryFlags flags, PlayerParty party)
    {
        this.dialogue = dialogue;
        this.flags = flags;
        this.party = party;
        OnConstructed();
    }

    protected virtual void OnConstructed() { }

    public void Interact(PlayerInteractor who)
    {
        if (dialogue == null || dialogue.IsRunning) return;
        
        // Panggil method Face untuk menoleh
        Face(who.transform.position - transform.position);
        
        dialogue.Run(Talk());
    }

    protected virtual IEnumerator Talk()
    {
        yield return dialogue.Say(npcName, "Halo, petualang muda!");
    }

    // LOGIKA P4: Ganti Sprite sesuai arah
    protected void Face(Vector2 d)
    {
        if (body == null) return;

        // Cek apakah arah dominan Horizontal atau Vertikal
        if (Mathf.Abs(d.x) > Mathf.Abs(d.y))
        {
            // Jika Player di kiri/kanan
            body.sprite = d.x > 0 ? faceRight : faceLeft;
        }
        else
        {
            // Jika Player di atas/bawah
            body.sprite = d.y > 0 ? faceUp : faceDown;
        }
    }
}