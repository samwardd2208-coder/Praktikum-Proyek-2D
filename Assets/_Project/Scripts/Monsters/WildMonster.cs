using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WildMonster : MonoBehaviour, IWildAgent
{
    [SerializeField] private MonsterData species;   // monster apa (Mon_*)
    [SerializeField] private int level;             // level liar/boss
    [SerializeField] private bool isBoss;           // Raijugar = true
    [SerializeField] private GameObject alert;      // child "Alert" (sprite UI_Alert)

    private const float SightRadius = 3.25f;        // slide 19/22
    private const float GiveUpRadius = 6.25f;       // slide 19/22
    private const float CooldownSec = 2.5f;         // slide 22

    private StateMachine fsm;
    private WildStates states;
    private BattleSession session;
    private SceneFlow scenes;
    private PlayerParty party;
    private Transform target;
    private Rigidbody2D rb;
    private Vector2 home;
    private Vector2 desiredVelocity;
    private float cooldown;

    private bool stationary => isBoss;                          // slide 24
    private bool inBattle  => session != null && session.InBattle;
    private bool Calm      => cooldown > 0f || (session != null && session.IsSafe);
    private float DistToTarget => target == null ? float.MaxValue
                                   : Vector2.Distance(Position, target.position);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;  // slide 33: anti-putar
        home = transform.position;
        fsm = new StateMachine();
        states = new WildStates(this, fsm);
        fsm.ChangeTo(states.Wander);
        if (alert != null)
            alert.transform.localPosition = new Vector3(0f, isBoss ? 2.2f : 1.3f, 0f); // slide 22
            var sr = GetComponent<SpriteRenderer>();
        if (sr != null && species != null && species.Frames != null && species.Frames.Length > 0)
            sr.sprite = species.Frames[0];   // gambar statis = pose Idle0 monster ini
    }

    // dipanggil WorldInstaller (slide 28) — urut param HARUS sama
    public void Construct(BattleSession s, SceneFlow sc, PlayerParty p, Transform player)
    {
        session = s; scenes = sc; party = p; target = player;
    }

    void Update()                                              // slide 22 harfiah
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;
        if (stationary || inBattle || target == null) desiredVelocity = Vector2.zero;
        else fsm.Tick(Time.deltaTime);
        if (alert != null) alert.SetActive(fsm.Current == states.Chase && !inBattle);
    }

    void FixedUpdate() => rb.linearVelocity = desiredVelocity;       // Move() nggak punya dt → bridge ini

    // ---- IWildAgent ----
    public Vector2 Position          => rb.position;
    public Vector2 Home              => home;
    public Vector2 DirectionToTarget => target == null ? Vector2.zero
                                   : ((Vector2)target.position - Position).normalized;
    public bool CanSeeTarget => party != null && party.Partner != null
                              && !Calm && DistToTarget < SightRadius;   // slide 22
    public bool LostTarget   => Calm || DistToTarget > GiveUpRadius;     // slide 22
    public void Move(Vector2 direction, float speed) => desiredVelocity = direction * speed;
    public void Stop() => desiredVelocity = Vector2.zero;

// 1) ganti parameter 2 callback ini:
void OnCollisionEnter2D(Collision2D col) => TryBegin(col);
void OnCollisionStay2D(Collision2D col) => TryBegin(col);

void TryBegin(Collision2D col)
{
    // col.collider = benda yang ditabrak (otherCollider = diri sendiri)
    if (target == null || !col.collider.transform.IsChildOf(target)) return;

    if (session == null || scenes == null)
    {
        Debug.LogWarning("WildMonster belum di-Construct oleh WorldInstaller.");
        return;
    }
    if (inBattle || Calm) return;

    MonsterInstance enemy = new MonsterInstance(species, level);
    session.Begin(enemy, isBoss, _ => cooldown = CooldownSec);
    scenes.StartBattle();
}
}