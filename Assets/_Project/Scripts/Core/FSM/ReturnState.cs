
using UnityEngine;

public class ReturnState : IState
{
    private WildMonster monster;
    private StateMachine stateMachine;

    public ReturnState(WildMonster monster, StateMachine stateMachine)
    {
        this.monster = monster;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        Debug.Log($"{monster.name} masuk ke Return State (Pulang ke Home)");
    }

    public void Tick(float deltaTime)
    {
        // Gerakkan monster kembali ke posisi awal
        Vector2 currentPosition = monster.transform.position;

        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            monster.HomePosition,
            monster.wanderSpeed * deltaTime
        );

        monster.transform.position = new Vector3(
            nextPosition.x,
            nextPosition.y,
            monster.transform.position.z
        );

        // Periksa apakah monster sudah sampai di Home
        float distanceToHome = Vector2.Distance(
            monster.transform.position,
            monster.HomePosition
        );

        if (distanceToHome <= 0.1f)
        {
            // Kembali ke Wander agar siap mengejar lagi
            stateMachine.ChangeTo(monster.WanderState);
        }
    }

    public void Exit()
    {
        Debug.Log($"{monster.name} keluar dari Return State");
    }
}
