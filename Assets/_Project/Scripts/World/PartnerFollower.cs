using UnityEngine;

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

    void LateUpdate()
    {
        MonsterInstance m = party != null ? party.Partner : null;
        body.enabled = m != null;
        if (m == null) return;

        Vector2 f = player.Facing;          // offset piksel demo
        Vector2 offset = f.y < 0 ? new Vector2(-12, 2)
                       : f.x < 0 ? new Vector2(10, 4)
                       : f.x > 0 ? new Vector2(-10, 4)
                       : new Vector2(12, -2);
        transform.position = player.transform.position
                           + (Vector3)(offset / 16f);

        int frame = Mathf.FloorToInt(Time.time / 0.25f) % 2;
        body.sprite = m.Data.Frames[frame];
        body.flipX = f.x < 0;
        float s = m.Data.Frames[0].rect.width <= 16f ? 1f : 0.75f;
        transform.localScale = new Vector3(s, s, 1f);
    }
}
