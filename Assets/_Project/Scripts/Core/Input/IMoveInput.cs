using UnityEngine;

public interface IMoveInput
{
    Vector2 Move { get; }
    bool IsRunning { get; }
}