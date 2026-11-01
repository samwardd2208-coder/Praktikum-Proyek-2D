using UnityEngine;

public class WorldInstaller : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;

    public void Install(IMoveInput moveInput) => player.Construct(moveInput);
}