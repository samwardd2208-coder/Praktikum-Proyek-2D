using UnityEngine;
using UnityEngine.UI;

public class PartnerHud : MonoBehaviour
{
    [Header("Partner Panel")]
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Image hpFill;
    [SerializeField] private Text hpText;
    [SerializeField] private Image expFill; // TAMBAHAN: Bar EXP
    [SerializeField] private Image portrait;

    [Header("Area Panel")]
    [SerializeField] private Text areaText;   // TAMBAHAN: Nama Area
    [SerializeField] private Text potionText;

    private PlayerParty party;

    public void Construct(PlayerParty party)
    {
        Debug.Log("✅ PartnerHud.Construct() DIPANGGIL!"); // TAMBAHKAN INI
        this.party = party;
        party.PartyChanged += UpdateDisplay;
        party.PotionsChanged += UpdatePotions;
        
        UpdateDisplay();
        UpdatePotions();
        Debug.Log($"✅ Name Text: {nameText?.text}");
        Debug.Log($"✅ HP Fill: {hpFill?.fillAmount}");
        Debug.Log($"✅ Area Text: {areaText?.text}");
    }

    // Panggil method ini dari WorldInstaller untuk set nama area
    public void SetAreaName(string areaName)
    {
        if (areaText != null) areaText.text = areaName;
    }

    private void UpdateDisplay()
    {
        if (party.Partner == null) return;
        
        if (nameText != null) nameText.text = party.Partner.Data.DisplayName;
        if (levelText != null) levelText.text = $"Lv {party.Partner.Level}";
        if (portrait != null) portrait.sprite = party.Partner.Data.Frames[0];
        
        // Update HP
        float hpRatio = party.Partner.MaxHP > 0 
            ? (float)party.Partner.CurrentHP / party.Partner.MaxHP : 0;
        if (hpFill != null)
        {
            hpFill.fillAmount = hpRatio;
            hpFill.color = HpColors.For(hpRatio);
        }
        if (hpText != null) hpText.text = $"{party.Partner.CurrentHP}/{party.Partner.MaxHP}";
        
        // Update EXP (Sederhana: Level * 10 adalah max exp saat ini)
        if (expFill != null)
        {
            // Karena di MonsterInstance belum ada property 'CurrentExp', 
            // kita anggap 0 dulu atau bisa ditambahkan logika EXP nanti.
            // Untuk sekarang, biarkan 0 atau isi manual untuk tes.
            expFill.fillAmount = 0f; 
        }
    }

    private void UpdatePotions()
    {
        if (potionText != null) potionText.text = $"Ramuan ×{party.Potions}";
    }

    void OnDestroy()
    {
        if (party != null)
        {
            party.PartyChanged -= UpdateDisplay;
            party.PotionsChanged -= UpdatePotions;
        }
    }
}