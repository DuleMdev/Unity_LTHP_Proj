using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassYIconControl : MonoBehaviour
{
    public Color activeColor;
    public Color inactiveColor;

    public bool clickDisableIfActive = false;   // Nem adja vissza a kattintási eseményt ha már aktív

    Image background;
    RectTransform rectTranformImage;    // A pörgetéshez
    Image imageDiamond;
    Image imageIcon;
    Text textIcon;  // Szöveg szinének beállításához
    SetLanguageText setLanguageText;

    //PrefabNotice notice;

    public string buttonName { get; private set; }
    Common.CallBack_In_String buttonClick;

    bool active;

    void Awake()
    {
        background = gameObject.SearchChild("Background").GetComponent<Image>();
        rectTranformImage = gameObject.SearchChild("ImageDiamond").GetComponent<RectTransform>();
        imageDiamond = rectTranformImage.GetComponent<Image>();
        imageIcon = gameObject.SearchChild("ImageIcon").GetComponent<Image>();
        textIcon = gameObject.SearchChild("TextIcon").GetComponent<Text>(); //  transform.Find("Background/Text").GetComponent<Text>();
        setLanguageText = textIcon.GetComponent<SetLanguageText>(); //  transform.Find("Background/Text").GetComponent<Text>();

        //notice = gameObject.SearchChild("Notice").GetComponent<PrefabNotice>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="textID">Az icon alatt megjelenítendő szöveg</param>
    /// <param name="buttonName">Ha megnyomták az ikont milyen szöveget adjon vissza</param>
    /// <param name="buttonClick">Gomb nyomás esetén melyik metódust hívjuk meg</param>
    /// <param name="diamondColor">Milyen szine legyen a rombusznak</param>
    /// <param name="showIcon">Látszódjon-e az ikon</param>
    public void Initialize(string textID, /*string notice,*/ string buttonName, Common.CallBack_In_String buttonClick, Color? diamondColor = null, bool? showIcon = null, Sprite icon = null)
    {
        // Amíg nincs az összes ikonControl gameObject-en létrehozva a SetLanguageText komponens addig ennek itt kell lenni
        if (setLanguageText)
            setLanguageText.SetTextID(textID); // Ha már mindenütt lesz SetLanguageText komponens, attól fogva elég ez a sor
        else
            textIcon.text = textID;

        //this.notice.SetText(notice);
        this.buttonName = buttonName;
        this.buttonClick = buttonClick;

        if (diamondColor != null)
            imageDiamond.color = diamondColor.Value;

        if (icon != null) {
            imageIcon.sprite = icon;
            ((RectTransform)imageIcon.transform).sizeDelta = icon.rect.size;
        }

        if (showIcon != null)
            imageIcon.enabled = showIcon.Value;

        // Alaphelyzetbe állítjuk a gombot
        UpdateIconRotation(0);
        textIcon.color = inactiveColor;

        active = false;
    }

    public void SetActive(bool active)
    {
        this.active = active;

        // Beállítjuk a háttérkép láthatóságát
        background.enabled = active;

        // Beállítjuk a szöveg színét 
        textIcon.color = (active) ? activeColor : inactiveColor;

        // Pörgés animáció indítása ha aktív
        if (active)
            iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", -360, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "UpdateIconRotation", "onupdatetarget", gameObject));
    }

    void UpdateIconRotation(float rotation)
    {
        rectTranformImage.transform.eulerAngles = new Vector3(0, 0, rotation);
    }

    public void ButtonClick()
    {
        if (buttonClick != null && (!active || !clickDisableIfActive))
            buttonClick(buttonName);
    }
}
