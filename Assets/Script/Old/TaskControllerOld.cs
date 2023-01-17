using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using SimpleJSON;

/*
A feladatokat menedzseli.

A GetNextTask metódus vissza adja a következő feladatot JSON formában.

 


*/

public class TaskControllerOld : MonoBehaviour {

    enum ExitType       // Hogy fejeződött be a feladatok lejátszása
    {
        Undefined,      // Még nincs meghatározva
        TasksEnd,       // A feladatok elfogytak, ezért fejeződött be a feladatok lejátszása
        Exit,           // Kiléptek a feladatokból az Exit menü választásával
    }

    enum MultiPlayerGameMode
    {
        EveryoneAlone,      // Mindenki külön játszik a játékosok eredményei összeadódnak a csapatban
        ChangeForTask,      // Mindenki egy külön feladatot kap
        ChangeForSubTask,   // All feladatonként csere
        EveryoneTogether,   // Mindenki játszik egyszerre
    }

    public enum GameEvent
    {
        GoodAnswer,
        WrongAnswer,
        OutOfTime,
    }

    public TextAsset task;

    public bool exitEnabled { get; private set; } // Engedélyezett a feladatból való kilépés?

    bool playerActive;      // A játékos kezeli a játékot vagy valaki más?

    //List<TaskAncestor> questionList = new List<TaskAncestor>();
    LessonMosaicData mosaicData; // A jétékokat tartalmazó óraMozaik
    int gameCounter;        // Hányadik játéknál tartunk
    int screenCounter;      // Hányadik képernyőnél tartunk

    MultiPlayerGameMode multiPlayerGameMode; // Milyen módon folyik a multiPlayer játék

    // Hányadik alfeladatnál tartunk a multiplayergameMode-tól függően változik. 
    // Ez határozza meg, hogy mikor melyik játékos jön.
    int taskCounter;

    GameData.GameEngine gameType; // A feladat típusa

    bool preview; // Előnézetből hívták meg a játékot

    Common.CallBack callBack;       // Ezen a függvényen keresztűl szól vissza, hogy végzet a feladatok lejátszásával, vagy kiléptek belőle.

    ExitType exitType;              // Hogy fejeződött be a feladatok lejátszása 

    // Kliens tablet esetén a szervertől ezeket az adatokat fogjuk kapni a csoportba sorolásról
    [HideInInspector]
    public int groupID;         // Csoport azonosító, melyik csoportba tartozik a felhasználó
    [HideInInspector]
    public int indexInGroup;    // Csoporton belül hányadik játékos
    [HideInInspector]
    public int groupHeadCount;  // Csoport létszám. Hányan vannak a csoportba

    void Awake() {
        Common.taskControllerOld = this;

        // Ez valahonnan majd jön, de honnan ? ??????????????????????????????????????????????????
        multiPlayerGameMode = MultiPlayerGameMode.ChangeForSubTask;
        groupID = -1;
    }

    // Visszaadja a következő feladatot JSON formában
    /*
    public string GetNextTask() {
        return task.text;
    }
    */

    /*
    // Igaz értéket ad vissza, ha van még kérdés
    public bool IsLeftQuestion() {
        return screenCounter + 1 < questionList.Count;
    }
    */

    // Vissza adja a következő kérdést
    public TaskAncestor GetTask() {
        return (screenCounter < mosaicData.listOfGames[gameCounter].screens.Count) ? 
            mosaicData.listOfGames[gameCounter].screens[screenCounter] : null;
    }

    // 
    /// <summary>
    /// A megadott feladatokat lejátsza.
    /// </summary>
    /// <remarks>
    /// Meglehet adni egy játékot (GameData) vagy egy óramozaikot (LessonMosaicData)
    /// továbbá egy start indexet, hogy melyik képernyőtől, vagy melyik játéktól kezdje a lejátszást.
    /// Ez az index alapesetben nullának kell lennie, ekkor az összes játékot és feladatot lejátsza.
    /// 
    /// A tanári tablet LessonMosaicData-val fogja meghívni.
    /// Az előnézet pedig egy GameData-val.
    /// Innen tudjuk, hogy milyen üzemmódban kell dolgoznia a programnak.
    /// LessonMosaicData esetben a LessonMosaicData-ban meghatározott módon.
    /// GameData esetben pedig EveryoneAlone módban.
    /// </remarks>
    /// <param name="tasks">A GameData vagy a LessonMosaicData.</param>
    /// <param name="startIndex">Melyik elem legyen az első.</param>
    /// <param name="exitEnabled">A játékból lehetőség van-e kilépni az Exit gomb megnyomásával.</param>
    /// <param name="callBack">Melyik eljárást hívja meg ha végzett a feladatok lejátszásával.</param>
    public void PlayQuestionList(object tasks, int startIndex, bool exitEnabled, Common.CallBack callBack) {
        this.exitEnabled = exitEnabled;
        this.callBack = callBack;
        taskCounter = 0;

        if (tasks is GameData)
        {
            // preview üzemmódban vagyunk (Szerver gépen játszik a tanár)
            preview = true;

            // A GameData-t beteszem egy LessonMosaicData-ba, hogy egységesen tudjam kezelni őket
            mosaicData = new LessonMosaicData();
            mosaicData.Add(tasks as GameData);
            mosaicData.multiPlayerGameMode = LessonMosaicData.MultiPlayerGameMode.Single;
            gameCounter = 0;
            screenCounter = startIndex;
        }
        else if (tasks is LessonMosaicData) {
            // Szerver - Diák üzemmódban vagyunk
            preview = false;

            // Ha eleve LessonMosaicData-t kaptam, akkor csak kasztolom
            mosaicData = tasks as LessonMosaicData;
            gameCounter = startIndex;
            screenCounter = 0;

            // A hálózati eseményekről értesítést szeretnénk kapni
            Common.HHHnetwork.callBackNetworkEvent = ReceivedNetworkEvent;
        }
        else {
            // HIBA: ismeretlen objektumot kaptunk
            Debug.Log("GameData vagy LessonMosaicData objektumot vártam, ehelyett ez jött: " + tasks.GetType().Name);

            if (callBack != null)
                callBack();

            return;
        }

        exitType = ExitType.Undefined;

        SetNextUser();

        gameType = GameData.GameEngine.Unknown;
        StartCoroutine(TaskStart());
    }

    // Az aktuális feladatot befejezték
    // Meglehet adni a feladat végrehajtásának adatait a feladathoz tartozó resultObjektumba
    public void TaskEnd(object result) {
        if (NextTask())
        {
            // Ha van következő feladat, akkor elindítjuk
            StartCoroutine(TaskStart());
        }
        else {
            // Ha végig játszottuk az összes játékot, akkor kilépünk a menübe
            exitType = ExitType.TasksEnd;
            Common.canvasDark.Dark(false); // Kikapcsoljuk a sötétítést

            if (callBack != null)
                callBack();
        }
        //screenCounter++; // A következő feladatra lépünk
    }

    /// <summary>
    /// A játékból kiléptek az Exit gombbal.
    /// </summary>
    /// <param name="result"></param>
    public void GameExit(object result) {
        exitType = ExitType.Exit;

        if (callBack != null)
            callBack();
    }

    /// <summary>
    /// Következő feladatra lép.
    /// </summary>
    /// <returns>A visszaadott érték mutatja, hogy van-e még következő feladat.</returns>
    bool NextTask() {
        // Növeljük az aktuális képernyő számát
        screenCounter++;
        if (screenCounter >= mosaicData.listOfGames[gameCounter].screens.Count) {
            gameCounter++;
            screenCounter = 0;

            if (gameCounter >= mosaicData.listOfGames.Count) {
                return false;
            }
        }

        return true;
    }




    /// <summary>
    /// Elindítja az aktuális feladatot, amit a gameCounter és a screenCounter meghatároz
    /// </summary>
    /// <returns></returns>
    IEnumerator TaskStart() {

        Common.HHHnetwork.messageProcessingEnabled = false; // Leállítjuk a hálózaton érkező üzenetek feldolgozását

        // Meghatározzuk, hogy melyik képernyőnek kell a következő feladatot végrehajtani
        string newGameScreen = "";
        switch (mosaicData.listOfGames[gameCounter].gameEngine)
        {
            case GameData.GameEngine.TrueOrFalse:
                newGameScreen = "TrueOrFalseGame";
                break;
            case GameData.GameEngine.Bubble:
                newGameScreen = "BubbleGame3";
                break;
            case GameData.GameEngine.Sets:
                newGameScreen = "SetGame";
                break;
            case GameData.GameEngine.MathMonster:
                newGameScreen = "MathMonsterGame";
                break;
            case GameData.GameEngine.Millionaire:
                newGameScreen = "MillionaireGame";
                break;
            case GameData.GameEngine.Fish:
                newGameScreen = "FishGame";
                break;
            case GameData.GameEngine.Affix:
                newGameScreen = "AffixGame";
                break;
            case GameData.GameEngine.Boom:
                newGameScreen = "BoomGame";
                break;
            case GameData.GameEngine.Hangman:
                newGameScreen = "HangmanGame";
                break;
            case GameData.GameEngine.Read:
                newGameScreen = "ReadGame";
                break;
        }

        if (Common.screenController.actScreen.name == newGameScreen)
        {
            // Elrejtjük az előző kérdés elemeit
            yield return StartCoroutine(Common.screenController.actScreen.HideGameElement());

            // Inicializáljuk a következő kérdést
            yield return StartCoroutine(Common.screenController.actScreen.InitCoroutine());

            // ScreenShowStart
            yield return StartCoroutine(Common.screenController.actScreen.ScreenShowStartCoroutine());

            // ScreenShowFinish
            yield return StartCoroutine(Common.screenController.actScreen.ScreenShowFinishCoroutine());
        }
        else {
            Common.screenController.ChangeScreen(newGameScreen);
        }










        /*
        // Ha a következő feladat ugyan olyan típusú mint az előző, akkor 
        if (mosaicData.listOfGames[gameCounter].gameType  == gameType)
        {
            // Elrejtjük az előző kérdés elemeit
            yield return StartCoroutine(Common.screenController.actScreen.HideGameElement());

            // Inicializáljuk a következő kérdést
            yield return StartCoroutine(Common.screenController.actScreen.InitCoroutine());

            // ScreenShowStart
            yield return StartCoroutine(Common.screenController.actScreen.ScreenShowStartCoroutine());

            // ScreenShowFinish
            yield return StartCoroutine(Common.screenController.actScreen.ScreenShowFinishCoroutine());
        }
        else { // Ha a következő feladat nem olyan típusú mint az előző, akkor képernyőt váltunk
            switch (mosaicData.listOfGames[gameCounter].gameType)
            {
                case GameData.GameType.TrueOrFalse:
                    Common.screenController.ChangeScreen("TrueOrFalseGame");
                    break;
                case GameData.GameType.Bubble:
                    Common.screenController.ChangeScreen("BubbleGame3");
                    break;
                case GameData.GameType.Sets:
                    Common.screenController.ChangeScreen("SetGame");
                    break;
                case GameData.GameType.MathMonster:
                    Common.screenController.ChangeScreen("MathMonsterGame");
                    break;
                case GameData.GameType.Millionaire:
                    Common.screenController.ChangeScreen("MillionaireGame");
                    break;
                case GameData.GameType.Fish:
                    Common.screenController.ChangeScreen("FishGame");
                    break;
                case GameData.GameType.Affix:
                    Common.screenController.ChangeScreen("AffixGame");
                    break;
                case GameData.GameType.Boom:
                    Common.screenController.ChangeScreen("BoomGame");
                    break;
                case GameData.GameType.Hangman:
                    Common.screenController.ChangeScreen("HangmanGame");
                    break;
                case GameData.GameType.Read:
                    Common.screenController.ChangeScreen("ReadGame");
                    break;
            }
        }
        */

        gameType = mosaicData.listOfGames[gameCounter].gameEngine;

        Common.canvasNetworkHUD.gameName = mosaicData.listOfGames[gameCounter].name;

        yield return null;
    }

    /// <summary>
    /// A játék hívja ezt az eljárást ha történt valamilyen felhasználói interakció a játékban
    /// amit el kell küldeni a többi játékosnak is.
    /// </summary>
    /// <param name="jsonNode">A küldendő üzenet json formában.</param>
    public void SendMessageToServer(JSONNode jsonNode) {
        // Ha nem a játékos az aktív, akkor figyelmen kívűl hagyjuk az eseményt
        if (!playerActive) return;

        // Elküldjük az eseményt a szervernek, hogy továbbítsa a többi csoport tagnak is
        //Common.HHHnetwork.SendJSONClientToServer(jsonNode);

        // Az elküldendő üzenetet visszaküldjük a játéknak mintha hálózaton jött volna
        SendMessageToGame(jsonNode);

        //NetworkGameEvent networkGameEvent = (NetworkGameEvent)System.Enum.Parse(typeof(NetworkGameEvent), jsonNode["event"].ToString());
        //Common.screenController.actScreen.EventOccur(networkGameEvent, jsonNode);
    }

    /// <summary>
    /// Eseményt küldünk az aktuális játéknak, amit végre kell hajtania
    /// </summary>
    public void SendMessageToGame(JSONNode jsonNode) {
        //NetworkGameEvent networkGameEvent = (NetworkGameEvent)System.Enum.Parse(typeof(NetworkGameEvent), jsonNode["gameEvent"]);
        Common.screenController.actScreen.EventHappened(jsonNode);
    }

    // Beállítja, hogy ki a következő játékos
    void SetNextUser()
    {
        playerActive = true;

        switch (multiPlayerGameMode)
        {
            /* // Amúgy is true a playerActive változó, szóval az nem kell
            case MultiPlayerGameMode.EveryoneAlone:
                playerActive = true;
                break;
                */
            case MultiPlayerGameMode.ChangeForTask:
                if (groupHeadCount > 0)
                    playerActive = taskCounter % groupHeadCount == indexInGroup;
                break;
            case MultiPlayerGameMode.ChangeForSubTask:
                if (groupHeadCount > 0)
                    playerActive = taskCounter % groupHeadCount == indexInGroup;
                break;
        }

        Common.canvasDark.Dark(!playerActive);
    }

    public void GameEventHappend(GameEvent gameEvent) {
        // Ha a játékos az aktív, akkor jóváírjuk neki a pontot
        if (playerActive)
        {
            switch (gameEvent)
            {
                case GameEvent.GoodAnswer: // Jó választ adott a játékos

                    break;

                case GameEvent.WrongAnswer: // Rossz választ adott a játékos

                    break;

                case GameEvent.OutOfTime: // Lejárt az ideje a játékosnak, rossz válasznak minősül ???
                    // Ez attól függ, hogy milyen cooperatív játék módban vagyunk

                    break;
            }
        }

        // Ha al feladatonként új játékos játszik, akkor tovább lépünk a következő játékosra
        if (multiPlayerGameMode == MultiPlayerGameMode.ChangeForSubTask)
        {
            taskCounter++;
            SetNextUser();
        }
    }

    /// <summary>
    /// Esemény érkezett a hálózaton.
    /// </summary>
    /// <param name="networkEventType">A hálózati esemény típusa.(connect, data, disconnect, stb.)</param>
    /// <param name="connectionID">Melyik kapcsolat azonosítón jött be.</param>
    /// <param name="receivedData">A fogadott adat JSONNode formájában.</param>
    void ReceivedNetworkEvent(NetworkEventType networkEventType, int connectionID, JSONNode receivedData)
    {
        switch (networkEventType)
        {
            case NetworkEventType.DataEvent:
                string dataContent = receivedData[C.JSONKeys.dataContent];

                // Ha le kell állítani a játékot, akkor vissza lépünk a kliens várakozó képernyőre
                if (dataContent == C.JSONValues.playStop) {
                    Common.canvasDark.Dark(false); // Kikapcsoljuk a sötétítést
                    Common.screenController.ChangeScreen("CanvasScreenClientWaitStart");
                }

                // A hálózaton jött a játéknak szóló esemény
                if (dataContent ==  C.JSONValues.groupData)
                    SendMessageToGame(receivedData);
                break;
        }
    }
}
