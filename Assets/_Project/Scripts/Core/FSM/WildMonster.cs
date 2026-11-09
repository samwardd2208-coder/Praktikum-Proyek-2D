using System.Collections;
using UnityEngine;

public class WildMonster : MonoBehaviour
{
    // ==============================
    // Service (diisi WorldInstaller lewat Construct)
    // ==============================

    private BattleSession session;
    private SceneFlow scenes;
    private PlayerParty party;

    public void Construct(BattleSession session, SceneFlow scenes, PlayerParty party, Transform playerTransform)
    {
        this.session = session;
        this.scenes = scenes;
        this.party = party;
        if (playerTransform != null) Player = playerTransform;
    }

    // ==============================
    // FSM
    // ==============================

    public StateMachine StateMachine { get; private set; }

    public WanderState WanderState { get; private set; }
    public ChaseState ChaseState { get; private set; }
    public ReturnState ReturnState { get; private set; }

    // ==============================
    // Monster Data
    // ==============================

    [Header("Monster Data")]
    [SerializeField] private MonsterData species;     // contoh: Mon_Sprig
    [SerializeField] private int level = 3;           // barat 2-4, timur 5-7, boss 9
    [SerializeField] private bool isBoss;
    [SerializeField] private float respawnSeconds = 20f;

    public MonsterInstance MonsterInstance { get; private set; }

    // ==============================
    // References & Settings
    // ==============================

    [Header("References")]
    public Transform Player;
    public Vector2 HomePosition;

    [SerializeField] private GameObject alert;

    [Header("Movement Settings")]
    public float wanderSpeed = 1.125f;
    public float chaseSpeed = 2.5f;
    public Vector2 desiredVelocity;

    [Header("Detection Settings")]
    public float sightRadius = 3.25f;
    public float giveUpRadius = 6.25f;

    [Header("Monster Settings")]
    [SerializeField] private bool stationary;
    private bool inBattle;

    private float cooldown;
    private const float CooldownDuration = 2.5f;

    private Rigidbody2D rb;

    // ==============================
    // Target Information
    // ==============================

    public Transform Target => Player;

    public float DistanceToTarget =>
        Player != null
        ? Vector2.Distance(transform.position, Player.position)
        : Mathf.Infinity;

    // ==============================
    // Calm & Detection Conditions
    // ==============================

    // Tenang: masih cooldown (setelah kalah/kabur) atau masa aman 2,2 s setelah battle.
    private bool Calm => cooldown > 0f || (session != null && session.IsSafe);

    // Hanya mengejar kalau pemain punya partner.
    public bool CanSeeTarget =>
        Player != null &&
        party != null && party.Partner != null &&
        !Calm &&
        DistanceToTarget < sightRadius;

    public bool LostTarget =>
        Calm ||
        Player == null ||
        DistanceToTarget > giveUpRadius;

    // ==============================
    // Lifecycle
    // ==============================

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        StateMachine = new StateMachine();
        WanderState = new WanderState(this, StateMachine);
        ChaseState = new ChaseState(this, StateMachine);
        ReturnState = new ReturnState(this, StateMachine);

        HomePosition = transform.position;
    }

    private void Start()
    {
        if (Player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) Player = playerObj.transform;
        }

        if (species != null)
            MonsterInstance = new MonsterInstance(species, level);
        else
            Debug.LogWarning($"{name}: slot Species belum diisi", this);

        StateMachine.ChangeTo(WanderState);
    }

    private void Update()
    {
        if (cooldown > 0f)
            cooldown -= Time.deltaTime;

        if (stationary || inBattle || Target == null)
            desiredVelocity = Vector2.zero;
        else
            StateMachine.Tick(Time.deltaTime);

        if (alert != null)
            alert.SetActive(StateMachine.Current == ChaseState && !inBattle);
    }

    // Gerakkan Rigidbody. Kalau state Anda sudah menggerakkan transform sendiri
    // dan monster jadi bergerak dobel, hapus method ini.
    private void FixedUpdate()
    {
        if (rb == null) return;
        rb.linearVelocity = inBattle ? Vector2.zero : desiredVelocity;
    }

    // ==============================
    // Encounter
    // ==============================

    private void OnCollisionEnter2D(Collision2D c) => TryStartBattle(c.collider);

    // Boss diam: sentuh terus-menerus juga harus memicu battle.
    private void OnCollisionStay2D(Collision2D c) => TryStartBattle(c.collider);

    private void TryStartBattle(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (inBattle || Calm) return;
        if (scenes == null || MonsterInstance == null) return;
        if (party == null || party.Partner == null) return;

        SetInBattle(true);
        desiredVelocity = Vector2.zero;
        scenes.StartBattle(MonsterInstance, isBoss, OnBattleFinished);
    }

    private void OnBattleFinished(BattleResult result)
    {
        if (result == BattleResult.Win && !isBoss)
        {
            StartCoroutine(RespawnRoutine());   // monster hilang, muncul lagi 20 s kemudian
            return;
        }

        SetInBattle(false);
        if (result != BattleResult.Win)
            StartCooldown();                    // kalah/kabur: tenang 2,5 s
    }

    private IEnumerator RespawnRoutine()
    {
        var sprites = GetComponentsInChildren<SpriteRenderer>();
        var colliders = GetComponentsInChildren<Collider2D>();
        foreach (var s in sprites) s.enabled = false;
        foreach (var c in colliders) c.enabled = false;

        yield return new WaitForSeconds(respawnSeconds);

        transform.position = HomePosition;
        MonsterInstance = new MonsterInstance(species, level);   // HP penuh lagi
        foreach (var s in sprites) s.enabled = true;
        foreach (var c in colliders) c.enabled = true;
        StateMachine.ChangeTo(WanderState);
        SetInBattle(false);
        StartCooldown();
    }

    // ==============================
    // Battle & Cooldown
    // ==============================

    public void StartCooldown()
    {
        cooldown = CooldownDuration;
    }

    public void SetInBattle(bool value)
    {
        inBattle = value;

        if (inBattle && alert != null)
            alert.SetActive(false);
    }
}