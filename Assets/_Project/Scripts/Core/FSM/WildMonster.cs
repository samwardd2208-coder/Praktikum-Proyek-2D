
using UnityEngine;

public class WildMonster : MonoBehaviour
{
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

    public MonsterInstance MonsterInstance { get; private set; }

    // ==============================
    // References & Settings
    // ==============================

    [Header("References")]
    public Transform Player;
    public Vector2 HomePosition;

    [SerializeField] private GameObject alert;

    // ==============================
    // Movement Settings
    // ==============================

    [Header("Movement Settings")]
    public float wanderSpeed = 1.125f;
    public float chaseSpeed = 2.5f;

    public Vector2 desiredVelocity;

    // ==============================
    // Detection Settings
    // ==============================

    [Header("Detection Settings")]
    public float sightRadius = 3.25f;
    public float giveUpRadius = 6.25f;

    // ==============================
    // Monster Settings
    // ==============================

    [Header("Monster Settings")]
    [SerializeField] private bool stationary;
    private bool inBattle;

    private float cooldown;
    private const float CooldownDuration = 2.5f;

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

    private bool Calm => cooldown > 0f;

    public bool CanSeeTarget =>
        Player != null &&
        !Calm &&
        DistanceToTarget < sightRadius;

    public bool LostTarget =>
        Calm ||
        Player == null ||
        DistanceToTarget > giveUpRadius;

    // ==============================
    // AWAKE
    // ==============================

    private void Awake()
    {
        // Inisialisasi StateMachine
        StateMachine = new StateMachine();

        // Inisialisasi ketiga state
        WanderState = new WanderState(this, StateMachine);
        ChaseState = new ChaseState(this, StateMachine);
        ReturnState = new ReturnState(this, StateMachine);

        // Simpan posisi awal monster
        HomePosition = transform.position;
    }

    // ==============================
    // START
    // ==============================

    private void Start()
    {
        // Cari Player otomatis jika belum di-assign
        if (Player == null)
        {
            GameObject playerObj =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
                Player = playerObj.transform;
        }

        // Mulai dari state Wander
        StateMachine.ChangeTo(WanderState);
    }

    // ==============================
    // UPDATE
    // ==============================

    private void Update()
    {
        // Kurangi cooldown
        if (cooldown > 0f)
            cooldown -= Time.deltaTime;

        // Monster diam jika kondisi tertentu terpenuhi
        if (stationary || inBattle || Target == null)
        {
            desiredVelocity = Vector2.zero;
        }
        else
        {
            // Jalankan FSM setiap frame
            StateMachine.Tick(Time.deltaTime);
        }

        // Tampilkan tanda seru saat Chase
        if (alert != null)
        {
            alert.SetActive(
                StateMachine.Current == ChaseState && !inBattle
            );
        }
    }

    // ==============================
    // BATTLE & COOLDOWN
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
