using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;

/*

Ez az osztály háromféle üzemmódba tud dolgozni.

1. Az egyik preview üzemmód, amikor a tanár megtekintheti az elkészített játékokat és végig tud rajtuk lépkedni
esetleg kipróbálni.

2. A második, amikor a tanári tablet gyűjti a tanulókat, azaz lehet hozzá csatlakozni.

3. A harmadik, amikor el lett indítva egy óramozaik és a Play módban van az osztály.


Azt, hogy milyen módban van az objektum a configurationController.lessonPlanView logikai változó tartalmazza.
Ha true, akkor preview módban, ha false, akkor play módban.


*/

public class MenuLessonPlan : HHHScreen {


    public enum Status
    {
        StudentList,    // A tanulók csatlakozhatnak a tanári tablethez
        StudentPlay,    // A csatlakozott tanulók játszanak a játékokkal
        Preview         // 
    }

    MenuLessonMosaicList menuLessonMosaicList;      // Az óraterv megjelenítéséhez
    MenuLessonStudentList menuLessonStudentList;    // A tanulók / csoportok megjelenítéséhez
    MenuLessonPreview menuLessonPreview;            // A kiválasztott játék előnézetéhez

    [HideInInspector]
    public Status status;           // Melyik üzemmódban dolgozik az objektum

    [HideInInspector]
    public int lessonMosaicIndex;   // Melyik óramozaik előnézete látszik
    [HideInInspector]
    public int gameIndex;           // Az óramozaikon belül melyik játék előnézete látszik
    [HideInInspector]
    public int screenIndex;         // A játékon belül melyik képernyő látszik

    public string selectedLessonPlanName;   // Az óraterv listázó képernyőn kiválasztott óraterv

    public TextAsset taskJSON;      // Átmenetileg ebből az assetből jön az óraterv

    public TextAsset classRoster;   // Átmenetileg ebből az assetből jön az osztálynévsor

    public LessonPlanData lessonPlanData;  // Az óraterv adatai

    string startLessonPlan;          // Mikor kezdtük el az óratervet

    int nextClientID = 1;

    GameMaster gameMaster;

    bool autoContinuation; // Ha el van indítva egy óra-mozaik, akkor automatikusan folytatja, míg cancel-t nem mondanak a folytatás kérdésre

	// Use this for initialization
	new void Awake () {
        // Bekapcsoljuk a Canvas-t mert az Editorban való átláthatóbb munka miatt ki vannak kapcsolva
        Common.SearchGameObject(gameObject, "Canvas").gameObject.SetActive(true);

        Common.menuLessonPlan = this;

        menuLessonMosaicList = Common.SearchGameObject(gameObject, "ScrollViewMosaicList").GetComponent<MenuLessonMosaicList>();
        menuLessonStudentList = Common.SearchGameObject(gameObject, "ScrollViewStudentList").GetComponent<MenuLessonStudentList>();
        menuLessonPreview = Common.SearchGameObject(gameObject, "Preview").GetComponent<MenuLessonPreview>();
	}

    override public IEnumerator InitCoroutine()
    {
        gameMaster = null;  // A korábbi gameMaster objektumot töröljük

        // Készítünk egy report objektumot, amiben gyűjteni lehet a tanulók eredményeit
        Common.configurationController.reportLessonPlan = new ReportLessonPlan(Common.configurationController.lessonPlanData.id);
        startLessonPlan = Common.TimeStamp();

        // Kibővítjük a lecketervet egy új óra-mozaikkal ami a freePlay gombot tartalmazza ha nem preview üzemmódban vagyunk
        if (status != Status.Preview)
        {
            GameData game = new GameData();
            game.gameEngine = GameData.GameEngine.FreePlay;

            LessonMosaicData mosaic = new LessonMosaicData();
            mosaic.name = "Free Play";
            mosaic.Add(game);

            lessonPlanData.lessonMosaicsList.Add(mosaic);
        }

        // Inicializáljuk az al objektumokat is
        menuLessonMosaicList.Initialize(lessonPlanData, ButtonClickGame);
        menuLessonStudentList.Initialize();

        menuLessonPreview.gameObject.SetActive(status == Status.Preview);
        menuLessonStudentList.gameObject.SetActive(status != Status.Preview);

        // Ha preview módban vagyunk akkor megmutatjuk az első óra-mozaik első játékát
        if (status == Status.Preview)
        {
            menuLessonMosaicList.Selected(lessonMosaicIndex, gameIndex);
            yield return StartCoroutine(menuLessonPreview.ShowGame(lessonPlanData.lessonMosaicsList[lessonMosaicIndex].listOfGames[gameIndex]));
        }
    }

    override public IEnumerator ScreenShowStartCoroutine()
    {
        Common.menuBackground.ChangeBackgroundIndex(1); // Keskeny menüsáv
        Common.menuStripe.SetItem(true, status != Status.Preview, Common.menuLessonPlan.selectedLessonPlanName, status == Status.Preview, status != Status.Preview, (status != Status.Preview) ? "" : null);
        Common.menuStripe.ResetElapsedTime();

        Common.menuStripe.buttonClick = ButtonClick;

        // Elindítjuk a szervert ha nem preview módban vagyunk
        if (status != Status.Preview) {






            // ***********************************************************************************************
            // Igazából ezt már az óraterv listázóban kellene meghívni, hiszen ha nem sikerül betölteni, akkor el sem kellene indítani az óratervet
            // Ha majd a szerverről fogom letölteni az óraterveket meg az osztály névsort, akkor ez ott lesz

            menuLessonMosaicList.SetPlay(0);
            lessonPlanData.lessonMosaicsList[0].neverEnding = true;

            gameMaster = new GameMaster(Common.configurationController.lessonPlanData.lessonMosaicsList[0]);
            gameMaster.Initialize(0);

            Common.configurationController.StartLog();
            Common.configurationController.Log("Szerver indítása");

            Common.HHHnetwork.StartServerHost();
            Common.HHHnetwork.callBackNetworkEvent = MessageArrived;

            Common.HHHnetwork.messageProcessingEnabled = true;
        }

        yield return null;
    }

    /// <summary>
    /// Vissza adja a megadott nevű képet Sprite objektumként, ha nincs olyan kép, akkor null-t ad vissza.
    /// </summary>
    /// <param name="pictureName">A keresett kép</param>
    /// <returns></returns>
    public Sprite GetPicture(string pictureName)
    {
        return (string.IsNullOrEmpty(pictureName)) ? null : lessonPlanData.GetPicture(pictureName);
    }

    // Update is called once per frame
    void Update () {
        // Megvizsgáljuk, hogy létezik-e már a gameMaster objektum
        if (gameMaster != null)
        {
            gameMaster.Update();

            // Ha befejeződött az óra-mozaik, akkor a következő indítása
            if (autoContinuation && gameMaster.allGroupEnd)
            {
                // Keresünk egy olyan óra-mozaikot, ahol az automatikus folytatás be van állítva
                bool next = false;  // Volt következő óra-mozaik
                for (int i = lessonMosaicIndex + 1; i < lessonPlanData.lessonMosaicsList.Count; i++)
                {
                    if (lessonPlanData.lessonMosaicsList[i].autoContinuation)
                    {
                        // Szimulálunk egy gombnyomást
                        ButtonClickGame(i, -1);
                        next = true;
                        break;
                    }
                }

                // Ha nincs következő óra-mozaik, akkor elküldjük az ügyes voltál szöveget
                if (!next)
                    gameMaster.LessonPlanEnd();

                autoContinuation = false;
            }
        }
	}

    /// <summary>
    /// A UI felületen lévő button komponensek hívják meg ha rákattintottak.
    /// </summary>
    /// <param name="buttonName">Melyik gombra kattintottak.</param>
    public void ButtonClick(string buttonName)
    {
        Debug.Log(buttonName);

        switch (buttonName)
        {
            case "Badge":

                // Befejezzük a játékot, minden kliensnek elküldjük
                gameMaster.EvaluateLessonMosaic();









                /*
                kamuLessonMosaicIndex = 0;
                kamuGameIndex = 0;
                KamuPlay();
                */










                //StartCoroutine(Common.configurationController.KamuPlay());
                break;

            /*
            case "Home":
                if (status == Status.Preview) {
                    Common.screenController.ChangeScreen("MenuSynchronize");
                } else {
                    Common.infoPanelExitFromLessonPlan.Show((string name) => {
                        switch (name)
                        {
                            case "Ok":
                                Common.menuInformation.Hide(() =>
                                {
                                    Common.screenController.ChangeScreen("MenuSynchronize");
                                });
                                break;
                            case "Cancel":
                                Common.menuInformation.Hide();
                                break;
                        }
                    });
                }
                break;
                */

            case "Back": // Megnyomták a back gombot

                // Nem erre fog eltünni a play gomb, hanem akkor ha a play gomb mellé kattintanak, tehát a panelre újfent
                /*
                // Ha valamelyik óramozaikon látható a playgomb akkor a back gombbal eltüntethető
                if (menuLessonMosaicList.showPlayButton)
                    menuLessonMosaicList.ShowPlayButton(-1);
                else
                */
                // Ezt majd törölni lehet, ha valóban nem lesz rá szükség

                {
                    if (status == Status.Preview) {
                        Common.screenController.ChangeScreen("MenuLessonPlanList");
                    } else {
                        // Ha egyik óramozaikon sincs play gomb, akkor vissza megy a óratervek listázását végző képernyőre
                        Common.infoPanelExitFromLessonPlan.Show((string name) =>
                        {
                            switch (name)
                            {
                                case "Ok":
                                    Common.menuInformation.Hide(() =>
                                    {
                                        Common.HHHnetwork.StopHost(); // Leállítjuk a szervert
                                        Common.screenController.ChangeScreen("MenuLessonPlanList");

                                        // Elmentjük az óratervben gyűjtött adatokat
                                        SaveLessonPlanReport();
                                        //Common.configurationController.reportLessonPlan.End();

                                        // Elmentjük az osztály adatait is
                                        Common.configurationController.classRoster.SaveToFile(Common.configurationController.GetClassDirectory());
                                    });
                                    break;
                                case "Cancel":
                                    Common.menuInformation.Hide();
                                    break;
                            }
                        });
                    }
                }
                break;

            case "Play": // Megnyomták a play gombot
                Debug.Log(Common.Now() + " - Play");

                if (status == Status.Preview)
                {
                    menuLessonPreview.ClickButton("PreviewPlay");
                }
                else {
                    // Tovább engedjük a játék futását
                    gameMaster.SetPause(false);

                    // pause gombot play-re változtatjuk
                    Common.menuStripe.ChangeItem(false, true);
                }

                /*
                if (status == Status.StudentList)
                {


                    menuLessonPreview.ClickButton("PreviewPlay");
                }
                */
                //Common.screenController.ChangeScreen("MenuLessonPlanList");
                break;

            case "Pause": // Megnyomták a pause gombot

                // Leállítjuk a játékok futását
                gameMaster.SetPause(true);

                // pause gombot play-re változtatjuk
                Common.menuStripe.ChangeItem(true, false);

                // Feldobjuk az infoPanelt
                Common.infoPanelPauseLessonPlan.Show((string button) => {
                    Common.menuInformation.Hide();
                });

                //Common.screenController.ChangeScreen("MenuLessonPlanList");
                break;
        }
    }

    void SaveLessonPlanReport()
    {

        bool needSave = false;

        // Elkészítjük a json fájlt
        JSONClass jsonClass = new JSONClass();

        jsonClass[C.JSONKeys.lessonid].AsInt = lessonPlanData.id;
        jsonClass[C.JSONKeys.startTime] = startLessonPlan;
        jsonClass[C.JSONKeys.endTime] = Common.TimeStamp();

        // Létrehozzuk a tanulók tárolására használatos tömböt
        JSONArray studentArray = new JSONArray();
        jsonClass[C.JSONKeys.students] = studentArray;

        // Végig megyünk a tanulók listáján
        foreach (ClientData clientData in gameMaster.listOfClients)
        {
            // Ha egy tanuló be van azonosítva, akkor az eredményeit eltároljuk
            if (clientData.studentData != null) {

                JSONClass jsonStudent = new JSONClass();
                studentArray.Add(jsonStudent);

                jsonStudent[C.JSONKeys.studentID].AsInt = clientData.studentData.id;

                JSONArray gamesArray = new JSONArray();
                jsonStudent[C.JSONKeys.games] = gamesArray;

                foreach (ReportEvent report in clientData.listOfReportEvent)
                {
                    gamesArray.Add(report.ToJSON());
                    needSave = true;
                }
            }     
        }

        // Az eredményt elmentjük a tanár mappájába ha szükséges
        if (needSave)
        {
            string fileName = System.IO.Path.Combine(Common.configurationController.GetReportDirectory(), Common.TimeStampWithSpace());
            System.IO.File.WriteAllText(fileName + ".json", jsonClass.ToString(" "));
        }
    }



    int kamuGameIndex;
    int kamuLessonMosaicIndex;



    public void KamuPlay() {

        if (kamuGameIndex >= lessonPlanData.lessonMosaicsList[kamuLessonMosaicIndex].listOfGames.Count)
        {
            kamuLessonMosaicIndex++;
            kamuGameIndex = 0;
        }

        if (kamuLessonMosaicIndex < lessonPlanData.lessonMosaicsList.Count)
        {

            LessonMosaicData lessonMosaicData = lessonPlanData.lessonMosaicsList[kamuLessonMosaicIndex];
            GameData gameData = lessonPlanData.lessonMosaicsList[kamuLessonMosaicIndex].listOfGames[kamuGameIndex];

            switch (gameData.gameEngine)
            {
                /*
                case GameData.GameType.Sets:
                    Common.taskControllerOld.PlayQuestionList(gameData, 0, true, () =>
                    {
                        KamuPlay();
                    });

                    break;
                    */

                case GameData.GameEngine.TrueOrFalse:
                case GameData.GameEngine.Millionaire:
                case GameData.GameEngine.Boom:
                case GameData.GameEngine.Hangman:
                case GameData.GameEngine.Bubble:
                case GameData.GameEngine.Read:
                case GameData.GameEngine.MathMonster:
                case GameData.GameEngine.Fish:
                case GameData.GameEngine.Affix:
                    Common.taskController.PlayGameInServer(gameData, 0, () =>
                    {
                        KamuPlay();
                    });

                    break;
            }

            kamuGameIndex++;
        }
        else {
            Common.screenController.ChangeScreen("MenuLessonPlan");
        }
    }




    public IEnumerator KamuPlayOld() {

        bool next = false;

        for (int i = 0; i < lessonPlanData.lessonMosaicsList.Count; i++)
        {
            LessonMosaicData lessonMosaicData = lessonPlanData.lessonMosaicsList[i];

            for (int j = 0; j < lessonMosaicData.listOfGames.Count; j++)
            {
                GameData gameData = lessonMosaicData.listOfGames[j];

                next = false;

                switch (gameData.gameEngine)
                {
                    case GameData.GameEngine.Bubble:
                    case GameData.GameEngine.Sets:
                    case GameData.GameEngine.MathMonster:
                    case GameData.GameEngine.Affix:
                    case GameData.GameEngine.Fish:
                    case GameData.GameEngine.Hangman:
                    case GameData.GameEngine.Read:
                        Common.taskControllerOld.PlayQuestionList(gameData, 0, true, () => {
                            next = true;
                        });

                        break;

                    case GameData.GameEngine.TrueOrFalse:
                    case GameData.GameEngine.Millionaire:
                    case GameData.GameEngine.Boom:
                        Common.taskController.PlayGameInServer(gameData, 0, () => {
                            next = true;
                        });

                        break;
                }

                while (!next)
                    yield return null;

                
            }
        }

        Common.screenController.ChangeScreen("MenuLessonPlan");

        yield return null;
    }

    /// <summary>
    /// Üzenet érkezett a hálózaton.
    /// </summary>
    /// <param name="networkEventType"></param>
    /// <param name="connectionID"></param>
    /// <param name="jsonMessage"></param>
    void MessageArrived(NetworkEventType networkEventType, int connectionID, JSONNode jsonMessage) {

        JSONClass answerNode;

        switch (networkEventType)
        {
            case NetworkEventType.ConnectEvent:

                break;

            case NetworkEventType.DisconnectEvent:

                // Valamelyik klienssel megszakadt a kapcsolat

                // Ki kell venni a listából


                if (!Common.HHHnetwork.clientIsConnected)
                {

                }
                break;
            case NetworkEventType.DataEvent:
                switch (jsonMessage[C.JSONKeys.dataContent])
                {
                    case C.JSONValues.IDRequest:
                        // Megnézzük, hogy az azonosítót kérő kliens szerepel-e már a listába, mert ha igen
                        // akkor nem hozunk létre új clientData objektumot, hanem a korábbit adjuk vissza.

                        string tabletUniqueIdentifier = jsonMessage[C.JSONKeys.clientUniqueIdentifier];

                        // Megnézzük, hogy a tanuló listában ven-e már a csatlakozott tablet
                        ClientData clientData = gameMaster.GetClientDataByUniqueIdentifier(tabletUniqueIdentifier);

                        // Megnézzük, hogy utoljára melyik tanulóhoz rendeltük ezt a tablettet
                        StudentData studentData = Common.configurationController.classRoster.GetStudentDataByTabletUniqueIdentifier(tabletUniqueIdentifier);

                        // Ha ez az első azonosító kérés, akkor létrehozunk neki egy új clientData objektumot
                        if (clientData == null)
                        {
                            // Létrehozunk egy bejegyzést student listában
                            if (studentData == null)
                            {
                                clientData = gameMaster.AddClient(connectionID, GetNextClientID(), tabletUniqueIdentifier);
                            }
                            else {
                                clientData = gameMaster.AddClient(connectionID, GetNextClientID(), tabletUniqueIdentifier, studentData);
                            }

                        }
                        else
                            clientData.connectionID = connectionID;

                        // Küldünk a kliensnek egy azonosítót
                        answerNode = new JSONClass();
                        answerNode[C.JSONKeys.dataContent] = C.JSONValues.clientID;
                        answerNode[C.JSONKeys.clientID].AsInt = clientData.tabletID; // GetNextClientID().ToString();
                        answerNode[C.JSONKeys.clientName] = (clientData.studentData != null) ? clientData.studentData.name : "";
                        // Elküldjük a lejátszandó lecketerv azonosítóját és a szinkronizáció idejét is
                        answerNode[C.JSONKeys.lessonid].AsInt = lessonPlanData.id;
                        answerNode[C.JSONKeys.lessonSynchronizeTime] = lessonPlanData.lessonSynchronizeTime;
                        // Elküldjük a szerver idejét
                        answerNode[C.JSONKeys.serverDateTime] = Common.TimeStamp();

                        // Elküldjük a kliensnek az adatokat
                        Common.HHHnetwork.SendMessage(connectionID, answerNode);

                        break;

                    case C.JSONValues.lessonPlanRequest:

                        Debug.Log(System.DateTime.Now.ToString("HH:mm:ss.fff") + " Óraterv küldéshez json összeállításának kezdete.");

                        // Elküldjük a kliensnek a lecketervet
                        answerNode = new JSONClass();
                        answerNode[C.JSONKeys.dataContent] = C.JSONValues.lessonPlan;
                        answerNode[C.JSONKeys.lessonPlan] = Common.configurationController.lessonPlanJSON;

                        Debug.Log(System.DateTime.Now.ToString("HH:mm:ss.fff") + " Óraterv küldéshez json összeállításának vége.");

                        // Elküldjük a kliensnek az adatokat
                        Common.HHHnetwork.SendMessage(connectionID, answerNode);

                        break;

                    case C.JSONValues.lessonPlanTransferOk: // A kliens vette az óratervet
                        // Elindítjuk az első óramozaikot a kliensen
                        gameMaster.StartFirstLessonMosaic(connectionID, lessonPlanData.lessonMosaicsList[0]);

                        break;

                    case C.JSONValues.gameEvent:
                        // Ez az utolsó utasítás ebben az eljárásban
                        //gameMaster.MessageArrived(networkEventType, connectionID, jsonMessage);

                        break;
                }

                break;
        }

        // Minden hálózati eseményt tovább adunk a gameMester-nek
        gameMaster.MessageArrived(networkEventType, connectionID, jsonMessage);
    }

    /// <summary>
    /// Visszaad egy diák azonosítót ami még nem foglalt.
    /// </summary>
    /// <returns>A szabad diák azonosító.</returns>
    int GetNextClientID() {
        return nextClientID++;
    }

    /// <summary>
    /// Ha preview módban van az objektum, akkor a lessonMosaicList hívja meg ezt a metódust,
    /// ha rákattintottak egy játék gombjára.
    /// </summary>
    /// <param name="lessonMosaicIndex">Hányadik óramozaikra kattintottak.</param>
    /// <param name="gameIndex">Hányadik játékra kattintottak.</param>
    public void ButtonClickGame(int lessonMosaicIndex, int gameIndex) {
        Debug.Log(Common.Now() + "Play : " + lessonMosaicIndex + " mosaicIndex");
        this.lessonMosaicIndex = lessonMosaicIndex;
        this.gameIndex = gameIndex;
        screenIndex = 0;

        if (gameIndex == -1) // Play gombra kattintottak
        {
            // Eltüntetjük a play gombot
            menuLessonMosaicList.ShowPlayButton(-1);

            // El akarja indítani a tanár a lessonMosaicIndex által meghatározott óramozaikot
            LessonMosaicData lessonMosaicData = lessonPlanData.lessonMosaicsList[lessonMosaicIndex];
            lessonMosaicData.neverEnding = lessonMosaicIndex == 0; // Az első óra-mozaik végtelenített lejászással használatos

            // Ha az óra-mozaik első játéka freePlay akkor összeszedjük az összes óra-mozaikban található 
            // játékokat egy óra-mozaikba és azt indítjuk el
            if (lessonMosaicData.listOfGames[0].gameEngine == GameData.GameEngine.FreePlay) {
                lessonMosaicData = new LessonMosaicData();
                lessonMosaicData.multiPlayerGameMode = LessonMosaicData.MultiPlayerGameMode.Single;

                for (int i = 0; i < lessonPlanData.lessonMosaicsList.Count - 1; i++) // végig megyünk az összes óra-mozaikon kivéve az utolsón mivel az a freePlay óra-mozaik
                    for (int j = 0; j < lessonPlanData.lessonMosaicsList[i].listOfGames.Count; j++)
                        lessonMosaicData.Add(lessonPlanData.lessonMosaicsList[i].listOfGames[j]);

                lessonMosaicData.neverEnding = true; // Az utolsó óra-mozaik a FreePlay is végtelenített lejátszással használatos
            }

            // Meg kell vizsgálni, hogy csoportjáték van-e beállítva a kiválasztott óramozaikban vagy egyszemélyes játék
            if (lessonMosaicData.multiPlayerGameMode == LessonMosaicData.MultiPlayerGameMode.Single)
            {
                Common.infoPanelSinglePlayerStart.Show((string buttonName) => {
                    Common.menuInformation.Hide( () => {
                        if (buttonName == "Ok")
                        {
                            Debug.Log("Ok selected.");
                            // Elindítjuk az egyszemélyes óra-mozaikot
                            gameMaster.Grouping(lessonMosaicData);
                            gameMaster.StartLessonMosaic();
                            autoContinuation = true;
                            menuLessonMosaicList.SetPlay(lessonMosaicIndex);

                            //Common.screenController.ChangeScreen("");
                        }
                    } );
                } );
            }
            else {
                Common.infoPanelAutoGroup.Show((name) => {
                    switch (name)
                    {
                        case "Ok":
                            // Automatikus csoportosítás
                            StudentGrouping(lessonMosaicData, 4);

                            break;
                        case "Custom":
                            Common.infoPanelSelectGroupNumber.Show((string buttonName2) => {
                                switch (buttonName2)
                                {
                                    case "Ok":
                                        // Ellenőrizni, hogy kiválasztottak-e egy gombot vagy sem.
                                        StudentGrouping(lessonMosaicData, Common.infoPanelSelectGroupNumber.selectedNumber);

                                        break;
                                    case "Cancel":
                                        Common.menuInformation.Hide();

                                        break;
                                }
                            });

                            break;
                        case "Cancel":
                            Common.menuInformation.Hide();

                            break;
                    }
                });
            }
            //Common.taskController.PlayQuestionList(lessonPlanData.lessonMosaicsList[lessonMosaicIndex].listOfGames[gameIndex], )

        }
        else
        {
            // Megmutatjuk a kiválasztott játék előnézetét
            // Beállítjuk, hogy a kiválasztott gomb legyen aktív
            menuLessonMosaicList.Selected(lessonMosaicIndex, gameIndex);
            StartCoroutine(menuLessonPreview.ShowGame(lessonPlanData.lessonMosaicsList[lessonMosaicIndex].listOfGames[gameIndex]));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lessonMosaicData"></param>
    /// <param name="idealHeadcount"></param>
    void StudentGrouping(LessonMosaicData lessonMosaicData, int idealHeadcount) {
        // Csoportokba szervezzük a tanulókat
        gameMaster.Grouping(lessonMosaicData, idealHeadcount);

        // Megkérdezzük, hogy biztosan indítsuk-e az óra-mozaikot
        Common.infoPanelAfterGrouping.Show((string name) => {
            Common.menuInformation.Hide( () => {
                if (name == "Ok") {
                    // Elindítjuk a csoportjátékot
                    Debug.Log("Csoport játék indítása " + idealHeadcount + " fő/csoport.");

                    gameMaster.StartLessonMosaic();
                    autoContinuation = true;
                    menuLessonMosaicList.SetPlay(lessonMosaicIndex);
                }
            } );
        });
    }


}
