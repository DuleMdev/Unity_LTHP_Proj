using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;

public class OTPMain : HHHScreen
{

    static public OTPMain instance;

    public GameObject LThPTextandButton;

    public enum Panels // A látható panelt mutatja
    {
        empty,                  // Egyik panel sem látszik
        liveStream,             // A LiveStreamet tartalmazó panel látszik
        subjectList,            // A tantárgyakat tartalmazó panel látszik
        subMenu,                // Az almenüket tartalmazó panel látszik
        curriculumList,         // A tananyagokat tartalmazó panel látszik (gyakorlandó, legutóbb lejátszott tananyagok)
        curriculumPlay,         // A tananyag lejátszás panel látszik
        marketPlace,            // A piactér panel látszik
        playRoutes,             // Az útvonalak lejátszása panel látszik
        mainPageGroupBrowser,   // A főoldal csoport böngésző panel látszik
        myGroupsEdit,           // A csoportjaim szerkesztése panel látszik
        myRewards,              // Kincstár / Jutalmaim
        myLearnPlay               // Teljesített és megkezdett útvonalak (szülőknek pin visszaállítás)
    }

    MainMenuClassY mainMenu;

    PanelLiveStream liveStream;
    PanelSubjectList subjectList;
    OTPPanelSubMenu subMenu;
    OTPPanelCurriculumList curriculumList;
    OTPPanelCurriculumPlay curriculumPlay;
    MarketPlacePanel marketPlace;
    OTPPanelPlayRoutes playRoutes;
    OTPPanelGroupsList mainPageGroupBrowser;
    OTPPanelGroupsList myGroupsEdit;
    MyRewards myRewards;
    OTPPanelLearnPlay myLearnPlay;

    // Panelek mozgatásához
    RectTransform rectTransformLiveStream;
    RectTransform rectTransformSubjectList;
    RectTransform rectTransformSubMenu;
    RectTransform rectTransformCurriculumList;
    RectTransform rectTransformCurriculumPlay;
    RectTransform rectTransformMarketPlace;
    RectTransform rectTransPlayRoutes;
    RectTransform rectTransformMainPageGroupBrowser;
    RectTransform rectTransformMyGroupsEdit;
    RectTransform rectTransformMyRewards;
    RectTransform rectTransformLearnPlay;


    public Panels actVisiblePanel; // Melyik panel látható

    float panelHidePos; // Hol van a panel amikor rejtve van
    float panelShowPos; // Hol van a panel amikor látszik

    //PanelSortSelector panelSortSelector;
    //ScroolViewCurriculumsListDrive scroolViewCurriculumListDrive;
    //PopUpInfo popUpInfo;

    string scope;   // Mit akarunk listázni (mindent - all, saját - own, vagy megosztott - shared, stb.)

    // Néhol indexet adunk a gomboknak, néhol meg id-t
    // Az id az sok esetben jó, de ha nem lekérdezést akarunk rögtön, hanem a korábban letöltött adatokkal
    // van még teendönk, akkor inkább az index a célravezető.
    // Ilyen a tananyagok listája, amit ha kiválasztunk, akkor egy felbukkanó panelen ki kell írni az adatait
    // A másik ilyen a tanfolyam útvonal, amit ha kiválasztanak, akkor folytatni kell az utoljára
    // félbehagyott tanfolyamot és ehhez szükséges a letöltött adatok újabb feldolgozása.
    // 0 - MainMenu:1(index)
    // 1 - Subject:5(id)
    // 2 - Topic:18(id)
    // 3 - Course:27(index)
    string[] buttonNames = new string[4];
    int level;

    List<CurriculumItemDriveData> listOfCurriculums;

    //EmailGroupLists emailGroupLists;

    string subjectId;
    string topicId;
    string courseId;
    string curriculumId;
    int curriculumIndex;

    Common.CallBack playGamesCallBack;

    bool hideFinish;    // Az aktuális panel elrejtése befejeződött

    /// <summary>
    /// Az különböző útvonalakat tartalmazza egy listában.
    /// </summary>
    List<CurriculumPathData> listOfCurriculumPath;

    /// <summary>
    /// A getUsableLanguages lekérdezés által vissza adott nyelveket tartalmazza.
    /// </summary>
    List<LanguageData> listOfLanguageData;

    bool userActivityDisabled; // Panel cserék alatt a User le van tiltva

    string lastImportantButtonClick; // Ez alapján tudjuk, hogy mit kell frissíteni a következő képernyő betöltésnél

    EmailGroup subMenuEmailGroup;

    string startMenu;   // Melyik menüvel kezdődjön az alkalmazás

    public JSONNode marketPlaceData; // Ha sikeres volt a szerver kommunikáció, akkor itt található a marketPlace adatai

    // Use this for initialization
    void Awake()
    {
        instance = this;

        mainMenu = gameObject.SearchChild("MainMenu").GetComponent<MainMenuClassY>();

        liveStream = gameObject.SearchChild("LiveStream").GetComponent<PanelLiveStream>();
        subjectList = gameObject.SearchChild("SubjectList").GetComponent<PanelSubjectList>();
        subMenu = gameObject.SearchChild("OTPPanelSubMenu").GetComponent<OTPPanelSubMenu>();
        curriculumList = gameObject.SearchChild("PanelCurriculumList").GetComponent<OTPPanelCurriculumList>();
        curriculumPlay = gameObject.SearchChild("PanelCurriculumPlay").GetComponent<OTPPanelCurriculumPlay>();
        marketPlace = gameObject.SearchChild("MarketPlace").GetComponent<MarketPlacePanel>();
        playRoutes = gameObject.SearchChild("OTPPanelPlayRoutes").GetComponent<OTPPanelPlayRoutes>();
        mainPageGroupBrowser = gameObject.SearchChild("OTPPanelMainPageGroupBrowser").GetComponent<OTPPanelGroupsList>(); ;
        myGroupsEdit = gameObject.SearchChild("OTPPanelMyGroupsEdit").GetComponent<OTPPanelGroupsList>();
        myRewards = gameObject.SearchChild("MyRewards").GetComponent<MyRewards>();
        myLearnPlay = gameObject.SearchChild("OTPPanelLearnPlay").GetComponent<OTPPanelLearnPlay>();

        rectTransformLiveStream = liveStream.GetComponent<RectTransform>();
        rectTransformSubjectList = subjectList.GetComponent<RectTransform>();
        rectTransformSubMenu = subMenu.GetComponent<RectTransform>();
        rectTransformCurriculumList = curriculumList.GetComponent<RectTransform>();
        rectTransformCurriculumPlay = curriculumPlay.GetComponent<RectTransform>();
        rectTransformMarketPlace = marketPlace.GetComponent<RectTransform>();
        rectTransPlayRoutes = playRoutes.GetComponent<RectTransform>();
        rectTransformMainPageGroupBrowser = mainPageGroupBrowser.GetComponent<RectTransform>();
        rectTransformMyGroupsEdit = myGroupsEdit.GetComponent<RectTransform>();
        rectTransformMyRewards = myRewards.GetComponent<RectTransform>();
        rectTransformLearnPlay = myLearnPlay.GetComponent<RectTransform>();

        //panelSortSelector = gameObject.SearchChild("PanelSortSelector").GetComponent<PanelSortSelector>();
        //popUpInfo = gameObject.SearchChild("PopUpInfo").GetComponent<PopUpInfo>();

        //scroolViewCurriculumListDrive = gameObject.SearchChild("CurriculumListDrive").GetComponent<ScroolViewCurriculumsListDrive>();
    }

    void Start()
    {
        //popUpInfo.Initialize(ButtonClick);

        /*
        panelHidePos = 0;
        panelShowPos = rectTransformSubjectList.sizeDelta.y; // Mindegyik panel ugyan akkora
        */

    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni
    override public IEnumerator InitCoroutine()
    {
        scope = C.Program.groupShared;


        // Létrehozzuk a MainMenu-ben a menü elemeket
        List<string> menuItems = new List<string>();
        /*
        menuItems.Add(C.Texts.CurriculumsPlay);
        // Live stream menü elem csak OTPFIA esetében van
        if (Common.configurationController.appID == ConfigurationController.AppID.OTPFIA)
            menuItems.Add(C.Texts.LiveStream);
        // MarketPlace menü elem csak Provocent és az entrepreneur alkalmazás esetében van
        if (Common.configurationController.appID == ConfigurationController.AppID.PROVOCENT ||
            Common.configurationController.appID == ConfigurationController.AppID.ENTREPRENEUR)
            menuItems.Add(C.Texts.MarketPlace);
        //menuItems.Add(C.Texts.SubjectsList);
        //menuItems.Add(C.Texts.ExercisedCurriculum);
        //menuItems.Add(C.Texts.LastPlayedCurriculums);
        menuItems.Add(C.Texts.GroupBrowser);
        menuItems.Add(C.Texts.MyGroups);
        menuItems.Add(C.Texts.MyGroupsEdit);
        */
        if (Common.configurationController.setupData.PlayRoutes) menuItems.Add(C.Texts.PlayRoutes);
        if (Common.configurationController.setupData.MainPageGroupBrowser) menuItems.Add(C.Texts.MainPageGroupBrowser);
        if (Common.configurationController.setupData.MyGroupsEdit) menuItems.Add(C.Texts.MyGroupsEdit);
        if (Common.configurationController.setupData.CurriculumPlay) menuItems.Add(C.Texts.CurriculumsPlay);
        //if (Common.configurationController.setupData.LearnPlay) menuItems.Add(C.Texts.LearnThenPlay);   //Itt volt eredetileg a LearnThenPlay, de megkellett cserélnem a sorrendet ~Ádám
        if (Common.configurationController.setupData.SubjectList) menuItems.Add(C.Texts.SubjectsList);
        if (Common.configurationController.setupData.LiveStream) menuItems.Add(C.Texts.LiveStream);
        if (Common.configurationController.setupData.MarketPlace) menuItems.Add(C.Texts.MarketPlace);
        if (Common.configurationController.setupData.MarketPlace2021) menuItems.Add(C.Texts.MarketPlace2021);
        if (Common.configurationController.setupData.ExercisedCurriculum) menuItems.Add(C.Texts.ExercisedCurriculum);
        if (Common.configurationController.setupData.LastPlayedCurriculum) menuItems.Add(C.Texts.LastPlayedCurriculums);
        if (Common.configurationController.setupData.MyResult) menuItems.Add(C.Texts.MyResult);
        if (Common.configurationController.setupData.MyRewards) menuItems.Add(C.Texts.MyRewards);
        if (Common.configurationController.setupData.LearnPlay) menuItems.Add(C.Texts.LearnThenPlay); 

        if (menuItems.Count > 0)
            startMenu = menuItems[0];

        mainMenu.Initialize(true, menuItems, ButtonClick);

        subMenu.Initialize(ButtonClick);

        // Meghatározzuk a panelek rejtett és teljesen látható pozícióját
        panelHidePos = -rectTransformCurriculumPlay.rect.height; // Mindegyik panel ugyan akkora
        panelShowPos = 0;

        // Az összes panelt eltüntetjük ha még nincs kiválasztott panel
        if (actVisiblePanel == Panels.empty)
            HidePanelsImmediatelly();

        yield return null;
    }

    /// <summary>
    /// Amikor az új képernyő megjelenítése elkezdődik akkor ezt meghívja a ScreenController 
    /// </summary>
    /// <returns></returns>
    override public IEnumerator ScreenShowStartCoroutine()
    {
        CanvasBorder_16_9.instance.SetBorderColor(Common.MakeColor("#F6F6F6"));

        yield return null;
    }

    override public IEnumerator ScreenShowFinishCoroutine()
    {
        // Lekérdezzük a használható nyelveket és beállítjuk az elsőt
        bool ready = false;

        GetUsableLanguagesData((List<LanguageData> list) =>
        {
            if (list != null)
            {
                //LanguageData languageData = LanguageData.GetLanguageData(list, langCode: Common.configurationController.curriculumLang.langCode);
                LanguageData languageData = LanguageData.GetLanguageData(list, Common.configurationController.curriculumLang.langCode);

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
                    if (list.Count > 0)
                        Common.configurationController.curriculumLang = list[0];
                }

                mainMenu.menuStripe.SetCountryFlag(Common.configurationController.curriculumLang.langFlag);
            }

            ready = true;
        });




        /*
        ClassYServerCommunication.instance.GetUsableLanguages(scope, true,
            (bool success, JSONNode response) => {
                // Válasz feldolgozása
                if (success)
                {
                    List<LanguageData> list = LanguageData.GetListOfLanguageData(response[C.JSONKeys.answer][C.JSONKeys.usableLangCodes]);
                    Common.configurationController.curriculumLangCount = list.Count;

                    LanguageData languageData = LanguageData.GetLanguageData(list, langCode: Common.configurationController.curriculumLang.langCode);

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
                        if (list.Count > 0)
                            Common.configurationController.curriculumLang = list[0];
                    }

                    mainMenu.menuStripe.SetCountryFlag(Common.configurationController.curriculumLang.langFlag);
                }

                // A zászló csak akkor látszik, ha legalább két nyelv van
                mainMenu.menuStripe.SetCountryFlagVisible(Common.configurationController.curriculumLangCount > 1);

                ready = true;
            }
        );
        */

        yield return new WaitUntil(() => ready); // Várakozik amíg a ready nem lesz igaz

        if (actVisiblePanel == Panels.empty)
        {
            ButtonClick(C.Program.MainMenu + ":" + startMenu); //    C.Texts.MainPageGroupBrowser); // CurriculumsPlay); // Alapesetben a tananyag lejátszás gomb az aktív
        }
        else
        {
            Refresh();
        }

        yield return null;
    }


    public void Refresh()
    {
        StartCoroutine(RefreshCoroutine());
    }

    // Újra lekérdezi a szervertől az információkat ha szükséges, az az frissíti a tartalmat az OTPMain képernyőn
    IEnumerator RefreshCoroutine()
    {
        string[] buttonNameSplitted = lastImportantButtonClick.Split(':');

        // Ha utoljára kurzust listáztak ki, akkor ki kell a kurzust is és a témákat is
        bool ready = true;
        if (buttonNameSplitted[0] == C.Program.Course)
        {
            ready = false;
            string[] buttonNameTopicSplitted = buttonNames[2].Split(':');

            if (subMenuEmailGroup == null)
            {
                ClassYServerCommunication.instance.GetCurriculumCourses(buttonNameTopicSplitted[1], scope, true,
                    (bool success, JSONNode response) =>
                    {
                        // Válasz feldolgozása
                        if (success)
                        {
                            subMenu.panelSubMenu.Initialize(true, SubFolderDatas.JsonArrayToList(response[C.JSONKeys.answer][C.JSONKeys.courses]), C.Program.Course, ButtonClick);
                            //subMenu.panelSubMenu.SetSelected(buttonNames[level + 1]);

                            ready = true;
                        }
                    }
                );
            }
            else
            {
                ClassYServerCommunication.instance.GetCoursesByMailList(subMenuEmailGroup.id, buttonNameTopicSplitted[1],
                    (bool success, JSONNode response) =>
                    {
                        // Válasz feldolgozása
                        if (success)
                        {
                            subMenu.panelSubMenu.Initialize(true, SubFolderDatas.JsonArrayToList(response[C.JSONKeys.answer]), C.Program.Course, ButtonClick);
                            //otpPanelSubMenu.panelSubMenu.Initialize(true, FillSubjectList(response, C.JSONKeys.courses), C.Program.Course, ButtonClick);
                            //subMenu.panelSubMenu.SetSelected(buttonNames[level + 1]);
                            ready = true;
                        }
                    }
                );
            }

            yield return new WaitUntil(() => ready); // Várakozik amíg a nem lesz kész
        }

        ButtonClickInside(lastImportantButtonClick);
    }

    // Ha átváltottunk egy másik képernyőre, akkor a tananyag infót el kell tüntetni, 
    // mert az egy másik képernyő
    override public IEnumerator ScreenHideFinish()
    {
        ClassYCurriculumInfo.instance.HideImmediatelly();

        yield break;
    }

    void HidePanelsImmediatelly()
    {
        // A paneleket alaphelyzetbe állítjuk
        rectTransformLiveStream.anchoredPosition = new Vector2(rectTransformLiveStream.anchoredPosition.x, panelHidePos);
        rectTransformSubjectList.anchoredPosition = new Vector2(rectTransformSubjectList.anchoredPosition.x, panelHidePos);
        rectTransformSubMenu.anchoredPosition = new Vector2(rectTransformSubMenu.anchoredPosition.x, panelHidePos);
        rectTransformCurriculumList.anchoredPosition = new Vector2(rectTransformCurriculumList.anchoredPosition.x, panelHidePos);
        rectTransformCurriculumPlay.anchoredPosition = new Vector2(rectTransformCurriculumPlay.anchoredPosition.x, panelHidePos);
        rectTransformMarketPlace.anchoredPosition = new Vector2(rectTransformMarketPlace.anchoredPosition.x, panelHidePos);
        rectTransPlayRoutes.anchoredPosition = new Vector2(rectTransPlayRoutes.anchoredPosition.x, panelHidePos);
        rectTransformMainPageGroupBrowser.anchoredPosition = new Vector2(rectTransformMainPageGroupBrowser.anchoredPosition.x, panelHidePos);
        rectTransformMyGroupsEdit.anchoredPosition = new Vector2(rectTransformMyGroupsEdit.anchoredPosition.x, panelHidePos);
        rectTransformMyRewards.anchoredPosition = new Vector2(rectTransformMyRewards.anchoredPosition.x, panelHidePos);
        rectTransformLearnPlay.anchoredPosition = new Vector2(rectTransformLearnPlay.anchoredPosition.x, panelHidePos);
    }

    void ShowPanel(Panels needPanel)
    {
        userActivityDisabled = true;
        StartCoroutine(ShowPanelCoroutine(needPanel));
    }

    IEnumerator ShowPanelCoroutine(Panels needPanel)
    {
        // Ha a panel már látszik, akkor nem csinálunk semmit
        if (needPanel == actVisiblePanel)
        {
            userActivityDisabled = false;
            yield break;
        }

        // Ha van látható panel, akkor eltüntetjük
        if (actVisiblePanel != Panels.empty)
            yield return StartCoroutine(HidePanelCoroutine());

        // Kikapcsoljuk a LiveStream-et ha esetleg menne
        liveStream.Reject();
        //throw new System.Exception();

        // Az új panel lesz mostantól aktív
        actVisiblePanel = needPanel;

        float showAnimTime = 1f;
        iTween.ValueTo(gameObject, iTween.Hash(
            "from", panelHidePos,
            "to", panelShowPos, // Mindegyik panel ugyan akkora
            "time", showAnimTime,
            "easetype", iTween.EaseType.easeOutCubic,
            "onupdate", "UpdatePanelPos", "onupdatetarget", gameObject,
            "oncomplete", "ShowFinish", "oncompletetarget", gameObject));
    }

    void ShowFinish()
    {
        userActivityDisabled = false;
    }

    void HidePanel(Common.CallBack callBack = null)
    {
        StartCoroutine(HidePanelCoroutine(callBack));
    }

    IEnumerator HidePanelCoroutine(Common.CallBack callBack = null)
    {
        float hideAnimTime = 0.5f;

        // Ha nincs mit eltüntetni, akkor nem csinálunk semmit
        if (actVisiblePanel != Panels.empty)
        {
            float actPanelPos = 0;

            switch (actVisiblePanel)
            {
                case Panels.liveStream: actPanelPos = rectTransformLiveStream.anchoredPosition.y; break;
                case Panels.subjectList: actPanelPos = rectTransformSubjectList.anchoredPosition.y; break;
                case Panels.subMenu: actPanelPos = rectTransformSubMenu.anchoredPosition.y; break;
                case Panels.curriculumList: actPanelPos = rectTransformCurriculumList.anchoredPosition.y; break;
                case Panels.curriculumPlay: actPanelPos = rectTransformCurriculumPlay.anchoredPosition.y; break;
                case Panels.marketPlace: actPanelPos = rectTransformMarketPlace.anchoredPosition.y; break;
                case Panels.playRoutes: actPanelPos = rectTransPlayRoutes.anchoredPosition.y; break;
                case Panels.mainPageGroupBrowser: actPanelPos = rectTransformMainPageGroupBrowser.anchoredPosition.y; break;
                case Panels.myGroupsEdit: actPanelPos = rectTransformMyGroupsEdit.anchoredPosition.y; break;
                case Panels.myRewards: actPanelPos = rectTransformMyRewards.anchoredPosition.y; break;
                case Panels.myLearnPlay: actPanelPos = rectTransformLearnPlay.anchoredPosition.y; break;
            }

            iTween.ValueTo(gameObject, iTween.Hash(
                "from", actPanelPos,
                "to", panelHidePos,
                "time", hideAnimTime,
                "easetype", iTween.EaseType.easeInCubic,
                "onupdate", "UpdatePanelPos", "onupdatetarget", gameObject,
                "oncomplete", "HideFinish", "oncompletetarget", gameObject));

            hideFinish = false;
            yield return new WaitUntil(() => hideFinish);   // Várakozunk amíg a panel teljesen eltűnik
        }

        if (callBack != null)
            callBack();
    }

    void HideFinish()
    {
        hideFinish = true;
    }

    void UpdatePanelPos(float pos)
    {
        switch (actVisiblePanel)
        {
            case Panels.liveStream:
                rectTransformLiveStream.anchoredPosition = new Vector2(rectTransformLiveStream.anchoredPosition.x, pos);
                break;
            case Panels.subjectList:
                rectTransformSubjectList.anchoredPosition = new Vector2(rectTransformSubjectList.anchoredPosition.x, pos);
                break;
            case Panels.subMenu:
                rectTransformSubMenu.anchoredPosition = new Vector2(rectTransformSubMenu.anchoredPosition.x, pos);
                break;
            case Panels.curriculumList:
                rectTransformCurriculumList.anchoredPosition = new Vector2(rectTransformCurriculumList.anchoredPosition.x, pos);
                break;
            case Panels.curriculumPlay:
                rectTransformCurriculumPlay.anchoredPosition = new Vector2(rectTransformCurriculumPlay.anchoredPosition.x, pos);
                break;
            case Panels.marketPlace:
                rectTransformMarketPlace.anchoredPosition = new Vector2(rectTransformMarketPlace.anchoredPosition.x, pos);
                break;
            case Panels.playRoutes:
                rectTransPlayRoutes.anchoredPosition = new Vector2(rectTransPlayRoutes.anchoredPosition.x, pos);
                break;
            case Panels.mainPageGroupBrowser:
                rectTransformMainPageGroupBrowser.anchoredPosition = new Vector2(rectTransformMainPageGroupBrowser.anchoredPosition.x, pos);
                break;
            case Panels.myGroupsEdit:
                rectTransformMyGroupsEdit.anchoredPosition = new Vector2(rectTransformMyGroupsEdit.anchoredPosition.x, pos);
                break;
            case Panels.myRewards:
                rectTransformMyRewards.anchoredPosition = new Vector2(rectTransformMyRewards.anchoredPosition.x, pos);
                break;
            case Panels.myLearnPlay:
                rectTransformLearnPlay.anchoredPosition = new Vector2(rectTransformLearnPlay.anchoredPosition.x, pos);
                break;
        }
    }

    public void ButtonClick(string buttonName)
    {
        // Ellenörízzük, hogy a felhasználói interactivitás engedélyezve van-e
        if (userActivityDisabled)
            return;

        ButtonClickInside(buttonName);
    }

    void ButtonClickInside(string buttonName)
    {
        Debug.Log(buttonName);

        string[] buttonNameSplitted = buttonName.Split(':');

        switch (buttonNameSplitted[0])
        {
            // ***********************************************************************************
            //                                Főmenü eseményei
            // ***********************************************************************************
            case C.Program.MainMenu: // A fűmenüben nyomtak meg egy gombot
                mainMenu.SetSelected(buttonName); // A megnyomott gombot megjelöljük

                switch (buttonNameSplitted[1])
                {
                    case C.Texts.CurriculumsPlay: // A főmenüben kiválasztották az útvonal lejátszását
                        lastImportantButtonClick = buttonName;

                        // Bekapcsoljuk az útvonal választó panelt
                        ShowPanel(Panels.curriculumPlay);

                        Common.CallBack_In_Bool_JSONNode responseProcessor = (bool success, JSONNode response) =>
                        {
                            // Válasz feldolgozása
                            if (success)
                            {
                                listOfCurriculumPath = CurriculumPathData.JsonArrayToList(response[C.JSONKeys.answer]);
                                curriculumPlay.Initialize(listOfCurriculumPath, C.Program.CurriculumPath, ButtonClick);
                            }
                        };

                        if (Common.configurationController.isServer2020) //  Common.configurationController.link == ConfigurationController.Link.Server2020Link)
                            ClassYServerCommunication.instance.GetPlayableLearnRoutePathList("-1", responseProcessor);
                        //ClassYServerCommunication.instance.CreateFatalError();

                        else
                            ClassYServerCommunication.instance.GetLearnRoutePathForPlay(responseProcessor);

                        //ClassYServerCommunication.instance.GetPlayableLearnRoutePathList(
                        //    (bool success, JSONNode response) => 
                        //    {
                        //        // Válasz feldolgozása
                        //        if (success)
                        //        {
                        //            listOfCurriculumPath = CurriculumPathData.JsonArrayToList(response[C.JSONKeys.answer]);
                        //            curriculumPlay.Initialize(listOfCurriculumPath, C.Program.CurriculumPath, ButtonClick);
                        //        }
                        //    }
                        //);

                        //ClassYServerCommunication.instance.GetLearnRoutePathForPlay(
                        //    (bool success, JSONNode response) => 
                        //    {
                        //        // Válasz feldolgozása
                        //        if (success)
                        //        {
                        //            listOfCurriculumPath = CurriculumPathData.JsonArrayToList(response[C.JSONKeys.answer]);
                        //            curriculumPlay.Initialize(listOfCurriculumPath, C.Program.CurriculumPath, ButtonClick);
                        //        }
                        //    }
                        //);
                        break;

                    case C.Texts.LiveStream: // A főmenüben kiválasztották az élő stream-et
                        lastImportantButtonClick = "";

                        // Bekapcsoljuk a LiveStream panelt
                        ShowPanel(Panels.liveStream);

                        ClassYServerCommunication.instance.GetStreamData(
                            (bool success, JSONNode response) =>
                            {
                                // Válasz feldolgozása
                                if (success)
                                {
                                    liveStream.Initialize(response);
                                }
                            }
                        );

                        break;

                    case C.Texts.MarketPlace:
                        lastImportantButtonClick = buttonName;

                        // Bekapcsoljuk a Piactér panelt
                        ShowPanel(Panels.marketPlace);
                        marketPlace.PreInitialize();

                        ClassYServerCommunication.instance.getMarketModuleItems(true,
                            (bool success, JSONNode response) =>
                            {
                                // Válasz feldolgozása
                                if (success)
                                {
                                    marketPlace.Initialize(response[C.JSONKeys.answer]);
                                }
                            }
                        );

                        break;

                    case C.Texts.MarketPlace2021:
                        ClassYServerCommunication.instance.getMarketModuleItems(true,
                            (bool success, JSONNode response) =>
                            {
                                // Válasz feldolgozása
                                if (success)
                                {
                                    marketPlaceData = response[C.JSONKeys.answer];
                                    Common.screenController.ChangeScreen(C.Screens.MarketPlace2021);
                                }
                            }
                        );

                        break;

                    case C.Texts.SubjectsList: // A főmenüben kiválasztották a tantárgyak listázását
                        lastImportantButtonClick = buttonName;

                        ShowPanel(Panels.subMenu);

                        level = 0;
                        buttonNames[level] = buttonName;

                        subMenuEmailGroup = null;
                        ReFreshSubMenu();
                        break;

                    case C.Texts.ExercisedCurriculum: // A főmenüben kiválasztották a gyakorolandó tananyagokat
                        lastImportantButtonClick = buttonName;

                        ShowPanel(Panels.curriculumList);

                        ClassYServerCommunication.instance.GetUnderXPercent(
                            (bool success, JSONNode response) =>
                            {
                                // Válasz feldolgozása
                                if (success)
                                {
                                    listOfCurriculums = CurriculumItemDriveData.JsonArrayToList(response[C.JSONKeys.answer][C.JSONKeys.curriculums]); //[C.JSONKeys.curriculums]);
                                    curriculumList.Initialize(listOfCurriculums, C.Program.GetUnderXPercentCurriculum, ButtonClick);
                                }
                            }
                        );
                        break;

                    case C.Texts.LastPlayedCurriculums: // A főmenüben kiválasztották a legutóbb lejátszott tananyagokat
                        lastImportantButtonClick = buttonName;

                        ShowPanel(Panels.curriculumList);

                        ClassYServerCommunication.instance.GetLastX(
                            (bool success, JSONNode response) =>
                            {
                                // Válasz feldolgozása
                                if (success)
                                {
                                    listOfCurriculums = CurriculumItemDriveData.JsonArrayToList(response[C.JSONKeys.answer][C.JSONKeys.curriculums]); //[C.JSONKeys.curriculums]);
                                    curriculumList.Initialize(listOfCurriculums, C.Program.GetLastXCurriculum, ButtonClick);
                                }
                            }
                        );
                        break;

                    case C.Texts.MainPageGroupBrowser: // A főmenüben kiválasztották a főoldal csoport böngészést
                        lastImportantButtonClick = buttonName;

                        ShowPanel(Panels.mainPageGroupBrowser);

                        mainPageGroupBrowser.Initialize(C.Program.MainPageGroupBrowser, ButtonClick);

                        // ezt mostantól az initialize metódus csinálja
                        /*
                        ClassYServerCommunication.instance.appMainPageGroupBrowser( // getMyGroupsEmailLists(
                            (bool success, JSONNode response) =>
                            {
                                // Válasz feldolgozása
                                if (success)
                                {
                                    emailGroupLists = new EmailGroupLists(response[C.JSONKeys.answer]);
                                    mainPageGroupBrowser.Initialize(emailGroupLists, C.Program.MainPageGroupBrowser, ButtonClick);
                                }
                            }
                        );
                        */
                        break;

                    case C.Texts.PlayRoutes: // A főmenüben kiválasztották az útvonalak lejátszását
                        lastImportantButtonClick = buttonName;

                        ShowPanel(Panels.playRoutes);

                        playRoutes.Initialize(C.Program.PlayRoutes, buttonNameSplitted.Length > 2 ? buttonNameSplitted[2] : "", ButtonClick);



                        /*
                        ClassYServerCommunication.instance.getPlayRoutes(
                            (bool success, JSONNode response) =>
                            {
                                // Válasz feldolgozása
                                if (success)
                                {
                                    //emailGroupLists = new EmailGroupLists(response[C.JSONKeys.answer]);
                                    //playRoutes.InitializeEmailGroupButtons(emailGroupLists.list[0], C.Program.PlayRoutes, ButtonClick);
                                    playRoutes.InitializeEmailGroupButtons(new EmailGroupLists(response[C.JSONKeys.answer]).list[0], C.Program.PlayRoutes, ButtonClick);
                                    playRoutes.SetEmailGroup(buttonNameSplitted.Length > 2 ? buttonNameSplitted[2] : "");
                                }
                            }
                        );
                        */

                        break;

                    case C.Texts.MyGroupsEdit: // A főmenüben kiválasztották a csoportjaim szerkesztését
                        lastImportantButtonClick = buttonName;

                        ShowPanel(Panels.myGroupsEdit);

                        myGroupsEdit.Initialize(C.Program.MyGroupsEdit, ButtonClick);

                        // ezt mostantól az initialize metódus csinálja
                        /*
                        ClassYServerCommunication.instance.appMyGroupsEdit( // getMyGroupEditEmailLists(
                            (bool success, JSONNode response) =>
                            {
                                // Válasz feldolgozása
                                if (success)
                                {
                                    emailGroupLists = new EmailGroupLists(response[C.JSONKeys.answer]);
                                    myGroupsEdit.Initialize(emailGroupLists, C.Program.MyGroupsEdit, ButtonClick);
                                }
                            }
                        );
                        */
                        break;
                    case C.Texts.LearnThenPlay:
                        lastImportantButtonClick = buttonName;
                        Debug.Log("Learn and Play Clicked!!!!!!!!!!!!!!!!");
                        ShowPanel(Panels.myLearnPlay);

                        LThPTextandButton.SetActive(false); //Ezzel kikapcsolom

                        ClassYServerCommunication.instance.getFamilyConnections((bool success, JSONNode response) =>
                        {
                            if (string.IsNullOrEmpty(response[C.JSONKeys.answer][C.JSONKeys.parents][0][C.JSONKeys.unlockPin]) == false)
                            {
                                LThPTextandButton.SetActive(true);
                            }
                        });
                        break;

                    case C.Texts.MyResult:

                        //Common.screenController.ChangeScreen(C.Screens.CastleGameSelectCharacters);
                        //Common.screenController.ChangeScreen(C.Screens.CastleGameRoom);

                        //CastleGameRoomScreen.Load(null, () => { Common.screenController.ChangeScreen(C.Screens.OTPMain); } );

                        //CastleGameLevelDownScreen.Load(null, null, () => { Common.screenController.ChangeScreen(C.Screens.OTPMain); } );

                        //CastleGameLevelUpScreen.Load(null, null, () => { Common.screenController.ChangeScreen(C.Screens.OTPMain); } );

                        //CastleGameInstructionScreen.Load(null, null);

                        //CastleGameFinishScreen.Load(null, () => { Common.screenController.ChangeScreen(C.Screens.OTPMain); } );

                        //Common.screenController.ChangeScreen(C.Screens.EmptyScreen);


                        CastleGameInventoryScreen.DownloadInventoryData(
                            (bool success) =>
                            {
                                if (success)
                                {
                                    CastleGameInventoryScreen.Load(
                                        () => { Common.screenController.ChangeScreen(C.Screens.OTPMain); }
                                    );
                                }

                                //CastleGameInventoryScreen.instance.Show(() =>
                                //{
                                //    CastleGameInventory.instance.Hide();
                                //});
                            }
                        );



                        break;

                    case C.Texts.MyRewards:
                        lastImportantButtonClick = buttonName;

                        ShowPanel(Panels.myRewards);

                        myRewards.Initialize(127);
                        //Common.screenController.ChangeScreen(C.Screens.Ball_Game);

                        ClassYServerCommunication.instance.GetUserBonusCoins(
                            (bool success, JSONNode response) =>
                            {
                                // Válasz feldolgozása
                                if (success)
                                {

                                }
                            }
                        );

                        break;

                }

                break;

            // ***********************************************************************************
            //                        Főmenü baloldali menüsáv eseményei
            // ***********************************************************************************
            case "Menu": // A főmenüben rákattintottak a menüre (három csík)
                Debug.Log("Ok");

                if (Common.configurationController.sideMenu)
                {
                    // Megjelenítjük az oldal menüt
                    SideMenu.instance.Show(gameObject);
                }
                else
                {
                    ErrorPanel.instance.Show(Common.languageController.Translate(C.Texts.ExitConfirmation), Common.languageController.Translate(C.Texts.Ok), button2Text: Common.languageController.Translate(C.Texts.Cancel), callBack: (string bName) =>
                    {
                        ErrorPanel.instance.Hide(() =>
                        {
                            if (bName == "button1")
                            {
                                // ha kilépünk az OTPMain képernyőből a bejelentkező képernyőre, akkor 
                                ClassYServerCommunication.instance.sessionToken = "";
                                Common.configurationController.Save(); // Elmentjük, hogy töröltük a session tokent
                                Common.screenController.LoadScreenAfterIntro();
                            }
                        });
                    });
                }



                break;

            case "QuestionMark": // A főmenüben rákattintottak a kérdőjelre

                ErrorPanel.instance.Show("Biztosan törli a felhasználóhoz tartozó eredményeket?", "Igen", button2Text: "Mégsem", callBack: (string bName) =>
                {
                    if (bName == "button1")
                        ClassYServerCommunication.instance.EraseUserLog();

                    ErrorPanel.instance.Hide();
                });

                break;

            case "Flag": // A főmenüben rákattintottak a zászlóra
                // Kilistázzuk a használható nyelveket
                GetUsableLanguagesData((List<LanguageData> list) =>
                {
                    if (list != null)
                    {
                        listOfLanguageData = list;
                        ClassYLanguageSelector.instance.Initialize(listOfLanguageData, C.Program.LanguageSelector);
                        ClassYLanguageSelector.instance.Show(Common.configurationController.curriculumLang.langCode, C.Texts.SelectCurriculumLanguage, ButtonClick);
                    }
                });

                /*
                ClassYServerCommunication.instance.GetUsableLanguages(scope, true,
                    (bool success, JSONNode response) => {
                        // Válasz feldolgozása
                        if (success)
                        {
                            listOfLanguageData = LanguageData.GetListOfLanguageData(response[C.JSONKeys.answer][C.JSONKeys.usableLangCodes]);
                            Common.configurationController.curriculumLangCount = listOfLanguageData.Count;
                            mainMenu.menuStripe.SetCountryFlagVisible(Common.configurationController.curriculumLangCount > 1);



                            ClassYLanguageSelector.instance.Initialize(listOfLanguageData, C.Program.LanguageSelector);

                            //ClassYLanguageSelector.instance.Initialize(response, C.Program.LanguageSelector);
                            ClassYLanguageSelector.instance.Show(Common.configurationController.curriculumLang.langCode, C.Texts.SelectCurriculumLanguage, ButtonClick);
                        }
                    }
                );
                */

                break;

            case C.Program.LanguageSelector: // A nyelvválasztó panelen kiválasztottak egy zászlót
                // Kiválasztottak egy nyelvet
                // Ha más nyelvet választottak mint korábban, csak akkor csinálunk valamit
                if (Common.configurationController.curriculumLang.langCode != buttonNameSplitted[1])
                {
                    mainMenu.menuStripe.SetCountryFlag(ClassYLanguageSelector.instance.GetFlag(buttonNameSplitted[1]));

                    //Common.configurationController.curriculumLang = LanguageData.GetLanguageData(listOfLanguageData, langCode: buttonNameSplitted[1]);
                    Common.configurationController.curriculumLang = LanguageData.GetLanguageData(listOfLanguageData, buttonNameSplitted[1]);

                    // Ha a zászló választás előtt a tananyagot listáztuk ki, akkor újra a tananyagot kell, nem számít, hogy korábban már milyen mélyen is mentünk bele
                    string[] lastImportantButtonNameSplitted = lastImportantButtonClick.Split(':');

                    if (lastImportantButtonNameSplitted[0] == C.Program.Subject ||
                        lastImportantButtonNameSplitted[0] == C.Program.Topic ||
                        lastImportantButtonNameSplitted[0] == C.Program.Course)
                    {
                        lastImportantButtonClick = C.Program.MainMenu + ":" + C.Texts.SubjectsList;
                    }

                    Refresh();
                }

                ClassYLanguageSelector.instance.Hide();

                break;

            case C.Program.FadeClickLanguageSelector: // A fade panelre kattintottak, mikor a nyelvválasztó panel volt látható
                // Eltüntetjük a felbukkanó ablakot
                ClassYLanguageSelector.instance.Hide();
                break;

            // ***********************************************************************************
            //                            Tanulási útvonal eseményei
            // ***********************************************************************************
            case C.Program.CurriculumPath: // A tanfolyam útvonalak közül kiválasztottak egyet
                // A tanulási útvanal adatait megszerezni
                CurriculumPathData curriculumPathData = listOfCurriculumPath[System.Int32.Parse(buttonNameSplitted[1])];

                Common.CallBack end = () =>
                {
                    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                };

                if (Common.configurationController.isServer2020) // Common.configurationController.link == ConfigurationController.Link.Server2020Link)
                    ServerPlay.instance.PlayLearnPathByNextGameMode(curriculumPathData, end);
                else
                    ServerPlay.instance.PlayLearnPath(curriculumPathData, end);


                //ServerPlay.instance.PlayLearnPath(curriculumPathData, () => {
                //    // Vissza megyünk az OTP Main képernyőre
                //    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                //
                //    // Frissíteni az adatokat
                //    //Refresh();
                //    //ButtonClickInside(C.Program.MainMenu + ":0");
                //});


                /*
                // Van-e mit folytatni
                if (curriculumPathData.progress < 100)
                {
                    ServerPlay.instance.PlayLearnPath(curriculumPathData, () => {
                        // Vissza megyünk az OTP Main képernyőre
                        Common.screenController.ChangeScreen(C.Screens.OTPMain);

                        // Frissíteni az adatokat
                        //Refresh();
                        //ButtonClickInside(C.Program.MainMenu + ":0");
                    });
                }
                else {

                    // A tanfolyam útvonal már teljesítve van
                    ErrorPanel.instance.Show(
                        Common.languageController.Translate(C.Texts.ThisLearnRouteIsFinished),
                        Common.languageController.Translate(C.Texts.restart),
                        button2Text: Common.languageController.Translate(C.Texts.Ok),
                        callBack: (string buttonName_) =>
                        {
                            if (buttonName_ == "button1")
                                ServerPlay.instance.PlayLearnPath(curriculumPathData, () => {
                                    // Vissza megyünk az OTP Main képernyőre
                                    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                                });
                        });

                    
                    ErrorPanel.instance.Show(Common.languageController.Translate(C.Texts.ThisLearnRouteIsFinished), Common.languageController.Translate(C.Texts.Ok), callBack: (string bName) =>
                    {
                        ErrorPanel.instance.Hide();
                    });
                    
                }*/
                break;

            // ***********************************************************************************
            //                             Tantárgy lista eseményei
            // ***********************************************************************************
            case C.Program.Subject: // Az almenü tantárgyak listájából kiválaszotottak egyet
            case C.Program.Topic: // Az almenü témák listájából kiválasztottak egyet
                lastImportantButtonClick = buttonName;

                if (buttonNameSplitted[0] == C.Program.Subject)
                {
                    subjectId = buttonNameSplitted[1];
                    level = 1;
                }
                else
                {
                    topicId = buttonNameSplitted[1];
                    level = 2;
                }

                buttonNames[level] = buttonName;

                subMenu.panelSubMenu.SetSelected(buttonNames[level]);

                ReFreshSubMenu();
                break;

            case C.Program.Course: // Az almenü kurzusok listájából kiválasztottak egyet
                lastImportantButtonClick = buttonName;

                // A kiválasztott kurzus azonosítóját elmentjük későbbi felhasználásra (PlayGames)
                courseId = buttonNameSplitted[1];

                // Kiválasztottá tesszük a megnyomott gombot
                subMenu.panelSubMenu.SetSelected(buttonName);

                // Kilistázzuk a kurzus tananyagjait
                if (subMenuEmailGroup == null)
                {
                    ClassYServerCommunication.instance.GetCurriculums(buttonNameSplitted[1], scope, true,
                        (bool success, JSONNode response) =>
                        {
                            // Válasz feldolgozása
                            if (success)
                            {
                                listOfCurriculums = CurriculumItemDriveData.JsonArrayToList(response[C.JSONKeys.answer][C.JSONKeys.curriculums]);
                                subMenu.DrawCurriculumItems(listOfCurriculums);
                                //otpPanelSubMenu.DrawCurriculumItems(FillCurriculumList(response));
                                //scroolViewCurriculumListDrive.Initialize(FillCurriculumList(response), ButtonClick);
                            }
                        }
                    );
                }
                else
                {
                    ClassYServerCommunication.instance.GetCurriculumsByMailList(subMenuEmailGroup.id, buttonNameSplitted[1],
                        (bool success, JSONNode response) =>
                        {
                            // Válasz feldolgozása
                            if (success)
                            {
                                listOfCurriculums = CurriculumItemDriveData.JsonArrayToList(response[C.JSONKeys.answer]);
                                subMenu.DrawCurriculumItems(listOfCurriculums);
                            }
                        }
                    );
                }

                break;

            case C.Program.Curriculum: // A tananyagok listájából kiválasztottak egyet
                // A kiválasztott tananyag indexét elmentjük későbbi felhasználásra
                curriculumIndex = System.Int32.Parse(buttonNameSplitted[1]);


                // Egyből lejátszuk
                curriculumId = listOfCurriculums[curriculumIndex].curriculumID;

                if (Common.configurationController.isServer2020)
                {
                    ServerPlay.instance.PlayCurriculumServer2020(subjectId, topicId, courseId, listOfCurriculums[curriculumIndex], () =>
                    {
                        // Vissza megyünk az OTP Main képernyőre
                        Common.screenController.ChangeScreen(C.Screens.OTPMain);
                    });
                }
                else
                {
                    ServerPlay.instance.PlayCurriculum("NULL", "NULL", subjectId, topicId, courseId, listOfCurriculums[curriculumIndex].curriculumID, () =>
                    {
                        // Vissza megyünk az OTP Main képernyőre
                        Common.screenController.ChangeScreen(C.Screens.OTPMain);
                    });
                }

                // Nem jelenítjük meg, hanem azonnal elindítjuk
                // A tananyag adatait megjelenítjük egy felbukkanó ablakban
                /*
                ClassYCurriculumInfo.instance.Initialize(listOfCurriculums[curriculumIndex], ButtonClick);
                ClassYCurriculumInfo.instance.Show();
                */
                break;

            case "Back": // Az almenüben megnyomták a vissza gombot, visszalépünk egy szinttel
                level--;
                if (level < 0)
                    ButtonClickInside(C.Program.MainMenu + ":" + C.Texts.PlayRoutes);
                else
                    ReFreshSubMenu();
                break;

            // Felbukkanó ablakon található megosztás gombok
            case C.Texts.ShareWithStudentsForLearn: // A tananyag infó panelen rákattintottak a megosztás diákoknak tanulásra gombra
                Debug.Log("Ok");

                break;

            case C.Texts.ShareInCommonWork: // A tananyag infó panelen rákattintottak a megosztás közös munkára gombra
                Debug.Log("Ok");

                break;

            case C.Texts.ShareOnFacebook: // A tananyag infó panelen rákattintottak a megosztás facebookon gombra
                Debug.Log("Ok");

                break;

            case C.Texts.ShareOnClassYStore: // A tananyag infó panelen rákattintottak a megosztás classY storeba gombra
                Debug.Log("Ok");

                break;

            case "Play": // A tananyag infó panelet rákattintottak a lejátszásra

                // *** Igazából ez már nincs, mivel a tananyagot azonnal elindítjuk

                curriculumId = listOfCurriculums[curriculumIndex].curriculumID;
                ServerPlay.instance.PlayCurriculum("NULL", "NULL", subjectId, topicId, courseId, listOfCurriculums[curriculumIndex].curriculumID, () =>
                {
                    // Vissza megyünk az OTP Main képernyőre
                    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                });
                break;

            case "FadeClick": // A fade panelre kattintottak, mikor a tananyag infó panel volt látható
                // Eltüntetjük a felbukkanó ablakot
                ClassYCurriculumInfo.instance.Hide();
                break;

            // ***********************************************************************************
            //                        Gyakorolandó tananyag lista eseményei
            // ***********************************************************************************
            case C.Program.GetUnderXPercentCurriculum:
                CurriculumItemDriveData curriculumItemDriveDataUnderXPercent = CurriculumItemDriveData.GetItemByCurriculumID(listOfCurriculums, buttonNameSplitted[1]);

                ServerPlay.instance.PlayCurriculum(
                    curriculumItemDriveDataUnderXPercent.learnRoutePathID,
                    curriculumItemDriveDataUnderXPercent.learnRoutePathStart,
                    curriculumItemDriveDataUnderXPercent.subjectID,
                    curriculumItemDriveDataUnderXPercent.topicID,
                    curriculumItemDriveDataUnderXPercent.courseID,
                    curriculumItemDriveDataUnderXPercent.curriculumID,
                    () =>
                    {
                        // Vissza megyünk az OTP Main képernyőre
                        Common.screenController.ChangeScreen(C.Screens.OTPMain);

                        // Frissíteni az adatokat
                        //ButtonClickInside(C.Program.MainMenu + ":3");
                    }
                );

                break;

            // ***********************************************************************************
            //                     Utolsó X lejátszott tananyag lista eseményei
            // ***********************************************************************************
            case C.Program.GetLastXCurriculum:
                CurriculumItemDriveData curriculumItemDriveDataLastX = CurriculumItemDriveData.GetItemByCurriculumID(listOfCurriculums, buttonNameSplitted[1]);

                ServerPlay.instance.PlayCurriculum(
                    curriculumItemDriveDataLastX.learnRoutePathID,
                    curriculumItemDriveDataLastX.learnRoutePathStart,
                    curriculumItemDriveDataLastX.subjectID,
                    curriculumItemDriveDataLastX.topicID,
                    curriculumItemDriveDataLastX.courseID,
                    curriculumItemDriveDataLastX.curriculumID,
                    () =>
                    {
                        // Vissza megyünk az OTP Main képernyőre
                        Common.screenController.ChangeScreen(C.Screens.OTPMain);

                        // Frissíteni az adatokat
                        //ButtonClickInside(C.Program.MainMenu + ":4");
                    }
                );
                break;

            // ***********************************************************************************
            //                           Email csoport listák eseményei
            // ***********************************************************************************
            case C.Program.MainPageGroupBrowser:
            case C.Program.MyGroupsEdit:

                // MainPageGroupBrowser:1:54:Dots
                // MainPageGroupBrowser:1:54:Default
                EmailGroup emailGroup = null; // = emailGroupLists.GetEmailGroup(Int32.Parse(buttonNameSplitted[1]), buttonNameSplitted[2]);

                switch (buttonNameSplitted[0])
                {
                    case C.Program.MainPageGroupBrowser:
                        emailGroup = mainPageGroupBrowser.emailGroupLists.GetEmailGroup(Int32.Parse(buttonNameSplitted[1]), buttonNameSplitted[2]);
                        break;

                    case C.Program.MyGroupsEdit:
                        emailGroup = myGroupsEdit.emailGroupLists.GetEmailGroup(Int32.Parse(buttonNameSplitted[1]), buttonNameSplitted[2]);
                        break;
                }

                switch (buttonNameSplitted[3])
                {
                    case C.Program.Play:
                        OTPPanelPlayRoutes.instance.requestedLanguage = emailGroup.languageID;
                        ButtonClickInside(C.Program.MainMenu + ":" + C.Texts.PlayRoutes + ":" + buttonNameSplitted[2]);
                        break;

                    case C.Program.Default:
                        OTPEmailGroupJumpingPanel.instance.DefaultButtonClick(emailGroup, (bool success) =>
                            {
                                if (success)
                                    Refresh();
                            });
                        break;

                    case C.Program.Dots:
                        OTPEmailGroupJumpingPanel.instance.Show(emailGroup, (string buttonN) =>
                        {

                            Debug.Log(buttonN);

                            OTPEmailGroupJumpingPanel.instance.Hide(() =>
                            {
                                if (buttonN == C.Program.Play)
                                {
                                    OTPPanelPlayRoutes.instance.requestedLanguage = emailGroup.languageID;
                                    //OTPPanelPlayRoutes.instance.requestedLanguage = OTPEmailGroupJumpingPanel.instance.emailGroup.ownerLanguageID;
                                    ButtonClickInside(C.Program.MainMenu + ":" + C.Texts.PlayRoutes + ":" + buttonNameSplitted[2]); // Csoport azonosító van benne
                                }
                                else
                                    Refresh();
                            });
                        });
                        break;
                }

                break;

            // ***********************************************************************************
            //                      Tanulási útvonal lejátszásának eseményei
            // ***********************************************************************************
            case C.Program.PlayRoutes:
                // PlayRoutes:mailListID:54     - emailGroupID - 
                // PlayRoutes:Play:232          - útvonal azonosító
                // PlayRoutes:BrowseCurriculum  - A kiválasztott email csoport tananyagjainak böngészéséhez
                // PlayRoutes:Dots              - A kiválasztott email csoport adatainak megjelenítéséhez
                // PlayRoutes:Flag              - Útvonalak nyelvének szűrése

                switch (buttonNameSplitted[1])
                {
                    case C.JSONKeys.mailListID:
                        playRoutes.SetEmailGroup(buttonNameSplitted[2]);
                        break;
                    case C.Program.Play:
                        // A tanulási útvanal adatait megszerezni
                        CurriculumPathData currPathData = playRoutes.GetCurriculumPathData(buttonNameSplitted[2]);

                        Common.CallBack end2 = () =>
                        {
                            Common.screenController.ChangeScreen(C.Screens.OTPMain);
                        };

                        if (Common.configurationController.isServer2020) // Common.configurationController.link == ConfigurationController.Link.Server2020Link)
                            ServerPlay.instance.PlayLearnPathByNextGameMode(currPathData, end2);
                        else
                            ServerPlay.instance.PlayLearnPath(currPathData, end2);

                        break;
                    case C.Program.Dots:
                        OTPEmailGroupJumpingPanel.instance.Show(playRoutes.GetActEmailGroup(), (string buttonN) =>
                        {

                            Debug.Log(buttonN);

                            OTPEmailGroupJumpingPanel.instance.Hide(() =>
                            {
                                /*
                                Elvileg nem lehet Play-re kattintani, ha az útvonal lejátszásnál nyitjuk meg az email csoport adatait
                                if (buttonN == C.Program.Play)
                                {
                                    OTPPanelPlayRoutes.instance.requestedLanguage = playRoutes.GetActEmailGroup();
                                    OTPPanelPlayRoutes.instance.requestedLanguage = OTPEmailGroupJumpingPanel.instance.emailGroup.ownerLanguageID;
                                    ButtonClickInside(C.Program.MainMenu + ":" + C.Texts.PlayRoutes + ":" + buttonNameSplitted[2]); // Csoport azonosító van benne
                                }
                                else*/
                                Refresh();
                            });
                        },
                        oneShoot: true,
                        playEnabled: false
                        );

                        break;
                    case C.Program.BrowseCurriculum:
                        lastImportantButtonClick = buttonName;

                        ShowPanel(Panels.subMenu);

                        level = 0;
                        buttonNames[level] = buttonName;

                        subMenuEmailGroup = playRoutes.GetActEmailGroup();
                        ReFreshSubMenu();

                        //ButtonClickInside(C.Texts.SubjectsList);
                        break;
                    case C.Program.Flag:
                        break;
                }

                break;
        }
    }

    void GetUsableLanguagesData(Common.CallBack_In_ListLanguageData callBack)
    {
        ClassYServerCommunication.instance.GetUsableLanguages(scope, true,
            (bool success, JSONNode response) =>
            {
                // Válasz feldolgozása
                List<LanguageData> list = null;

                if (success)
                {
                    list = LanguageData.GetListOfLanguageData(response[C.JSONKeys.answer][C.JSONKeys.usableLangCodes]);
                    Common.configurationController.curriculumLangCount = list.Count;
                }

                // A zászló csak akkor látszik, ha legalább két nyelv van
                mainMenu.menuStripe.SetCountryFlagVisible(Common.configurationController.curriculumLangCount > 1);

                callBack(list);
            }
        );
    }

    void ReFreshSubMenu()
    {
        string[] buttonNameSplitted = buttonNames[level].Split(':');

        if (subMenuEmailGroup == null)
        {
            switch (buttonNameSplitted[0])
            {
                case C.Program.MainMenu: // A főmenüre kattintottak
                    switch (buttonNameSplitted[1])
                    {
                        case C.Texts.SubjectsList: // Kilistázzuk a velünk megosztott tananyagok tantárgyait
                            ClassYServerCommunication.instance.GetCurriculumSubjects(scope, true,
                                (bool success, JSONNode response) =>
                                {
                                    // Válasz feldolgozása
                                    if (success)
                                    {
                                        subMenu.panelSubMenu.Initialize(false, SubFolderDatas.JsonArrayToList(response[C.JSONKeys.answer][C.JSONKeys.subjects]), C.Program.Subject, ButtonClick);
                                        //otpPanelSubMenu.panelSubMenu.Initialize(false, FillSubjectList(response, C.JSONKeys.subjects), C.Program.Subject, ButtonClick);
                                        subMenu.panelSubMenu.SetSelected(buttonNames[level + 1]);
                                    }
                                }
                            );
                            break;
                    }
                    break;

                case C.Program.Subject: // Egy tantárgyra kattintottak 
                    ClassYServerCommunication.instance.GetCurriculumTopics(buttonNameSplitted[1], scope, true,
                        (bool success, JSONNode response) =>
                        {
                            // Válasz feldolgozása
                            if (success)
                            {
                                subMenu.panelSubMenu.Initialize(true, SubFolderDatas.JsonArrayToList(response[C.JSONKeys.answer][C.JSONKeys.topics]), C.Program.Topic, ButtonClick);
                                //otpPanelSubMenu.panelSubMenu.Initialize(true, FillSubjectList(response, C.JSONKeys.topics), C.Program.Topic, ButtonClick);
                                subMenu.panelSubMenu.SetSelected(buttonNames[level + 1]);
                            }
                        }
                    );
                    break;

                case C.Program.Topic: // Egy téma nevére kattintottak
                    ClassYServerCommunication.instance.GetCurriculumCourses(buttonNameSplitted[1], scope, true,
                        (bool success, JSONNode response) =>
                        {
                            // Válasz feldolgozása
                            if (success)
                            {
                                subMenu.panelSubMenu.Initialize(true, SubFolderDatas.JsonArrayToList(response[C.JSONKeys.answer][C.JSONKeys.courses]), C.Program.Course, ButtonClick);
                                //otpPanelSubMenu.panelSubMenu.Initialize(true, FillSubjectList(response, C.JSONKeys.courses), C.Program.Course, ButtonClick);
                                subMenu.panelSubMenu.SetSelected(buttonNames[level + 1]);
                            }
                        }
                    );
                    break;
            }
        }
        else
        {
            switch (buttonNameSplitted[0])
            {
                case C.Program.PlayRoutes: // A főmenüre kattintottak
                    switch (buttonNameSplitted[1])
                    {
                        case C.Program.BrowseCurriculum: // Kilistázzuk a velünk megosztott tananyagok tantárgyait
                            ClassYServerCommunication.instance.GetSubjectsByMailList(subMenuEmailGroup.id,
                                (bool success, JSONNode response) =>
                                {
                                    // Válasz feldolgozása
                                    if (success)
                                    {
                                        subMenu.panelSubMenu.Initialize(true, SubFolderDatas.JsonArrayToList(response[C.JSONKeys.answer]), C.Program.Subject, ButtonClick);
                                        //otpPanelSubMenu.panelSubMenu.Initialize(false, FillSubjectList(response, C.JSONKeys.subjects), C.Program.Subject, ButtonClick);
                                        subMenu.panelSubMenu.SetSelected(buttonNames[level + 1]);
                                    }
                                }
                            );
                            break;
                    }
                    break;

                case C.Program.Subject: // Egy tantárgyra kattintottak 
                    ClassYServerCommunication.instance.GetTopicsByMailList(subMenuEmailGroup.id, buttonNameSplitted[1],
                        (bool success, JSONNode response) =>
                        {
                            // Válasz feldolgozása
                            if (success)
                            {
                                subMenu.panelSubMenu.Initialize(true, SubFolderDatas.JsonArrayToList(response[C.JSONKeys.answer]), C.Program.Topic, ButtonClick);
                                //otpPanelSubMenu.panelSubMenu.Initialize(true, FillSubjectList(response, C.JSONKeys.topics), C.Program.Topic, ButtonClick);
                                subMenu.panelSubMenu.SetSelected(buttonNames[level + 1]);
                            }
                        }
                    );
                    break;

                case C.Program.Topic: // Egy téma nevére kattintottak
                    ClassYServerCommunication.instance.GetCoursesByMailList(subMenuEmailGroup.id, buttonNameSplitted[1],
                        (bool success, JSONNode response) =>
                        {
                            // Válasz feldolgozása
                            if (success)
                            {
                                subMenu.panelSubMenu.Initialize(true, SubFolderDatas.JsonArrayToList(response[C.JSONKeys.answer]), C.Program.Course, ButtonClick);
                                //otpPanelSubMenu.panelSubMenu.Initialize(true, FillSubjectList(response, C.JSONKeys.courses), C.Program.Course, ButtonClick);
                                subMenu.panelSubMenu.SetSelected(buttonNames[level + 1]);
                            }
                        }
                    );
                    break;
            }
        }

        subMenu.DrawCurriculumItems(null);

        //scroolViewCurriculumListDrive.Initialize(null, null);
    }
}
