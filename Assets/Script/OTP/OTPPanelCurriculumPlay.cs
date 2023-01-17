using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OTPPanelCurriculumPlay : MonoBehaviour
{
    RectTransform scrollView; // Azért van erre szükség, hogy tudjuk mekkora a képernyő szélessége
    RectTransform content;  // A subMenuItem-eket tartalmazó rectTransform
    RectTransform rectTransformPrefabCurriculumPathItem;
    OTPCurriculumPath prefabCurriculumPathItem; // Ezt klónozzuk ha egy útvonalat kell készíteni

    //TextHelper textHelperAppPlayTimeText;
    TextHelper textHelperAppPlayTimeValue;

    Text textUserName;


    Common.CallBack_In_String buttonClick;

    List<OTPCurriculumPath> listOfCurriculumPathItems = new List<OTPCurriculumPath>(); // A létrehozott tananyag útvonalak, a törlésükhöz kell

    // Use this for initialization
    void Awake()
    {
        scrollView = gameObject.SearchChild("Scroll View").GetComponent<RectTransform>();
        content = gameObject.SearchChild("Content").GetComponent<RectTransform>();
        prefabCurriculumPathItem = gameObject.SearchChild("CurriculumPath").GetComponent<OTPCurriculumPath>();
        rectTransformPrefabCurriculumPathItem = prefabCurriculumPathItem.GetComponent<RectTransform>();

        //textHelperAppPlayTimeText = gameObject.SearchChild("TextAppPlayTimeText").GetComponent<TextHelper>();
        textHelperAppPlayTimeValue = gameObject.SearchChild("TextAppPlayTimeValue").GetComponent<TextHelper>();

        textUserName = gameObject.SearchChild("TextUserName").GetComponent<Text>();

        prefabCurriculumPathItem.gameObject.SetActive(false);
    }

    void Start() {
        Initialize(null, "", null);
    }

    public void Initialize(List<CurriculumPathData> list, string buttonNamePrefix, Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;

        // Kiiírjuk az alkalmazásban töltött játék időt, ha legalább egy útvonal van, mivel ha nincs, akkor nem kapok erről információt
        if (list == null || list.Count == 0)
        {
            //textHelperAppPlayTimeText.SetText("");
            textHelperAppPlayTimeValue.SetText("");
        }
        else
        {
            //textHelperAppPlayTimeText.SetText(Common.languageController.Translate(C.Texts.AppPlayTime));
            textHelperAppPlayTimeValue.SetText(list[0].appPlayTimeString);
        }

        // Kiírjuk a felhasználó nevét
        textUserName.text = Common.configurationController.userName;

        // A már létező menü elemeket töröljük
        for (int i = 0; i < listOfCurriculumPathItems.Count; i++)
            Destroy(listOfCurriculumPathItems[i].gameObject);
        listOfCurriculumPathItems.Clear();

        // Létrehozzuk az új menü elemeket
        int gapSize = 50; // Az elemek közötti távolság
        float screenSize = scrollView.rect.width; // 2048;
        int listCount = (list != null) ? list.Count : 0;
        int piece = (listCount < 1) ? 1 : listCount;
        for (int i = 0; i < piece; i++)
        {
            OTPCurriculumPath newCurriculumPath = Instantiate(prefabCurriculumPathItem, content.transform);
            newCurriculumPath.gameObject.SetActive(true);

            if (i < listCount)
                newCurriculumPath.Initialize(list[i], buttonNamePrefix + ":" + i, ButtonClick); // list[i].name, list[i].scorePercent, list[i].progress / 100, buttonNamePrefix + ":" + i /*list[i].ID*/, ButtonClick);
            else
                newCurriculumPath.Empty();

            RectTransform rectTranform = newCurriculumPath.GetComponent<RectTransform>();
            //recTranform.localPosition = recTranform.localPosition.SetX(rectTransformCurriculumPathItem.localPosition.x + (recTranform.sizeDelta.x + gapSize) * i);
            rectTranform.localPosition = rectTranform.localPosition.SetX((screenSize - rectTranform.sizeDelta.x) / 2 + (rectTranform.sizeDelta.x + gapSize) * i);

            listOfCurriculumPathItems.Add(newCurriculumPath);
        }

        // Beállítjuk a tartalom méretét
        content.sizeDelta = new Vector2(screenSize + (rectTransformPrefabCurriculumPathItem.sizeDelta.x + gapSize) * (listCount - 1), content.sizeDelta.y);
    }

    public void ButtonClick(string buttonName)
    {
        if (buttonClick != null)
            buttonClick(buttonName);
    }
}
