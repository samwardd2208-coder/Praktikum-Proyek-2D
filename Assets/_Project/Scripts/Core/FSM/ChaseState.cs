
using UnityEngine;

public class ChaseState : IState
{
    private WildMonster monster;
    private StateMachine stateMachine;

    public ChaseState(WildMonster monster, StateMachine stateMachine)
    {
        this.monster = monster;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        Debug.Log($"{monster.name} mendeteksi Player! Masuk ke Chase State (!)");
    }

    public void Tick(float deltaTime)
    {
        // Pastikan Player tersedia
        if (monster.Player == null) return;

        // Hitung jarak monster dengan Player
        float distanceToPlayer = Vector2.Distance(
            monster.transform.position,
            monster.Player.position
        );

        // Jika Player terlalu jauh, kembali ke Home
        if (distanceToPlayer > monster.giveUpRadius)
        {
            stateMachine.ChangeTo(monster.ReturnState);
            return;
        }

        // Gerakkan monster menuju Player
        monster.transform.position = Vector2.MoveTowards(
            monster.transform.position,
            monster.Player.position,
            monster.chaseSpeed * deltaTime
        );
    }

    public void Exit()
    {
        Debug.Log($"{monster.name} keluar dari Chase State");
    }
}
