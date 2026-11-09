using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    [SerializeField] private Rigidbody2D rb;
    private IMoveInput input;
    private Vector2 velocity;

    public void Construct(IMoveInput moveInput) => input = moveInput;

    public Vector2 Velocity => velocity;
    public Vector2 Facing { get; private set; } = Vector2.down;

    void Update()
    {
        if (input == null) return;
        Vector2 move = input.Move;
        float speed = input.IsRunning ? config.RunSpeed :
                config.WalkSpeed;
        velocity = move * speed;
        if (move.sqrMagnitude > 0.01f) Facing = ToCardinal(move);
    }

    private static Vector2 ToCardinal(Vector2 v) =>
        Mathf.Abs(v.x) > Mathf.Abs(v.y)
            ? new Vector2(Mathf.Sign(v.x), 0f)
            : new Vector2(0f, Mathf.Sign(v.y));

    void FixedUpdate() => rb.linearVelocity = velocity;

    // Dipakai WorldInstaller saat Lanjutkan: pindahkan pemain ke posisi save.
    public void Teleport(Vector2 position, Vector2 facing)
    {
        velocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        rb.position = position;
        transform.position = position;
        if (facing.sqrMagnitude > 0.01f) Facing = ToCardinal(facing);
    }
}