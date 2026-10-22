using UnityEngine;

public interface IActionInput
{
    bool InteractPressed { get; }
}

public interface IInteractable
{
    void Interact(PlayerInteractor who);
}

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private float reach = 0.6f;
    [SerializeField] private float radius = 0.3f;
    [SerializeField] private LayerMask interactableMask;
    private IActionInput input;

    public void Construct(IActionInput actionInput) => input = actionInput;
    private Vector2 ProbePoint => (Vector2)transform.position + movement.Facing * reach;

    void Update()
    {
        if (input == null || !input.InteractPressed) return;
        Collider2D hit = Physics2D.OverlapCircle(ProbePoint, radius, interactableMask);
        if (hit != null && hit.TryGetComponent(out IInteractable target))
        {
            target.Interact(this);
        }
    }
}