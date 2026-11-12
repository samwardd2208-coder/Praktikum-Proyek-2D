using UnityEngine;
using UnityEngine.UI;

public class PartnerHud : MonoBehaviour
{
    [Header("Partner Panel")]
    [Tooltip("Panel_Background. Kosongkan jika ingin otomatis memakai parent dari Portrait.")]
    [SerializeField] private GameObject partnerPanel;
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Image hpFill;
    [SerializeField] private Text hpText;
    [SerializeField] private Image expFill;
    [SerializeField] private Image portrait;

    [Header("Area Panel")]
    [SerializeField] private Text areaText;
    [SerializeField] private Text potionText;

    private PlayerParty party;

    public void Construct(PlayerParty party)
    {
        // Lepas langganan lama kalau Construct dipanggil lagi (mis. setelah kembali dari battle)
        Unsubscribe();

        this.party = party;
        party.PartyChanged += UpdateDisplay;
        party.PotionsChanged += UpdatePotions;

        UpdateDisplay();
        UpdatePotions();
    }

    // Panggil dari WorldInstaller untuk mengatur nama area
    public void SetAreaName(string areaName)
    {
        if (areaText != null) areaText.text = areaName;
    }

    // Panel partner: tampil hanya kalau pemain sudah punya partner.
    private GameObject PanelObject
    {
        get
        {
            if (partnerPanel != null) return partnerPanel;
            if (portrait != null && portrait.transform.parent != null)
                return portrait.transform.parent.gameObject;
            return null;
        }
    }

    private void UpdateDisplay()
    {
        if (party == null) return;

        MonsterInstance p = party.Partner;

        GameObject panel = PanelObject;
        if (panel != null) panel.SetActive(p != null);     // sembunyikan placeholder "New Text"
        if (p == null) return;

        if (nameText != null) nameText.text = p.Data.DisplayName;
        if (levelText != null) levelText.text = $"Lv {p.Level}";
        if (portrait != null) portrait.sprite = p.Data.Frames[0];

        // HP
        float hpRatio = p.MaxHP > 0 ? (float)p.CurrentHP / p.MaxHP : 0f;
        if (hpFill != null)
        {
            hpFill.fillAmount = hpRatio;
            hpFill.color = HpColors.For(hpRatio);
        }
        if (hpText != null) hpText.text = $"{p.CurrentHP}/{p.MaxHP}";

        // EXP: sekarang MonsterInstance punya Exp dan ExpToNext
        if (expFill != null)
        {
            expFill.fillAmount = p.ExpToNext > 0
                ? Mathf.Clamp01((float)p.Exp / p.ExpToNext) : 0f;
        }
    }

    private void UpdatePotions()
    {
        if (party == null) return;
        if (potionText != null) potionText.text = $"Ramuan ×{party.Potions}";
    }

    private void Unsubscribe()
    {
        if (party == null) return;
        party.PartyChanged -= UpdateDisplay;
        party.PotionsChanged -= UpdatePotions;
    }

    void OnDestroy() => Unsubscribe();
}