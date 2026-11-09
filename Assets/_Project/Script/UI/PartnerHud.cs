using UnityEngine;
using UnityEngine.UI;
 
/// <summary>
/// HUD dunia versi demo (koordinat piksel demo 320 x 180, dikali 4):
///   Panel partner  4, 4, 112, 34      Potret   8, 9, 24 x 24
///   Bar HP         36, 18, 74 x 6     angka HP rata kanan di x = 110
///   Bar EXP        36, 32, 74 x 3     Panel area 226, 4, 90, 24 (nama area + "Ramuan ×N")
///
/// Pasang di objek anak sebuah Canvas (Scale With Screen Size 1280 x 720, Match 0,5), lalu isi 3 slot:
/// panelSprite = UI_Panel_Navy, pixelSprite = UI_Pixel (wajib, agar bar bisa "Filled"), font = PixelifySans-Regular.
/// Semua elemen dibangun lewat kode dari koordinat di atas, jadi tidak perlu menyusun hierarki manual.
/// </summary>
public class PartnerHud : MonoBehaviour
{
    [SerializeField] Sprite panelSprite;     // UI_Panel_Navy (Sliced, Border 4)
    [SerializeField] Sprite pixelSprite;     // UI_Pixel (putih polos)
    [SerializeField] Font font;              // PixelifySans-Regular (Hinted Raster)
 
    PlayerParty party;
    bool built;
 
    RectTransform partnerPanel;              // P6: disembunyikan selama belum punya partner
    Image portrait, hpFill, expFill;
    Text nameText, levelText, hpText, areaText, potionText;
    float hpShow, expShow;
    MonsterInstance lastPartner;
 
    // ---------------------------------------------------------------- publik
 
    public void Bind(PlayerParty newParty)
    {
        if (party != null) party.PotionsChanged -= RefreshPotions;
        party = newParty;
        Build();
        if (party != null)
        {
            party.PotionsChanged += RefreshPotions;
            RefreshPotions();
        }
    }
 
    public void SetArea(string areaName)
    {
        Build();
        areaText.text = areaName;
    }
 
    void OnDestroy()
    {
        if (party != null) party.PotionsChanged -= RefreshPotions;
    }
 
    // ---------------------------------------------------------------- tampilan
 
    void RefreshPotions()
    {
        if (potionText != null && party != null) potionText.text = "Ramuan ×" + party.Potions;
    }
 
    void Update()
    {
        if (!built || party == null) return;
 
        // P6: panel partner hanya tampil setelah Tetua memberi starter
        bool hasPartner = party.Partner != null;
        if (partnerPanel.gameObject.activeSelf != hasPartner)
            partnerPanel.gameObject.SetActive(hasPartner);
        if (!hasPartner) { lastPartner = null; return; }
 
        MonsterInstance m = party.Partner;
 
        if (m != lastPartner)                                         // partner baru / berevolusi: snap
        {
            lastPartner = m;
            hpShow = m.CurrentHP;
            expShow = m.ExpToNext > 0 ? (float)m.Exp / m.ExpToNext : 0f;
        }
 
        nameText.text = m.Data.DisplayName;
        levelText.text = "Lv" + m.Level;
        if (m.Data.Frames != null && m.Data.Frames.Length > 0) portrait.sprite = m.Data.Frames[0];
 
        float k = 1f - Mathf.Pow(0.85f, Time.deltaTime * 60f);
        hpShow = Mathf.Lerp(hpShow, m.CurrentHP, k);
        float hp = m.CurrentHP <= 0 ? 0f : Mathf.Clamp01(hpShow / m.MaxHP);
        hpFill.fillAmount = hp;
        hpFill.color = HpColors.For(hp);
        hpText.text = $"{Mathf.RoundToInt(hpShow)}/{m.MaxHP}";          // HP angka
 
        float expTarget = m.ExpToNext > 0 ? Mathf.Clamp01((float)m.Exp / m.ExpToNext) : 0f;
        expShow = Mathf.Lerp(expShow, expTarget, k);
        expFill.fillAmount = expShow;
    }
 
    // ---------------------------------------------------------------- bangun UI dari kode
 
    void Build()
    {
        if (built) return;
        built = true;
 
        // Objek ini harus anak Canvas: bentangkan penuh supaya koordinat demo dihitung dari pojok kiri-atas layar
        RectTransform self = transform as RectTransform;
        if (self != null)
        {
            self.anchorMin = Vector2.zero;
            self.anchorMax = Vector2.one;
            self.offsetMin = self.offsetMax = Vector2.zero;
        }
 
        Font useFont = font != null ? font : Font.CreateDynamicFontFromOSFont("Arial", 16);
 
        // Panel partner
        partnerPanel = MakePanel("PartnerPanel", transform, 4, 4, 112, 34);
        portrait = MakeImage("Portrait", partnerPanel, null, Color.white);
        PlaceInside(portrait.rectTransform, 4, 4, 8, 9, 24, 24);
        portrait.preserveAspect = true;
 
        nameText  = MakeText("Name",  partnerPanel, useFont, 8, UiTheme.Text, TextAnchor.MiddleLeft);
        levelText = MakeText("Level", partnerPanel, useFont, 8, UiTheme.Gold, TextAnchor.MiddleRight);
        PlaceInside(nameText.rectTransform, 4, 4, 36, 6, 46, 10);
        PlaceInside(levelText.rectTransform, 4, 4, 76, 6, 34, 10);
 
        hpFill  = MakeBar("HpBar",  partnerPanel, 4, 4, 36, 18, 74, 6);
        hpText  = MakeText("HpText", partnerPanel, useFont, 7, UiTheme.Muted, TextAnchor.MiddleRight);
        PlaceInside(hpText.rectTransform, 4, 4, 60, 23, 50, 9);       // rata kanan di x = 110
        expFill = MakeBar("ExpBar", partnerPanel, 4, 4, 36, 32, 74, 3);
        expFill.color = UiTheme.Exp;
        partnerPanel.gameObject.SetActive(false);                     // tampil lewat Update() saat partner ada
 
        // Panel area (nama area + jumlah Ramuan)
        RectTransform areaPanel = MakePanel("AreaPanel", transform, 226, 4, 90, 24);
        areaText   = MakeText("AreaName", areaPanel, useFont, 8, UiTheme.Gold, TextAnchor.MiddleLeft);
        potionText = MakeText("Potions",  areaPanel, useFont, 7, UiTheme.Text, TextAnchor.MiddleLeft);
        PlaceInside(areaText.rectTransform, 226, 4, 230, 6, 82, 9);
        PlaceInside(potionText.rectTransform, 226, 4, 230, 16, 82, 8);
        areaText.text = "";
        RefreshPotions();
    }
 
    // Anak dari panel diletakkan dengan koordinat demo absolut (px, py = pojok panel)
    static void PlaceInside(RectTransform rt, float px, float py, float x, float y, float w, float h)
        => UiTheme.Place(rt, x - px + 1f, y - py + 1f, w, h);        // +1: tepi gelap panel ada di luar
 
    RectTransform MakePanel(string objName, Transform parent, float x, float y, float w, float h)
    {
        Image img = MakeImage(objName, parent, panelSprite, UiTheme.Panel);
        if (panelSprite != null)
        {
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1.5625f;                   // 1 piksel art = 4 unit kanvas
        }
        UiTheme.PlacePanel(img.rectTransform, x, y, w, h);
        return img.rectTransform;
    }
 
    // Bar = latar #0B0918 + anak Fill (Image Filled, Horizontal, inset 1 px)
    Image MakeBar(string objName, RectTransform parent, float px, float py, float x, float y, float w, float h)
    {
        Image back = MakeImage(objName, parent, pixelSprite, UiTheme.BarBack);
        PlaceInside(back.rectTransform, px, py, x, y, w, h);
 
        Image fill = MakeImage("Fill", back.rectTransform, pixelSprite, UiTheme.Exp);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        UiTheme.Place(fill.rectTransform, 1f, 1f, w - 2f, h - 2f);   // inset 1 px
        return fill;
    }
 
    static Image MakeImage(string objName, Transform parent, Sprite sprite, Color color)
    {
        GameObject go = new GameObject(objName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }
 
    static Text MakeText(string objName, Transform parent, Font font, float px, Color color, TextAnchor anchor)
    {
        GameObject go = new GameObject(objName, typeof(RectTransform), typeof(Text), typeof(Shadow));
        go.transform.SetParent(parent, false);
 
        Text t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = UiTheme.FontSize(px);                           // teks 8 px -> fontSize 32
        t.color = color;
        t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
 
        Shadow shadow = go.GetComponent<Shadow>();                   // bayangan 0,5 px di demo
        shadow.effectColor = UiTheme.Shadow;
        shadow.effectDistance = new Vector2(2f, -2f);
        return t;
    }
}