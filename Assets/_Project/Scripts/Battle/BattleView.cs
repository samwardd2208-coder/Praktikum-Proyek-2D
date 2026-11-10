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
        if (rings == null || rings.Length == 0) return;
    
        StartCoroutine(AnimateRings(duration));
    }

    private IEnumerator AnimateRings(float duration)
    {
    // Aktifkan semua cincin
        for (int i = 0; i < rings.Length; i++)
        {
            if (rings[i] != null)
            {
                rings[i].gameObject.SetActive(true);
                rings[i].color = new Color(1, 0.78f, 0.1f, 0); // emas, alpha 0
                rings[i].rectTransform.localScale = Vector3.one * 0.5f;
            }
        }
    
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration; // 0 sampai 1
        
            for (int i = 0; i < rings.Length; i++)
            {
                if (rings[i] == null) continue;
            
                // Setiap cincin mulai dengan delay berbeda
                float ringDelay = i * 0.15f;
                float ringProgress = Mathf.Clamp01((progress - ringDelay / duration) * 1.5f);
            
                // Scale membesar dari 0.5x ke 4x
                float scale = 0.5f + ringProgress * 3.5f;
                rings[i].rectTransform.localScale = Vector3.one * scale;
            
                // Alpha: muncul lalu memudar
                float alpha = ringProgress < 0.3f 
                ? ringProgress / 0.3f 
                : 1f - (ringProgress - 0.3f) / 0.7f;
                alpha = Mathf.Clamp01(alpha);
            
                rings[i].color = new Color(1, 0.78f, 0.1f, alpha);
            }
        
            yield return null;
        }
    
        // Sembunyikan cincin setelah selesai
        for (int i = 0; i < rings.Length; i++)
        {
            if (rings[i] != null)
                rings[i].gameObject.SetActive(false);
        }
    }
}