using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Monster liar: Wander -> Chase (tanda !) -> Return, dan memicu battle saat bersentuhan.
// Pasang di GameObject yang punya: SpriteRenderer, Rigidbody2D (Gravity 0, Freeze Rotation Z), Collider2D (BUKAN trigger).
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class WildMonster : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private MonsterData species;
    [SerializeField] private int minLevel = 2;
    [SerializeField] private int maxLevel = 4;
    [SerializeField] private bool isBoss;
    [SerializeField] private bool stationary;          // boss: diam, tidak berkeliaran

    [Header("Perilaku")]
    [SerializeField] private float wanderSpeed = 1.125f;
    [SerializeField] private float chaseSpeed = 2.5f;
    [SerializeField] private float wanderRadius = 3f;   // maks jarak dari Home
    [SerializeField] private float sightRadius = 3.25f; // mulai mengejar
    [SerializeField] private float giveUpRadius = 6.25f;// berhenti mengejar
    [SerializeField] private float cooldownAfterBattle = 2.5f;
    [SerializeField] private float respawnSeconds = 20f;

    [Header("Referensi")]
    [SerializeField] private Transform target;          // kosong = otomatis cari tag "Player"
    [SerializeField] private GameObject alert;          // sprite tanda "!" (child object)
    [SerializeField] private float alertHeight = 1.3f;  // boss: 2.2

    [Header("Animasi idle (dari MonsterData.Frames[0..1])")]
    [SerializeField] private float idleFps = 4f;

    [Header("Battle")]
    [SerializeField] private bool useBattleScene = false; // false = battle palsu 2 detik (buat tes)
    [SerializeField] private string battleSceneName = "Scn_Battle";

    public MonsterInstance Instance { get; private set; }

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D col;
    private StateMachine fsm;
    private WanderState wander;
    private ChaseState chase;
    private ReturnState returnHome;

    private Vector2 home;
    private Vector2 desiredVelocity;
    private float cooldown;
    private bool inBattle;

    private float DistanceToTarget => target == null ? float.MaxValue
        : Vector2.Distance(transform.position, target.position);
    private bool Calm => cooldown > 0f || BattleBridge.IsSafe || BattleBridge.InBattle;
    public bool CanSeeTarget => target != null && !Calm && DistanceToTarget < sightRadius;
    public bool LostTarget => Calm || DistanceToTarget > giveUpRadius;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        home = transform.position;
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
            else Debug.LogWarning("WildMonster: tidak ada object ber-tag Player!", this);
        }

        wander = new WanderState(this);
        chase = new ChaseState(this);
        returnHome = new ReturnState(this);
        fsm = new StateMachine();

        if (alert != null)
            alert.transform.localPosition = new Vector3(0f, isBoss ? 2.2f : alertHeight, 0f);
    }

    void Start()
    {
        SpawnInstance();
        fsm.ChangeTo(wander);
    }

    private void SpawnInstance()
    {
        int level = isBoss ? maxLevel : Random.Range(minLevel, maxLevel + 1);
        Instance = new MonsterInstance(species, level);
    }

    void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;

        if (stationary || inBattle || target == null)
            desiredVelocity = Vector2.zero;
        else
            fsm.Tick(Time.deltaTime);

        if (alert != null)
            alert.SetActive(fsm.Current == chase && !inBattle);

        AnimateIdle();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + desiredVelocity * Time.fixedDeltaTime);
    }

    private void AnimateIdle()
    {
        if (species == null || species.Frames == null || species.Frames.Length < 2) return;
        int frame = Mathf.FloorToInt(Time.time * idleFps) % 2;
        sr.sprite = species.Frames[frame];
        if (Mathf.Abs(desiredVelocity.x) > 0.01f) sr.flipX = desiredVelocity.x < 0f;
    }

    // ---------- TRIGGER BATTLE ----------
    void OnCollisionEnter2D(Collision2D c) => TryEncounter(c);
    void OnCollisionStay2D(Collision2D c) => TryEncounter(c);   // sentuh terus-menerus juga memicu

    private void TryEncounter(Collision2D c)
    {
        if (inBattle || Calm || !c.collider.CompareTag("Player")) return;
        StartBattle();
    }

    private void StartBattle()
    {
        inBattle = true;
        desiredVelocity = Vector2.zero;
        if (alert != null) alert.SetActive(false);

        BattleBridge.Begin(Instance, isBoss, OnBattleFinished);

        if (useBattleScene) SceneManager.LoadSceneAsync(battleSceneName, LoadSceneMode.Additive);
        else StartCoroutine(FakeBattle());
    }

    private IEnumerator FakeBattle()
    {
        Debug.Log($"BATTLE! lawan {Instance.Data.DisplayName} Lv {Instance.Level}");
        yield return new WaitForSeconds(2f);
        BattleBridge.Finish(BattleResult.Win);
    }

    private void OnBattleFinished(BattleResult result)
    {
        inBattle = false;
        if (result == BattleResult.Win) StartCoroutine(Respawn());
        else cooldown = cooldownAfterBattle;   // kalah/kabur: monster tenang sebentar
    }

    private IEnumerator Respawn()
    {
        sr.enabled = false; col.enabled = false;
        if (alert != null) alert.SetActive(false);
        yield return new WaitForSeconds(respawnSeconds);

        transform.position = home;
        SpawnInstance();
        sr.enabled = true; col.enabled = true;
        fsm.ChangeTo(wander);
    }

    // ---------- STATES ----------
    private class WanderState : IState
    {
        private readonly WildMonster m;
        private Vector2 point;
        private float timer;
        private bool moving;
        public WanderState(WildMonster m) { this.m = m; }

        public void Enter() { PickNext(); }
        public void Exit() { m.desiredVelocity = Vector2.zero; }

        public void Tick(float dt)
        {
            if (m.CanSeeTarget) { m.fsm.ChangeTo(m.chase); return; }

            timer -= dt;
            if (timer <= 0f) PickNext();

            if (moving)
            {
                Vector2 pos = m.transform.position;
                if (Vector2.Distance(pos, point) < 0.1f) { moving = false; m.desiredVelocity = Vector2.zero; }
                else m.desiredVelocity = (point - pos).normalized * m.wanderSpeed;
            }
        }

        private void PickNext()
        {
            timer = Random.Range(1f, 3f);
            moving = Random.value > 0.4f;      // kadang jalan, kadang diam
            point = m.home + Random.insideUnitCircle * m.wanderRadius;
            if (!moving) m.desiredVelocity = Vector2.zero;
        }
    }

    private class ChaseState : IState
    {
        private readonly WildMonster m;
        public ChaseState(WildMonster m) { this.m = m; }
        public void Enter() { }
        public void Exit() { m.desiredVelocity = Vector2.zero; }

        public void Tick(float dt)
        {
            if (m.LostTarget) { m.fsm.ChangeTo(m.returnHome); return; }
            Vector2 dir = ((Vector2)m.target.position - (Vector2)m.transform.position).normalized;
            m.desiredVelocity = dir * m.chaseSpeed;
        }
    }

    private class ReturnState : IState
    {
        private readonly WildMonster m;
        public ReturnState(WildMonster m) { this.m = m; }
        public void Enter() { }
        public void Exit() { m.desiredVelocity = Vector2.zero; }

        public void Tick(float dt)
        {
            Vector2 pos = m.transform.position;
            if (Vector2.Distance(pos, m.home) < 0.1f) { m.fsm.ChangeTo(m.wander); return; }
            m.desiredVelocity = (m.home - pos).normalized * m.wanderSpeed * 2f;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? (Vector3)home : transform.position;
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(center, wanderRadius);
        Gizmos.color = Color.red;    Gizmos.DrawWireSphere(transform.position, sightRadius);
        Gizmos.color = Color.gray;   Gizmos.DrawWireSphere(transform.position, giveUpRadius);
    }
#endif
}
