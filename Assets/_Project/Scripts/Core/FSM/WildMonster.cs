using UnityEngine;

public class WildMonster : MonoBehaviour
{
    public StateMachine StateMachine { get; private set; }

    // Instance Data Monster (bisa diinisialisasi dari database atau Inspector)
    public MonsterInstance MonsterInstance { get; private set; }

    [Header("FSM States")]
    public WanderState WanderState { get; private set; }
    public ChaseState ChaseState { get; private set; }
    public ReturnState ReturnState { get; private set; }

    [Header("References & Settings")]
    public Transform Player;          // Tarik objek Player ke sini di Inspector
    public Vector2 HomePosition;      // Titik asal (Home) monster

    [Header("Movement Settings")]
    public float wanderSpeed = 1.125f;
    public float chaseSpeed = 2.5f;

    private void Awake()
    {
        // Inisialisasi StateMachine
        StateMachine = new StateMachine();

        // Inisialisasi ketiga state dan daftarkan referensi ke script ini
        WanderState = new WanderState(this, StateMachine);
        ChaseState = new ChaseState(this, StateMachine);
        ReturnState = new ReturnState(this, StateMachine);

        // Simpan posisi awal saat game mulai sebagai HomePosition
        HomePosition = transform.position;
    }

    private void Start()
    {
        // Cari objek Player secara otomatis jika belum di-assign di Inspector
        if (Player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) Player = playerObj.transform;
        }

        // Mulai state awal monster (Wander)
        StateMachine.ChangeTo(WanderState);
    }

    private void Update()
    {
        // Jalankan Tick FSM setiap frame berdasarkan DeltaTime
        StateMachine.Tick(Time.deltaTime);
    }
}