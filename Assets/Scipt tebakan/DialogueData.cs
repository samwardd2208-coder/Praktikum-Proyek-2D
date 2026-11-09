using UnityEngine;

[CreateAssetMenu(menuName = "Pulau Monster/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string speaker;
    [TextArea(1, 3)] public string[] lines;
}