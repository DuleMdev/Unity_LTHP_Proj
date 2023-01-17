using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using SimpleJSON;
using UnityEngine.Networking;

public class ClientGroup {

    enum Status
    {
        Init,       // Inicializálás folyik
        Play,       // Játék folyik
        Evaluation, // Értékelés folyik
        GameEnd     // Befejeződött a játék, leállították vagy ilyesmi
    }

    /*
    A kliensek egy csoportját kezeli
    */

    //int groupID;    // Hányadik csoport (A csoport színét határozza meg)
    //int groupMemberCount;   // A csoportban levő tagok száma

    LessonMosaicData lessonMosaicData;              // Melyik óra-mozaikot kell lejátszania
    public int groupNumber { get; private set; }    // A csoport száma, -1 ha nincs csoportosítva

    public List<ClientData> listOfClientData;       // A csoportba tartozó kliensek
    List<GameControl> listOfGameControl;            // Játék vezérlők

    Common.CallBack_In_Int_JSONNode sendMessage;

    int[] gameOrder;            // Milyen sorrendben követték a játékok egymást
    int gameIndex;              // Melyik játék következik

    bool neverEnding;           // Ha végig játszottuk a játékot, akkor kezdődik előlről
    public bool lessonMosaicEnd;       // Mindent óra-mozaik végrehajtva

    Status status;

    LessonMosaicData.MultiPlayerGameMode multiMode; // Milyen módban kellene futtatni a játékot, néha más mint ami be van állítva a lessonMosaicData.multiPlayerGameMode-ban pl. olvasás értés, ami nem futhat multiOneAfterAnother módban

    public int clientCount { get { return listOfClientData.Count; } }

    /// <summary>
    /// Létrehozzuk az objektumot és megadjuk, hogy hányadik csoport adatait fogja tárolni.
    /// Ha -1 -et adunk meg, akkor nincs csoportosítva.
    /// </summary>
    /// <param name="groupNumber">Az objektum hányadik csoport adatait tárolja.</param>
    public ClientGroup(int groupNumber = -1, Common.CallBack_In_Int_JSONNode sendMessage = null, bool neverEnding = false) {
        this.groupNumber = groupNumber;
        this.sendMessage = sendMessage;
        this.neverEnding = neverEnding;

        listOfClientData = new List<ClientData>();
        listOfGameControl = new List<GameControl>();

        status = Status.Init;
    }

    // Hozzáadunk egy klienst a csoporthoz
    public void AddClient(ClientData clientData) {
        if (clientData != null)
        {
            clientData.groupID = groupNumber;
            listOfClientData.Add(clientData);
        }
    }

    /// <summary>
    /// Eltávolítja a csoportból a megadott kapcsolat azonosítójú klienst.
    /// </summary>
    /// <param name="clientConnectionID">Az eltávolítandó kliens kapcsolat azonosítója.</param>
    public void RemoveClient(int clientConnectionID) {
        foreach (ClientData client in listOfClientData)
            if (client.connectionID != clientConnectionID) {
                RemoveClient(client);
                break;
            }
    }

    /// <summary>
    /// Eltávolítja a csoportból a megadott klienst.
    /// </summary>
    /// <param name="clientConnectionID">Az eltávolítandó kliens adatai.</param>
    public void RemoveClient(ClientData clientData)
    {
        // Eltávolítjuk a kliens listából
        listOfClientData.Remove(clientData);

        // Eltávolítjuk a GameControl-ból is
        GameControl gameControl = GetGameControlByClientData(clientData);

        if (gameControl != null)
        {
            gameControl.RemoveClient(clientData);

            // Ha a GameControl ennek hatására üres lett, akkor eltávolítjuk a GameControl-t is.
            if (gameControl.clientCount == 0)
                listOfGameControl.Remove(gameControl);
        }
    }

    GameControl GetGameControlByClientData(ClientData clientData) {
        foreach (GameControl gameControl in listOfGameControl)
        {
            if (gameControl.ContainClient(clientData))
                return gameControl;
        }

        return null;
    }

    /// <summary>
    /// A csoportba tartozik a megadott kacpcsolat azonosítójú kliens?
    /// </summary>
    /// <param name="ConnectionID">A kérdéses kapcsolat azonosító.</param>
    /// <returns>A válasz true, ha a csoportba tartozik, egyébként false.</returns>
    public bool ContainClient(int connectionID) {
        foreach (ClientData clientData in listOfClientData)
        {
            if (clientData.connectionID == connectionID)
                return true;
        }

        return false;
    }

    /// <summary>
    /// A ClientGroup-hoz tartozik a megadott kliens?
    /// </summary>
    /// <param name="clientData">A kérdéses kliens.</param>
    /// <returns>A válasz true, ha a ClientGroup-hoz tartozik, egyébként false.</returns>
    public bool ContainClient(ClientData clientData)
    {
        return listOfClientData.Contains(clientData);
    }

    public void StartLessonMosaic(LessonMosaicData lessonMosaicData) {
        this.lessonMosaicData = lessonMosaicData;

        // Beállítjuk, hogy még nincs vége az óra-mozaiknak
        foreach (ClientData clientData in listOfClientData)
        {
            clientData.lessomMosaicEnd = false;

            // nullázzuk a csoport csillag számlálókat
            clientData.groupStars = 0;
            clientData.groupStarsNew = 0;
        }

        /*
        // Lérehozzuk a gameControl-okat a játékmódnak megfelelően
        listOfGameControl = new List<GameControl>();
        GameControl gameControl;
        switch (lessonMosaicData.multiPlayerGameMode)
        {
            case LessonMosaicData.MultiPlayerGameMode.Single:
            case LessonMosaicData.MultiPlayerGameMode.MultiOneAfterAnother:
                gameControl = new GameControl(sendMessage, lessonMosaicData.multiPlayerGameMode);
                foreach (ClientData clientData in listOfClientData)
                    gameControl.AddClient(clientData);
                listOfGameControl.Add(gameControl);

                break;

            case LessonMosaicData.MultiPlayerGameMode.MultiSingle:
                foreach (ClientData clientData in listOfClientData)
                {
                    gameControl = new GameControl(sendMessage, lessonMosaicData.multiPlayerGameMode);
                    gameControl.AddClient(clientData);
                    listOfGameControl.Add(gameControl);
                }

                break;
        }
        */

        // Meghatározzuk a játékok sorrendjét
        gameOrder = Common.GetRandomNumbersWithFix(lessonMosaicData.listOfGames.Count, lessonMosaicData.fixGames);

        /*
        // Létrehozzuk a szükséges méretű tömböt a kevert indexek tárolására
        int[] gameOrder = new int[lessonMosaicData.listOfGames.Count];
        // A nem fix játékok indexeire kérünk egy kevert listát
        int[] randomized = Common.GetRandomNumbers(lessonMosaicData.listOfGames.Count - lessonMosaicData.fixGames, lessonMosaicData.fixGames);

        // Feltöltjük a játék sorrend tömböt
        for (int i = 0; i < lessonMosaicData.listOfGames.Count; i++)
        {
            if (i < lessonMosaicData.fixGames)
                // Amíg fix a játékok sorrendje addig sorba tesszük a játékok indexét a tömbbe
                gameOrder[i] = i;
            else
                // Ahol már nem fix a sorrend, akkor a kevert sorrendet másoljuk át a játékok sorrendjébe
                gameOrder[i] = randomized[i - lessonMosaicData.fixGames];
        }
        */

        // gameInd
        lessonMosaicEnd = false;
        gameIndex = 0;

        StartGame();
    }

    /// <summary>
    /// Elindítja a gameIndex által meghatározott játékot
    /// </summary>
    void StartGame() {
        status = Status.Play;

        GameData gameData = lessonMosaicData.listOfGames[gameOrder[gameIndex]];

        // Lérehozzuk a gameControl-okat a játékmódnak megfelelően
        // Ha olvasás értéses játéknál csoportos multiplayer van beállítva, akkor ezt módosítjuk csoportos single-re
        listOfGameControl = new List<GameControl>();
        GameControl gameControl;
        multiMode = (gameData.gameEngine == GameData.GameEngine.Read && lessonMosaicData.multiPlayerGameMode == LessonMosaicData.MultiPlayerGameMode.MultiOneAfterAnother) ? LessonMosaicData.MultiPlayerGameMode.MultiSingle : lessonMosaicData.multiPlayerGameMode;
        switch (multiMode)
        {
            case LessonMosaicData.MultiPlayerGameMode.Single:
            case LessonMosaicData.MultiPlayerGameMode.MultiOneAfterAnother:
                gameControl = new GameControl(sendMessage, multiMode);
                foreach (ClientData clientData in listOfClientData)
                    gameControl.AddClient(clientData);
                listOfGameControl.Add(gameControl);

                break;

            case LessonMosaicData.MultiPlayerGameMode.MultiSingle:
                foreach (ClientData clientData in listOfClientData)
                {
                    gameControl = new GameControl(sendMessage, multiMode);
                    gameControl.AddClient(clientData);
                    listOfGameControl.Add(gameControl);
                }

                break;
        }

        // Minden kliensnek létrehozunk egy ReportEvent objektumot, amiben a játék adatait gyűjtjük
        foreach (ClientData client in listOfClientData)
        {
            client.StartReport(gameData.ID, (int)multiMode);
        }

        // Elindítjuk a játékokat
        foreach (GameControl gameControl1 in listOfGameControl)
        {
            gameControl1.StartGame(gameData);
        }
    }

    /*
    // Üzenetet küldünk a csoport minden tagjának szöveges formában
    // ignoredClientID      Ebben a változóban megadhatjuk annak a kliensnek az azonosítóját amelynek nem kell elküldeni az üzenetet.
    //      Ez akkor lehetséges, ha az a kliens küldte az üzenetet a csoportban lévő többi kliensnek, így neki nem kell ezt megkapnia.
    public void SendDataToAllGroupClient(string stringData, string ignoredClientID = "") {
        foreach (Server_ClientData client in listOfClientData)
            if (client.clientID != ignoredClientID)
                client.SendDataToClient(stringData);
    }*/

    /// <summary>
    /// Üzenetet küldünk a csoport minden tagjának json formában
    /// </summary>
    /// <param name="jsonData">A küldendő adat</param>
    /// <param name="ignoredConnectionID">Ebben a változóban megadhatjuk annak a kliensnek az azonosítóját amelynek nem kell elküldeni az üzenetet.
    ///      Ez akkor lehetséges, ha az a kliens küldte az üzenetet a csoportban lévő többi kliensnek, így neki nem kell ezt megkapnia.</param>
    public void SendMessageForAllClient(JSONNode jsonData, int ignoredConnectionID = -1)
    {
        /*
        foreach (Server_ClientData client in listOfClientData)
            if (client.connectionID != ignoredConnectionID)
                client.SendJSONToClient(jsonData);

    */
    }

    /// <summary>
    /// Üzenet érkezett a hálózaton a megadott kapcsolat azonosítójú klienstől.
    /// </summary>
    /// <param name="networkEventType"></param>
    /// <param name="connectionId"></param>
    /// <param name="jsonNodeMessage"></param>
    public void MessageArrived(NetworkEventType networkEventType, int connectionId, JSONNode jsonNodeMessage) {

        switch (networkEventType)
        {
            // Adathalmaz érkezett
            case NetworkEventType.DataEvent:
                switch (jsonNodeMessage[C.JSONKeys.dataContent])
                {
                    case C.JSONValues.gameEvent:
                        SendMessageForGameControl(networkEventType, connectionId, jsonNodeMessage);

                        break;
                    case C.JSONValues.nextOk:
                        ClientData clientData = Common.gameMaster.GetClientDataByConnectionID(connectionId);
                        clientData.nextOk = true;

                        break;
                }
                 
                break;
            case NetworkEventType.ConnectEvent:
                break;
            case NetworkEventType.DisconnectEvent:
                break;
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.BroadcastEvent:
                break;
        }
    }

    /// <summary>
    /// Üzenetet küld annak a ClientGroup-nak amelyik a megadott connectionID-val foglalkozik.
    /// </summary>
    /// <param name="networkEventType"></param>
    /// <param name="connectionId"></param>
    /// <param name="jsonNodeMessage"></param>
    void SendMessageForGameControl(NetworkEventType networkEventType, int connectionId, JSONNode jsonNodeMessage)
    {
        foreach (GameControl gameControl in listOfGameControl)
        {
            if (gameControl.ContainClient(connectionId))
                gameControl.MessageArrived(networkEventType, connectionId, jsonNodeMessage);
        }
    }

    public void Update() {

        switch (status)
        {
            case Status.Init:
                break;
            case Status.Play:
                foreach (GameControl gameControl in listOfGameControl)
                    gameControl.Update();

                // Megvizsgáljuk, hogy a kliens csoporthoz tartozó összes gameControl befejezte-e a munkát
                bool finish = true;
                foreach (GameControl gameControl in listOfGameControl)
                {
                    if (!gameControl.gameIsEnd)
                    {
                        finish = false;
                        break;
                    }
                }

                // Ha nem mindegyik gameControl fejezte ba a munkát, akkor amelyek befejezték azokat átváltjuk az értékelő képernyőre
                // Ez csak egyéni kooperatív játék esetén lehetséges
                if (!finish) {
                    //  Elküldjük az eredményeket a klienseknek
                    foreach (GameControl gameControl in listOfGameControl)
                    {
                        // Elküldjük az eredményeket a klienseknek (csak egy kliens lehet egyéni kooperatív játékmódban)
                        foreach (ClientData clientData in gameControl.listOfClientData)
                        {
                            if (clientData.connectionID >= 0 && gameControl.gameIsEnd && !gameControl.priorEvaluate)
                            {
                                // Ha esetleg csak az értékelésre csatlakozott be, akkor beállítjuk, hogy a játékban van ismét
                                clientData.clientInGame = true;

                                // Létrehozzuk a json-t amit elküldünk a kliensnek.
                                JSONClass jsonData = new JSONClass();
                                jsonData[C.JSONKeys.dataContent] = C.JSONValues.EvaluationScreen;
                                jsonData[C.JSONKeys.multi].AsBool = true;
                                jsonData[C.JSONKeys.onlyBadge].AsBool = false;
                                jsonData[C.JSONKeys.evaluate].AsBool = false;
                                jsonData[C.JSONKeys.allStar].AsInt = clientData.starNumber;

                                jsonData[C.JSONKeys.groupStar].AsInt = clientData.groupStars;
                                jsonData[C.JSONKeys.groupStarNew].AsInt = clientData.groupStarsNew;

                                jsonData[C.JSONKeys.allGroupStar].AsInt = clientData.allGroupStar;
                                jsonData[C.JSONKeys.monsterOrder] = Common.ArrayToJSON(clientData.monsterOrder);
                                jsonData[C.JSONKeys.levelBorder].AsInt = Common.configurationController.levelBorder;


                                sendMessage(clientData.connectionID, jsonData);

                                gameControl.priorEvaluate = true;
                            }
                        }
                    }
                }
                else {  
                    // Ha mindegyik gameControl befejezte a játékot, akkor értékelés következik
                    GameData gameData = lessonMosaicData.listOfGames[gameOrder[gameIndex]];

                    /*
                    float percent = 0;
                    foreach (TaskAncestor task in gameData.screens)
                        percent += task.resultPercent;
                    percent /= gameData.screens.Count;
                    */

                    // Meghatározzuk a pontszámot
                    float percent = 0;
                    foreach (GameControl item in listOfGameControl)
                        percent += item.resultPercent;
                    percent /= listOfGameControl.Count;

                    int starNumber = 0;              // -
                    if (percent >= 15) starNumber++; // *
                    if (percent >= 60) starNumber++; // **
                    if (percent >= 80) starNumber++; // ***

                    //  Elküldjük az eredményeket a klienseknek
                    foreach (ClientData clientData in listOfClientData)
                    {
                        // Ha három csillagot szerzett a játékos, akkor számontartjuk
                        if (starNumber == 3)
                        {
                            clientData.act3StarSeries++;
                            if (clientData.act3StarSeries > clientData.longes3StarSeries)
                                clientData.longes3StarSeries = clientData.act3StarSeries;
                        }
                        else {
                            clientData.act3StarSeries = 0;
                        }

                        // Töröljük a kliens következő játékra való felkészülését
                        clientData.nextOk = false;

                        //Ha több csillagot értünk el mint korábban, akkor a különbséggel növeljük az össz csillag számlálót
                        int result = clientData.gameResultList.GetStar(gameData.lessonMosaicIndex, gameData.ID);
                        int starDifferent = 0;
                        if (result < starNumber)
                        {
                            // Most több csillagot szereztünk mint korábban
                            starDifferent = starNumber - result;
                            clientData.starNumber += starDifferent;
                            clientData.gameResultList.Add(gameData.lessonMosaicIndex, gameData.ID, starNumber);
                        }

                        clientData.point += percent / 10;

                        // Az eredményeket beírjuk a ReportLessonPlan osztályba is
                        clientData.EndReport(percent, ReportEvent.GameEndType.studentEnd);

                        Debug.Log("Game report 2 : " + percent);

                        // Létrehozzuk a json-t amit elküldünk a kliensnek.
                        JSONClass jsonData = new JSONClass();
                        jsonData[C.JSONKeys.dataContent] = C.JSONValues.EvaluationScreen;

                        jsonData[C.JSONKeys.multi].AsBool = false;

                        if (multiMode != LessonMosaicData.MultiPlayerGameMode.Single)
                        {
                            clientData.allGroupStar += starNumber;
                            clientData.groupStars += starNumber;
                            clientData.groupStarsNew += starDifferent;
                            jsonData[C.JSONKeys.multi].AsBool = true;
                        }

                        jsonData[C.JSONKeys.onlyBadge].AsBool = false;
                        jsonData[C.JSONKeys.evaluate].AsBool = true;
                        jsonData[C.JSONKeys.allStar].AsInt = clientData.starNumber;

                        jsonData[C.JSONKeys.result].AsInt = result;
                        jsonData[C.JSONKeys.resultNew].AsInt = starNumber;
                        jsonData[C.JSONKeys.groupStar].AsInt = clientData.groupStars;
                        jsonData[C.JSONKeys.groupStarNew].AsInt = clientData.groupStarsNew;
                        jsonData[C.JSONKeys.itWasLastGame].AsBool = (gameIndex + 1 >= lessonMosaicData.listOfGames.Count);

                        jsonData[C.JSONKeys.allGroupStar].AsInt = clientData.allGroupStar;
                        jsonData[C.JSONKeys.monsterOrder] = Common.ArrayToJSON(clientData.monsterOrder);
                        jsonData[C.JSONKeys.bestTeamMember].AsBool = false;
                        jsonData[C.JSONKeys.cleverestStudent].AsBool = false;
                        jsonData[C.JSONKeys.fastestStudent].AsBool = false;
                        jsonData[C.JSONKeys.longest3StarSeries].AsInt = clientData.longes3StarSeries;
                        jsonData[C.JSONKeys.showTime].AsInt = 30;
                        jsonData[C.JSONKeys.levelBorder].AsInt = Common.configurationController.levelBorder;

                        if (clientData.connectionID >= 0)
                        {
                            // Ha esetleg csak az értékelésre csatlakozott be, akkor beállítjuk, hogy a játékban van ismét
                            clientData.clientInGame = true;

                            sendMessage(clientData.connectionID, jsonData);
                        }
                    }

                    status = Status.Evaluation;
                }

                break;
            case Status.Evaluation:
                // Megvizsgáljuk, hogy mehet tovább-e a játék (Minden kliens rányomott a play gombra vagy lejárt az ideje)
                bool allOk = true;
                foreach (ClientData clientData in listOfClientData)
                {
                    if (!clientData.nextOk && clientData.connectionID >= 0 && clientData.clientInGame) // connectionIsLive)
                    {
                        allOk = false;
                        break;
                    }
                }

                if (allOk) {
                    // Növeljük a játék számlálót
                    gameIndex++;
                    if (gameIndex < lessonMosaicData.listOfGames.Count)
                    {
                        // Ha van még következő játék, akkor elindítjuk
                        StartGame();
                    }
                    else
                    {
                        // Ha nincs már több játék, akkor beállítjuk az óra-mozaik vége jelzőt
                        lessonMosaicEnd = true;

                        // Beállítjuk, hogy a kliensek befejezték a játékot
                        foreach (ClientData clientData in listOfClientData)
                            clientData.lessomMosaicEnd = true;

                        // Ha végtelenített lejátszású az óra-mozaik, akkor újra indítjuk a játékokat
                        if (lessonMosaicData.neverEnding) {
                            // Újra keverjük a játékok sorrendjét
                            gameOrder = Common.GetRandomNumbersWithFix(lessonMosaicData.listOfGames.Count, lessonMosaicData.fixGames);
                            gameIndex = 0;
                            StartGame();
                        }
                    }
                }

                break;
            default:
                break;
        }

    }

    /// <summary>
    /// Leállítjuk a játékokat. A továbiakban nem küldhetnek üzeneteket.
    /// </summary>
    public void GameDestroy() {
        foreach (GameControl gameControl in listOfGameControl)
            gameControl.gameDestroy = true;
    }
}
