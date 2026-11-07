using UnityEngine;

public class PlayerMovementzane : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    private IMoveInput input;
    private Vector2 velocity;
    private Rigidbody2D rb; 

    
    public Vector2 Velocity => velocity;
    public Vector2 Facing { get; private set; } = Vector2.down;

    void Awake() => rb = GetComponent<Rigidbody2D>(); 

    public void Construct(IMoveInput moveInput) => input = moveInput;

    void Update()
    {
        if (input == null) return;

        Vector2 move = input.Move;
        float speed = input.IsRunning ? config.RunSpeed : config.WalkSpeed;
        
        
        velocity = move * speed;

        
        if (move.sqrMagnitude > 0.01f)
        {
            Facing = ToCardinal(move);
        }
    }

    
    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
    }

    private static Vector2 ToCardinal(Vector2 v) =>
        Mathf.Abs(v.x) > Mathf.Abs(v.y)
            ? new Vector2(Mathf.Sign(v.x), 0f)
            : new Vector2(0f, Mathf.Sign(v.y));
}