using System.Collections;
using UnityEngine;

public class Npc : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Warga";
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private Sprite faceDown, faceLeft, faceRight, faceUp;
    [SerializeField] private DialogueData dialogue;      // dipakai Sari

    protected string NpcName => npcName;
    protected GameServices Services { get; private set; }
    protected DialogueRunner Dialogue => Services.Dialogue;
    protected StoryFlags Flags => Services.Flags;

    // Dipanggil WorldInstaller untuk setiap NPC di scene.
    public void Construct(GameServices s)
    {
        Services = s;
        OnConstructed();
    }

    public void Interact(PlayerInteractor who)
    {
        Face(who.transform.position - transform.position);

        if (Services == null)                       // belum di-Construct (WorldInstaller belum dipatch)
        {
            Debug.Log($"{npcName}: Halo!");
            return;
        }
        if (Dialogue.IsRunning) return;
        Dialogue.Run(Talk());
    }

    public void Face(Vector2 d)
    {
        if (body == null) return;
        Sprite pick = Mathf.Abs(d.x) > Mathf.Abs(d.y)
            ? (d.x > 0 ? faceRight : faceLeft)
            : (d.y > 0 ? faceUp : faceDown);
        if (pick != null) body.sprite = pick;
    }

    // Versi dasar: DialogueData biasa (dipakai Sari).
    protected virtual IEnumerator Talk() { yield return Dialogue.PlayData(dialogue); }
    protected virtual void OnConstructed() { }
}