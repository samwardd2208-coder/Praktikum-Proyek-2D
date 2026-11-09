// Finite State Machine sederhana (IState + StateMachine)
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
        Current?.Exit();     // Exit lama -> Enter baru, selalu berurutan
        Current = next;
        Current.Enter();
    }

    public void Tick(float deltaTime) => Current?.Tick(deltaTime);
}
