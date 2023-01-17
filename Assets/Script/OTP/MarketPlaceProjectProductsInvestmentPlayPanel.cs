using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketPlaceProjectProductsInvestmentPlayPanel : MonoBehaviour
{
    static public MarketPlaceProjectProductsInvestmentPlayPanel instance;

    Text textProductName;
    Text textCompanyName;

    Text textButtonInvestment;
    Text textButtonPlay;

    GameObject gameObjectButtonInvestment;

    RectTransform rectTransformPanel;

    Common.CallBack_In_String buttonCallBack; // Gomb lenyomásnál melyik metódust hívja meg

    Common.CallBack hideCallBack;   // Mit kell meghívni, ha eltünt a Panel

    // Use this for initialization
    void Awake ()
    {
        instance = this;

        textProductName = gameObject.SearchChild("TextProductName").GetComponent<Text>();
        textCompanyName = gameObject.SearchChild("TextCompanyName").GetComponent<Text>();

        textButtonInvestment = gameObject.SearchChild("TextButtonInvestment").GetComponent<Text>();
        textButtonPlay = gameObject.SearchChild("TextButtonPlay").GetComponent<Text>();

        gameObjectButtonInvestment = gameObject.SearchChild("ButtonInvestment").gameObject;

        rectTransformPanel = gameObject.SearchChild("Panel").GetComponent<RectTransform>();
    }

    public void Show(string productName, string companyName, string investablePcoin, bool investable, Common.CallBack_In_String buttonCallBack = null)
    {
        this.buttonCallBack = buttonCallBack;

        // Beállítjuk a szövegeket
        textProductName.text = productName;
        textCompanyName.text = Common.languageController.Translate(C.Texts.Company) + ": " + companyName;

        textButtonInvestment.text = Common.languageController.Translate(C.Texts.ImInvestingMyPcoin).Replace("[x]", investablePcoin);
        textButtonPlay.text = Common.languageController.Translate(C.Texts.PlayProjectProduct);

        gameObjectButtonInvestment.SetActive(investable);

        // Elindítjuk az animációt
        float showAnimTime = 1;
        Common.fadeOTPMain.Show(Color.white, 0.4f, showAnimTime, ButtonClick); // ButtonClick);
        iTween.ValueTo(gameObject, iTween.Hash(
            "from", 0,
            "to", rectTransformPanel.sizeDelta.y,
            "time", showAnimTime,
            "easetype", iTween.EaseType.easeOutCubic,
            "onupdate", "UpdatePanelPos", "onupdatetarget", gameObject));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hideCallBack">Milyen függvényt hívjon meg ha befejezte az errorPanel elrejtését.</param>
    /// <param name="fadeHide">A háttér elfédelése is legyen (Ha az egyik hibát egy másik követi, akkor nem kell)</param>
    public void Hide(Common.CallBack hideCallBack = null)
    {
        this.hideCallBack = hideCallBack;

        float hideAnimTime = 0.5f;

        iTween.Stop(gameObject);

        Common.fadeOTPMain.Hide(hideAnimTime);
        iTween.ValueTo(gameObject, iTween.Hash(
            "from", rectTransformPanel.anchoredPosition.y,
            "to", 0,
            "time", hideAnimTime,
            "easetype", iTween.EaseType.easeOutCubic,
            "onupdate", "UpdatePanelPos", "onupdatetarget", gameObject,
            "oncomplete", "HideCompleted", "oncompletetarget", gameObject)
            );
    }

    void UpdatePanelPos(float value)
    {
        rectTransformPanel.anchoredPosition = new Vector2(rectTransformPanel.anchoredPosition.x, value);
    }

    void HideCompleted()
    {
        if (hideCallBack != null)
        {
            hideCallBack();
            hideCallBack = null;
        }
    }

    public void ButtonClick(string buttonName)
    {
        buttonCallBack(buttonName);
    }
}
