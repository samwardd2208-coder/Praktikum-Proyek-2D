using UnityEngine;

public class WanderState : IState
{
    private WildMonster monster;
    private StateMachine stateMachine;

    public WanderState(WildMonster monster, StateMachine stateMachine)
    {
        this.monster = monster;
        this.stateMachine = stateMachine;
    }
    
    public void Enter()
    {
        Debug.Log($"{monster.name} masuk ke Wander State (Berkeliaran)");
    }

    public void Tick(float deltaTime)
    {
        float distanceToPlayer = Vector2.Distance(monster.transform.position, monster.Player.position);
        if (distanceToPlayer > 6.25f)
        {
            stateMachine.ChangeTo(monster.ReturnState);
        }
    }

    public void Exit()
    {
        // Matikan tanda seru (!) saat berhenti mengejar
    }
}