using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    [SerializeField] private Rigidbody2D rb;

    private IMoveInput input;
    private Vector2 velocity;

    // Tambahan dari modul untuk akses animasi
    public Vector2 Velocity => velocity;
    public Vector2 Facing { get; private set; } = Vector2.down;
    public bool IsRunning => input != null && input.IsRunning;
    public void Construct(IMoveInput moveInput) => input = moveInput;

    // BARU: pindahkan pemain (Lanjutkan dari save, kembali ke Batu Simpan, dll.)
    public void Teleport(Vector2 position, Vector2 facing)
    {
        velocity = Vector2.zero;
        transform.position = position;
        if (rb != null)
        {
            rb.position = position;
            rb.linearVelocity = Vector2.zero;
        }
        Facing = facing;
    }

    void Update()
    {
        if (input == null) return;

        // Diubah sesuai modul
        Vector2 move = input.Move;
        float speed = input.IsRunning ? config.RunSpeed : config.WalkSpeed;
        velocity = move * speed;

        // Tambahan untuk mengunci arah hadap kardinal (Atas, Bawah, Kiri, Kanan)
        if (move.sqrMagnitude > 0.01f)
            Facing = ToCardinal(move);
    }

    void FixedUpdate() => rb.linearVelocity = velocity;

    // Tambahan fungsi Helper dari modul
    private static Vector2 ToCardinal(Vector2 v) =>
        Mathf.Abs(v.x) > Mathf.Abs(v.y)
            ? new Vector2(Mathf.Sign(v.x), 0f)
            : new Vector2(0f, Mathf.Sign(v.y));
}
