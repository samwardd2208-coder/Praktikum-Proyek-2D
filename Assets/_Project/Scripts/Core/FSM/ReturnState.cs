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
        Debug.Log($"{monster.name} kehilangan jejak, masuk ke Return State (Pulang ke Home)");
    }

    public void Tick(float deltaTime)
    {
        // Logika perilaku Return: berjalan kembali ke posisi Home
        // ... (implementasi movement kembali ke Home)

        // Cek syarat pindah ke Wander: Sampai di posisi Home
        float distanceToHome = Vector2.Distance(monster.transform.position, monster.HomePosition);
        if (distanceToHome <= 0.1f) // Toleransi sampai
        {
            stateMachine.ChangeTo(monster.WanderState);
        }
    }

    public void Exit()
    {
        // Cleanup saat kembali ke posisi Home
    }
}