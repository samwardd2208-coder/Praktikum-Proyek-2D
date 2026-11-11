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
    protected AudioService audio;

    public virtual void Construct(DialogueRunner dialogue, StoryFlags flags, PlayerParty party, AudioService audio)
    {
        this.dialogue = dialogue;
        this.flags = flags;
        this.party = party;
        this.audio = audio;
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
    protected void Face(Vector2 direction)
    {
        if (body == null) return;

        // Cek apakah arah dominan Horizontal atau Vertikal
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Jika Player di kiri/kanan
            body.sprite = direction.x > 0 ? faceRight : faceLeft;
        }
        else
        {
            // Jika Player di atas/bawah
            body.sprite = direction.y > 0 ? faceUp : faceDown;
        }
    }
}