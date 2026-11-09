using UnityEngine;

// Kunci gerak player selama battle. Tempel di Player, seret script gerak player ke "movement".
// (Di modul ini dilakukan oleh InputGate; ini versi ringkasnya.)
public class PlayerBattleLock : MonoBehaviour
{
    [SerializeField] private MonoBehaviour movement;   // script gerak player
    private Rigidbody2D rb;

    void Awake() { rb = GetComponent<Rigidbody2D>(); }
    void OnEnable()  { BattleBridge.Started += Lock;   BattleBridge.Ended += Unlock; }
    void OnDisable() { BattleBridge.Started -= Lock;   BattleBridge.Ended -= Unlock; }

    private void Lock()
    {
        if (movement != null) movement.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;   // Unity 6. Unity lama: rb.velocity
    }

    private void Unlock(BattleResult r, bool wasBoss)
    {
        if (movement != null) movement.enabled = true;
    }
}
