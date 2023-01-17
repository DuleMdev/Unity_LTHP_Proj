using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class CanvasClassYMain : HHHScreen
{
    enum Status
    {
        initial,        // Kezdeti állapot
        curriculumInfo, // Látszik a tananyag információit tartalmazó panel (valószínűleg lejátszás volt kérve)
    }

    MainMenuClassY mainMenu;
    PanelSubMenu panelDriveSubMenu;
    PanelSortSelector panelSortSelector;
    ScroolViewCurriculumsListDrive scroolViewCurriculumListDrive;
    //PopUpInfo popUpInfo;

    string scope;   // Mit akarunk listázni (mindent - all, saját - own, vagy megosztott - shared, stb.)

    // 0 - MainMenu:1(index)
    // 1 - Subject:5(id)
    // 2 - Topic:18(id)
    // 3 - Course:27(index)
    string[] buttonNames = new string[4];
    int level;

    List<CurriculumItemDriveData> listOfCurriculums;

    string courseId;
    int curriculumIndex;

	// Use this for initialization
	void Awake()
    {
        // Bekapcsoljuk a Canvas-t mert az Editorban való átláthatóbb munka miatt ki vannak kapcsolva
        Common.SearchGameObject(gameObject, "Canvas").gameObject.SetActive(true);

        mainMenu = gameObject.SearchChild("MainMenuClassY").GetComponent<MainMenuClassY>();
        panelDriveSubMenu = gameObject.SearchChild("PanelDriveSubMenu").GetComponent<PanelSubMenu>();
        panelSortSelector = gameObject.SearchChild("PanelSortSelector").GetComponent<PanelSortSelector>();
        //popUpInfo = gameObject.SearchChild("PopUpInfo").GetComponent<PopUpInfo>();

        scroolViewCurriculumListDrive = gameObject.SearchChild("CurriculumListDrive").GetComponent<ScroolViewCurriculumsListDrive>();
	}

    void Start() {
        //popUpInfo.Initialize(ButtonClick);
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    override public IEnumerator InitCoroutine()
    {
        Common.menuBackground.ChangeBackgroundIndex(2);
        Common.menuStripe.SetItem();

        mainMenu.Initialize(true, new List<string> {
            Common.languageController.Translate(C.Texts.SharedWithMe),
            Common.languageController.Translate(C.Texts.OwnCurriculum),
            Common.languageController.Translate(C.Texts.MakeNewCurriculum),
            Common.languageController.Translate(C.Texts.EducationInClassroom),
            Common.languageController.Translate(C.Texts.Students),
        }, 
        ButtonClick
        );

        panelSortSelector.Initialize(ButtonClick);

        scroolViewCurriculumListDrive.Initialize(null, null);

        // Lekérdezzük a használható nyelveket és beállítjuk az elsőt
        ClassYServerCommunication.instance.GetUsableLanguages(scope, true,
            (bool success, JSONNode response) => {
                // Válasz feldolgozása
                if (success)
                {
                    if (ClassYLanguageSelector.instance.listOfLanguageDatas.Count > 0)
                    {
                        //mainMenu.menuStripe.SetCountryFlag(ClassYLanguageSelector.instance.listOfLanguageDatas[0].langFlag);
                        //Common.configurationController.curriculumLang = ClassYLanguageSelector.instance.listOfLanguageDatas[0].langCode;
                    }
                }
            }
        );


        yield return null;
    }

    override public IEnumerator ScreenHideFinish()
    {
        ClassYCurriculumInfo.instance.HideImmediatelly();

        yield break;
    }

    public void ButtonClick(string buttonName)
    {
        Debug.Log(buttonName);

        string[] buttonNameSplitted = buttonName.Split(':');

        switch (buttonNameSplitted[0])
        {
            case C.Program.MainMenu:
                mainMenu.SetSelected(buttonName); // A megnyomott gombot megjelöljük

                switch (buttonNameSplitted[1])
                {
                    case "0":
                        scope = "shared";
                        break;
                    case "1":
                        scope = "own";
                        break;
                }

                level = 0;
                buttonNames[level] = buttonName;

                ReFreshSubMenu();
                break;

            case C.Program.Subject:
            case C.Program.Topic:
                level++;
                buttonNames[level] = buttonName;

                panelDriveSubMenu.SetSelected(buttonNames[level]);

                ReFreshSubMenu();
                break;

            case C.Program.Course:
                // A kiválasztott kurzus azonosítóját elmentjük későbbi felhasználásra (PlayGames)
                courseId = buttonNameSplitted[1];

                // Kiválasztottá tesszük a megnyomott gombot
                panelDriveSubMenu.SetSelected(buttonName);

                // Kilistázzuk a kurzus tananyagjait
                ClassYServerCommunication.instance.GetCurriculums(buttonNameSplitted[1], scope, true,
                    (bool success, JSONNode response) => {
                        // Válasz feldolgozása
                        if (success)
                        {
                            scroolViewCurriculumListDrive.Initialize(FillCurriculumList(response), ButtonClick);
                        }
                    }
                );
                break;

            case "Back": // Megnyomták a vissza gombot, visszalépünk egy szinttel
                level--;
                ReFreshSubMenu();
                break;

            case C.Program.Curriculum:
                // A kiválasztott tananyag indexét elmentjük későbbi felhasználásra
                curriculumIndex = System.Int32.Parse(buttonNameSplitted[1]);

                // A tananyag adatait megjelenítjük egy felbukkanó ablakban
                ClassYCurriculumInfo.instance.Initialize(listOfCurriculums[curriculumIndex], ButtonClick);
                ClassYCurriculumInfo.instance.Show();
                break;

            case "FadeClick":
                // Eltüntetjük a felbukkanó ablakot
                ClassYCurriculumInfo.instance.Hide();
                break;

            case "FadeClickLanguageSelector":
                // Eltüntetjük a felbukkanó ablakot
                ClassYLanguageSelector.instance.Hide();
                break;

            case "Play":
                /*
                ServerPlay.instance.PlayCurriculum("", "", courseId, listOfCurriculums[curriculumIndex].curriculumID, () => {
                    // Vissza megyünk a ClassY képernyőre
                    Common.screenController.ChangeScreen(C.Screens.ClassYEDUDrive);
                });
                */
                break;

            case "Flag":
                // Kilistázzuk a használható nyelveket
                ClassYServerCommunication.instance.GetUsableLanguages(scope, true,
                    (bool success, JSONNode response) => {
                        // Válasz feldolgozása
                        if (success)
                        {
                            //ClassYLanguageSelector.instance.Initialize(response, C.Program.LanguageSelector);
                            //ClassYLanguageSelector.instance.Show(Common.configurationController.curriculumLang, ButtonClick);
                        }
                    }
                );

                break;

            case C.Program.LanguageSelector:
                // Kiválasztottak egy nyelvet
                mainMenu.menuStripe.SetCountryFlag(ClassYLanguageSelector.instance.GetFlag(buttonNameSplitted[1]));
                //Common.configurationController.curriculumLang = buttonNameSplitted[1];

                ClassYLanguageSelector.instance.SetSelected(buttonNameSplitted[1]);
                ClassYLanguageSelector.instance.Hide();

                break;

            case "QuestionMark":
                Debug.Log("Ok");
                break;

            case "Menu":
                Debug.Log("Ok");
                break;




                // Felbukkanó ablakon található megosztás gombok
            case C.Texts.ShareWithStudentsForLearn:
                Debug.Log("Ok");
                break;

            case C.Texts.ShareInCommonWork:
                Debug.Log("Ok");

                break;

            case C.Texts.ShareOnFacebook:
                Debug.Log("Ok");

                break;

            case C.Texts.ShareOnClassYStore:
                Debug.Log("Ok");

                break;
        }
    }

    /*
    IEnumerator PlayGames()
    {
        CurriculumData curriculumData = null;

        // Letöltjük a lejátszni kívánt tananyagot
        bool ready = false;
        ClassYServerCommunication.instance.GetCurriculumForPlay(curriculumID,
            (bool success, JSONNode response) =>
            {
                // Válasz feldolgozása
                if (success)
                    curriculumData = new CurriculumData(response[C.JSONKeys.answer]);

                ready = true;
            }
        );

        // Várunk amíg a tananyag letöltése és feldolgozás befejeződik
        while (!ready) yield return null;

        // Ha sikerült a letöltés
        if (curriculumData != null)
        {
            // Elkészítjük a lecketervet
            List<GameData> listOfGames = new List<GameData>(curriculumData.automatedGames);
            listOfGames.AddRange(curriculumData.plannedGames);

            LessonPlanData lessonPlanData = new LessonPlanData();
            LessonMosaicData lessonMosaicData = new LessonMosaicData();
            lessonMosaicData.listOfGames = listOfGames;
            lessonPlanData.lessonMosaicsList.Add(lessonMosaicData);

            Common.configurationController.lessonPlanData = lessonPlanData;

            // Sorban elindítjuk a játékokat
            for (int i = 0; i < listOfGames.Count; i++)
            {
                listOfGames[i].lessonMosaicIndex = 0;
                listOfGames[i].gameIndex = i;

                ready = false;
                Common.taskController.PlayGameInServer(listOfGames[i], 0, () => {
                    ready = true;
                });

                // Várunk amíg az előzőleg elindított játék befejeződik
                while (!ready) yield return null;
                while (!ready) yield return null;
            }
        }

        // Vissza megyünk a ClassY képernyőre
        Common.screenController.ChangeScreen(C.Screens.MenuClassYMain);
    }
    */


    void ReFreshSubMenu() {
        string[] buttonNameSplitted = buttonNames[level].Split(':');

        switch (buttonNameSplitted[0])
        {
            case C.Program.MainMenu: // A főmenüre kattintottak
                switch (buttonNameSplitted[1])
                {
                    case "0": // Kilistázzuk a velünk megosztott tananyagok tantárgyait
                    case "1": // Kilistázzuk a saját tananyagok tantárgyait
                        ClassYServerCommunication.instance.GetCurriculumSubjects(scope, true,
                            (bool success, JSONNode response) => {
                                // Válasz feldolgozása
                                if (success) {
                                    panelDriveSubMenu.Initialize(false, FillSubjectList(response, C.JSONKeys.subjects), C.Program.Subject,  ButtonClick);
                                    panelDriveSubMenu.SetSelected(buttonNames[level + 1]);
                                }
                            }
                        );
                        break;
                }
                break;

            case C.Program.Subject: // Egy tantárgyra kattintottak 
                ClassYServerCommunication.instance.GetCurriculumTopics(buttonNameSplitted[1], scope, true,
                    (bool success, JSONNode response) => {
                        // Válasz feldolgozása
                        if (success)
                        {
                            panelDriveSubMenu.Initialize(true, FillSubjectList(response, C.JSONKeys.topics), C.Program.Topic,  ButtonClick);
                            panelDriveSubMenu.SetSelected(buttonNames[level + 1]);
                        }
                    }
                );
                break;

                
            case C.Program.Topic: // Egy téma nevére kattintottak
                ClassYServerCommunication.instance.GetCurriculumCourses(buttonNameSplitted[1], scope, true,
                    (bool success, JSONNode response) => {
                                // Válasz feldolgozása
                                if (success)
                        {
                            panelDriveSubMenu.Initialize(true, FillSubjectList(response, C.JSONKeys.courses), C.Program.Course, ButtonClick);
                            panelDriveSubMenu.SetSelected(buttonNames[level + 1]);
                        }
                    }
                );
                break;
        }

        scroolViewCurriculumListDrive.Initialize(null, null);
    }

    /*
    {
        "error":false,
        "answer":{
            "courses":[
                {
                    "id":"1",
                    "name":"testCourse",
                    "incompleteCurriculumNumber":"1"
                }
            ]
        }
    }
    */
    List<SubFolderDatas> FillSubjectList(JSONNode json, string scope)
    {
        List<SubFolderDatas> list = new List<SubFolderDatas>();

        try
        {
            for (int i = 0; i < json[C.JSONKeys.answer][scope].Count; i++)
                list.Add(new SubFolderDatas(json[C.JSONKeys.answer][scope][i]));
        }
        catch (System.Exception e)
        {
            // Hibát megmutatjuk a felhasználónak
            //throw;
        }

        return list;
    }

    /*
    {
        "error":false,
        "answer":{
            "curriculums":[
                {
                    "id":"1",
                    "name":"testCurriculum",
                    "courseID":"1"
                }
            ]
        }
    }
    */
    List<CurriculumItemDriveData> FillCurriculumList(JSONNode json)
    {
        listOfCurriculums  = new List<CurriculumItemDriveData>();

        try
        {
            for (int i = 0; i < json[C.JSONKeys.answer][C.JSONKeys.curriculums].Count; i++)
                listOfCurriculums.Add(new CurriculumItemDriveData(json[C.JSONKeys.answer][C.JSONKeys.curriculums][i]));
        }
        catch (System.Exception e)
        {
            // Hibát megmutatjuk a felhasználónak
            //throw;
        }

        return listOfCurriculums;
    }

    // Update is called once per frame
    void Update () {
	
	}
}
