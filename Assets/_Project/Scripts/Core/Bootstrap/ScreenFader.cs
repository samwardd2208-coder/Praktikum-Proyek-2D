using System.Collections;
using UnityEngine;
 

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float duration = 0.35f;
 
    void Awake()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.alpha = 1f;               
            group.blocksRaycasts = true;
        }
    }
 
    public IEnumerator FadeOut() => FadeTo(1f);
    public IEnumerator FadeIn() => FadeTo(0f);
 
    private IEnumerator FadeTo(float target)
    {
        if (group == null) yield break;
 
        group.blocksRaycasts = true;
        float start = group.alpha;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        group.alpha = target;
        group.blocksRaycasts = target > 0f;
    }
}
 