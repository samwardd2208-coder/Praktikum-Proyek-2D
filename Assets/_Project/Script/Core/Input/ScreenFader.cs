using System.Collections;
using UnityEngine;

// Layar memudar hitam. Pasang di Canvas (Screen Space Overlay) yang punya CanvasGroup
// dan satu Image hitam di dalamnya.
public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float duration = 0.35f;

    void Awake()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
        }
    }

    public IEnumerator FadeOut() => Fade(1f);   // layar jadi hitam
    public IEnumerator FadeIn() => Fade(0f);    // layar terang lagi

    private IEnumerator Fade(float target)
    {
        if (group == null) yield break;

        float start = group.alpha;
        float t = 0f;
        group.blocksRaycasts = true;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        group.alpha = target;
        group.blocksRaycasts = target > 0.5f;
    }
}
