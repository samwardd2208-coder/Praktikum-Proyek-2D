using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int AnimSpeed = Animator.StringToHash("AnimSpeed");
    [SerializeField] private PlayerMovement movement;
    private Animator animator;
    void Awake() => animator = GetComponent<Animator>();
    void Update()
    {
        animator.SetBool(IsMoving, movement.Velocity.sqrMagnitude > 0.01f);
        animator.SetFloat(MoveX, movement.Facing.x);
        animator.SetFloat(MoveY, movement.Facing.y);
        animator.SetFloat(AnimSpeed, movement.IsRunning ? 1.5f : 1.0f);
    }
}