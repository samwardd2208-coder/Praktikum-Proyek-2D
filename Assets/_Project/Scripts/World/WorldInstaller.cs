using UnityEngine;

public class WorldInstaller : MonoBehaviour
{
    [SerializeField] private PlayerMovementzane player;

    public void Install(IMoveInput moveInput)
    {
        player.Construct(moveInput);
    }
}