using UnityEngine;

public class InputGate : IMoveInput, IActionInput
{
    private readonly IMoveInput moveInput;
    private readonly IActionInput actionInput;
    private bool isLocked;

    public InputGate(IMoveInput moveInput, IActionInput actionInput)
    {
        this.moveInput = moveInput;
        this.actionInput = actionInput;
    }

    public void Lock() => isLocked = true;
    public void Unlock() => isLocked = false;
    public bool IsLocked => isLocked;

    public Vector2 Move => isLocked ? Vector2.zero : moveInput.Move;
    public bool IsRunning => !isLocked && moveInput.IsRunning;
    public bool InteractPressed => !isLocked && actionInput.InteractPressed;
}