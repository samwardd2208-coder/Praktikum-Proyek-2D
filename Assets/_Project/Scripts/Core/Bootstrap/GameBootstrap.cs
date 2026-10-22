using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private string firstScene = "Scn_World";
    private IMoveInput moveInput;
    private IActionInput actionInput;

    void Awake()
    {
        moveInput = new InputSystemMoveInput();
        actionInput = new InputSystemActionInput();
    }

    IEnumerator Start()
    {
        yield return SceneManager.LoadSceneAsync(firstScene, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(firstScene));
        FindAnyObjectByType<WorldInstaller>().Install(moveInput, actionInput);
    }
}