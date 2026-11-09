using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogueView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject root;
    [SerializeField] private Button clickCatcher;

    [Header("Box")]
    [SerializeField] private Image box;
    [SerializeField] private Text body;
    [SerializeField] private Sprite navySprite;
    [SerializeField] private Sprite narrationSprite;

    [Header("Speaker")]
    [SerializeField] private GameObject speakerTag;
    [SerializeField] private RectTransform speakerTagRect;
    [SerializeField] private Text speakerLabel;

    [Header("Hint & Choices")]
    [SerializeField] private Text continueHint;
    [SerializeField] private GameObject choicesRoot;
    [SerializeField] private TextMenu choiceMenu;

    public event Action Clicked;
    public event Action<int> ChoicePicked;

    private const int OrderNormal = 20;
    private const int OrderNarration = 110;   // di atas Fader (100)
    private bool hintOn;

    private void Awake()
    {
        clickCatcher.onClick.AddListener(() => Clicked?.Invoke());
        choiceMenu.Picked += i => ChoicePicked?.Invoke(i);
        root.SetActive(false);
        choicesRoot.SetActive(false);
        continueHint.enabled = false;
    }

    public void Show(bool narration)
    {
        root.SetActive(true);
        box.sprite = narration ? narrationSprite : navySprite;
        canvas.sortingOrder = narration ? OrderNarration : OrderNormal;
    }

    public void Hide()
    {
        root.SetActive(false);
        choicesRoot.SetActive(false);
        hintOn = false;
        continueHint.enabled = false;
        canvas.sortingOrder = OrderNormal;
    }

    public void SetSpeaker(string speaker)
    {
        bool has = !string.IsNullOrEmpty(speaker);
        speakerTag.SetActive(has);
        if (!has) return;
        speakerLabel.text = speaker;
        float width = Mathf.Min(488f, speaker.Length * 20f + 64f);
        speakerTagRect.sizeDelta = new Vector2(width, speakerTagRect.sizeDelta.y);
    }

    public void SetBody(string text) => body.text = text;

    public void ShowHint(bool on)
    {
        hintOn = on;
        continueHint.enabled = on;
    }

    public void ShowChoices(string[] options)
    {
        choicesRoot.SetActive(true);
        choiceMenu.Open(options);
    }

    public void HideChoices()
    {
        choiceMenu.Close();
        choicesRoot.SetActive(false);
    }

    private void Update()
    {
        if (hintOn)   // berkedip tiap 0,3 s
            continueHint.enabled = Mathf.FloorToInt(Time.unscaledTime / 0.3f) % 2 == 0;
    }
}