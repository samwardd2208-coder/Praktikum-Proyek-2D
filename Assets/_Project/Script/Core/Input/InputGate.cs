using UnityEngine;

// Decorator input: meneruskan input asli, tapi mengembalikan nol saat Lock().
// Dipakai supaya Player diam selama battle.
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
    public void Unlock() => lockCount = Mathf.Max(0, lockCount - 1);

    public Vector2 Move => IsLocked ? Vector2.zero : move.Move;
    public bool IsRunning => !IsLocked && move.IsRunning;
    public bool InteractPressed => !IsLocked && action.InteractPressed;
}