public interface IState
{
    void Enter();
    void Tick(float deltaTime);
    void Exit();
}

public class StateMachine
{
    public IState Current { get; private set; }
 public void ChangeTo(IState next)
    {
        if (next == Current) return;
        Current?.Exit();
        Current = next;
        Current.Enter();
    }
    public void Tick(float deltaTime) =>
            Current?.Tick(deltaTime);
}