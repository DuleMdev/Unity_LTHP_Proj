using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OTPPanelGroupsList : MonoBehaviour
{
    GameObject prefabLabel;
    PanelEmailGroupCompactList prefabEmailGroupCompactList;
    PanelEmailGroupBigList prefabEmailGroupBigList;

    RectTransform content;
    Text textNotCurriculum; // "nincs megjelenítendő tananyag" szöveg kiírásához

    Common.CallBack_In_String buttonClick;
    string buttonNamePrefix;

    List<GameObject> madeGameObjects = new List<GameObject>();

    List<LanguageData> listOfLanguageDatas = null;
    bool getLanguagesSuccess; // Igaz értéke van, ha a GetLanguages lekérdezés sikeres volt

    public EmailGroupLists emailGroupLists; // Az internetről sikeresen letöltött emailGroup listákat tartalmazza
    bool getEmailGroupListSuccess; // Jelzi, hogy a GetEmailGroupList metódus sikeresen lefutott-e

    string viewType;    // melyik listát kell lekérdezni. Jelenleg a MainPageGroupBrowser és a MyGroupsEdit listák léteznek

    void Awake()
    {
        prefabLabel = gameObject.SearchChild("PanelLabel").gameObject;
        prefabEmailGroupCompactList = gameObject.SearchChild("PanelEmailGroupCompactList").GetComponent<PanelEmailGroupCompactList>();
        prefabEmailGroupBigList = gameObject.SearchChild("PanelEmailGroupBigList")?.GetComponent<PanelEmailGroupBigList>(); // Az OTPPanelMyGroupsEdit panelen nincs, ezért kell a kérdőjel

        content = gameObject.SearchChild("Content").GetComponent<RectTransform>();
        textNotCurriculum = gameObject.SearchChild("TextNotCurriculum").GetComponent<Text>();

        prefabLabel.SetActive(false);
        prefabEmailGroupCompactList.gameObject.SetActive(false);
        prefabEmailGroupBigList?.gameObject.SetActive(false); // Ha nincs nem kapcsoljuk ki, ezért kell a kérdőjel
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Initialize(string buttonNamePrefix, Common.CallBack_In_String buttonClick)
    {
        StartCoroutine(InitializeCoroutine(buttonNamePrefix, buttonClick));
    }

    public IEnumerator InitializeCoroutine(string buttonNamePrefix, Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;
        this.buttonNamePrefix = buttonNamePrefix;

        yield return StartCoroutine(GetLanguages());

        if (getLanguagesSuccess)
        {
            StartCoroutine(GetEmailGroupList());
        }
    }

    // Kirajzolja a emailGroupList listában található tartalmat a korábbit pedig kitörli
    void DrawList()
    {
        // Kitöröljük a korábban létrehozott GameObject-eket
        for (int i = 0; i < madeGameObjects.Count; i++)
            Destroy(madeGameObjects[i]);

        madeGameObjects.Clear();

        // Létrehozzuk az új tartalmat
        float posY = 0;
        for (int i = 0; i < emailGroupLists.list.Count; i++)
        {
            //if (emailGroupLists.list[i].list.Count == 0)
            //    continue;

            // Fejléc létrehozása
            GameObject newPrefabLabel = Instantiate(prefabLabel, content);
            newPrefabLabel.SetActive(true);
            madeGameObjects.Add(newPrefabLabel);

            newPrefabLabel.GetComponentInChildren<SetLanguageText>().SetTextID(emailGroupLists.list[i].requestName);
            if (!string.IsNullOrEmpty(emailGroupLists.list[i].color))
                newPrefabLabel.GetComponent<Image>().color = Common.MakeColor(emailGroupLists.list[i].color);

            GameObject flagGo = newPrefabLabel.SearchChild("ImageFlag");
            if (flagGo)
            {
                // Ha volt nyelvre szűrés és legalább két nyelv van, akkor mutatjuk a zászlót
                if (emailGroupLists.list[i].languageFilter && listOfLanguageDatas.Count > 1)
                    flagGo.GetComponent<Image>().sprite = Common.configurationController.publicEmailGroupsLang.langFlag;
                else
                    flagGo.transform.parent.gameObject.SetActive(false);
            }

            RectTransform newPrefabLabelRectTransform = newPrefabLabel.GetComponent<RectTransform>();
            newPrefabLabelRectTransform.anchoredPosition = new Vector2(0, posY);

            posY -= newPrefabLabelRectTransform.sizeDelta.y;

            // Lista elemek létrehozása
            float listSize = 0;
            RectTransform listRectTransform = null;
            if (emailGroupLists.list[i].bigItems)
            {
                // Készítünk egy nagy listát
                PanelEmailGroupBigList newPrefabEmailGroupBigList = Instantiate(prefabEmailGroupBigList.gameObject, content).GetComponent<PanelEmailGroupBigList>();
                newPrefabEmailGroupBigList.gameObject.SetActive(true);
                madeGameObjects.Add(newPrefabEmailGroupBigList.gameObject);

                listSize = newPrefabEmailGroupBigList.Initialize(emailGroupLists.list[i].list, buttonNamePrefix + ":" + i, buttonClick);
                listRectTransform = newPrefabEmailGroupBigList.GetComponent<RectTransform>();
            }
            else
            {
                // Készítünk egy kompakt listát
                PanelEmailGroupCompactList newPrefabEmailGroupCompactList = Instantiate(prefabEmailGroupCompactList.gameObject, content).GetComponent<PanelEmailGroupCompactList>();
                newPrefabEmailGroupCompactList.gameObject.SetActive(true);
                madeGameObjects.Add(newPrefabEmailGroupCompactList.gameObject);

                listSize = newPrefabEmailGroupCompactList.Initialize(emailGroupLists.list[i].list, buttonNamePrefix + ":" + i, buttonClick);
                listRectTransform = newPrefabEmailGroupCompactList.GetComponent<RectTransform>();
            }

            listRectTransform.anchoredPosition = new Vector2(0, posY);
            listRectTransform.sizeDelta = new Vector2(0, listSize);

            posY -= listSize;
        }

        content.sizeDelta = new Vector2(0, -posY);

        // Bekapcsoljuk a nincs tananyag szöveget ha nincs útvonal a kiválasztott email listában
        textNotCurriculum.enabled = (emailGroupLists.list.Count == 0);
    }

    IEnumerator GetEmailGroupList()
    {
        bool ready = false;

        Common.CallBack_In_Bool_JSONNode answerProcessor = (bool success, JSONNode response) =>
        {
            getEmailGroupListSuccess = success;

            // Válasz feldolgozása
            if (success)
            {
                emailGroupLists = new EmailGroupLists(response[C.JSONKeys.answer]);
            }

            ready = true;
        };

        switch (buttonNamePrefix)
        {
            case C.Program.MainPageGroupBrowser:
                ClassYServerCommunication.instance.appMainPageGroupBrowser(answerProcessor);
                break;

            case C.Program.MyGroupsEdit:
                ClassYServerCommunication.instance.appMyGroupsEdit(answerProcessor);
                break;
        }

        // Várunk amíg a lekérdezés megjön és feldolgozódik
        while (!ready) { yield return null; }

        // Ha a lekérdezés sikeres volt, akkor kirajzoljuk a listát
        if (getEmailGroupListSuccess)
            DrawList();
    }

    IEnumerator GetLanguages()
    {
        bool ready = false;

        // Lekérdezzük a használható nyelveket
        ClassYServerCommunication.instance.getPublicMailListsLanguages("", true,
            (bool success, JSONNode response) =>
            {
                getLanguagesSuccess = success;

                // Válasz feldolgozása
                if (success)
                {
                    listOfLanguageDatas = LanguageData.GetListOfLanguageData(response[C.JSONKeys.answer][C.JSONKeys.usableLangCodes]);
                }

                ready = true;
            }
        );

        // Várunk amíg a lekérdezés megjön és feldolgozódik
        while (!ready) { yield return null; }

        // Ha sikeres volt a lekérdezés, akkor meghatározzuk a használandó nyelvet
        if (getLanguagesSuccess)
        {
            // Ha a beállított nyelv nincs benne a listába
            LanguageData languageData = LanguageData.GetLanguageData(listOfLanguageDatas, Common.configurationController.publicEmailGroupsLang.langCode);

            // A korábban beállított nyelv nincs
            if (languageData == null)
            {
                // Beállítjuk az alkalmazás nyelvét
                languageData = LanguageData.GetLanguageData(listOfLanguageDatas, Common.configurationController.applicationLangName);

                if (languageData == null)
                {
                    // Az alkalmazás nyelve sincs a listában
                    // Töröljük a adatait
                    Common.configurationController.publicEmailGroupsLang = new LanguageData();
                }
                else
                {
                    // Ha az alkalmazás nyelve benne van a listában, akkor rögzítjük
                    Common.configurationController.publicEmailGroupsLang = languageData;
                }
            }

            // Ha még nem határoztuk meg egyszer sem, vagy éppen az előbb töröltük
            if (string.IsNullOrEmpty(Common.configurationController.publicEmailGroupsLang.langCode))
            {
                // Akkor az első nyelv lesz a beállított ha van legalább egy nyelv
                if (listOfLanguageDatas.Count > 0)
                    Common.configurationController.publicEmailGroupsLang = listOfLanguageDatas[0];
            }

            Common.configurationController.Save();
        }
    }

    IEnumerator SelectLanguage()
    {
        // Lekérdezzük a használható nyelveket
        yield return StartCoroutine(GetLanguages());

        // Ha a lekérdezés sikeres volt
        if (getLanguagesSuccess)
        {
            if (listOfLanguageDatas != null && listOfLanguageDatas.Count > 0)
            {
                // Megnyitjuk a nyevlválasztó panelt
                ClassYLanguageSelector.instance.Initialize(listOfLanguageDatas, "");
                ClassYLanguageSelector.instance.Show(Common.configurationController.publicEmailGroupsLang.langCode, C.Texts.SelectPublicEmailListLanguage,
                    (string buttonName) =>
                    {
                        ClassYLanguageSelector.instance.Hide();

                        // Ha más nyelvet választottak mint korábban, csak akkor csinálunk valamit
                        if (Common.configurationController.publicEmailGroupsLang.langCode != buttonName && buttonName != C.Program.FadeClickLanguageSelector)
                        {
                            Common.configurationController.publicEmailGroupsLang = LanguageData.GetLanguageData(listOfLanguageDatas, buttonName);

                            StartCoroutine(GetEmailGroupList());
                        }
                    }
                );
            }
        }
    }

    // Két esetben kerül meghívásra, 
    // 1. Ha rákattintottak a zászlóra 
    // 2. Ha egy email csoportra kattintottak vagy azon belül egy gombra.
    public void ButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Flag":
                // Megnyitjuk a zászló választást
                StartCoroutine(SelectLanguage());
                break;

            default:
                buttonName = buttonNamePrefix + ":" + buttonName;

                if (buttonClick != null)
                    buttonClick(buttonName);

                break;
        }
    }
}
