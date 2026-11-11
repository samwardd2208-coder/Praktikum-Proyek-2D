using UnityEngine;

public class TitleBackground : MonoBehaviour
{
    [SerializeField] private RectTransform background;   // Image BG_Title_World
    [SerializeField] private float scrollAmplitude = 240f;
    [SerializeField] private float scrollPeriod = 6f;
    [SerializeField] private RectTransform[] starters;   // 4 Image starter
    [SerializeField] private float bobHeight = 6f;
    [SerializeField] private float bobPeriod = 0.35f;

    private Vector2 bgStart;
    private Vector2[] starterStart;

    void Awake()
    {
        if (background != null) bgStart = background.anchoredPosition;
        starterStart = new Vector2[starters != null ? starters.Length : 0];
        for (int i = 0; i < starterStart.Length; i++)
            if (starters[i] != null) starterStart[i] = starters[i].anchoredPosition;
    }

    void Update()
    {
        float t = Time.unscaledTime;
        if (background != null)
            background.anchoredPosition = bgStart + new Vector2(Mathf.Sin(t / scrollPeriod) * scrollAmplitude, 0f);
        for (int i = 0; i < starterStart.Length; i++)
            if (starters[i] != null)
                starters[i].anchoredPosition = starterStart[i] + new Vector2(0f, Mathf.Sin(t / bobPeriod + i) * bobHeight);
    }
}