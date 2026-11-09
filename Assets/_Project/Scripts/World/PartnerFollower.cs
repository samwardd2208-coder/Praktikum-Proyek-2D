using UnityEngine;
 
// Partner berjalan di samping Player. Pasang di objek "Partner" (punya SpriteRenderer).
public class PartnerFollower : MonoBehaviour
{
    [SerializeField] private SpriteRenderer body;
 
    private PlayerParty party;
    private PlayerMovementzane player;
 
    public void Construct(PlayerParty partyService, PlayerMovementzane playerMovement)
    {
        party = partyService;
        player = playerMovement;
    }
 
    void LateUpdate()
    {
        if (body == null || player == null) return;
 
        MonsterInstance m = party != null ? party.Partner : null;
        body.enabled = m != null;
        if (m == null || m.Data.Frames == null || m.Data.Frames.Length < 2) return;
 
        Vector2 f = player.Facing;
        // offset dalam piksel, dibagi 16 = unit dunia
        Vector2 offset = f.y < 0 ? new Vector2(-12, 2)
                       : f.x < 0 ? new Vector2(10, 4)
                       : f.x > 0 ? new Vector2(-10, 4)
                       : new Vector2(12, -2);
        transform.position = player.transform.position + (Vector3)(offset / 16f);
 
        int frame = Mathf.FloorToInt(Time.time / 0.25f) % 2;
        body.sprite = m.Data.Frames[frame];
        body.flipX = f.x < 0;
 
        // Tahap 2-3 dikecilkan supaya tidak terlalu besar di samping pemain
        float s = m.Data.Frames[0].rect.width <= 16f ? 1f : 0.75f;
        transform.localScale = new Vector3(s, s, 1f);
    }
}