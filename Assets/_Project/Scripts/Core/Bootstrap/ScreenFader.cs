using System.Collections;
using UnityEngine;

// Pasang di Canvas (Image hitam penuh layar) di Scn_Bootstrap.
[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    [SerializeField] private float duration = 0.4f;
    private CanvasGroup group;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
    }

    public IEnumerator FadeOut() => Fade(1f);
    public IEnumerator FadeIn() => Fade(0f);

    private IEnumerator Fade(float target)
    {
        group.blocksRaycasts = true;
        float start = group.alpha;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        group.alpha = target;
        group.blocksRaycasts = target > 0.5f;
    }
}
