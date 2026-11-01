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
        // Nyalakan tanda seru (!) di atas kepala monster jika ada
    }

    public void Tick(float deltaTime)
    {
        // Logika perilaku Chase: mendekati Player dengan kecepatan 2.5 unit/s
        // ... (implementasi movement chase)

        // Cek syarat pindah ke Return: Jarak Player > 6.25
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