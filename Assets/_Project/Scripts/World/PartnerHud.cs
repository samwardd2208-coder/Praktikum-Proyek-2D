using UnityEngine;
using UnityEngine.UI;

public class PartnerHud : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Image hpFill;
    [SerializeField] private Text hpText;
    [SerializeField] private Image expFill;
    [SerializeField] private Text potionText;
    [SerializeField] private Image portrait;

    private PlayerParty party;

    public void Construct(PlayerParty party)
    {
        this.party = party;
        party.PartyChanged += UpdateDisplay;
        party.PotionsChanged += UpdatePotions;
        UpdateDisplay();
        UpdatePotions();
    }

    private void UpdateDisplay()
    {
        if (party.Partner == null) return;
        
        nameText.text = party.Partner.Data.DisplayName;
        levelText.text = $"Lv {party.Partner.Level}";
        portrait.sprite = party.Partner.Data.Frames[0];
        
        float ratio = party.Partner.MaxHP > 0 
            ? (float)party.Partner.CurrentHP / party.Partner.MaxHP 
            : 0;
        hpFill.fillAmount = ratio;
        hpFill.color = HpColors.For(ratio);
        hpText.text = $"{party.Partner.CurrentHP}/{party.Partner.MaxHP}";
    }

    private void UpdatePotions()
    {
        potionText.text = $"Ramuan ×{party.Potions}";
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