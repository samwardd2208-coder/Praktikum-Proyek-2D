using UnityEngine;

public class WorldInstaller : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private PlayerInteractor interactor;
    public void Install(IMoveInput moveInput, IActionInput actionInput)
    {
        player.Construct(moveInput);
        interactor.Construct(actionInput);
    }
}