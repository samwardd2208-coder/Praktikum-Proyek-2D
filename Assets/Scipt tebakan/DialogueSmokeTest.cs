using System.Collections;
using UnityEngine;

public class DialogueSmokeTest : MonoBehaviour
{
    [SerializeField] private DialogueRunner runner;

    [ContextMenu("Run Test")]
    private void RunTest() => runner.Run(Sequence());

    private IEnumerator Sequence()
    {
        yield return runner.Say("Tetua Rahmat", "Ah, kamu datang juga! Hutan di utara penuh monster liar.");
        int choice = -1;
        yield return runner.Choose("Tetua Rahmat", "Siapa yang akan menjadi partnermu?",
            new[] { "Pyrit (Pyro)", "Blubi (Aqua)", "Sprig (Sprout)" }, c => choice = c);
        yield return runner.Narrate("Kamu memilih nomor " + (choice + 1) + ".");
    }
}