using UnityEngine;
 
public class InputGate : IMoveInput, IActionInput
{
    private readonly IMoveInput move;
    private readonly IActionInput action;
    private int lockCount;
 
    public InputGate(IMoveInput move, IActionInput action)
    {
        this.move = move;
        this.action = action;
    }
 
    public bool IsLocked => lockCount > 0;
 
    public void Lock() => lockCount++;
 
    public void Unlock()
    {
        if (lockCount > 0) lockCount--;
    }
 
    public Vector2 Move => IsLocked ? Vector2.zero : move.Move;
    public bool IsRunning => !IsLocked && move.IsRunning;
    public bool InteractPressed => !IsLocked && action.InteractPressed;
}
 