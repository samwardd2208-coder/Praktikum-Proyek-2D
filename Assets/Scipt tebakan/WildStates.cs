using UnityEngine;

// Tiga state monster liar. Masing-masing tahu kapan harus pindah.
//   Wander : jalan acak / diam, <= WanderRadius dari Home      -> Chase  bila Player terlihat
//   Chase  : mendekati Player                                  -> Return bila Player hilang
//   Return : pulang ke Home                                    -> Wander saat sampai

public class WanderState : IState
{
    private readonly WildMonster m;
    private Vector2 destination;
    private bool walking;
    private float timer;

    public WanderState(WildMonster monster) { m = monster; }

    public void Enter() => StartIdle();

    public void Tick(float dt)
    {
        if (m.CanSeeTarget) { m.ChangeState(m.StateChase); return; }

        timer -= dt;
        if (walking)
        {
            Vector2 toDest = destination - (Vector2)m.transform.position;
            if (toDest.magnitude <= 0.1f || timer <= 0f) StartIdle();
            else m.Move(toDest.normalized * m.WanderSpeed);
        }
        else if (timer <= 0f)
        {
            StartWalk();
        }
    }

    public void Exit() => m.Move(Vector2.zero);

    private void StartIdle()
    {
        walking = false;
        timer = Random.Range(1f, 2.5f);
        m.Move(Vector2.zero);
    }

    private void StartWalk()
    {
        walking = true;
        destination = m.Home + Random.insideUnitCircle * m.WanderRadius;
        timer = Random.Range(2f, 4f);
    }
}

public class ChaseState : IState
{
    private readonly WildMonster m;

    public ChaseState(WildMonster monster) { m = monster; }

    public void Enter() { }   // tanda ! dinyalakan WildMonster saat state ini aktif

    public void Tick(float dt)
    {
        if (m.LostTarget) { m.ChangeState(m.StateReturn); return; }

        Vector2 toTarget = (Vector2)m.Target.position - (Vector2)m.transform.position;
        m.Move(toTarget.normalized * m.ChaseSpeed);
    }

    public void Exit() => m.Move(Vector2.zero);
}

public class ReturnState : IState
{
    private const float GiveUpSeconds = 8f;   // jaga-jaga kalau tersangkut penghalang
    private readonly WildMonster m;
    private float elapsed;

    public ReturnState(WildMonster monster) { m = monster; }

    public void Enter() => elapsed = 0f;

    public void Tick(float dt)
    {
        elapsed += dt;
        Vector2 toHome = m.Home - (Vector2)m.transform.position;
        if (toHome.magnitude <= 0.15f || elapsed > GiveUpSeconds)
        {
            m.ChangeState(m.StateWander);
            return;
        }
        m.Move(toHome.normalized * m.ReturnSpeed);
    }

    public void Exit() => m.Move(Vector2.zero);
}
