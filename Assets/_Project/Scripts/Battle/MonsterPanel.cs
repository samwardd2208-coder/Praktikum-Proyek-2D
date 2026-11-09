using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MonsterPanel : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text elementText;
    [SerializeField] private Image hpFill;
    [SerializeField] private Text hpText;
    [SerializeField] private SpriteRenderer body;

    private MonsterInstance shown;
    private float hpShow;
    private bool hurting;
    private bool evolving;
    private Vector3 home;

    void Awake()
    {
        home = body.transform.localPosition;
    }

    public void Show(MonsterInstance monster, bool showExp = false, MonsterInstance displayAs = null)
    {
        shown = displayAs ?? monster;
        if (nameText != null) nameText.text = shown.Data.DisplayName;
        if (levelText != null) levelText.text = $"Lv {shown.Level}";
        if (elementText != null) elementText.text = shown.Data.Element.ToString();
    
        hpShow = shown.CurrentHP;
    
        if (body != null) body.sprite = shown.Data.Frames[0];
    }

    void Update()
    {
        if (shown == null) return;

        float k = 1f - Mathf.Pow(0.85f, Time.deltaTime * 60f);
        hpShow = Mathf.Lerp(hpShow, shown.CurrentHP, k);
    
        float ratio = shown.MaxHP > 0 ? hpShow / shown.MaxHP : 0;
    
        if (hpFill != null)
        {
            hpFill.fillAmount = shown.CurrentHP <= 0 ? 0f : ratio;
            hpFill.color = HpColors.For(ratio);
        }
    
        if (hpText != null) 
        {
            hpText.text = $"{Mathf.RoundToInt(hpShow)}/{shown.MaxHP}";
        }

        if (!hurting && !evolving && body != null && shown.Data.Frames.Length >= 2)
        {
            int frame = Mathf.FloorToInt(Time.time / 0.35f) % 2;
            body.sprite = shown.Data.Frames[frame];
        }
    }

    public void PlayLunge()
    {
        // Animasi menerjang (untuk sekarang biarkan kosong)
    }

    public void PlayHurt()
    {
        hurting = true;
        StartCoroutine(HurtEffect());
    }

    private IEnumerator HurtEffect()
    {
        float t = 0;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            body.color = t % 0.1f < 0.05f ? Color.white : Color.gray;
            yield return null;
        }
        body.color = Color.white;
        hurting = false;
    }

    public void PlayFaint()
    {
        StartCoroutine(FaintEffect());
    }

    private IEnumerator FaintEffect()
    {
        float t = 0;
        Vector3 start = body.transform.localPosition;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            body.transform.localPosition = start + new Vector3(0, -20f * t / 0.5f, 0);
            body.color = new Color(1, 1, 1, 1f - t / 0.5f);
            yield return null;
        }
    }

    public void PlayEvolve(MonsterInstance from, MonsterInstance to, float duration)
    {
        evolving = true;
        StartCoroutine(EvolveEffect(from, to, duration));
    }

    private IEnumerator EvolveEffect(MonsterInstance from, MonsterInstance to, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            // Kedip-kedip antara sprite lama dan baru
            bool showNew = Mathf.FloorToInt(t * 4) % 2 == 0;
            body.sprite = showNew ? to.Data.Frames[0] : from.Data.Frames[0];
            yield return null;
        }
        evolving = false;
    }
}