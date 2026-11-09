using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ToastView : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform panel;
    [SerializeField] private Text label;
    [SerializeField] private float holdSeconds = 2.5f;

    private Coroutine routine;

    private void Awake() { group.alpha = 0f; }

    public void Show(string message)
    {
        label.text = message;
        panel.sizeDelta = new Vector2(Mathf.Min(1000f, message.Length * 20f + 64f), panel.sizeDelta.y);
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        yield return Fade(0f, 1f, 0.15f);
        float t = 0f;
        while (t < holdSeconds) { t += Time.unscaledDeltaTime; yield return null; }
        yield return Fade(1f, 0f, 0.3f);
        routine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
    }
}