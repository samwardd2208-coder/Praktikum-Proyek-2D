using UnityEngine;
 

public interface IWildAgent
{
    Vector2 Position { get; }
    Vector2 Home { get; }
    Vector2 DirectionToTarget { get; }  
    bool CanSeeTarget { get; }           
    bool LostTarget { get; }            
    void Move(Vector2 direction, float speed);
    void Stop();
}
 

public class WildStates
{
    public const float WanderSpeed = 1.125f;
    public const float ChaseSpeed = 2.5f;
    public const float ReturnSpeed = 2f;
    public const float WanderRadius = 3f;   
 
    public readonly WanderState Wander;
    public readonly ChaseState Chase;
    public readonly ReturnState Return;
 
    public WildStates(IWildAgent agent, StateMachine fsm)
    {
        Wander = new WanderState(agent, fsm, this);
        Chase = new ChaseState(agent, fsm, this);
        Return = new ReturnState(agent, fsm, this);
    }
}
 

public class WanderState : IState
{
    private readonly IWildAgent agent;
    private readonly StateMachine fsm;
    private readonly WildStates all;
 
    private Vector2 destination;
    private bool walking;
    private float timer;
 
    public WanderState(IWildAgent agent, StateMachine fsm, WildStates all)
    {
        this.agent = agent;
        this.fsm = fsm;
        this.all = all;
    }
 
    public void Enter()
    {
        agent.Stop();
        walking = false;
        timer = Random.Range(0.5f, 1.5f);
    }
 
    public void Tick(float deltaTime)
    {
        if (agent.CanSeeTarget)
        {
            fsm.ChangeTo(all.Chase);
            return;
        }
 
        timer -= deltaTime;
        if (timer <= 0f) PickNext();
 
        if (!walking) return;
 
        Vector2 toDestination = destination - agent.Position;
        if (toDestination.magnitude < 0.1f)
        {
            walking = false;
            agent.Stop();
        }
        else
        {
            agent.Move(toDestination.normalized, WildStates.WanderSpeed);
        }
    }
 
    public void Exit() => agent.Stop();
 
    private void PickNext()
    {
        timer = Random.Range(1f, 2.5f);
        walking = Random.value < 0.6f;
        if (walking)
            destination = agent.Home + Random.insideUnitCircle * WildStates.WanderRadius;
        else
            agent.Stop();
    }
}
 

public class ChaseState : IState
{
    private readonly IWildAgent agent;
    private readonly StateMachine fsm;
    private readonly WildStates all;
 
    public ChaseState(IWildAgent agent, StateMachine fsm, WildStates all)
    {
        this.agent = agent;
        this.fsm = fsm;
        this.all = all;
    }
 
    public void Enter() { }
 
    public void Tick(float deltaTime)
    {
        if (agent.LostTarget)
        {
            fsm.ChangeTo(all.Return);
            return;
        }
        agent.Move(agent.DirectionToTarget, WildStates.ChaseSpeed);
    }
 
    public void Exit() => agent.Stop();
}
 

public class ReturnState : IState
{
    private readonly IWildAgent agent;
    private readonly StateMachine fsm;
    private readonly WildStates all;
 
    public ReturnState(IWildAgent agent, StateMachine fsm, WildStates all)
    {
        this.agent = agent;
        this.fsm = fsm;
        this.all = all;
    }
 
    public void Enter() { }
 
    public void Tick(float deltaTime)
    {
        Vector2 toHome = agent.Home - agent.Position;
        if (toHome.magnitude < 0.15f)
        {
            fsm.ChangeTo(all.Wander);
            return;
        }
        agent.Move(toHome.normalized, WildStates.ReturnSpeed);
    }
 
    public void Exit() => agent.Stop();
}