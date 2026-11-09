using System.Collections;
using UnityEngine;

// Monster liar di dunia. Perilaku diatur FSM (WildStates).
// Dependensi (battle, scenes, party, target) disuntik WorldInstaller lewat Construct().
[RequireComponent(typeof(Rigidbody2D))]
public class WildMonster : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private MonsterData species;
    [SerializeField] private int level = 3;
    [SerializeField] private bool isBoss;
    [SerializeField] private bool stationary;      // boss: diam, tidak berkeliaran

    [Header("Gerak (unit/detik)")]
    [SerializeField] private float wanderSpeed = 1.125f;
    [SerializeField] private float chaseSpeed = 2.5f;
    [SerializeField] private float returnSpeed = 2f;
    [SerializeField] private float wanderRadius = 3f;     // maks. jarak dari Home

    [Header("Penglihatan")]
    [SerializeField] private float sightRadius = 3.25f;   // mulai mengejar
    [SerializeField] private float giveUpRadius = 6.25f;  // menyerah

    [Header("Waktu (detik)")]
    [SerializeField] private float fleeCooldown = 2.5f;   // setelah pemain kalah/kabur
    [SerializeField] private float respawnDelay = 20f;    // setelah dikalahkan

    [Header("Visual")]
    [SerializeField] private GameObject alert;            // sprite UI_Alert (tanda !)
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private float idleFrameSeconds = 0.4f;

    private Rigidbody2D rb;
    private Collider2D[] colliders;
    private Animator animator;

    private readonly StateMachine fsm = new StateMachine();
    private WanderState wander;
    private ChaseState chase;
    private ReturnState returnHome;

    private BattleSession session;
    private SceneFlow scenes;
    private PlayerParty party;
    private DialogueRunner dialogue;   // P6: auman boss (boleh null)
    private StoryFlags flags;          // P6: boss yang sudah kalah tidak muncul lagi (boleh null)

    private Vector2 home;
    private Vector2 desiredVelocity;
    private float cooldown;
    private bool inBattle;
    private bool defeated;
    private float respawnAt;

    // ---------- API untuk state ----------
    public MonsterInstance Monster { get; private set; }
    public Transform Target { get; private set; }
    public Vector2 Home => home;
    public float WanderSpeed => wanderSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float ReturnSpeed => returnSpeed;
    public float WanderRadius => wanderRadius;
    public IState StateWander => wander;
    public IState StateChase => chase;
    public IState StateReturn => returnHome;

    public void ChangeState(IState next) => fsm.ChangeTo(next);
    public void Move(Vector2 velocity) => desiredVelocity = velocity;

    public float DistanceToTarget =>
        Target != null ? Vector2.Distance(transform.position, Target.position) : float.MaxValue;

    // Tenang = tidak mengejar: 2,5 s setelah kalah/kabur, atau selama waktu aman
    // setelah battle (2,2 s), atau saat ada battle lain berlangsung.
    private bool Calm => cooldown > 0f || (session != null && (session.IsSafe || session.IsActive));

    public bool CanSeeTarget => party != null && party.Partner != null && !Calm
                                && DistanceToTarget < sightRadius;
    public bool LostTarget => Calm || DistanceToTarget > giveUpRadius;

    private bool Ready => Monster != null && session != null && scenes != null
                          && party != null && Target != null;

    // ---------- Siklus hidup ----------
    // dialogue dan flags opsional, jadi pemanggilan lama Construct(4 argumen) tetap valid.
    public void Construct(BattleSession battle, SceneFlow sceneFlow, PlayerParty playerParty, Transform target,
                          DialogueRunner dialogueRunner = null, StoryFlags storyFlags = null)
    {
        session = battle;
        scenes = sceneFlow;
        party = playerParty;
        Target = target;
        dialogue = dialogueRunner;
        flags = storyFlags;

        // P6: boss yang sudah dikalahkan (mis. setelah Lanjutkan) tidak muncul lagi.
        if (isBoss && flags != null && flags.Has(StoryFlags.BossDefeated))
        {
            defeated = true;
            respawnAt = float.PositiveInfinity;
            SetVisible(false);
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponents<Collider2D>();
        animator = GetComponent<Animator>();
        if (body == null) body = GetComponent<SpriteRenderer>();

        home = transform.position;
        wander = new WanderState(this);
        chase = new ChaseState(this);
        returnHome = new ReturnState(this);

        if (species != null) Monster = new MonsterInstance(species, level);
        else Debug.LogWarning($"{name}: field Species belum diisi.", this);

        if (alert != null) alert.SetActive(false);
        fsm.ChangeTo(wander);
        AnimateIdle();
    }

    void Update()
    {
        if (defeated)
        {
            if (Time.time >= respawnAt) Respawn();
            return;
        }

        if (cooldown > 0f) cooldown -= Time.deltaTime;

        if (stationary || inBattle || !Ready)
            desiredVelocity = Vector2.zero;
        else
            fsm.Tick(Time.deltaTime);

        if (alert != null)
            alert.SetActive(fsm.Current == chase && !inBattle);

        AnimateIdle();
    }

    void FixedUpdate()
    {
        if (defeated) return;
        rb.linearVelocity = (inBattle || !Ready) ? Vector2.zero : desiredVelocity;
    }

    // ---------- Sentuhan -> battle ----------
    // OnCollisionStay2D: sentuh terus-menerus juga memicu battle (penting untuk boss yang diam).
    void OnCollisionEnter2D(Collision2D c) => TouchedBy(c.collider);
    void OnCollisionStay2D(Collision2D c) => TouchedBy(c.collider);

    private void TouchedBy(Collider2D other)
    {
        if (!IsPlayer(other)) return;
        TryBeginBattle();
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null;
    }

    private void TryBeginBattle()
    {
        if (defeated || inBattle || !Ready || Calm) return;
        if (party.Partner == null) return;   // belum punya partner: belum boleh battle

        inBattle = true;
        desiredVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        if (alert != null) alert.SetActive(false);

        // P6: boss mengaum dulu lewat DialogueRunner, baru battle dimulai.
        if (isBoss && dialogue != null) StartCoroutine(RoarThenBattle());
        else BeginBattleNow();
    }

    private void BeginBattleNow()
    {
        session.Begin(Monster, isBoss, OnBattleFinished);   // 1. Begin: musuh + level
        scenes.StartBattle();                               // 2. Kunci input, fade, Scn_Battle
    }

    private IEnumerator RoarThenBattle()
    {
        dialogue.Run(Roar());
        yield return null;
        while (dialogue.IsRunning) yield return null;
        BeginBattleNow();
    }

    private IEnumerator Roar()
    {
        yield return dialogue.Say("",
            $"GRRRAAAH!! {Monster.Data.DisplayName} menghalangi jalan, percikan listrik memenuhi gua!");
    }

    // Dipanggil BattleSession.Finish (callback khusus monster yang memulai battle).
    private void OnBattleFinished(BattleResult result)
    {
        inBattle = false;

        if (result == BattleResult.Win)
        {
            Defeat();
            return;
        }

        cooldown = fleeCooldown;       // kalah / kabur: 2,5 s tidak mengejar
        fsm.ChangeTo(returnHome);
    }

    // ---------- Kalah & respawn ----------
    private void Defeat()
    {
        defeated = true;
        // P6: boss tidak respawn (ending ada di P7). Monster biasa respawn seperti biasa.
        respawnAt = isBoss ? float.PositiveInfinity : Time.time + respawnDelay;
        desiredVelocity = Vector2.zero;
        SetVisible(false);
    }

    private void Respawn()
    {
        defeated = false;
        transform.position = home;
        rb.position = home;
        if (species != null) Monster = new MonsterInstance(species, level);
        cooldown = 0f;
        SetVisible(true);
        fsm.ChangeTo(wander);
    }

    private void SetVisible(bool visible)
    {
        if (body != null) body.enabled = visible;
        foreach (Collider2D c in colliders) c.enabled = visible;
        rb.simulated = visible;
        if (!visible && alert != null) alert.SetActive(false);
    }

    // ---------- Animasi idle ----------
    // Kalau Animator + AOC_Mon_<Nama> (Praktikum 3) terpasang, Animator yang mengurus.
    // Kalau tidak, frame idle diganti lewat kode supaya monster tetap terlihat bergerak.
    private void AnimateIdle()
    {
        if (body == null || Monster == null) return;
        if (animator != null && animator.runtimeAnimatorController != null) return;

        Sprite[] frames = Monster.Data.Frames;
        if (frames == null || frames.Length == 0) return;

        int count = Mathf.Min(2, frames.Length);
        int index = Mathf.FloorToInt(Time.time / idleFrameSeconds) % count;
        body.sprite = frames[index];
    }
}