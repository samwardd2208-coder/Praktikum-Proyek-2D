using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattleView : MonoBehaviour
{
    [SerializeField] private Text messageText;
    [SerializeField] private Text hintText;
    [SerializeField] private TextMenu menu;
    [SerializeField] public MonsterPanel enemyPanel;
    [SerializeField] public MonsterPanel playerPanel;
    [SerializeField] private Image flash;
    [SerializeField] private Image wipeTop;
    [SerializeField] private Image wipeBottom;
    [SerializeField] private Image[] rings;

    private Coroutine typing;
    private int lastPick;

    public event System.Action<int> ActionClicked;

    public void ShowMessage(string text)
    {
        hintText.enabled = false;
        if (typing != null) StopCoroutine(typing);
        typing = StartCoroutine(Type(text));
    }

    public void ShowMenu(string[] labels, bool[] usable, string prompt, string hint)
    {
        if (typing != null) StopCoroutine(typing);
        messageText.text = prompt;
        hintText.text = hint;
        hintText.enabled = true;
        
        int start = usable[lastPick] ? lastPick : 0;
        menu.Open(labels, usable, start);
        menu.Picked += OnMenuPicked;
    }

    private void OnMenuPicked(int index)
    {
        lastPick = index;
        menu.Picked -= OnMenuPicked;
        ActionClicked?.Invoke(index);
    }

    private IEnumerator Type(string text)
    {
        messageText.text = "";
        for (int i = 0; i < text.Length; i++)
        {
            messageText.text += text[i];
            yield return new WaitForSeconds(0.018f);
        }
    }

    public void PlayIntro()
    {
        // Animasi tirai menutup
    }

    public void PlayFlash()
    {
        flash.color = new Color(1, 1, 1, 1);
        StartCoroutine(FadeFlash());
    }

    private IEnumerator FadeFlash()
    {
        float t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            float a = 1f - t / 0.5f;
            flash.color = new Color(1, 1, 1, a);
            yield return null;
        }
    }

    public void PlayEvolveRings(float duration)
    {
        // Animasi cincin evolusi
    }
}