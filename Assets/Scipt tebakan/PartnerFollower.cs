using UnityEngine;

// Partner berjalan di samping pemain. Posisi dihitung di LateUpdate
// supaya dipakai SETELAH player selesai bergerak.
public class PartnerFollower : MonoBehaviour
{
    [SerializeField] private SpriteRenderer body;

    private PlayerParty party;
    private PlayerMovement player;

    public void Construct(PlayerParty playerParty, PlayerMovement playerMovement)
    {
        party = playerParty;
        player = playerMovement;
    }

    void Awake()
    {
        if (body == null) body = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        MonsterInstance m = party != null ? party.Partner : null;
        if (body == null) return;
        body.enabled = m != null && player != null;
        if (!body.enabled) return;

        Vector2 f = player.Facing;          // offset piksel demo
        Vector2 offset = f.y < 0 ? new Vector2(-12, 2)
                       : f.x < 0 ? new Vector2(10, 4)
                       : f.x > 0 ? new Vector2(-10, 4)
                       : new Vector2(12, -2);
        transform.position = player.transform.position
                           + (Vector3)(offset / 16f);

        Sprite[] frames = m.Data.Frames;
        if (frames == null || frames.Length == 0) return;

        int frame = Mathf.FloorToInt(Time.time / 0.25f) % Mathf.Min(2, frames.Length);
        body.sprite = frames[frame];
        body.flipX = f.x < 0;
        float s = frames[0].rect.width <= 16f ? 1f : 0.75f;   // tahap 2-3 dikecilkan 0,75
        transform.localScale = new Vector3(s, s, 1f);
    }
}
