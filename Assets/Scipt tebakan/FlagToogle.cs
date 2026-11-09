using UnityEngine;

public class FlagToggle : MonoBehaviour
{
    [SerializeField] private string flagKey = StoryFlags.HasStarter;
    [SerializeField] private GameObject hideWhenSet;     // Gate_Hutan

    private StoryFlags flags;

    public void Construct(StoryFlags f)
    {
        flags = f;
        flags.Changed += OnChanged;
        Apply();
    }

    private void OnDestroy() { if (flags != null) flags.Changed -= OnChanged; }
    private void OnChanged(string key) { if (key == flagKey) Apply(); }
    private void Apply() { if (hideWhenSet != null) hideWhenSet.SetActive(!flags.Has(flagKey)); }
}