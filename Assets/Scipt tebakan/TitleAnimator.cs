using UnityEngine;
using UnityEngine.UI;

public class TitleAnimator : MonoBehaviour
{
    [System.Serializable]
    public class Starter { public Image image; public Sprite[] frames; }

    [SerializeField] private RectTransform background;
    [SerializeField] private float travel = 240f;
    [SerializeField] private float period = 6f;
    [SerializeField] private Starter[] starters;
    [SerializeField] private float frameSeconds = 0.35f;

    private void Update()
    {
        float t = Time.unscaledTime;
        background.anchoredPosition = new Vector2(Mathf.Sin(t / period) * travel, 0f);   // sin(t/6) × 240

        int frame = Mathf.FloorToInt(t / frameSeconds);                                  // 0,35 s
        foreach (var s in starters)
            if (s.frames != null && s.frames.Length > 0)
                s.image.sprite = s.frames[frame % s.frames.Length];
    }
}
