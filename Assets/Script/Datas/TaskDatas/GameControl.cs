using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.Networking;

/*

Egy darab játékot vezényel le, a játék összes képernyőjével együtt. 
•	Indításkor létrehozza a játékhoz szükséges extra információkat (elemek elhelyezkedése, stb) és elküldi a klienseknek. 
•	Ha több kliens is tartozik hozzá, akkor eldönti, hogy mikor melyik jön lépéssel.
•	Méri az időt, ha szükséges. Ha letelt az idő, akkor értesíti erről a klienseket.
•	Ha befejezte a játék futtatását, akkor visszaszól a megadott callBack-en keresztűl.

*/

public class GameControl {

    enum Status
    {
        Start,      // Indítjuk a játékot
        Play,       // Megy a játék várunk a válaszra
        Evaluate,   // Az aktív játékos választ küldött aminek a kiértékelése folyik
        WaitFinalMessage,   // Várakozunk a végső üzenetre
        Pause,      // ----- Nem használom
        End,        // Vége a játéknak, mindjárt a következő képernyő indul
    }

    Status status;

    public List<ClientData> listOfClientData = new List<ClientData>(); // Mely kliensek tartoznak a GameControl-hoz

    LessonMosaicData.MultiPlayerGameMode gameMode;  // Milyen módban fut a játék

    public GameData gameData;   // Melyik játékot kell végrehajtani
    public TaskAncestor task;   // Melyik feladatnál (képernyőnél) tartunk
    TaskAncestor previousTask;  // Az előző feladat adatai képernyő váltásnál fontos, hogy tudjuk mennyi ideig tart az előző feladat elemeit eltávolítani a képernyőről

    int[] screenOrder;          // Milyen sorrendben követik a képernyők egymást
    int screenIndex;            // Melyik képenyő következik

    int actClient;              // Melyik kliens jön lépéssel

    public bool priorEvaluate;  // Megjelent az értékelő képernyő, de még nem kapott értékelést

    Common.CallBack_In_Int_JSONNode sendMessage; // Az üzeneteket hová küldjük

    public int clientCount { get { return listOfClientData.Count; } } // Hány kliens tartozik a játékhoz

    public bool gameIsEnd { get; set; } // A játék összes képernyője végig lett játszva

    public bool gameDestroy;    // A játékot eltávolították vagy leállították

    float waitFinalMessage;     // Itt számolom, hogy mennyi idő telt el a FinalMessage várakozására

    public bool screenAgain;    // Újra kell kezdeni az aktuális képernyőt

    public float resultPercent {
        get {
            float percent = 0;
            foreach (TaskAncestor task in gameData.screens)
                percent += task.resultPercent;
            percent /= gameData.screens.Count;

            return percent;
        }
    }

    public GameControl(Common.CallBack_In_Int_JSONNode sendMessage, LessonMosaicData.MultiPlayerGameMode gameMode) {
        this.sendMessage = sendMessage;
    }

    // Hozzáadunk egy klienst a csoporthoz
    public void AddClient(ClientData clientData)
    {
        if (clientData != null)
            listOfClientData.Add(clientData);
    }

    /// <summary>
    /// Eltávolítja a csoportból a megadott klienst.
    /// </summary>
    /// <param name="clientConnectionID">Az eltávolítandó kliens adatai.</param>
    public void RemoveClient(ClientData clientData) {
        listOfClientData.Remove(clientData);
    }

    /// <summary>
    /// A GameControl-hoz tartozik a megadott kacpcsolat azonosítójú kliens?
    /// </summary>
    /// <param name="ConnectionID">A kérdéses kapcsolat azonosító.</param>
    /// <returns>A válasz true, ha a csoportba tartozik, egyébként false.</returns>
    public bool ContainClient(int connectionID)
    {
        foreach (ClientData clientData in listOfClientData)
        {
            if (clientData.connectionID == connectionID)
                return true;
        }

        return false;
    }

    /// <summary>
    /// A GameControl-hoz tartozik a megadott kliens?
    /// </summary>
    /// <param name="clientData">A kérdéses kliens.</param>
    /// <returns>A válasz true, ha a csoportba tartozik, egyébként false.</returns>
    public bool ContainClient(ClientData clientData)
    {
        return listOfClientData.Contains(clientData);
    }

    /// <summary>
    /// Ezzel az metódussal határozzuk meg, hogy melyik játékot játsza le a GameControl objektum.
    /// </summary>
    /// <param name="gameData">A játék adatai.</param>
    /// <param name="startScreenIndex">Melyik képernyőtől kezdje a lejátszást.</param>
    /// <remarks>
    /// Előfordulhat, hogy a játék lejátszását nem az első képernyőtől akarjuk.
    /// </remarks>
    public void StartGame(GameData gameData, int startScreenIndex = 0) {
        this.gameData = gameData.Clone();
        gameIsEnd = false;
        status = Status.Start;

        // Meghatározzuk a képernyők lejátszásának sorrendjét
        screenOrder = Common.GetRandomNumbersWithFix(gameData.screens.Count, gameData.fixScreens);
        screenIndex = startScreenIndex;

        task = null;

        StartScreen();
    }

    /// <summary>
    /// Elindítja a soron következő képernyőt
    /// </summary>
    void StartScreen() {
        screenAgain = false;

        previousTask = task;

        task = gameData.screens[screenOrder[screenIndex]];
        task.Start();
        task.sendMessageAllClient = SendMessageAllClient;

        foreach (ClientData clientData in listOfClientData) {
            // Az összes kliensnek frissítsük a haladás állapotát
            clientData.mosaic = task.gameData.lessonMosaicIndex + 1;
            clientData.game = task.gameData.gameIndex + 1;
            clientData.screen = screenIndex + 1;

            // A megszakadt klienseket vissza tesszük a játékba
            if (clientData.connectionID != -1)
                clientData.clientInGame = true;
        }

        // Elkészítjük a json-t amit elküldünk a kliensnek
        JSONClass jsonData = new JSONClass();

        jsonData[C.JSONKeys.dataContent] = C.JSONValues.playStart;
        jsonData[C.JSONKeys.lessonMosaicIndex].AsInt = gameData.lessonMosaicIndex;
        jsonData[C.JSONKeys.gameIndex].AsInt = gameData.gameIndex;
        jsonData[C.JSONKeys.screenIndex].AsInt = screenIndex;

        // Az extra információkat is a json-ba tesszük
        jsonData[C.JSONKeys.extraInfo] = gameData.screens[screenIndex].GetExtraInfo();

        // Elküldjük a klienseknek a json-t
        SendMessageAllClient(jsonData);

        Debug.Log(Common.Now() + " - Server wait before start");
        Debug.Log("waitBeforeStart = " + task.waitBeforeStart);

        Common.configurationController.WaitTime(
            ((previousTask != null) ? previousTask.waitUntilHideElements : 0) + task.waitScreenChange + task.waitBeforeStart, 
            () =>
        {
            Debug.Log(Common.Now() + " - Server wait before start - end");

            // Lehet, hogy már az előtt tovább léptek a játékból a kilépés, vagy a következő tanegység gombbal, hogy a kezdeti animációk befejeződtek volna
            if (!gameIsEnd)
            {
                SetNextUser();
                status = Status.Play;
            }
        });
    }

    /// <summary>
    /// Meghatározza, hogy melyik játékos jön lépéssel.
    /// </summary>
    void SetNextUser() {
        // Megvizsgáljuk, hogy valaki játékban van-e
        bool anybodyIsInGame = false;
        foreach (ClientData clientData in listOfClientData)
            if (clientData.clientInGame)
            {
                anybodyIsInGame = true;
                break;
            }

        // Ha valaki még játékban van, akkor megkeressük a következő játékost
        if (anybodyIsInGame) {
            do {
                actClient++;
                if (actClient >= listOfClientData.Count)
                    actClient = 0;
            } while (!listOfClientData[actClient].clientInGame);

            SendActiveUser();
        }
    }

    /// <summary>
    /// Elküldjük a klienseknek, hogy ki jön lépéssel.
    /// </summary>
    void SendActiveUser() {
        for (int i = 0; i < listOfClientData.Count; i++)
        {
            // Elkészítjük a json-t amit elküldünk a kliensnek
            JSONClass jsonData = new JSONClass();

            jsonData[C.JSONKeys.dataContent] = C.JSONValues.nextPlayer;
            jsonData[C.JSONKeys.player] = (i == actClient) ? C.JSONValues.playerActive : C.JSONValues.playerPassive;

            // Elküldjük az aktuális időt is
            jsonData[C.JSONKeys.timeEvent].AsFloat = task.gameRemainTime;

            // Elküldjük a kliensnek, hogy ő jön-e lépéssel
            SendMessage(listOfClientData[i].connectionID, jsonData);
        }
    }

    /// <summary>
    /// Üzenet érkezett a hálózaton a megadott kapcsolat azonosítójú klienstől.
    /// </summary>
    /// <param name="networkEventType"></param>
    /// <param name="connectionId"></param>
    /// <param name="jsonNodeMessage"></param>
    public void MessageArrived(NetworkEventType networkEventType, int connectionId, JSONNode jsonNodeMessage)
    {
        // Megvizsgáljuk, hogy kitől jött a válasz, csak az aktív felhasználótól fogadunk el üzenetet
        if (connectionId != listOfClientData[actClient].connectionID)
            return;

        switch (networkEventType)
        {
            // Adathalmaz érkezett
            case NetworkEventType.DataEvent:
                switch (jsonNodeMessage[C.JSONKeys.dataContent])
                {
                    case C.JSONValues.gameEvent:

                        // Ha végső esemény érkezett, akkor azt elküldjük a feladatnak
                        if (jsonNodeMessage[C.JSONKeys.gameEventType].Value == C.JSONValues.finalMessage) {
                            task.FinalMessage(jsonNodeMessage);

                            // Ha valóban végső üzenetként jött az esemény és nem azért mert a Game menüből kiválasztottak valamit, akkor tovább lépünk a következő képernyőre
                            if (status == Status.WaitFinalMessage)
                                NextScreen();
                        }

                        if (status == Status.Play)
                        {
                            switch (jsonNodeMessage[C.JSONKeys.gameEventType].Value)
                            {
                                case C.JSONValues.exitScreen:
                                    task.LetTheTaskEnd();   // Véget vetünk a feladatnak
                                    break;
                                case C.JSONValues.screenAgain:
                                    task.LetTheTaskEnd();
                                    screenAgain = true;
                                    break;
                                case C.JSONValues.nextReplay: // Vissza játszásnál a következő választ kérjük
                                    JSONNode replayAnswer = task.GetNextReplayAnswer();
                                    if (replayAnswer != null)
                                    {
                                        SendMessageAllClient(replayAnswer);
                                        SendActiveUser();
                                    }
                                    break;
                                default:
                                    break;
                            }

                            // Ha válaszoltak egy feladatra és még megy a játék
                            if (jsonNodeMessage[C.JSONKeys.gameEventType].Value == C.JSONValues.answer && status == Status.Play)
                            {
                                status = Status.Evaluate;

                                // Az Answer metódus kiértékeli a választ és az eredményt elhelyezi a jsonban
                                string result = task.Answer(jsonNodeMessage);

                                string s = "----- PLAYER ANSWER -----" +
                                    "\nselectedQuestion : " + jsonNodeMessage[C.JSONKeys.selectedQuestion].AsInt +
                                    "\nselectedSusQuestion : " + jsonNodeMessage[C.JSONKeys.selectedSubQuestion].AsInt +
                                    "\nselectedAnswer : " + jsonNodeMessage[C.JSONKeys.selectedAnswer].AsInt +
                                    "\nANSWER : " + result +
                                    "\n\n" + jsonNodeMessage.ToString(" ");
                                Debug.Log(s);

                                // Megkeressük melyik kliens küldte az adatot és növeljük a jó vagy a rossz válaszainak a számát
                                for (int i = 0; i < listOfClientData.Count; i++)
                                {
                                    if (listOfClientData[i].connectionID == connectionId)
                                    {
                                        ClientData clientData = listOfClientData[i];

                                        // Szerveren nincs reportEvent objektum a clientData-ban
                                        if (clientData.reportEvent != null)
                                        {
                                            if (result == C.JSONValues.evaluateIsTrue)
                                                clientData.reportEvent.goodAnswers++;
                                            if (result == C.JSONValues.evaluateIsFalse)
                                                clientData.reportEvent.wrongAnswers++;
                                        }
                                    }
                                }

                                // Eltüntetjük a kiértékelés eredményét ha nem azonali válasz üzemmódban vagyunk
                                //if (Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately)
                                //    jsonNodeMessage[C.JSONKeys.evaluateAnswer] = C.JSONValues.evaluateIsSilent;

                                // Az összes kliensnek elküldjük a kiértékelés eredményét
                                SendMessageAllClient(jsonNodeMessage);

                                // Update-ben is szerepel, talán itt már nincs azért szükség rá
                                /*
                                // Ha vége a játéknak, akkor egy vége üzenetet is küldünk
                                if (task.gameIsEnd)
                                {
                                    JSONClass jsonClass = new JSONClass();
                                    jsonClass[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
                                    jsonClass[C.JSONKeys.gameEventType] = C.JSONValues.gameEnd;

                                    SendMessageAllClient(jsonClass);
                                }
                                */

                                Common.configurationController.WaitTime(task.waitBetweenQuestion, () =>
                                {
                                    if (!gameIsEnd) // Ha még nem léptek ki a játékból, akkor a szólunk egy játékosnak, hogy ő az aktív
                                    {
                                        if (!task.taskIsEnd)
                                        {
                                            // Ha a kiértékelés true vagy false, akkor továbblépünk a következő játékosra
                                            if (jsonNodeMessage[C.JSONKeys.evaluateAnswer].Value == C.JSONValues.evaluateIsTrue ||
                                                jsonNodeMessage[C.JSONKeys.evaluateAnswer].Value == C.JSONValues.evaluateIsFalse)
                                            {
                                                SetNextUser();
                                            }
                                            else {
                                                // Különben az aktuális játékos marad az aktív
                                                SendActiveUser();
                                            }
                                        }

                                        status = Status.Play;
                                    }
                                });
                            }
                            else { // 
                                SendMessageAllClient(jsonNodeMessage);
                            }
                        }

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
    /// Üzenetet küld a megadott kapcsolat azonosítójú kliensnek.
    /// </summary>
    /// <param name="connectionID">Melyik kliensnek küldje az üzenetet.</param>
    /// <param name="jsonData">A küldendő üzenet.</param>
    void SendMessage(int connectionID, JSONNode jsonData) {
        // Ha nincs még leállítva a játék, akkor küldjük az üzeneteket
        if (!gameDestroy)
            sendMessage(connectionID, jsonData);
    }

    /// <summary>
    /// Üzenetet küld a GameControl objektumban található összes kliensnek.
    /// </summary>
    /// <param name="jsonData">A küldendő üzenet.</param>
    void SendMessageAllClient(JSONNode jsonData) {
        foreach (ClientData clientData in listOfClientData)
            sendMessage(clientData.connectionID, jsonData);
    }

    /// <summary>
    /// Ugyan azt a képernyőt kell újra játszani, ha a resetScreen true vagy ha perfectGameEnd van kiválasztva viszont a játék nem lett tökéletes
    /// </summary>
    void NextScreen() {
        status = Status.End;

        float waitAtGameEnd = task.waitAtGameEnd;

        if (screenAgain)
            waitAtGameEnd = 0;

        // !(A és B) = !A vagy !B
        // A B 
        // 0 0  1 
        // 0 1  1
        // 1 0  1
        // 1 1  0

        // !(A vagy B) = !A és !B
        // A B 
        // 0 0  1 
        // 0 1  0
        // 1 0  0
        // 1 1  0

        Common.configurationController.WaitTime(waitAtGameEnd, () =>
        {
            if (!gameIsEnd) // Ha még nem léptek ki a játékból, akkor jön a következő képernyő ha van
            {
                if (!(screenAgain || (Common.configurationController.perfectAnswer && task.resultPercent != 100)))
                    screenIndex++;

                if (screenIndex >= gameData.screens.Count)
                {
                    gameIsEnd = true;
                }
                else {
                    StartScreen();
                }
            }
        });
    }

    public void Update()
    {
        if (status == Status.Play) {
            task.GoTime();

            // Ha lejárt az idő, akkor vége a játéknak
            if (task.outOfTime) {
                JSONClass jsonClass = new JSONClass();
                jsonClass[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
                jsonClass[C.JSONKeys.gameEventType] = C.JSONValues.outOfTime;

                SendMessageAllClient(jsonClass);

                /*
                status = Status.End;

                // Várakozunk a játék végén, majd jelezzük, hogy vége a játéknak
                Common.configurationController.WaitTime(task.waitAtGameEnd, () =>
                {
                    gameIsEnd = true;
                });
                */
            }

            // Ha vége a játéknak, akkor egy vége üzenetet is küldünk
            if (task.taskIsEnd)
            {
                status = Status.WaitFinalMessage;

                JSONClass jsonClass = new JSONClass();
                jsonClass[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
                jsonClass[C.JSONKeys.gameEventType] = C.JSONValues.gameEnd;

                SendMessageAllClient(jsonClass);

                //status = Status.WaitFinalMessage; // Ezt a blokk elejére helyeztem, mert ha nem hálózaton játszunk, hanem a "szerver" gépen, akkor rögtön a SendMessageAllClient után kapunk egy finalMessage-t, amit így nem tudunk feldolgozni, hiszen még a státusz nem változott WaitFinalMessage-re.
                //NextScreen();
            }
        }

        // Ha a végső  üzenetre várakozunk
        if (status == Status.WaitFinalMessage)
        {
            // Számoljuk, hogy mennyi idő telt már el
            waitFinalMessage += Time.deltaTime;

            // Ha letelt három másodperc és még nem jött meg a végső üzenet, akkor nem várunk tovább, jöhet a következő képernyő
            if (waitFinalMessage > 3)
                NextScreen();
        }


        // Ha még nincs vége a játéknak, akkor megvizsgáljuk, hogy az aktuális játékossal nem szakadt-e meg a 
        // kapcsolat, mert akkor a következő játékosnak kell adni a lehetőséget
        // Ha viszont mindenki megszakadt már és mindenki visszacsatlakozott, akkor elölről kezdjük a játékot
        if (!gameIsEnd)
        {
            bool anybodyIsInGame = false; // Valaki játékban van még?
            bool everybodyIsReconnect = true;   // Mindenki vissza csatlakozott?

            foreach (ClientData clientData in listOfClientData)
            {
                if (clientData.clientInGame)
                    anybodyIsInGame = true;
                if (clientData.connectionID == -1)
                    everybodyIsReconnect = false;
            }

            if (anybodyIsInGame)
            {
                // Ha játék üzemmódban vagyunk és az aktuális játékossal megszakadt a kapcsolat, akkor átadjuk a lehetőséget a következő játékosnak
                if (!listOfClientData[actClient].clientInGame)
                    SetNextUser();
            }
            else {
                // Ha mindenki megszakadt és mindenkinek sikerült visszacsatlakoznia, akkor újra indítjuk a játékot
                if (everybodyIsReconnect)
                    StartScreen();
            }

            task.Update();
        }
    }

}
