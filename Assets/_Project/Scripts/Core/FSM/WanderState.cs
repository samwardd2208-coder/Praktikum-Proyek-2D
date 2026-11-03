
using UnityEngine;

public class WanderState : IState
{
    private WildMonster monster;
    private StateMachine stateMachine;

    // Pengaturan Wander
    private Vector2 wanderTarget;
    private float waitTimer;

    private const float WanderRadius = 1.5f;
    private const float ReachDistance = 0.1f;
    private const float MinWaitTime = 0.5f;
    private const float MaxWaitTime = 1.5f;

    public WanderState(WildMonster monster, StateMachine stateMachine)
    {
        this.monster = monster;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        Debug.Log($"{monster.name} masuk ke Wander State (Berkeliaran)");

        // Pilih tujuan pertama
        ChooseNewTarget();
    }

    public void Tick(float deltaTime)
    {
        // Periksa apakah Player mendekat
        if (monster.CanSeeTarget)
        {
            stateMachine.ChangeTo(monster.ChaseState);
            return;
        }

        // Tunggu sebelum memilih tujuan baru
        if (waitTimer > 0f)
        {
            waitTimer -= deltaTime;
            return;
        }

        // Posisi monster saat ini
        Vector2 currentPosition = monster.transform.position;

        // Hitung jarak menuju tujuan
        float distance = Vector2.Distance(
            currentPosition,
            wanderTarget
        );

        // Jika sudah sampai tujuan
        if (distance <= ReachDistance)
        {
            waitTimer = Random.Range(MinWaitTime, MaxWaitTime);
            ChooseNewTarget();
            return;
        }

        // Gerakkan monster menuju tujuan
        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            wanderTarget,
            monster.wanderSpeed * deltaTime
        );

        monster.transform.position = new Vector3(
            nextPosition.x,
            nextPosition.y,
            monster.transform.position.z
        );
    }

    private void ChooseNewTarget()
    {
        // Tentukan titik acak di sekitar posisi awal
        Vector2 randomOffset =
            Random.insideUnitCircle * WanderRadius;

        wanderTarget = monster.HomePosition + randomOffset;
    }

    public void Exit()
    {
        Debug.Log($"{monster.name} keluar dari Wander State");
    }
}
