using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassYLanguageSelector : MonoBehaviour
{
    static public ClassYLanguageSelector instance;

    RectTransform rectTransformLanguageSelector;

    SetLanguageText setLanguageText;

    RectTransform content;  // A LanguageItem-eket tartalmazó rectTransform
    LanguageItem languageItem;

    Common.CallBack_In_String buttonClick;
    string buttonNamePrefix;


    [HideInInspector]
    public List<LanguageData> listOfLanguageDatas = new List<LanguageData>();
    List<LanguageItem> listOfLanguageItems = new List<LanguageItem>();

    Tween.TweenAnimation animation = new Tween.TweenAnimation();

    // Use this for initialization
    void Awake ()
    {
        instance = this;

        // Megkeressük a szükséges referenciákat
        rectTransformLanguageSelector = gameObject.SearchChild("Image").GetComponent<RectTransform>();

        setLanguageText = gameObject.SearchChild("Text").GetComponent<SetLanguageText>();
        content = gameObject.SearchChild("Content").GetComponent<RectTransform>();
        languageItem = gameObject.SearchChild("LanguageItem").GetComponent<LanguageItem>();

        // Eltüntetjük a másolandó elemet
        languageItem.gameObject.SetActive(false);

        transform.position = Vector3.zero;

        UpdateLanguageSelectorPos(0f);
    }

    //void FillLanguagesDataList(JSONNode json)
    //{
    //    listOfLanguageDatas.Clear();
    //
    //    try
    //    {
    //        for (int i = 0; i < json[C.JSONKeys.answer][C.JSONKeys.usableLangCodes].Count; i++)
    //            listOfLanguageDatas.Add(new LanguageData(json[C.JSONKeys.answer][C.JSONKeys.usableLangCodes][i]));
    //    }
    //    catch (System.Exception e)
    //    {
    //        // Hibát megmutatjuk a felhasználónak
    //        //throw;
    //    }
    //}

    public string GetLanguageID(string langCode)
    {
        for (int i = 0; i < listOfLanguageDatas.Count; i++)
        {
            if (listOfLanguageDatas[i].langCode == langCode)
                return listOfLanguageDatas[i].langID;
        }

        return "-1";
    }

    public void Initialize(List<LanguageData> list, string buttonNamePrefix)
    {
        if (!string.IsNullOrEmpty(buttonNamePrefix))
            buttonNamePrefix += ":";

        this.buttonNamePrefix = buttonNamePrefix;

        listOfLanguageDatas = list;

        // A már létező menü elemeket töröljük
        for (int i = 0; i < listOfLanguageItems.Count; i++)
        {
            Destroy(listOfLanguageItems[i].gameObject);
        }
        listOfLanguageItems.Clear();

        // Létrehozzuk az új elemeket
        for (int i = 0; i < listOfLanguageDatas.Count; i++)
        {
            LanguageItem newLanguageItem = Instantiate(languageItem);
            newLanguageItem.gameObject.SetActive(true);
            newLanguageItem.transform.SetParent(content.transform);
            newLanguageItem.transform.localScale = Vector3.one;

            newLanguageItem.Initialize(listOfLanguageDatas[i].langFlag, listOfLanguageDatas[i].langName, buttonNamePrefix + listOfLanguageDatas[i].langCode, ButtonClick);
            newLanguageItem.SetSelected(false);

            // Beállítjuk az újonnan létrehozott elem pozícióját
            int x = i % 5;
            int y = i / 5;
            RectTransform recTranform = newLanguageItem.GetComponent<RectTransform>();
            recTranform.localPosition = new Vector3(recTranform.sizeDelta.x * x, -recTranform.sizeDelta.y * y);
            //recTranform.sizeDelta = new Vector2(recTranform.sizeDelta.x, 0);
            //recTranform.anchoredPosition = new Vector2(recTranform.anchoredPosition.x, 0);

            listOfLanguageItems.Add(newLanguageItem);
        }

        // Beállítjuk a tartalom méretét
        content.sizeDelta = new Vector2(content.sizeDelta.x, languageItem.GetComponent<RectTransform>().sizeDelta.y * (listOfLanguageDatas.Count + 4) / 5);

        Debug.Log("Inicializálás vége : " + Common.TimeStamp());
    }

    /// <summary>
    /// Kiválasztottá teszi a megadott ID-jű elemet.
    /// </summary>
    /// <param name="ID"></param>
    public void SetSelected(string ID)
    {
        ID = buttonNamePrefix + ID;
        for (int i = 0; i < listOfLanguageItems.Count; i++)
            listOfLanguageItems[i].SetSelected(listOfLanguageItems[i].buttonName == ID);
    }

    public Sprite GetFlag(string langCode) {
        foreach (LanguageData languageData in listOfLanguageDatas)
        {
            if (languageData.langCode == langCode)
                return languageData.langFlag;
        }

        return null;
    }

    public void Show(string activeLangCode, string text, Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;

        SetSelected(activeLangCode);

        setLanguageText.SetTextID(text);

        float showAnimTime = 1;
        Common.fade.Show(Color.white, 0.4f, showAnimTime, ButtonClick);
        //iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", rectTransformLanguageSelector.sizeDelta.y, "time", showAnimTime, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "UpdateLanguageSelectorPos", "onupdatetarget", gameObject));

        animation = new Tween.TweenAnimation
        (
            startPos: 0f,
            endPos: rectTransformLanguageSelector.sizeDelta.y,
            easeType: Tween.EaseType.easeOutCubic,
            time: showAnimTime,
            onUpdate: UpdateLanguageSelectorPos,
            onComplete: null
        );

        Tween.StartAnimation(animation);
    }

    public void Hide(Tween.CallBack hideCallback = null)
    {
        float hideAnimTime = 0.5f;

        //iTween.Stop(gameObject);

        Common.fade.Hide(hideAnimTime);
        //iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformLanguageSelector.anchoredPosition.y, "to", 0, "time", hideAnimTime, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "UpdateLanguageSelectorPos", "onupdatetarget", gameObject));

        animation = new Tween.TweenAnimation
        (
            startPos: rectTransformLanguageSelector.anchoredPosition.y,
            endPos: 0f,
            easeType: Tween.EaseType.easeOutCubic,
            time: hideAnimTime,
            onUpdate: UpdateLanguageSelectorPos,
            onComplete: hideCallback
        );

        Tween.StartAnimation(animation);
    }

    public void HideImmediatelly()
    {
        iTween.Stop(gameObject);

        Common.fade.HideImmediatelly();
        UpdateLanguageSelectorPos(0f);
    }

    // panel mozgatásához
    void UpdateLanguageSelectorPos(object o)
    {
        float value = (float)o;
        rectTransformLanguageSelector.anchoredPosition = new Vector2(rectTransformLanguageSelector.anchoredPosition.x, value);
    }

    public void ButtonClick(string buttonName)
    {
        if (buttonName == "FadeClick")
            buttonName = C.Program.FadeClickLanguageSelector;

        if (buttonClick != null)
            buttonClick(buttonName);
    }
}
