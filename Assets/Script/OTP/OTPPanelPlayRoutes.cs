using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OTPPanelPlayRoutes : MonoBehaviour
{
    public static OTPPanelPlayRoutes instance;

    GameObject prefabEmailGroupButton;
    GameObject prefabCurriculumPath;

    RectTransform contentTopPanel;
    RectTransform contentBottomPanel;

    Fade fadePanelTopCover;
    Fade fadePanelBottomCover;

    ScrollRect scrollRectBottom;

    ClassYMainMenuItem menuItem;

    Text textNotCurriculum; // "nincs megjelenítendő tananyag" szöveg kiírásához

    Text textUserName;

    Common.CallBack_In_String buttonClick;

    string buttonNamePrefix;

    List<GameObject> makedEmailGroupButtons = new List<GameObject>();
    List<GameObject> makedCurriculumPaths = new List<GameObject>();

    List<CurriculumPathData> listOfCurriculumPath; // A kiválasztott email lista útvonalainak adatait tartalmazza

    string activeEmailGroupID;  // Mi az aktuálisan kiválasztott email csoportnak az azonossítója

    bool EmailGroupScrollPosSaveEnabled;

    List<LanguageData> listOfLanguageDatas = null;
    bool getLanguagesSuccess; // Igaz értéke van, ha a GetLanguages lekérdezés sikeres volt
    public string requestedLanguage;   // Valmelyik email listábn a play gombra kattintottak, ilyenkor az email lista nyelvével azonos csoportokat kell itt listázni

    EmailGroupList emailGroupList; // Az internetről sikeresen letöltött emailGroup listákat tartalmazza
    bool getEmailGroupListSuccess; // Jelzi, hogy a GetEmailGroupList metódus sikeresen lefutott-e

    void Awake()
    {
        instance = this;

        prefabEmailGroupButton = gameObject.SearchChild("EmailGroupButton").gameObject;
        prefabCurriculumPath = gameObject.SearchChild("CurriculumPath").gameObject;

        contentTopPanel = gameObject.SearchChild("PanelTop/Content").GetComponent<RectTransform>();
        contentBottomPanel = gameObject.SearchChild("PanelBottom/Content").GetComponent<RectTransform>();

        fadePanelTopCover = gameObject.SearchChild("PanelTopCover").GetComponent<Fade>();
        fadePanelBottomCover = gameObject.SearchChild("PanelBottomCover").GetComponent<Fade>();

        scrollRectBottom = contentBottomPanel.transform.parent.parent.GetComponent<ScrollRect>();

        menuItem = gameObject.SearchChild("CurriculumListOfSelectedGroup").GetComponent<ClassYMainMenuItem>();

        textNotCurriculum = gameObject.SearchChild("TextNotCurriculum").GetComponent<Text>();

        textUserName = gameObject.SearchChild("TextUserName").GetComponent<Text>();

        prefabEmailGroupButton.SetActive(false);
        prefabCurriculumPath.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Inicializáljuk a menü elemet
        menuItem.Initialize(C.Texts.CurriculumsOfGroup, "0", C.Program.BrowseCurriculum, ButtonClick);
    }

    public void Initialize(string buttonNamePrefix, string emailGroupID, Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;
        this.buttonNamePrefix = buttonNamePrefix;

        // Kiírjuk a felhasználó nevét
        textUserName.text = Common.configurationController.userName;
        
        StartCoroutine(InitializeCoroutine(emailGroupID));
    }

    IEnumerator InitializeCoroutine(string emailGroupID)
    {
        // Lekérdezzük a használható nyelveket
        yield return StartCoroutine(GetLanguages());

        // Lekérdezzük az email listákat a kiválasztott nyelvnek megfelelően
        if (getLanguagesSuccess)
        {
            yield return StartCoroutine(GetEmailGroupList());

            if (getEmailGroupListSuccess)
            {
                DrawEmailGroupButtons();

                SetEmailGroup(emailGroupID);
            }
        }
    }

    /*
    public void InitializeEmailGroupButtons(EmailGroupList emailGroupList, string buttonNamePrefix, Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;
        this.emailGroupList = emailGroupList;
        this.buttonNamePrefix = buttonNamePrefix;

        // Kitöröljük a korábban létrehozott GameObject-eket
        for (int i = 0; i < makedEmailGroupButtons.Count; i++)
            Destroy(makedEmailGroupButtons[i]);

        makedEmailGroupButtons.Clear();

        // Létrehozzuk az új tartalmat
        for (int i = 0; i < emailGroupList.list.Count; i++)
        {
            // Email csoport gomb létrehozása
            GameObject newPrefabEmailGroupButton = Instantiate(prefabEmailGroupButton, contentTopPanel);
            newPrefabEmailGroupButton.SetActive(true);
            makedEmailGroupButtons.Add(newPrefabEmailGroupButton);

            newPrefabEmailGroupButton.GetComponentInChildren<EmailGroupButton>().Initialize(emailGroupList.list[i], ButtonClick);
        }

        // A zászlót létrehozzuk
        GameObject flagGo = gameObject.SearchChild("ImageFlag");
        flagGo.GetComponent<Image>().sprite = Common.configurationController.curriculumLang.langFlag;

        // Ha csak egy nyelv van, akkor eltüntetjük a zászlót
        flagGo.transform.parent.gameObject.SetActive(Common.configurationController.curriculumLangCount > 1);

        fadePanelTopCover.Show();
        fadePanelTopCover.Hide();
    }
    */

    /// <summary>
    /// Beállítja a megadott email csoportott aktívvá, majd lekérdezi az email csoport útvonalait és megjeleníti
    /// </summary>
    /// <param name="emailGroupID"></param>
    public void SetEmailGroup(string emailGroupID)
    {
        // Letiltjuk a scroll pozíció mentését
        EmailGroupScrollPosSaveEnabled = false;

        // Ha nincsenek email csoportok, akkor kilépünk
        if (emailGroupList.list.Count == 0)
            return;

        // Ha nincs megadva email csoport akkor az aktuálisat használjuk
        if (emailGroupID == "")
            emailGroupID = activeEmailGroupID;

        // Megnézzük, hogy létezik-e a megadott ID-nak megfelelő csoport
        if (GetEmailGroup(emailGroupID) == null)
            // Ha nem létezik, akkor az első csoport lesz a kiválasztott
            emailGroupID = emailGroupList.list[0].id;

        // Kiválasztottá tesszük az email csoportot
        for (int i = 0; i < makedEmailGroupButtons.Count; i++)
            makedEmailGroupButtons[i].GetComponent<EmailGroupButton>().SetActive(emailGroupID);

        GetCurriculumPaths(emailGroupID);
    }

    void GetCurriculumPaths(string emailGroupID)
    {
        activeEmailGroupID = emailGroupID;
        ClassYServerCommunication.instance.GetPlayableLearnRoutePathList(emailGroupID, (bool success, JSONNode response) =>
            {
                // Válasz feldolgozása
                if (success)
                {
                    listOfCurriculumPath = CurriculumPathData.JsonArrayToList(response[C.JSONKeys.answer]);
                    DrawCurriculumPaths(listOfCurriculumPath, "", ButtonClick);

                    // Beállítjuk letöltött email csoporthoz a scrollpozíciót
                    //scrollRectBottom.gameObject.SetActive(false);
                    if (Common.configurationController.playRoutesEmailGroupScrollPos.ContainsKey(emailGroupID))
                        Common.configurationController.WaitTime(0.01f, () =>
                        {
                            scrollRectBottom.horizontalNormalizedPosition = Common.configurationController.playRoutesEmailGroupScrollPos[emailGroupID];
                            Debug.Log("Beállítás : " + activeEmailGroupID + " : " + Common.configurationController.playRoutesEmailGroupScrollPos[emailGroupID]);
                            //scrollRectBottom.gameObject.SetActive(true);

                            EmailGroupScrollPosSaveEnabled = true;
                        });
                }
            }
        );
    }

    public void DrawCurriculumPaths(List<CurriculumPathData> listOfCurriculumPath_, string buttonNamePrefix, Common.CallBack_In_String buttonClick)
    {
        // Kitöröljük a korábban létrehozott GameObject-eket
        for (int i = 0; i < makedCurriculumPaths.Count; i++)
            Destroy(makedCurriculumPaths[i]);

        makedCurriculumPaths.Clear();

        // Létrehozzuk az új tartalmat
        for (int i = 0; i < listOfCurriculumPath.Count; i++)
        {
            // Email csoport gomb létrehozása
            GameObject newPrefabCurriculumPath = Instantiate(prefabCurriculumPath, contentBottomPanel);
            newPrefabCurriculumPath.SetActive(true);
            makedCurriculumPaths.Add(newPrefabCurriculumPath);

            newPrefabCurriculumPath.GetComponentInChildren<OTPCurriculumPath>().Initialize(listOfCurriculumPath[i], "Play:" + listOfCurriculumPath[i].ID, buttonClick);
        }

        // Bekapcsoljuk a nincs tananyag szöveget ha nincs útvonal a kiválasztott email listában
        textNotCurriculum.enabled = (listOfCurriculumPath.Count == 0);

        fadePanelBottomCover.Show();
        fadePanelBottomCover.Hide();
    }

    /// <summary>
    /// Vissza adja a megadott ID-jű Tananyag útvonal adatokat
    /// </summary>
    /// <param name="ID"></param>
    /// <returns></returns>
    public CurriculumPathData GetCurriculumPathData(string ID)
    {
        for (int i = 0; i < listOfCurriculumPath.Count; i++)
            if (listOfCurriculumPath[i].ID == ID)
                return listOfCurriculumPath[i];

        return null;
    }

    public EmailGroup GetActEmailGroup()
    {
        return GetEmailGroup(activeEmailGroupID);
    }

    EmailGroup GetEmailGroup(string emailGroupID)
    {
        for (int i = 0; i < emailGroupList.list.Count; i++)
            if (emailGroupList.list[i].id == emailGroupID)
                return emailGroupList.list[i];

        return null;
    }

    void DrawEmailGroupButtons()
    {
        // Kitöröljük a korábban létrehozott GameObject-eket
        for (int i = 0; i < makedEmailGroupButtons.Count; i++)
            Destroy(makedEmailGroupButtons[i]);

        makedEmailGroupButtons.Clear();

        // Létrehozzuk az új tartalmat
        for (int i = 0; i < emailGroupList.list.Count; i++)
        {
            // Email csoport gomb létrehozása
            GameObject newPrefabEmailGroupButton = Instantiate(prefabEmailGroupButton, contentTopPanel);
            newPrefabEmailGroupButton.SetActive(true);
            makedEmailGroupButtons.Add(newPrefabEmailGroupButton);

            newPrefabEmailGroupButton.GetComponentInChildren<EmailGroupButton>().Initialize(emailGroupList.list[i], ButtonClick);
        }

        // A zászlót létrehozzuk
        GameObject flagGo = gameObject.SearchChild("ImageFlag");
        flagGo.GetComponent<Image>().sprite = Common.configurationController.curriculumLang.langFlag;

        // Ha csak egy nyelv van, akkor eltüntetjük a zászlót
        flagGo.transform.parent.gameObject.SetActive(Common.configurationController.curriculumLangCount > 1);

        fadePanelTopCover.Show();
        fadePanelTopCover.Hide();
    }

    IEnumerator GetEmailGroupList()
    {
        EmailGroupScrollPosSaveEnabled = false;

        bool ready = false;

        /*
        ClassYServerCommunication.instance.GetWhereIAmOnMailLists(
            (bool success, JSONNode response) =>
            {
                getEmailGroupListSuccess = success;

                // Válasz feldolgozása
                if (success)
                {
                    emailGroupList = new EmailGroupList(response[C.JSONKeys.answer]);

                    ready = true;
                }
            }
        );
        */
        
        ClassYServerCommunication.instance.getPlayRoutes(Common.configurationController.curriculumLang.langID,
            (bool success, JSONNode response) =>
            {
                getEmailGroupListSuccess = success;

                // Válasz feldolgozása
                if (success)
                {
                    emailGroupList = new EmailGroupLists(response[C.JSONKeys.answer]).list[0];

                    ready = true;
                }
            }
        );

        // Várunk amíg a lekérdezés megjön és feldolgozódik
        while (!ready) { yield return null; }

        // Ha a lekérdezés sikeres volt, akkor kirajzoljuk a listát
        if (getEmailGroupListSuccess)
        {
            DrawEmailGroupButtons();

            SetEmailGroup(activeEmailGroupID);
        }
    }

    // Lekérdezi a használható nyelvek listáját
    IEnumerator GetLanguages()
    {
        bool ready = false;

        // Lekérdezzük a használható nyelveket
        ClassYServerCommunication.instance.GetUsableLanguages("", true,
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
            LanguageData languageData = LanguageData.GetLanguageData(listOfLanguageDatas, Common.configurationController.curriculumLang.langCode);

            // A korábban beállított nyelv nincs
            if (languageData == null)
            {
                // Töröljük a adatait
                Common.configurationController.curriculumLang = new LanguageData();
            }

            // Ha még nem határoztuk meg egyszer sem, vagy éppen az előbb töröltük
            if (string.IsNullOrEmpty(Common.configurationController.curriculumLang.langCode))
            {
                // Akkor az első nyelv lesz a beállított ha van legalább egy nyelv
                if (listOfLanguageDatas.Count > 0)
                    Common.configurationController.curriculumLang = listOfLanguageDatas[0];
            }

            // Ha van kért nyelv, akkor azt állítjuk be (Elvileg kötelezően lennie kellene)
            languageData = LanguageData.GetLanguageData(listOfLanguageDatas, requestedLanguage);
            if (languageData != null)
                Common.configurationController.curriculumLang = languageData;

            requestedLanguage = "";
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
                ClassYLanguageSelector.instance.Show(Common.configurationController.curriculumLang.langCode, C.Texts.SelectPublicEmailListLanguage,
                    (string buttonName) =>
                    {
                        ClassYLanguageSelector.instance.Hide();

                        // Ha más nyelvet választottak mint korábban, csak akkor csinálunk valamit
                        if (Common.configurationController.curriculumLang.langCode != buttonName && buttonName != C.Program.FadeClickLanguageSelector)
                        {
                            Common.configurationController.curriculumLang = LanguageData.GetLanguageData(listOfLanguageDatas, buttonName);

                            // Frissítjük a tartalmat
                            StartCoroutine(GetEmailGroupList());
                        }
                    }
                );
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (EmailGroupScrollPosSaveEnabled)
            if (!string.IsNullOrWhiteSpace(activeEmailGroupID))
            {
                if (Common.configurationController.playRoutesEmailGroupScrollPos.ContainsKey(activeEmailGroupID))
                    Common.configurationController.playRoutesEmailGroupScrollPos[activeEmailGroupID] = scrollRectBottom.horizontalNormalizedPosition;
                else
                    Common.configurationController.playRoutesEmailGroupScrollPos.Add(activeEmailGroupID, scrollRectBottom.horizontalNormalizedPosition);

                //Debug.Log(activeEmailGroupID + " : " + scrollRectBottom.horizontalNormalizedPosition);
            }
    }

    public void ButtonClick(string buttonName)
    {
        // Ha az ikont nyomták meg, akkor megpörgetjük
        if (buttonName == C.Program.BrowseCurriculum)
            menuItem.SetActive(true);

        if (buttonName == "Flag")
        {
            StartCoroutine(SelectLanguage());
        }
        else
        {
            // A zászlóra kattintást helyben dolgozzuk fel
            // Ha a zásslóra kattintottak, akkor nem kell a buttonNamePrefix
            //if (buttonName != "Flag")
            //    buttonName = buttonNamePrefix + ":" + buttonName;

            if (buttonClick != null)
                buttonClick(buttonNamePrefix + ":" + buttonName);
        }
    }
}
