using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueView : MonoBehaviour
{
    [Header("Root & Catcher")]
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private Button clickCatcher;

    [Header("Box & Text")]
    [SerializeField] private GameObject box;
    [SerializeField] private Text bodyText;
    [SerializeField] private GameObject speakerTag;
    [SerializeField] private Text speakerText;
    [SerializeField] private Text continueHint;

    [Header("Choices")]
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private TextMenu choicesMenu; // TextMenu dari P5 (opsional)
    [SerializeField] private GameObject[] choiceButtons; // Array tombol pilihan (Choice_0, Choice_1, Choice_2)

    private Coroutine typingCoroutine;
    private bool isTyping;

    public void Show(bool isNarration)
    {
        dialogueRoot.SetActive(true);
        box.SetActive(true);
        speakerTag.SetActive(!isNarration);
        continueHint.gameObject.SetActive(false);
        choicesPanel.SetActive(false);

        Image boxImage = box.GetComponent<Image>();
        if (boxImage != null)
        {
            if (isNarration)
            {
                boxImage.color = new Color(0.2f, 0.2f, 0.4f, 1f);
            }
            else
            {
                boxImage.color = Color.white;
            }
        }
    }

    public void Hide()
    {
        dialogueRoot.SetActive(false);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
    }

    // --- PERBAIKAN DI SINI ---
    public void ShowChoices(string[] options)
    {
        choicesPanel.SetActive(true);
        continueHint.gameObject.SetActive(false);

        // Jika pakai TextMenu (dari P5)
        if (choicesMenu != null)
        {
            bool[] usable = new bool[options.Length];
            for (int i = 0; i < options.Length; i++) usable[i] = true;
            choicesMenu.Open(options, usable, 0);
        }
        // Jika pakai Button manual (Choice_0, Choice_1, dst)
        else if (choiceButtons != null && choiceButtons.Length > 0)
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] != null)
                {
                    Text btnText = choiceButtons[i].GetComponentInChildren<Text>();
                    if (btnText != null && i < options.Length)
                    {
                        btnText.text = options[i]; // Update teks button
                    }
                    choiceButtons[i].SetActive(i < options.Length); // Sembunyikan jika tidak ada teks
                }
            }
        }
    }

    public void HideChoices()
    {
        choicesPanel.SetActive(false);
        continueHint.gameObject.SetActive(true);
    }

    public void SetSpeaker(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            speakerTag.SetActive(false);
        }
        else
        {
            speakerTag.SetActive(true);
            speakerText.text = name;
        }
    }

    public void SetBodyText(string text)
    {
        bodyText.text = text;
    }

    public string GetBodyText()
    {
        return bodyText.text;
    }

    public void ShowContinueHint(bool show)
    {
        continueHint.gameObject.SetActive(show);
    }

    public IEnumerator TypeLine(string text, float secondsPerChar = 0.018f)
    {
        isTyping = true;
        bodyText.text = "";
        ShowContinueHint(false);

        for (int i = 0; i < text.Length; i++)
        {
            bodyText.text += text[i];
            yield return new WaitForSeconds(secondsPerChar);
        }

        isTyping = false;
        ShowContinueHint(true);
    }
}