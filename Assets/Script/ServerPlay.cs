using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerPlay : MonoBehaviour {

    public static ServerPlay instance;

    /// <summary>
    /// Az értékelő képernyőn lenyomott billentyű neve. Innen tudjuk, hogy ki kell-e lépni vagy jöhet a következő tananyag.
    /// </summary>
    public string evaluateButtonName;

    void Awake()
    {
        instance = this;
    }

    // Útvonal token lejátszása
    public void PlayRouteLink(string shareToken, Common.CallBack callBack)
    {
        StartCoroutine(PlayRouteLinkCoroutine(shareToken, callBack));
    }

    public IEnumerator PlayRouteLinkCoroutine(string shareToken, Common.CallBack callback)
    {
        bool ready = false;

        // Ha van deepLink, akkor lekérdezzük az útvonalat
        ready = false;

        ClassYServerCommunication.instance.getShareTokenData(
            shareToken,
            (bool success, JSONNode response) =>
            {
                // Ha a lekérdezés sikeres volt elindítjuk az útvonalat
                if (success)
                {
                    CurriculumPathData curriculumPathData = curriculumPathData = new CurriculumPathData(response[C.JSONKeys.answer]);

                    Common.CallBack end = () =>
                    {
                        ready = true;
                    };

                    if (Common.configurationController.isServer2020) // Common.configurationController.link == ConfigurationController.Link.Server2020Link)
                        ServerPlay.instance.PlayLearnPathByNextGameMode(curriculumPathData, end);
                    else
                        ServerPlay.instance.PlayLearnPath(curriculumPathData, end);

                }
                else
                    ready = true;
            }
            );

        // Várunk amíg be nem fejeződik az útvonal
        yield return new WaitUntil(() => ready);

        callback();
    }

    IEnumerator ShowEvaluatePanel(JSONNode statusData, bool frameGame = false)
    {
        bool ready = false;

        //frameGame = !frameGame;

        // Keretjátéktól (frameGame) függően más értékelő képernyőt kell mutatni
        if (frameGame)
        {
            GameEvaluation2021.instance.Show(
                statusData[C.JSONKeys.starCount].AsInt,
                statusData[C.JSONKeys.gameScore].AsInt,
                true,
                (string buttonName) =>
                {
                    evaluateButtonName = buttonName;
                    ready = true;
                },
                statusData
            );
        }
        else
        {
            GameEvaluation.instance.Show(
                statusData[C.JSONKeys.starCount].AsInt,
                statusData[C.JSONKeys.gameScore].AsInt,
                true,
                (string buttonName) =>
                {
                    evaluateButtonName = buttonName;
                    ready = true;
                },
                statusData
            );
        }

        // Várunk amíg az értékelő panelen nem választanak valamit
        while (!ready) yield return null;

        // Eltüntetjük az értékelő panelt
        ready = false;
        if (frameGame)
            GameEvaluation2021.instance.Hide(() => { ready = true; });
        else
            GameEvaluation.instance.Hide(() => { ready = true; });

        // Várunk amíg az értékelő panel eltűnik
        while (!ready) yield return null;
    }

    /// <summary>
    /// Folytatja a megadott tanulási útvonalat 2020-as szerveren
    /// </summary>
    public void PlayLearnPathByNextGameMode(CurriculumPathData curriculumPathData, Common.CallBack callBack)
    {
        //evaluateButtonName = "";
        StartCoroutine(PlayLearnPathByNextGameModeCoroutine(curriculumPathData, callBack));
    }

    public IEnumerator PlayLearnPathByNextGameModeCoroutine(CurriculumPathData curriculumPathData, Common.CallBack callBack)
    {
        // Lekérdezzük a felhasználó beállításait
        ClassYServerCommunication.instance.getUserProfile();

        int getNextGameCounter = 0; // Hányadik GetNextGame lekérdezésnél tartunk

        bool restart = false;   // restart lett kiválasztva?

        bool loggedGame = false;    // Egy korábbi játékot játszunk le?
        string logID = "";          // Az újra játszandó játék logID-ja

        bool evaluateIsEnabled = true;

        string learnRoutePathStart = "";

        do
        {
            // Megvizsgáljuk, hogy befejeződött-e az útvonal és van-e lehetőség az újrakezdésre
            bool ready = false;     // Mehetünk tovább a coroutine metódusban

            GameData gameData = null;

            bool success = true;        // Sikerült-e a szerver kommunikáció
            JSONNode jsonNode = null;   // A szerver által küldött json

            string previousLogID = null;
            string nextLogID = null;

            // Meghatározzuk az isolation Time-t

            if (getNextGameCounter < 2)
            {
                learnRoutePathStart = curriculumPathData.lastLearnRoutePathIsolationTimeForContinue;

                if (restart || learnRoutePathStart == "null")
                    learnRoutePathStart = curriculumPathData.newLearnRoutePathIsolationTimeForContinue;
            }

            getNextGameCounter++;

            ready = false;

            Common.CallBack_In_Bool_JSONNode gameDataResponseProcess =
                (bool ok, JSONNode response) =>
                {
                    success = ok;
                    jsonNode = response;
                    ready = true;
                };

            if (!loggedGame)
            {
                ClassYServerCommunication.instance.GetNextGame(
                    curriculumPathData.ID,
                    curriculumPathData.mailListID,
                    learnRoutePathStart,
                    gameDataResponseProcess);
            }
            else
            {
                ClassYServerCommunication.instance.GetLoggedGame(
                    curriculumPathData.ID,
                    curriculumPathData.mailListID,
                    logID,
                    gameDataResponseProcess);
            }

            // Várunk amíg a játék adatok megérkeznek
            while (!ready) yield return null;

            // Ha hiba történt, akkor kilépünk a ciklusból
            if (!success) break; // ---------------------------- EXIT Do-while -------------------------------

            
            JSONClass statusData = jsonNode[C.JSONKeys.answer][C.JSONKeys.statusData].AsObject;
            // Ha nem az első getNextGame lekérdezés és meg kell mutatni az értékelő képernyőt
            if (//statusData[C.JSONKeys.show].AsBool && 
                getNextGameCounter != 1 && 
                !restart &&
                evaluateIsEnabled)
            {
                if (curriculumPathData.frameGameExists)
                {
                    // Lejátszuk a szintváltó animációt ha volt szintváltás
                    int levelIndicator = statusData[C.JSONKeys.levelIndicator].AsInt;
                    if (levelIndicator != 0 && Common.configurationController.playAnimations)
                    {
                        ready = false;
                        if (levelIndicator == 1)
                            CastleGameLevelUpScreen.Load(curriculumPathData, statusData, () => { ready = true; });
                        else
                            CastleGameLevelDownScreen.Load(curriculumPathData, statusData, () => { ready = true; });

                        // Várunk amíg a levelUp vagy a levelDown animáció befejezi a dolgát
                        while (!ready) yield return null;
                    }
                }

                // Megmutatjuk az értékelő képernyőt ha szükséges
                if (statusData[C.JSONKeys.show].AsBool &&
                    ((statusData[C.JSONKeys.nextGamePartOfLastGame].AsBool && Common.configurationController.statusTableInSuper) ||
                    (!statusData[C.JSONKeys.nextGamePartOfLastGame].AsBool && Common.configurationController.statusTableBetweenSuper))
                    )
                {
                    yield return StartCoroutine(ShowEvaluatePanel(statusData, curriculumPathData.frameGameExists));

                    // Ha a kilépést választották, akkor kilépünk
                    if (evaluateButtonName == "EvaluateExit") break; // ------------------------- EXIT Do-while -------------------------------
                }

                if (curriculumPathData.frameGameExists && statusData[C.JSONKeys.frameGameWalk].AsBool && Common.configurationController.playAnimations)
                {
                    // Megmutatjuk a szobaképernyőt
                    ready = false;
                    CastleGameRoomScreen.Load(statusData, () => { ready = true; });

                    // Várunk amíg a szobából kimennek
                    while (!ready) yield return null;
                }
            }

            restart = false;
            // Ha van még játék az útvonalban azaz nincs befejezve
            if (!statusData[C.JSONKeys.isComplete].AsBool || loggedGame)
            {
                // A json-ban található játékot feldolgozzuk
                gameData = new GameData(jsonNode[C.JSONKeys.answer][C.JSONKeys.gameData]);
            }
            else
            {
                // Nincs több játék üzenetet kaptunk a szervertől
                if (getNextGameCounter != 1)
                {
                    // Útvonal vége animáció
                    if (curriculumPathData.frameGameExists)
                    {
                        ready = false;
                        CastleGameFinishScreen.Load(curriculumPathData, () => { ready = true; });

                        while (!ready) yield return null;
                    }

                    // Ha nem az első getNextGame lekérdezés volt, akkor végeztünk az útvonallal
                    // kilépünk a játékokat játszó ciklusból
                    break; // ---------------------------- EXIT Do-while -------------------------------------
                }
                else
                {
                    // Ha az első getNextGame lekérdezés volt, akkor tudatjuk a felhasználóval, hogy vége az útvonalnak
                    // Ha újra kezdhető az útvonal, akkor lesz Restart button is egyébként csak Ok gomb amivel tudomásul vehetjük, hogy teljesítve van és nem lehet újra kezdeni
                    string button2 = (curriculumPathData.replayable) ? Common.languageController.Translate(C.Texts.restart) : null;
                    ready = false;
                    ErrorPanel.instance.Show(
                        Common.languageController.Translate(C.Texts.ThisLearnRouteIsFinished),
                        Common.languageController.Translate(C.Texts.Ok),
                        button2Text: button2,
                        callBack: (string buttonName) =>
                        {
                            restart = buttonName == "button2";
                            ready = true;
                        });

                    // Várunk amíg a felhasználó meghozza a döntését a folytatásról vagy az újra kezdésről
                    while (!ready) yield return null;

                    // Eltüntetjük a panel-t
                    ready = false;
                    ErrorPanel.instance.Hide(() => { ready = true; });

                    // Várunk amíg a panel eltűnik
                    while (!ready) yield return null;

                    // Ha nem kértünk újra indítást, akkor kilépünk
                    if (!restart) break; // ---------------------------- EXIT Do-while -------------------------------------

                    // Egyébként folytatjuk a ciklust
                    //continue; // ---------------------------- CONTINUE Do-While -------------------------------------
                }
            }

            // Ha nincsenek még meghatározva a játékban a karakterek, akkor meghatározzuk őket
            if (curriculumPathData.frameGameExists && (!curriculumPathData.characterSelectExists || restart))
            {
                ready = false;
                CastleGameInstructionScreen.Load(curriculumPathData, () => { ready = true; });

                // Várunk amíg az instructionScreeen befejezi a dolgát
                while (!ready) yield return null;
            }

            if (restart) continue;

            // Ha nincs valamelyik ID meghatározva a json-ban, akkor -1 lesz az értéke alapértelmezetten
            previousLogID = jsonNode[C.JSONKeys.answer][C.JSONKeys.pathData].ContainsKey(C.JSONKeys.previousLogID) ?
                jsonNode[C.JSONKeys.answer][C.JSONKeys.pathData][C.JSONKeys.previousLogID].Value : "-1";
            nextLogID = jsonNode[C.JSONKeys.answer][C.JSONKeys.pathData].ContainsKey(C.JSONKeys.nextLogID) ?
                jsonNode[C.JSONKeys.answer][C.JSONKeys.pathData][C.JSONKeys.nextLogID].Value : "-1";

            // A GameMenu gombjainak láthatóságát beállítjuk
            GameMenu.instance.SetPreviousButton(previousLogID != "-1");
            GameMenu.instance.SetNextButton(true);
            GameMenu.instance.gameDataJson = jsonNode[C.JSONKeys.answer][C.JSONKeys.gameData];

            // Lejátszuk a játékot a kezdő és befejező időpontokat megjegyezzük
            System.DateTime gameStartdateTime = System.DateTime.Now;
            string gameStart = Common.TimeStamp();
            yield return StartCoroutine(PlayGame(gameData));
            string gameEnd = Common.TimeStamp(gameStartdateTime.AddSeconds(Common.taskController.elapsedGameTime));

            // Töröljük a játék információit a game menüből
            GameMenu.instance.gameDataJson = null;

            evaluateIsEnabled = GameMenu.instance.selectedButton == "";
            evaluateIsEnabled = true;

            Debug.Log("Game result : " + Common.taskController.resultPercent + "*********************************************************************************************");

            // Várunk amíg a GameMenüt bezárják
            while (GameMenu.instance.menuShow) yield return null;

            // Megvizsgáljuk, hogy a GameMenu-ben milyen gombot nyomtak, ha nyomtak valamit
            switch (GameMenu.instance.selectedButton)
            {
                case C.Program.GameMenuExit: // Ha a játékból kiléptek, akkor kilépünk
                    if (callBack != null)
                        callBack();
                    yield break;

                case "": // Ha nem nyomtak semmit, akkor az Next-et jelent
                case C.Program.GameMenuNext: // Ha a következő tananyag gombot választották
                    // Ha van nextLogID, akkor egy loggolt játékot kérünk
                    logID = nextLogID;
                    loggedGame = nextLogID != "-1";

                    //loggedGame = false;
                    //if (nextLogID != "")
                    //{
                    //    loggedGame = true;
                    //    logID = nextLogID;
                    //}
                    break;

                case C.Program.GameMenuPrevious:
                    // Elvileg ha van előző gomb, akkor kell lennie previousLogID-nak is
                    loggedGame = true;
                    logID = previousLogID;
                    continue; // ---------------------------- CONTINUE Do-While  -------------------------------------

                    //loggedGame = false;
                    //if (previousLogID != "")
                    //{
                    //    loggedGame = true;
                    //    logID = previousLogID;
                    //}
                    break;
            }

            gameData.played = true;
            gameData.lastGamePercent = Common.taskController.resultPercent;


            // Akkor nem kellene logot küldeni újra, ha egy logolt játék volt betöltve, de egy válaszra sem lett válaszolva
            // ?????? Honnan tudjuk, hogy egy válasz sem lett megadva?

            do
            {
                // A játék eredményét elküldjük a szervernek
                ready = false;

                ClassYServerCommunication.instance.addGameLogServer2020(
                    jsonNode[C.JSONKeys.answer][C.JSONKeys.pathData],
                    Common.taskController.screensEvaluations,
                    gameData.ID.ToString(),
                    Common.taskController.resultPercent, gameStart, gameEnd, Common.taskController.extraData,
                    jsonNode[C.JSONKeys.answer][C.JSONKeys.gameData][C.JSONKeys.gameTheme],
                    jsonNode[C.JSONKeys.answer][C.JSONKeys.gameData][C.JSONKeys.gameEnding],
                    true, (bool ok, JSONNode response) =>
                    {
                        success = ok;
                        ready = true;
                    });

                // Várunk amíg az adatokat elküldjük a szervernek
                while (!ready) yield return null;

                // Ha hiba volt a küldés során, akkor a felhasználót értesítsük, hogy újra küldjük az adatokat
                if (!success)
                {
                    ready = false; // A felhasználó érteséítéséhez

                    ErrorPanel.instance.Show(
                        Common.languageController.Translate(C.Texts.gameResultResend),
                        Common.languageController.Translate(C.Texts.Ok),
                        callBack: (string buttonName) =>
                        {
                            ready = true;
                        });

                    // Várunk amíg a felhasználó tudomásul vesz az újra küldést
                    while (!ready) yield return null;

                    ready = false; // Nem fejezzük be a do while ciklust, hanem ismétlünk
                }

            } while (!ready);


            // Eltüntetjük az error panelt
            ready = false;
            ErrorPanel.instance.Hide(() =>
            {
                ready = true;
            });

            // Várunk amíg az errorPanel eltűnik
            while (!ready) yield return null;
        
        } while (true);
        //

        if (callBack != null)
            callBack();
    }

    public IEnumerator PlayLearnPathByNextGameModeCoroutineOld(CurriculumPathData curriculumPathData, Common.CallBack callBack)
    {
        int getNextGameCounter = 0; // Hányadik GetNextGame lekérdezésnél tartunk

        bool restart = false;   // restart lett kiválasztva?

        bool loggedGame = false;    // Egy korábbi játékot játszunk le?
        string logID = "";          // Az újra játszandó játék logID-ja

        do
        {
            // Megvizsgáljuk, hogy befejeződött-e az útvonal és van-e lehetőség az újrakezdésre
            bool ready = false;     // Mehetünk tovább a coroutine metódusban

            GameData gameData = null;

            bool success = true;        // Sikerült-e a szerver kommunikáció
            JSONNode jsonNode = null;   // A szerver által küldött json

            string previousLogID = null;
            string nextLogID = null;

            // Meghatározzuk az isolation Time-t
            string learnRoutePathStart = curriculumPathData.lastLearnRoutePathIsolationTimeForContinue;

            if (restart || learnRoutePathStart == "null")
                learnRoutePathStart = curriculumPathData.newLearnRoutePathIsolationTimeForContinue;

            getNextGameCounter++;

            ready = false;

            Common.CallBack_In_Bool_JSONNode gameDataResponseProcess =
                (bool ok, JSONNode response) =>
                {
                    success = ok;
                    jsonNode = response;
                    ready = true;
                };

            if (!loggedGame)
            {
                ClassYServerCommunication.instance.GetNextGame(
                    curriculumPathData.ID,
                    curriculumPathData.mailListID,
                    learnRoutePathStart,
                    gameDataResponseProcess);
            }
            else
            {
                ClassYServerCommunication.instance.GetLoggedGame(
                    curriculumPathData.ID,
                    curriculumPathData.mailListID,
                    logID,
                    gameDataResponseProcess);
            }

            // Várunk amíg a játék adatok megérkeznek
            while (!ready) yield return null;

            // Ha hiba történt, akkor kilépünk a ciklusból
            if (!success) break; // ---------------------------- EXIT -------------------------------------

            // Ha az answer-ben egy osztály található, akkor van még játék az útvonalban
            if (jsonNode[C.JSONKeys.answer] is JSONClass)
            {
                // A json-ban található játékot feldolgozzuk
                gameData = new GameData(jsonNode[C.JSONKeys.answer][C.JSONKeys.gameData]);
            }
            else
            {
                // Nincs több játék üzenetet kaptunk a szervertől
                if (getNextGameCounter != 1)
                {
                    // Ha nem az első getNextGame lekérdezés volt, akkor végeztünk az útvonallal
                    // kilépünk a játékokat játszó ciklusból
                    break; // ---------------------------- EXIT -------------------------------------
                }
                else
                {
                    // Ha az első getNextGame lekérdezés volt, akkor tudatjuk a felhasználóval, hogy vége az útvonalnak
                    // Ha újra kezdhető az útvonal, akkor lesz Restart button is egyébként csak Ok gomb amivel tudomásul vehetjük, hogy teljesítve van és nem lehet újra kezdeni
                    string button2 = (curriculumPathData.replayable) ? Common.languageController.Translate(C.Texts.restart) : null;
                    ready = false;
                    ErrorPanel.instance.Show(
                        Common.languageController.Translate(C.Texts.ThisLearnRouteIsFinished),
                        Common.languageController.Translate(C.Texts.Ok),
                        button2Text: button2,
                        callBack: (string buttonName) =>
                        {
                            restart = buttonName == "button2";
                            ready = true;
                        });

                    // Várunk amíg a felhasználó meghozza a döntését a folytatásról vagy az újra kezdésről
                    while (!ready) yield return null;

                    // Eltüntetjük a panel-t
                    ready = false;
                    ErrorPanel.instance.Hide(() =>
                    {
                        ready = true;
                    });

                    // Várunk amíg a panel eltűnik
                    while (!ready) yield return null;

                    // Ha nem kértünk újra indítást, akkor kilépünk
                    if (!restart) break; // ---------------------------- EXIT -------------------------------------

                    // Egyébként folytatjuk a ciklust
                    continue; // ---------------------------- CONTINUE -------------------------------------
                }
            }

            // Ha nincs valamelyik ID meghatározva a json-ban, akkor -1 lesz az értéke alapértelmezetten
            previousLogID = jsonNode[C.JSONKeys.answer][C.JSONKeys.pathData].ContainsKey(C.JSONKeys.previousLogID) ?
                jsonNode[C.JSONKeys.answer][C.JSONKeys.pathData][C.JSONKeys.previousLogID].Value : "-1";
            nextLogID = jsonNode[C.JSONKeys.answer][C.JSONKeys.pathData].ContainsKey(C.JSONKeys.nextLogID) ?
                jsonNode[C.JSONKeys.answer][C.JSONKeys.pathData][C.JSONKeys.nextLogID].Value : "-1";

            // A GameMenu gombjainak láthatóságát beállítjuk
            GameMenu.instance.SetPreviousButton(previousLogID != "-1");
            GameMenu.instance.SetNextButton(true);

            // Lejátszuk a játékot a kezdő és befejező időpontokat megjegyezzük
            System.DateTime gameStartdateTime = System.DateTime.Now;
            string gameStart = Common.TimeStamp();
            yield return StartCoroutine(PlayGame(gameData));
            string gameEnd = Common.TimeStamp(gameStartdateTime.AddSeconds(Common.taskController.elapsedGameTime));

            Debug.Log("Game result : " + Common.taskController.resultPercent + "*********************************************************************************************");

            // Megvizsgáljuk, hogy a GameMenu-ben milyen gombot nyomtak, ha nyomtak valamit
            switch (GameMenu.instance.selectedButton)
            {
                case C.Program.GameMenuExit: // Ha a játékból kiléptek, akkor kilépünk
                    if (callBack != null)
                        callBack();
                    yield break;

                case "": // Ha nem nyomtak semmit, akkor az Next-et jelent
                case C.Program.GameMenuNext: // Ha a következő tananyag gombot választották
                    // Ha van nextLogID, akkor egy loggolt játékot kérünk
                    logID = nextLogID;
                    loggedGame = nextLogID != "-1";

                    //loggedGame = false;
                    //if (nextLogID != "")
                    //{
                    //    loggedGame = true;
                    //    logID = nextLogID;
                    //}
                    break;

                case C.Program.GameMenuPrevious:
                    // Elvileg ha van előző gomb, akkor kell lennie previousLogID-nak is
                    loggedGame = true;
                    logID = previousLogID;
                    continue; // ---------------------------- CONTINUE -------------------------------------

                    //loggedGame = false;
                    //if (previousLogID != "")
                    //{
                    //    loggedGame = true;
                    //    logID = previousLogID;
                    //}
                    break;
            }

            gameData.played = true;
            gameData.lastGamePercent = Common.taskController.resultPercent;


            // Akkor nem kellene logot küldeni újra, ha egy logolt játék volt betöltve, de egy válaszra sem lett válaszolva
            // ?????? Honnan tudjuk, hogy egy válasz sem lett megadva?

            do
            {
                // A játék eredményét elküldjük a szervernek
                ready = false;

                ClassYServerCommunication.instance.addGameLogServer2020(
                    jsonNode[C.JSONKeys.answer][C.JSONKeys.pathData],
                    Common.taskController.screensEvaluations,
                    gameData.ID.ToString(),
                    Common.taskController.resultPercent, gameStart, gameEnd, Common.taskController.extraData,
                    jsonNode[C.JSONKeys.answer][C.JSONKeys.gameData][C.JSONKeys.gameTheme],
                    jsonNode[C.JSONKeys.answer][C.JSONKeys.gameData][C.JSONKeys.gameEnding],
                    true, (bool ok, JSONNode response) =>
                    {
                        success = ok;
                        ready = true;
                    });

                // Várunk amíg az adatokat elküldjük a szervernek
                while (!ready) yield return null;

                // Ha hiba volt a küldés során, akkor a felhasználót értesítsük, hogy újra küldjük az adatokat
                if (!success)
                {
                    ready = false; // A felhasználó érteséítéséhez

                    ErrorPanel.instance.Show(
                        Common.languageController.Translate(C.Texts.gameResultResend),
                        Common.languageController.Translate(C.Texts.Ok),
                        callBack: (string buttonName) =>
                        {
                            ready = true;
                        });

                    // Várunk amíg a felhasználó tudomásul vesz az újra küldést
                    while (!ready) yield return null;

                    ready = false; // Nem fejezzük be a do while ciklust, hanem ismétlünk
                }

            } while (!ready);


            // Eltüntetjük az error panelt
            ready = false;
            ErrorPanel.instance.Hide(() =>
            {
                ready = true;
            });

            // Várunk amíg az errorPanel eltűnik
            while (!ready) yield return null;

        } while (true);
        //

        if (callBack != null)
            callBack();
    }

    /*
    public IEnumerator PlayLearnPathByNextGameModeCoroutine_Old(CurriculumPathData curriculumPathData, Common.CallBack callBack)
    {
        //Common.configurationController.answerFeedback = (ConfigurationController.AnswerFeedback)curriculumPathData.gameEnding - 1;
        //Common.configurationController.designType = (ConfigurationController.DesignType)curriculumPathData.gameTheme - 1;

        // Megvizsgáljuk, hogy befejeződött-e az útvonal és van-e lehetőség az újrakezdésre
        bool restart = false;   // restart lett kiválasztva?
        bool ready = false;     // Mehetünk tovább a coroutine metódusban

        // Ha már befejezte a tanulási útvonalat
        if (curriculumPathData.progress == 100)
        {
            // Ha újra kezdhető az útvonal, akkor lesz Restart button is egyébként csak Ok gomb amivel tudomásul vehetjük, hogy teljesítve van és nem lehet újra kezdeni
            string button2 = (curriculumPathData.replayable) ? Common.languageController.Translate(C.Texts.restart) : null;

            ErrorPanel.instance.Show(
                Common.languageController.Translate(C.Texts.ThisLearnRouteIsFinished),
                Common.languageController.Translate(C.Texts.Ok),
                button2Text: button2,
                callBack: (string buttonName) =>
                {
                    restart = buttonName == "button2";
                    ready = true;
                });

            // Várunk amíg a felhasználó meghozza a döntését a folytatásról vagy az újra kezdésről
            while (!ready) yield return null;

            // Eltüntetjük a panel-t
            ready = false;
            ErrorPanel.instance.Hide(() =>
            {
                ready = true;
            });

            // Várunk amíg a panel eltűnik
            while (!ready) yield return null;

            // Ha nem kértünk újra indítást, akkor kilépünk
            if (!restart)
            {
                callBack();
                yield break;
            }
        }

        // Újre kezdjük a tanulási útvonalat ha azt választották ki a felbukkanó ablakban vagy ha még nem volt elkezdve egyáltalán
        restart = restart || curriculumPathData.progress == 0;
        string learnRoutePathStart = restart ? curriculumPathData.newLearnRoutePathIsolationTimeForContinue : curriculumPathData.lastLearnRoutePathIsolationTimeForContinue;

        bool notMoreGame = false;
        do
        {
            GameData gameData = null;
            JSONNode pathData = null;

            ready = false;

            ClassYServerCommunication.instance.GetNextGame(
                curriculumPathData.ID,
                curriculumPathData.mailListID,
                learnRoutePathStart, // curriculumPathData.newLearnRoutePathIsolationTimeForContinue, 
                (bool success, JSONNode response) => {
                    // Ha az answer-ben egy osztály található, akkor van még játék az útvonalban
                    if (response[C.JSONKeys.answer] is JSONClass)
                    {
                        // A json-ban található játékot feldolgozzuk
                        gameData = new GameData(response[C.JSONKeys.answer][C.JSONKeys.gameData]);

                        // Megjegyezzük a pathData tartalmát, mivel azt vissza kell küldeni a gameData-val együtt
                        pathData = response[C.JSONKeys.answer][C.JSONKeys.pathData];
                    }
                    else
                    {
                        notMoreGame = true;
                    }

                    ready = true;
                }
                );

            // Várunk amíg a játék adatok megérkeznek
            while (!ready) yield return null;

            if (notMoreGame) break;

            // A GameMenu gombjainak láthatóságát beállítjuk
            GameMenu.instance.SetPreviousButton(false);
            GameMenu.instance.SetNextButton(true);

            // Lejátszuk a játékot a kezdő és befejező időpontokat megjegyezzük
            System.DateTime gameStartdateTime = System.DateTime.Now;
            string gameStart = Common.TimeStamp();
            yield return StartCoroutine(PlayGame(gameData));
            string gameEnd = Common.TimeStamp(gameStartdateTime.AddSeconds(Common.taskController.elapsedGameTime));

            Debug.Log("Game result : " + Common.taskController.resultPercent + "*********************************************************************************************");

            // Megvizsgáljuk, hogy a GameMenu-ben milyen gombot nyomtak, ha nyomtak valamit
            switch (GameMenu.instance.selectedButton)
            {
                case C.Program.GameMenuExit: // Ha a játékból kiléptek, akkor kilépünk
                    if (callBack != null)
                        callBack();
                    yield break;

                case C.Program.GameMenuNext: // Ha a következő tananyag gombot választották
                    break;
            }

            gameData.played = true;
            gameData.lastGamePercent = Common.taskController.resultPercent;

            do
            {
                // A játék eredményét elküldjük a szervernek
                bool error = false;
                ready = false;

                ClassYServerCommunication.instance.addGameLogServer2020(
                    pathData,
                    Common.taskController.screensEvaluations,
                    gameData.ID.ToString(),
                    Common.taskController.resultPercent, gameStart, gameEnd, Common.taskController.extraData,
                    true, (bool success, JSONNode response) =>
                    {
                        error = !success;
                        ready = true;
                    });

                // Várunk amíg az adatokat elküldjük a szervernek
                while (!ready) yield return null;

                // Ha hiba volt a küldés során, akkor a felhasználót értesítsük, hogy újra küldjük az adatokat
                if (error)
                {
                    ready = false;

                    ErrorPanel.instance.Show(
                        Common.languageController.Translate(C.Texts.gameResultResend),
                        Common.languageController.Translate(C.Texts.Ok),
                        callBack: (string buttonName) =>
                        {
                            ready = true;
                        });

                    // Várunk amíg a felhasználó tudomásul vesz az újra küldést
                    while (!ready) yield return null;

                    ready = false; // Nem fejezzük be a do while ciklust, hanem ismétlünk
                }

            } while (!ready);


            // Eltüntetjük az error panelt
            ready = false;
            ErrorPanel.instance.Hide(() =>
            {
                ready = true;
            });

            // Várunk amíg az errorPanel eltűnik
            while (!ready) yield return null;

            //loopEnd = true;
        } while (true);
        //

        if (callBack != null)
            callBack();
    }
    */

    /// <summary>
    /// Folytatja a megadott tanulási útvonalat
    /// </summary>
    public void PlayLearnPath(CurriculumPathData curriculumPathData, Common.CallBack callBack)
    {
        evaluateButtonName = "";
        StartCoroutine(PlayLearnPathCoroutine(curriculumPathData, callBack));
    }

    IEnumerator PlayLearnPathCoroutine(CurriculumPathData curriculumPathData, Common.CallBack callBack)
    {
        bool restart = false;
        bool ready = false;

        // Ha már befejezte a tanulási útvonalat
        if (curriculumPathData.progress == 100)
        {
            // Ha újra kezdhető az útvonal, akkor lesz Restart button is egyébként csak Ok gomb amivel tudomásul vehetjük, hogy teljesítve van és nem lehet újra kezdeni
            string button2 = (curriculumPathData.replayable) ? Common.languageController.Translate(C.Texts.restart) : null;

            ErrorPanel.instance.Show(
                Common.languageController.Translate(C.Texts.ThisLearnRouteIsFinished),
                Common.languageController.Translate(C.Texts.Ok),
                button2Text: button2,
                callBack: (string buttonName) =>
                {
                    restart = buttonName == "button2";
                    ready = true;
                });

            // Várunk amíg a felhasználó meghozza a döntését a folytatásról vagy az újra kezdésről
            while (!ready) yield return null;

            // Eltüntetjük a panel-t
            ready = false;
            ErrorPanel.instance.Hide(() =>
            {
                ready = true;
            });

            // Várunk amíg a panel eltűnik
            while (!ready) yield return null;

            // Ha nem kértünk újra indítást, akkor kilépünk
            if (!restart) {
                callBack();
                yield break;
            }
        }

        // Összeszámoljuk, hogy mennyi tanyagak van hátra
        int curriculumCount = 0;
        for (int i = 0; i < curriculumPathData.listOfCurriculumInfo.Count; i++)
        {
            if (curriculumPathData.listOfCurriculumInfo[i].maxCurriculumProgress < 100)
                curriculumCount++;
        }

        // Újre kezdjük a tanulási útvonalat ha azt választották ki a felbukkanó ablakban vagy ha még nem volt elkezdve egyáltalán
        restart = restart || curriculumPathData.progress == 0;
        string learnRoutePathStart = restart ? curriculumPathData.newLearnRoutePathIsolationTimeForContinue : curriculumPathData.lastLearnRoutePathIsolationTimeForContinue;

        // Ha újrakezdés vagy kezdés van, akkor töröljük a korábbi tananyag eredményeket
        if (restart) {
            foreach (CurriculumInfoForCurriculumPath curriculumInfo in curriculumPathData.listOfCurriculumInfo)
            {
                curriculumInfo.SetMaxCurriculumProgress(0);
            }
        }

        // Megkeressük az utolsó lejátszott tananyagot és onnan folytatjuk
        int act = 0;
        for (int i = 0; i < curriculumPathData.listOfCurriculumInfo.Count; i++)
        {
            // Ha nem fejeztük még be, akkor onnan folytatjuk
            if (curriculumPathData.listOfCurriculumInfo[i].maxCurriculumProgress > 0)
                act = i;

            // Ha már befejeztük, akkor a következőnél folytatjuk
            if (curriculumPathData.listOfCurriculumInfo[i].maxCurriculumProgress == 100)
                act = i + 1;
        }

        // Megvizsgálni, hogy melyik tananyagot kell folytatni
        // Végig megyünk a tananyagok listáján amelyik nem 100%-os azt folytatjuk
        for (int i = act; i < curriculumPathData.listOfCurriculumInfo.Count; i++)
        {
            CurriculumInfoForCurriculumPath curriculumInfo = curriculumPathData.listOfCurriculumInfo[i];

            if (curriculumInfo.maxCurriculumProgress < 100)
            {
                yield return StartCoroutine(PlayCurriculumCoroutine(
                    curriculumPathData.ID,
                    learnRoutePathStart,
                    curriculumInfo.subjectID,
                    curriculumInfo.topicID,
                    curriculumInfo.courseID,
                    curriculumInfo.curriculumID,
                    null,
                    //() => { ready = true; },
                    true,
                    curriculumCount > 1)
                );

                curriculumCount--;

                // Megvizsgáljuk, hogy az értékelő képernyőn vajon mit nyomtak
                if (evaluateButtonName == C.Program.EvaluateNext)
                    continue;
                else
                    break;
            }
        }

        if (callBack != null)
            callBack();
    }

    public void PlayGameProjectProduct(JSONNode json, Common.CallBack callBack)
    {
        //GameData gameData = new GameData(response[C.JSONKeys.answer]);
    }

    public IEnumerator PlayGameProjectProductdCoroutine(JSONNode json)
    {
        GameData gameData = new GameData(json[C.JSONKeys.answer]);

        yield return StartCoroutine(PlayGame(gameData));
    }

    public IEnumerator PlayGame(GameData gameData)
    {
        Common.configurationController.answerFeedback = (ConfigurationController.AnswerFeedback)gameData.gameEnding - 1;
        Common.configurationController.perfectAnswer = Common.configurationController.answerFeedback == ConfigurationController.AnswerFeedback.PerfectPlay;
        if (Common.configurationController.perfectAnswer)
            Common.configurationController.answerFeedback = ConfigurationController.AnswerFeedback.Immediately;

        Common.configurationController.designType = (ConfigurationController.DesignType)gameData.gameTheme - 1;

        Common.configurationController.playAnimation = gameData.playAnimation;

        Common.configurationController.textComponentType = gameData.isLatex ? ConfigurationController.TextComponentType.TEXDraw : ConfigurationController.TextComponentType.Text;

        do
        {
            // Elkészítjük a lecketervet
            LessonPlanData lessonPlanData = new LessonPlanData();
            LessonMosaicData lessonMosaicData = new LessonMosaicData();
            lessonMosaicData.listOfGames.Add(gameData);
            lessonPlanData.lessonMosaicsList.Add(lessonMosaicData);

            Common.configurationController.lessonPlanData = lessonPlanData;

            gameData.lessonMosaicIndex = 0;
            gameData.gameIndex = 0;

            bool ready = false;
            Common.taskController.PlayGameInServer(gameData, 0, () => {
                ready = true;
            });

            // Várunk amíg az előzőleg elindított játék befejeződik
            while (!ready) yield return null;

            // Ha tökéletes megoldást várunk, akkor addig megy ugyan az a játék, amíg kisebb mint 100% az eredmény és
            // nem nyomtak meg valamelyik gombot a GameMenu-ben.
        } while (
            Common.configurationController.perfectAnswer && 
            Common.taskController.resultPercent < 100 &&
            GameMenu.instance.selectedButton == "");

    }

    // Egy tananyagot játszik le a 2020-as szerveren
    public void PlayCurriculumServer2020(string subjectID, string topicID, string courseID, CurriculumItemDriveData curriculumData, Common.CallBack callBack, bool nextCurriculum = false)
    {
        StartCoroutine(PlayCurriculumServer2020Coroutine(subjectID, topicID, courseID, curriculumData, callBack));
    }

    // Egy tananyagot játszik le a 2020-as szerveren
    public IEnumerator PlayCurriculumServer2020Coroutine(string subjectID, string topicID, string courseID, CurriculumItemDriveData curriculumData, Common.CallBack callBack)
    {
        int getNextGameCounter = 0; // Hányadik GetNextGame lekérdezésnél tartunk

        bool restart = false;   // restart lett kiválasztva?

        bool loggedGame = false;    // Egy korábbi játékot játszunk le?
        string logID = "";          // Az újra játszandó játék logID-ja

        bool evaluateIsEnabled = true;

        bool ready = false;     // Mehetünk tovább a coroutine metódusban

        bool success = true;        // Sikerült-e a szerver kommunikáció
        JSONNode jsonNode = null;   // A szerver által küldött json

        GameData gameData = null;

        string previousLogID = null;
        string nextLogID = null;

        string isolationTime = curriculumData.lastCurriculumIsolation;

        Common.CallBack_In_Bool_JSONNode gameDataResponseProcess =
            (bool ok, JSONNode response) =>
            {
                success = ok;
                jsonNode = response;
                ready = true;
            };

        // Ha van lastCurriculumIsolation
        if (curriculumData.lastCurriculumIsolation != null &&
           curriculumData.lastCurriculumIsolation != "-1" &&
           curriculumData.lastCurriculumIsolation != "null")
        {
            // Lekérdez a GetNextPracticeGame-t
            ClassYServerCommunication.instance.GetNextPracticeGame(
                subjectID,
                topicID,
                courseID,
                curriculumData.curriculumID,
                isolationTime,
                gameDataResponseProcess);

            // Várunk amíg a játék adatok megérkeznek
            while (!ready) yield return null;

            // Ha hiba történt, akkor kilépünk a metódusból
            if (!success) yield break; // ---------------------------- EXIT -------------------------------------

            // Ha még nincs befejezve a tanulási útvonal, akkor kell lennie gameData-nak
            if (!jsonNode[C.JSONKeys.answer][C.JSONKeys.statusData][C.JSONKeys.isComplete].AsBool)
            {
                // A json-ban található játékot feldolgozzuk
                gameData = new GameData(jsonNode[C.JSONKeys.answer][C.JSONKeys.gameData]);

                // Megkérdezzük, hogy újrakezdés vagy folytatás legyen
                ready = false;
                ErrorPanel.instance.Show(
                    Common.languageController.Translate(C.Texts.curriculumPlayWasStopped),
                    Common.languageController.Translate(C.Texts.continue_),
                    button2Text: Common.languageController.Translate(C.Texts.restart),
                    callBack: (string buttonName) =>
                    {
                        restart = buttonName == "button2";
                        ready = true;
                    });

                // Várunk amíg a felhasználó meghozza a döntését a folytatásról vagy az újra kezdésről
                while (!ready) yield return null;

                // Eltüntetjük a panel-t
                ready = false;
                ErrorPanel.instance.Hide(() =>
                {
                    ready = true;
                });

                // Várunk amíg teljesen eltünt az error panel
                while (!ready) yield return null;

                // Ha a restart-ot választottuk, akkor töröljük a gameData-t, ez jelzi, hogy újra le kell kérdezni a getNextGame-t
                if (restart)
                {
                    isolationTime = curriculumData.newCurriculumIsolation;
                    gameData = null;
                }
            }
            else
            {
                // Ha nincs több játék az utolsó tananyag lejátszásban, akkor újra kezdjük
                isolationTime = curriculumData.newCurriculumIsolation;
            }
        }
        else
        {
            // Ha még nem játszottak ezzel a tananyaggal, tehát nicns lastCurriculumIsolation
            isolationTime = curriculumData.newCurriculumIsolation;
        }

        do
        {
            getNextGameCounter++;

            ready = false;

            // Ha még nincs gameData, akkor letöltjük
            if (gameData == null)
            {
                if (!loggedGame)
                {
                    ClassYServerCommunication.instance.GetNextPracticeGame(
                        subjectID,
                        topicID,
                        courseID,
                        curriculumData.curriculumID,
                        isolationTime,
                        gameDataResponseProcess);
                }
                else
                {
                    ClassYServerCommunication.instance.GetPracticeLoggedGame(
                        courseID,
                        curriculumData.curriculumID,
                        logID,
                        gameDataResponseProcess);
                }

                // Várunk amíg a játék adatok megérkeznek
                while (!ready) yield return null;

                // Ha hiba történt, akkor kilépünk a ciklusból
                if (!success) break; // ---------------------------- EXIT -------------------------------------

                // Ha az answer-ben egy osztály található, akkor van még játék az útvonalban
                if (!jsonNode[C.JSONKeys.answer][C.JSONKeys.statusData][C.JSONKeys.isComplete].AsBool || loggedGame)
                {
                    // A json-ban található játékot feldolgozzuk
                    gameData = new GameData(jsonNode[C.JSONKeys.answer][C.JSONKeys.gameData]);
                }
                else
                {
                    // Nincs több játék üzenetet kaptunk a szervertől
                    if (getNextGameCounter == 1)
                    {
                        // Ha az első lekérdezésnél már nem kaptam adatokat, akkor hibaüzenetbe közöljük a felhasználóval, hogy ebben a tanulási útvonalban egyetlen játék sincsen
                        // A tananyag nem tartalmaz játékokat 
                        // This curriculum not consists games
                        // This curriculum does not include games
                        ready = false;
                        ErrorPanel.instance.Show(
                            Common.languageController.Translate(C.Texts.ThisCurriculumDoesNotIncludeGames),
                            Common.languageController.Translate(C.Texts.Ok),
                            callBack: (string buttonName) => { ready = true; });

                        // Várunk amíg a felhasználó meghozza a döntését a folytatásról vagy az újra kezdésről
                        while (!ready) yield return null;

                        // Eltüntetjük a panel-t
                        ready = false;
                        ErrorPanel.instance.Hide(() => { ready = true; });

                        // Várunk amíg a panel eltűnik
                        while (!ready) yield return null;
                    }
                    else
                    {
                        // Itt mutathatnánk valami értékelő képernyőt ------------------------
                    }

                    // kilépünk a játékokat játszó ciklusból
                    break; // ---------------------------- EXIT -------------------------------------
                }
            }

            JSONClass statusData = jsonNode[C.JSONKeys.answer][C.JSONKeys.statusData].AsObject;
            // Megmutatjuk az értékelő képernyőt
            if (statusData[C.JSONKeys.show].AsBool &&
                getNextGameCounter != 1 &&
                evaluateIsEnabled)
            {
                yield return StartCoroutine(ShowEvaluatePanel(statusData));

                // Ha a kilépést választották, akkor kilépünk
                if (evaluateButtonName == "EvaluateExit") break; // ----------------------------EXIT-------------------------------------
            }

            // Ha nincs valamelyik ID meghatározva a json-ban, akkor -1 lesz az értéke alapértelmezetten
            previousLogID = jsonNode[C.JSONKeys.answer][C.JSONKeys.pathData].ContainsKey(C.JSONKeys.previousLogID) ?
                jsonNode[C.JSONKeys.answer][C.JSONKeys.pathData][C.JSONKeys.previousLogID].Value : "-1";
            nextLogID = jsonNode[C.JSONKeys.answer][C.JSONKeys.pathData].ContainsKey(C.JSONKeys.nextLogID) ?
                jsonNode[C.JSONKeys.answer][C.JSONKeys.pathData][C.JSONKeys.nextLogID].Value : "-1";

            // A GameMenu gombjainak láthatóságát beállítjuk
            GameMenu.instance.SetPreviousButton(previousLogID != "-1");
            GameMenu.instance.SetNextButton(true);

            // Lejátszuk a játékot a kezdő és befejező időpontokat megjegyezzük
            System.DateTime gameStartdateTime = System.DateTime.Now;
            string gameStart = Common.TimeStamp();
            yield return StartCoroutine(PlayGame(gameData));
            string gameEnd = Common.TimeStamp(gameStartdateTime.AddSeconds(Common.taskController.elapsedGameTime));

            evaluateIsEnabled = GameMenu.instance.selectedButton == "";

            Debug.Log("Game result : " + Common.taskController.resultPercent + "*********************************************************************************************");

            // Megvizsgáljuk, hogy a GameMenu-ben milyen gombot nyomtak, ha nyomtak valamit
            switch (GameMenu.instance.selectedButton)
            {
                case C.Program.GameMenuExit: // Ha a játékból kiléptek, akkor kilépünk
                    if (callBack != null)
                        callBack();
                    yield break;

                case "": // Ha nem nyomtak semmit, akkor az Next-et jelent
                case C.Program.GameMenuNext: // Ha a következő tananyag gombot választották
                    // Ha van nextLogID, akkor egy loggolt játékot kérünk
                    logID = nextLogID;
                    loggedGame = nextLogID != "-1";
                    break;

                case C.Program.GameMenuPrevious:
                    // Elvileg ha van előző gomb, akkor kell lennie previousLogID-nak is
                    loggedGame = true;
                    logID = previousLogID;
                    gameData = null;
                    continue; // ---------------------------- CONTINUE -------------------------------------
                    break;
            }

            gameData.played = true;
            gameData.lastGamePercent = Common.taskController.resultPercent;


            // Akkor nem kellene logot küldeni újra, ha egy logolt játék volt betöltve, de egy válaszra sem lett válaszolva
            // ?????? Honnan tudjuk, hogy egy válasz sem lett megadva?

            do
            {
                // A játék eredményét elküldjük a szervernek
                ready = false;

                ClassYServerCommunication.instance.addGameLogServer2020(
                    jsonNode[C.JSONKeys.answer][C.JSONKeys.pathData],
                    Common.taskController.screensEvaluations,
                    gameData.ID.ToString(),
                    Common.taskController.resultPercent, gameStart, gameEnd, Common.taskController.extraData,
                    jsonNode[C.JSONKeys.answer][C.JSONKeys.gameData][C.JSONKeys.gameTheme],
                    jsonNode[C.JSONKeys.answer][C.JSONKeys.gameData][C.JSONKeys.gameEnding],
                    true, (bool ok, JSONNode response) =>
                    {
                        success = ok;
                        ready = true;
                    });

                // Várunk amíg az adatokat elküldjük a szervernek
                while (!ready) yield return null;

                // Ha hiba volt a küldés során, akkor a felhasználót értesítsük, hogy újra küldjük az adatokat
                if (!success)
                {
                    ready = false; // A felhasználó érteséítéséhez

                    ErrorPanel.instance.Show(
                        Common.languageController.Translate(C.Texts.gameResultResend),
                        Common.languageController.Translate(C.Texts.Ok),
                        callBack: (string buttonName) =>
                        {
                            ready = true;
                        });

                    // Várunk amíg a felhasználó tudomásul vesz az újra küldést
                    while (!ready) yield return null;

                    ready = false; // Nem fejezzük be a do while ciklust, hanem ismétlünk
                }

            } while (!ready);

            // Eltüntetjük az error panelt
            ready = false;
            ErrorPanel.instance.Hide(() => { ready = true; });

            // Várunk amíg az errorPanel eltűnik
            while (!ready) yield return null;

            gameData = null;

        } while (true);
        //

        if (callBack != null)
            callBack();
    }

    /// <summary>
    /// Lejátsza a megadott tananyagot
    /// </summary>
    /// <param name="subjectID"></param>
    /// <param name="topicID"></param>
    /// <param name="courseID"></param>
    /// <param name="curriculumID"></param>
    /// <param name="callBack"></param>
    /// <param name="forceContinue">Ha ez be van kapcsolva, akkor folytatja ott ahol abbahagyta, ha nincs bekapcsolva, akkor megkérdi, hogy folytassa-e.</param>
    /// <param name="nextCurriculum">Az értékelő képernyő legyen-e 'következő tananyag' gomb</param>
    public void PlayCurriculum(string learnRoutePathID, string learnRoutePathStart, string subjectID, string topicID, string courseID, string curriculumID, Common.CallBack callBack, bool forceContinue = false, bool nextCurriculum = false)
    {
        StartCoroutine(PlayCurriculumCoroutine(learnRoutePathID, learnRoutePathStart, subjectID, topicID, courseID, curriculumID, callBack, forceContinue, nextCurriculum));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="learnRoutePathID"></param>
    /// <param name="learnRoutePathStart"></param>
    /// <param name="subjectID"></param>
    /// <param name="topicID"></param>
    /// <param name="courseID"></param>
    /// <param name="curriculumID"></param>
    /// <param name="callBack"></param>
    /// <param name="forceContinue">Ha ez be van kapcsolva, akkor folytatja ott ahol abbahagyta, ha nincs bekapcsolva, akkor megkérdi, hogy folytassa-e. Ha útvonalban játszuk le a tananyagot, akkor ez true, ha a tananyag listából indítottuk, akkor ez false</param>
    /// <param name="nextCurriculum">Az eredmény képernyőn legyen-e következő tananyag gomb. Ha útvonalban játszottuk le, akkor van ez a gomb, ha van kötvetkező tananyag. Ha a tananyag listából indítottuk, akkor nincs. </param>
    /// <returns></returns>
    IEnumerator PlayCurriculumCoroutine(string learnRoutePathID, string learnRoutePathStart, string subjectID, string topicID, string courseID, string curriculumID, Common.CallBack callBack, bool forceContinue = false, bool nextCurriculum = false)
    {
        evaluateButtonName = "";

        CurriculumData curriculumData = null;

        // Letöltjük a lejátszani kívánt tananyagot
        bool ready = false;
        ClassYServerCommunication.instance.GetCurriculumForPlay(learnRoutePathID, learnRoutePathStart, courseID, curriculumID, true,
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

        // Ha sikerült a letöltés és van tervetett játék
        if (curriculumData != null && curriculumData.plannedGames.Count > 0)
        {
            // Megvizsgáljuk, hogy folytatható-e a tananyag
            // Ha találunk egy játékot amit már lejátszottak és van olyan is amit még nem, akkor van mit folytatni
            bool played = false;
            bool unplayed = false;
            int nextPlannedGameIndex = curriculumData.plannedGames.Count; // Melyik az első olyan játék amivel még nem játszott
            for (int i = curriculumData.plannedGames.Count - 1; i >= 0; i--)
            {
                if (curriculumData.plannedGames[i].played)
                    played = true;
                else {
                    unplayed = true;
                    nextPlannedGameIndex = i;
                }
            }

            // Ha van játszott játék és van olyan is amit még nem játszottak, 
            // akkor megkérdezzük a felhasználót, hogy újat kezd vagy szeretné folytatni a korábbit
            bool restart = true; // Mit szeretne a felhasználó
            if (forceContinue)
            {
                // Restartott csinálunk ha még nem volt elkezdve a játék
                restart = !played;
            }
            else if (played && unplayed)
            {
                ready = false;
                ErrorPanel.instance.Show(
                    Common.languageController.Translate(C.Texts.curriculumPlayWasStopped),
                    Common.languageController.Translate(C.Texts.continue_),
                    button2Text: Common.languageController.Translate(C.Texts.restart),
                    callBack: (string buttonName) =>
                    {
                        restart = buttonName == "button2";
                        ready = true;
                    });

                // Várunk amíg a felhasználó meghozza a döntését a folytatásról vagy az újra kezdésről
                while (!ready) yield return null;

                // Eltüntetjük a panel-t
                ready = false;
                ErrorPanel.instance.Hide(() => {
                    ready = true;
                });
            }

            // Várunk amíg teljesen eltünt az error panel
            while (!ready) yield return null;

            // Ha restart van, akkor töröljük a korábbi eredményeket a tananyagok játékaiban
            string curriculumStart = curriculumData.lastCurriculumIsolationTimeForContinue;
            if (restart)
            {
                foreach (GameData gameData in curriculumData.plannedGames)
                {
                    gameData.played = false;
                    gameData.lastGamePercent = 0;
                }

                foreach (GameData gameData in curriculumData.automatedGames)
                {
                    gameData.played = false;
                    gameData.lastGamePercent = 0;
                }

                curriculumStart = curriculumData.newCurriculumIsolationTimeForContinue;
                nextPlannedGameIndex = 0;
            }

            /*
            // Elkészítjük a lecketervet
            List<GameData> listOfGames = new List<GameData>(); // curriculumData.automatedGames);
            listOfGames.AddRange(curriculumData.plannedGames);

            LessonPlanData lessonPlanData = new LessonPlanData();
            LessonMosaicData lessonMosaicData = new LessonMosaicData();
            lessonMosaicData.listOfGames = listOfGames;
            lessonPlanData.lessonMosaicsList.Add(lessonMosaicData);

            Common.configurationController.lessonPlanData = lessonPlanData;
            */

            // Sorban elindítjuk a még nem lejátszott játékokat
            while (nextPlannedGameIndex < curriculumData.plannedGames.Count)
            {
                GameData gameData = curriculumData.plannedGames[nextPlannedGameIndex];

                GameMenu.instance.SetPreviousButton(nextPlannedGameIndex > 0);
                GameMenu.instance.SetNextButton(true);

                System.DateTime gameStartdateTime = System.DateTime.Now;
                string gameStart = Common.TimeStamp();
                yield return StartCoroutine(PlayGame(gameData));
                string gameEnd = Common.TimeStamp(gameStartdateTime.AddSeconds(Common.taskController.elapsedGameTime));
                //string gameEnd = Common.TimeStamp();

                Debug.Log("Game result : " + Common.taskController.resultPercent + "*********************************************************************************************");

                // Megvizsgáljuk, hogy a GameMenu-ben milyen gombot nyomtak, ha nyomtak valamit
                switch (GameMenu.instance.selectedButton)
                {
                    case C.Program.GameMenuExit: // Ha a játékból kiléptek, akkor kilépünk
                        if (callBack != null)
                            callBack();
                        yield break;

                    case C.Program.GameMenuPrevious: // Ha az előző játék gombját választották
                        nextPlannedGameIndex--;
                        continue;

                    case C.Program.GameMenuNext: // Ha a következő tananyag gombot választották
                        // Megvizsgáljuk, hogy a játék játszva volt-e már, ha igen, akkor nem küldünk eredményt a szervernek kivéve ha YouTube vagy PDF játékról van szó
                        if (gameData.played &&
                            gameData.gameEngine != GameData.GameEngine.YouTube &&
                            gameData.gameEngine != GameData.GameEngine.PDF)
                        {
                            nextPlannedGameIndex++;
                            continue;
                        }
                        break;
                }

                gameData.played = true;
                gameData.lastGamePercent = Common.taskController.resultPercent;

                // Kiszámoljuk a tananyag haladási progress-t
                // Megszámoljuk hány játékot fejeztünk már be
                int finishedGames = 0;
                foreach (GameData gameDat in curriculumData.plannedGames)
                {
                    if (gameDat.played)
                        finishedGames++;
                }

                ready = false;
                do
                {
                    // A játék eredményét elküldjük a szervernek
                    bool error = false;
                    ready = false;
                    ClassYServerCommunication.instance.addGameLog(learnRoutePathID, learnRoutePathStart, subjectID, topicID, courseID, curriculumID, gameData.ID.ToString(),
                        Common.taskController.resultPercent, gameStart, gameEnd, Common.taskController.extraData,
                        curriculumStart,
                        (float)finishedGames / curriculumData.plannedGames.Count * 100, true, (bool success, JSONNode response) => {
                            error = !success;
                            ready = true;
                        });

                    // Várunk amíg az adatokat elküldjük a szervernek
                    while (!ready) yield return null;

                    // Ha hiba volt a küldés során, akkor a felhasználót értesítsük, hogy újra küldjük az adatokat
                    if (error)
                    {
                        ready = false;

                        ErrorPanel.instance.Show(
                            Common.languageController.Translate(C.Texts.gameResultResend),
                            Common.languageController.Translate(C.Texts.Ok),
                            callBack: (string buttonName) => {
                                ready = true;
                            });

                        // Várunk amíg a felhasználó tudomásul vesz az újra küldést
                        while (!ready) yield return null;

                        ready = false; // Nem fejezzük be a do while ciklust, hanem ismétlünk
                    }

                } while (!ready);

                // Eltüntetjük az error panelt
                ready = false;
                ErrorPanel.instance.Hide(() => {
                    ready = true;
                });

                // Várunk amíg az errorPanel eltűnik
                while (!ready) yield return null;

                nextPlannedGameIndex++;
            }

            // Összeadjuk az eredményt, hogy megtudjuk mutatni
            float sumPercent = 0;
            int gameNumber = 0; // A pdf, Youtube és a Psycho játék eredménye nem számít, tehát kevesebb game lehet mint ami a plannedGames-ben megtalálható
            foreach (GameData gameData in curriculumData.plannedGames)
                if (gameData.gameEngine != GameData.GameEngine.PDF &&
                    gameData.gameEngine != GameData.GameEngine.YouTube &&
                    gameData.gameEngine != GameData.GameEngine.Psycho)
                {
                    sumPercent += gameData.lastGamePercent;
                    gameNumber++;
                }

            //sumPercent = 0;
            //gameNumber = 0;

            sumPercent = (gameNumber != 0) ? sumPercent / gameNumber : 0;

            //sumPercent /= gameNumber;

            int starNumber = 0;                 // -
            if (sumPercent >= 15) starNumber++; // *
            if (sumPercent >= 60) starNumber++; // **
            if (sumPercent >= 80) starNumber++; // ***

            ready = false;
            GameEvaluation.instance.Show(starNumber, (gameNumber == 0) ? null : (int?)sumPercent, nextCurriculum, (string buttonName) => {
                evaluateButtonName = buttonName;
                ready = true;
                GameEvaluation.instance.HideImmediatelly();
            });

            // Várunk amíg az értékelő panelen válaszottak
            while (!ready) yield return null;
        }

        if (callBack != null)
            callBack();
    }

    /*
    /// <summary>
    /// 
    /// </summary>
    /// <param name="subjectID"></param>
    /// <param name="topicID"></param>
    /// <param name="courseID"></param>
    /// <param name="curriculumID"></param>
    /// <param name="callBack"></param>
    /// <param name="forceContinue">Ha ez be van kapcsolva, akkor folytatja ott ahol abbahagyta, ha nincs bekapcsolva, akkor megkérdi, hogy folytassa-e.</param>
    /// <returns></returns>
    IEnumerator PlayCurriculumCoroutine(string subjectID, string topicID, string courseID, string curriculumID, Common.CallBack callBack, bool forceContinue = false, bool nextCurriculum = false)
    {
        CurriculumData curriculumData = null;

        // Letöltjük a lejátszani kívánt tananyagot
        bool ready = false;
        ClassYServerCommunication.instance.GetCurriculumForPlay(courseID, curriculumID, true,
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
        if (curriculumData != null && curriculumData.plannedGames.Count > 0)
        {
            // Megvizsgáljuk, hogy folytatható-e a tananyag
            // Ha találunk egy játékot amit már lejátszottak és van olyan is amit még nem, akkor van mit folytatni
            bool played = false;
            bool unplayed = false;
            int firstUnplayedGameIndex = -1;
            for (int i = curriculumData.plannedGames.Count - 1; i >= 0; i--)
            {
                if (curriculumData.plannedGames[i].played)
                    played = true;
                else
                    unplayed = true;
            }

            // Ha van játszott játék és van olyan is amit még nem játszottak, 
            // akkor megkérdezzük a felhasználót, hogy újat kezd vagy szeretné folytatni a korábbit
            bool restart = true; // Mit szeretne a felhasználó
            if (forceContinue) {
                restart = false;
            }
            else if (played && unplayed)
            {
                ready = false;
                ErrorPanel.instance.Show(
                    Common.languageController.Translate(C.Texts.curriculumPlayWasStopped),
                    Common.languageController.Translate(C.Texts.continue_),
                    button2Text: Common.languageController.Translate(C.Texts.restart),
                    callBack: (string buttonName) =>
                    {
                        restart = buttonName == "button2";
                        ready = true;
                    });

                // Várunk amíg a felhasználó meghozza a döntését a folytatásról vagy az újra kezdésről
                while (!ready) yield return null;

                // Eltüntetjük a panel-t
                ready = false;
                ErrorPanel.instance.Hide(() => {
                    ready = true;
                });
            }

            // Várunk amíg teljesen eltünt az error panel
            while (!ready) yield return null;

            // Ha restart van, akkor töröljük a korábbi eredményeket a tananyagok játékaiban
            string curriculumStart = curriculumData.lastCurriculumIsolationTimeForContinue;
            if (restart)
            {
                foreach (GameData gameData in curriculumData.plannedGames)
                {
                    gameData.played = false;
                    gameData.lastGamePercent = 0;
                }

                foreach (GameData gameData in curriculumData.automatedGames)
                {
                    gameData.played = false;
                    gameData.lastGamePercent = 0;
                }

                curriculumStart = curriculumData.newCurriculumIsolationTimeForContinue;
            }

            // Elkészítjük a lecketervet
            List<GameData> listOfGames = new List<GameData>(); // curriculumData.automatedGames);
            listOfGames.AddRange(curriculumData.plannedGames);

            LessonPlanData lessonPlanData = new LessonPlanData();
            LessonMosaicData lessonMosaicData = new LessonMosaicData();
            lessonMosaicData.listOfGames = listOfGames;
            lessonPlanData.lessonMosaicsList.Add(lessonMosaicData);

            Common.configurationController.lessonPlanData = lessonPlanData;

            // Sorban elindítjuk a még nem lejátszott játékokat
            for (int i = 0; i < listOfGames.Count; i++)
            {
                if (listOfGames[i].played && !restart)
                    continue;

                listOfGames[i].lessonMosaicIndex = 0;
                listOfGames[i].gameIndex = i;

                string gameStart = Common.TimeStamp();

                ready = false;
                Common.taskController.PlayGameInServer(listOfGames[i], 0, () => {
                    ready = true;
                });

                // Várunk amíg az előzőleg elindított játék befejeződik
                while (!ready) yield return null;

                string gameEnd = Common.TimeStamp();

                Debug.Log("Game result : " + Common.taskController.resultPercent + "*********************************************************************************************");

                // Ha a játékból kiléptek, akkor kilépünk
                if (GameMenu.instance.selectedButton == C.Program.GameMenuExit)
                {
                    if (callBack != null)
                        callBack();
                    yield break;
                }

                listOfGames[i].played = true;
                listOfGames[i].lastGamePercent = Common.taskController.resultPercent;

                // Kiszámoljuk a tananyag haladási progress-t
                // Megszámoljuk hány játékot fejeztünk már be
                int finishedGames = 0;
                foreach (GameData gameData in listOfGames)
                {
                    if (gameData.played)
                        finishedGames++;
                }

                ready = false;
                do
                {
                    // A játék eredményét elküldjük a szervernek
                    bool error = false;
                    ready = false;
                    ClassYServerCommunication.instance.addGameLog(subjectID, topicID, courseID, curriculumID, listOfGames[i].ID.ToString(),
                        Common.taskController.resultPercent, gameStart, gameEnd, curriculumStart,
                        (float)finishedGames / listOfGames.Count * 100, true, (bool success, JSONNode response) => {
                            error = !success;
                            ready = true;
                        });

                    // Várunk amíg az adatokat elküldjük a szervernek
                    while (!ready) yield return null;

                    // Ha hiba volt a küldés során, akkor a felhasználót értesítsük, hogy újra küldjük az adatokat
                    if (error)
                    {
                        ready = false;

                        ErrorPanel.instance.Show(
                            Common.languageController.Translate(C.Texts.gameResultResend),
                            Common.languageController.Translate(C.Texts.Ok),
                            callBack: (string buttonName) => {
                                ready = true;
                            });

                        // Várunk amíg a felhasználó tudomásul vesz az újra küldést
                        while (!ready) yield return null;

                        ready = false; // Nem fejezzük be a do while ciklust, hanem ismétlünk
                    }

                } while (!ready);

                ready = false;
                ErrorPanel.instance.Hide(() => {
                    ready = true;
                });

                // Várunk amíg az errorPanel eltűnik
                while (!ready) yield return null;
            }

            // Összeadjuk az eredményt, hogy megtudjuk mutatni
            float sumPercent = 0;
            int gameNumber = 0; // A pdf és a Youtube játék eredménye nem számít, tehát kevesebb game lehet mint ami a plannedGames-ben megtalálható
            foreach (GameData gameData in curriculumData.plannedGames)
                if (gameData.gameEngine != GameData.GameEngine.PDF &&
                    gameData.gameEngine != GameData.GameEngine.YouTube)
                {
                    sumPercent += gameData.lastGamePercent;
                    gameNumber++;
                }

            sumPercent /= gameNumber;

            int starNumber = 0;                 // -
            if (sumPercent >= 15) starNumber++; // *
            if (sumPercent >= 60) starNumber++; // **
            if (sumPercent >= 80) starNumber++; // ***

            ready = false;
            GameEvaluation.instance.Show(starNumber, (int)sumPercent, nextCurriculum, (string buttonName) => {
                this.evaluateButtonName = buttonName;
                ready = true;
                GameEvaluation.instance.HideImmediatelly();
            });

            // Várunk amíg az értékelő panelen válaszottak
            while (!ready) yield return null;
        }

        if (callBack != null)
            callBack();
    }*/

}
