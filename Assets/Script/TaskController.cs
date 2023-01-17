using UnityEngine;
using System.Collections;
using SimpleJSON;
using UnityEngine.Networking;

/*
Egy képernyőt játszik le.

Ha a hálózaton jön egy üzenet, hogy milyen képernyőt játszon le, akkor azt lejátsza.
*/


public class TaskController : MonoBehaviour
{
    static public TaskController instance;

    public TaskAncestor task { get; private set; }
    GameData gameData;  // A lejátszandó képernyő ebben a játékban található
    public bool playerActive { get; private set; } // Igaz értéke van ha a játékos jön lépéssel

    // Ha a 'Tanári' tabletten kell a játékot lejátszani és nincs a Szerver és a Kliens külön gépen,
    // akkor ezekre a változókra szükség van a végrehajtáshoz
    int startScreenIndex; // Az eredmény számításnál fontos, hogy tudjuk hány játékot játszott a játékos
    public GameControl gameControl;    // Tanári tabletten vezérli a játékot (azért public, hogy a TaskAncestor vagyis a játékok is eltudják érni és beletudják írni a játékban eltelt időt)
    Common.CallBack callBack;   // Melyik metódust hívja meg ha befejeződött a játék
    public float resultPercent { get; private set; }
    public float elapsedGameTime{ get; private set; }
    public JSONNode screensEvaluations;
    public JSONNode extraData;
    
    void Awake()
    {
        instance = this;
        Common.taskController = this;
    }

    /// <summary>
    /// A megadott jsonNode-ban található, hogy melyik feladatot kell lejátszani.
    /// </summary>
    /// <param name="jsonNode">A feladat adatait tartalmazza.</param>
    public void PlayTask(JSONNode jsonNode)
    {
        // Valahová kell ezeket írni, de most nem tudom, hogy hová
        //startIndex = 0;
        //gameControl = null;

        // Megakarjuk kapni a hálózati üzeneteket, ha nem a szerveren vagyunk
        if (!Common.configurationController.deviceIsServer)
            Common.HHHnetwork.callBackNetworkEvent = ReceivedNetworkEvent;

        int lessonMosaicIndex = jsonNode[C.JSONKeys.lessonMosaicIndex].AsInt;
        int gameIndex = jsonNode[C.JSONKeys.gameIndex].AsInt;
        int screenIndex = jsonNode[C.JSONKeys.screenIndex].AsInt;

        gameData = Common.configurationController.lessonPlanData.
            lessonMosaicsList[lessonMosaicIndex].
            listOfGames[gameIndex];

        task = gameData.screens[screenIndex];
        task.AddExtraInfo(jsonNode[C.JSONKeys.extraInfo]);
        // task.Start(); A szerveren kell elindítani a játékot

        StartCoroutine(TaskStart());

        Common.canvasNetworkHUD.gameName = gameData.name;
    }


    public void PlayGameInServer(GameData gameData, int startScreenIndex, Common.CallBack callBack)
    {
        this.startScreenIndex = startScreenIndex;
        this.callBack = callBack;

        gameControl = new GameControl(ReceivedNetworkEvent, LessonMosaicData.MultiPlayerGameMode.Single);
        gameControl.AddClient(new ClientData(1, 0, ""));
        gameControl.StartGame(gameData, startScreenIndex);
    }

    /// <summary>
    /// Elindítja az aktuális feladatot, amit a gameCounter és a screenCounter meghatároz
    /// </summary>
    /// <returns></returns>
    IEnumerator TaskStart()
    {
        Common.HHHnetwork.messageProcessingEnabled = false; // Leállítjuk a hálózaton érkező üzenetek feldolgozását

        // Meghatározzuk, hogy melyik képernyőnek kell a következő feladatot végrehajtani
        string newGameScreen = "";
        switch (gameData.gameEngine)
        {
            case GameData.GameEngine.TrueOrFalse:
                newGameScreen = "TrueOrFalseGame";
                break;
            case GameData.GameEngine.Bubble:
                newGameScreen = "BubbleGame";
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
            case GameData.GameEngine.Read2:
                newGameScreen = "ReadGame2";
                break;
            case GameData.GameEngine.Texty:
                newGameScreen = "TextyGame";
                break;
            case GameData.GameEngine.PDF:
                newGameScreen = "PDFGame";
                break;
            case GameData.GameEngine.YouTube:
                newGameScreen = "YouTubeGame";
                break;
            case GameData.GameEngine.Psycho:
                newGameScreen = "PsychoGame";
                break;
        }

        // Ha útvonal link megosztás történt, akkor előfordulhat, hogy az első játék képernyő előtt nem volt másik
        if (Common.screenController.actScreen != null &&
            Common.screenController.actScreen.name == newGameScreen)
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
    }

    /// <summary>
    /// A játék hívja ezt az eljárást ha történt valamilyen felhasználói interakció a játékban
    /// amit el kell küldeni a szervernek.
    /// </summary>
    /// <param name="jsonNode">A küldendő üzenet json formában.</param>
    public void SendMessageToServer(JSONNode jsonNode)
    {
        // Ha nem a játékos az aktív és nem a végső üzenetet küldjük, akkor figyelmen kívűl hagyjuk az eseményt
        if (!playerActive && jsonNode[C.JSONKeys.gameEventType].Value != C.JSONValues.finalMessage) return;

        // Ha válasz történt, akkor a játékosnak inaktívvá tesszük, hogy ne tudjon többet válaszolni
        if (jsonNode[C.JSONKeys.gameEventType].Value == C.JSONValues.answer)
            playerActive = false;

        // Megvizsgáljuk, hogy a kliensen vagy a szerveren fut a TaskController
        if (Common.configurationController.deviceIsServer)
        {
            // A ha a szerveren fut, akkor a json-t a gameControl-nak küldjük el
            if (gameControl != null)
                gameControl.MessageArrived(NetworkEventType.DataEvent, 1, jsonNode);
        }
        else
        {
            // Ha a kliensen fut, akkor elküldjük az eseményt a szervernek, hogy továbbítsa a többi csoport tagnak is
            Common.HHHnetwork.SendMessageClientToServer(jsonNode);
        }
    }

    /// <summary>
    /// Eseményt küldünk az aktuális játéknak, amit végre kell hajtania
    /// </summary>
    public void SendMessageToGame(NetworkEventType networkEventType, int connectionID, JSONNode receivedData)
    {
        //NetworkGameEvent networkGameEvent = (NetworkGameEvent)System.Enum.Parse(typeof(NetworkGameEvent), jsonNode["gameEvent"]);
        ((GameAncestor)Common.screenController.actScreen).MessageArrived(networkEventType, connectionID, receivedData);
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

                switch (dataContent)
                {
                    case C.JSONValues.gameEvent:
                        SendMessageToGame(networkEventType, connectionID, receivedData);
                        break;

                    case C.JSONValues.nextPlayer:
                        SendMessageToGame(networkEventType, connectionID, receivedData);
                        playerActive = receivedData[C.JSONKeys.player].Value == C.JSONValues.playerActive;
                        Common.canvasDark.Dark(!playerActive);
                        break;

                    case C.JSONValues.pauseOn:
                        SendMessageToGame(networkEventType, connectionID, receivedData);
                        break;

                    case C.JSONValues.pauseOff:
                        SendMessageToGame(networkEventType, connectionID, receivedData);
                        break;

                    default:
                        break;
                }

                // Ha le kell állítani a játékot, akkor vissza lépünk a kliens várakozó képernyőre
                if (dataContent == C.JSONValues.playStop)
                {
                    Common.canvasDark.Dark(false); // Kikapcsoljuk a sötétítést
                    Common.screenController.ChangeScreen("CanvasScreenClientWaitStart");
                }

                // A hálózaton jött a játéknak szóló esemény
                if (dataContent == C.JSONValues.groupData)
                    SendMessageToGame(networkEventType, connectionID, receivedData);
                break;
        }
    }

    /// <summary>
    /// A GameControl hívja meg a tanári tabletten, ha eseményt akar küldeni a játéknak.
    /// </summary>
    /// <param name="connectionID">Nincs jelentősége.</param>
    /// <param name="receivedData">A fogadott adat.</param>
    public void ReceivedNetworkEvent(int connectionID, JSONNode receivedData)
    {
        // Ha új játékot kell indítani, akkor azt megtesszük
        if (receivedData[C.JSONKeys.dataContent].Value == C.JSONValues.playStart)
             Common.taskController.PlayTask(receivedData);
        else 
            // Egyébként a bejövő üzenetet továbbítjuk
            ReceivedNetworkEvent(NetworkEventType.DataEvent, connectionID, receivedData);
    }

    /// <summary>
    /// Befejeződött a játék vagy kiléptek az exit gombbal.
    /// </summary>
    /// <param name="result"></param>
    public void GameExit()
    {
        if (gameControl != null)
        {
            // Összeszámoljuk az eredményt
            resultPercent = 0;
            elapsedGameTime = 0;
            for (int i = startScreenIndex; i < gameControl.gameData.screens.Count; i++)
            {
                resultPercent += gameControl.gameData.screens[i].resultPercent;
                elapsedGameTime += gameControl.gameData.screens[i].elapsedGameTime;
            }
            resultPercent /= gameControl.gameData.screens.Count - startScreenIndex;

            screensEvaluations = gameControl.gameData.GetScreensEvaluations();
            extraData = gameControl.gameData.screens[0].GetExtraData();

            gameControl.gameIsEnd = true;
        }

        gameControl = null;
        //Common.canvasDark.Dark(!playerActive);

        if (callBack != null) {
            callBack();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameControl != null) {
            gameControl.Update();

            if (gameControl.gameIsEnd)
                GameExit();
        }
    }
}
