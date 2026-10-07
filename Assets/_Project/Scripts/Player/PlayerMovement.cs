using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Rigidbody2D rb;
    private InputAction moveAction;
    private Vector2 moveInput;
    void Start() => moveAction =
            InputSystem.actions.FindAction("Move");
    void Update() => moveInput = moveAction.ReadValue<Vector2>();
    void FixedUpdate() => rb.linearVelocity = moveInput * moveSpeed;
}
