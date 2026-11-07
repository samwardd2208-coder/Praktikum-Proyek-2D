using UnityEngine;

public class WorldInstaller : MonoBehaviour
{
    [SerializeField] private PlayerMovementzane player;
    [SerializeField] private PlayerInteractor interactor;

    public void Install(IMoveInput moveInput, IActionInput actionInput)
    {
        player.Construct(moveInput);
        interactor.Construct(actionInput);
    }
}