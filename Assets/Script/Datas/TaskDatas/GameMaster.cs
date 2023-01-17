using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.Networking;

/*

Ebben az objektumban található minden adat ami a játékokhoz szükséges.
Itt vannak létrehozva a csoportok és itt vannak nyilvántartva az egyéni eredmények is.



Az Initialize metódust kell meghívni, ha egy másik óratervet akarunk indítani.
Ekkor a korábbi játékok befejeződnek és elkezdődik az új.

Az óramozaikban található játékok listája ki lesz másolva a lessonMosaicData objektumba, amiket minden GameControl
objektum meg fog kapni.

FreePlay játékmód esetén ez az óra-mozaik létre lesz hozva az óraterv-ben található összes játékkal.
A játék módja Single lesz és randomGame be lesz kapcsolva, tehát a játékok véletlenszerű sorrendben fogják 
követni egymást.




*/

public class GameMaster {

    #region nestedObjects



    #endregion

    public enum GameMode
    {
        WarmUp,     // Első óra-mozaik megy (bemelegítés)
        Single,     // Egyéni óra-mozaik
        Multi_OneAfterAnother,  // Kooperatív, felváltva lépnek
        Multi_Single,           // Kooperatív, de külön-külön egyénileg játszanak
    }

    public List<ClientData> listOfClients = new List<ClientData>();  // A csatlakozott kliensek (tablettek) listája

    public LessonMosaicData lessonMosaicData;  // Melyik óra-mozaikot kell végrehajtani

    public List<ClientGroup> listOfClientGroup = new List<ClientGroup>();  // Csoportokba szervezett kliensek

    public GameMode gameMode;  // Milyen játékmódban működik a GameMaster objektum
    public bool pause { get; private set; } // 


    public bool allGroupEnd;       // Mindent csoport befejezte a  óra-mozaik végrehajtását

    public GameMaster(LessonMosaicData lessonMosaicData) {
        Common.gameMaster = this;
        gameMode = GameMode.WarmUp;
    }

    /// <summary>
    /// Egy újabb klienst ad a játékhoz.
    /// </summary>
    /// <param name="connectionID">A kliens kapcsolat azonosítója.</param>
    /// <param name="tabletID">A kliens sorszáma. (A tanárnak van rá szüksége a tablet-tanuló összerendelésnél)</param>
    /// <param name="uniqueIdentifier">A tablet egyedi azonosítója.</param>
    public ClientData AddClient(int connectionID, int tabletID, string uniqueIdentifier, StudentData studentData = null) {

        ClientData clientData = new ClientData(connectionID, tabletID, uniqueIdentifier);

        clientData.studentData = studentData;

        listOfClients.Add(clientData);

        return clientData;
    }

    /// <summary>
    /// Eltávolítja a szerverről a megadott klienst
    /// </summary>
    /// <param name="clientData">Az eltávolítandó kliens.</param>
    public void RemoveClient(ClientData clientData) {
        // Eltávolítjuk a GameMester objektumból
        listOfClients.Remove(clientData);

        // Eltávolítjuk a ClientGroup-ból is
        ClientGroup clientGroup = GetClientGroupByClientData(clientData);

        if (clientGroup != null)
        {
            clientGroup.RemoveClient(clientData);

            // Ha a ClientGroup ennek hatására üres lett, akkor eltávolítjuk a ClientGroup-ot is.
            if (clientGroup.clientCount == 0) {
                listOfClientGroup.Remove(clientGroup);
            }
        }
    }

    ClientGroup GetClientGroupByClientData(ClientData clientData)
    {
        foreach (ClientGroup clientGroup in listOfClientGroup)
        {
            if (clientGroup.ContainClient(clientData))
                return clientGroup;
        }

        return null;
    }

    /// <summary>
    /// Megkeresi azt a klienst amelyiknek a kapcsolat azonosítója megegyezik a megadottal.
    /// Ha nem találja meg null a vissza adott érték.
    /// </summary>
    /// <param name="connectionID">Ezzel a kapcsolat azonsítóval rendelkező klienst keressük.</param>
    /// <returns>A megtalált kliens adata vagy null, ha nincs megadott kapcsolat azonosítójú kliens.</returns>
    public ClientData GetClientDataByConnectionID(int connectionID)
    {
        foreach (ClientData clientData in listOfClients)
            if (clientData.connectionID ==  connectionID)
                return clientData;

        return null;
    }

    /// <summary>
    /// Megkeresi azt a klienst amelyiknek az azonosítója megegyezik a megadottal.
    /// Ha nem találja meg null a vissza adott érték.
    /// </summary>
    /// <param name="tabletID">Ezzel az azonsítóval rendelkező klienst keressük.</param>
    /// <returns>A megtalált kliens adata vagy null, ha nincs megadott azonosítójú kliens.</returns>
    public ClientData GetClientDataBytabletID(int tabletID)
    {
        foreach (ClientData clientData in listOfClients)
            if (clientData.tabletID == tabletID)
                return clientData;

        return null;
    }

    /// <summary>
    /// Megkeresi azt a klienst amelyiknek az egyedi azonosítója megegyezik a megadottal.
    /// Ha nem találta meg null a vissza adott érték.
    /// </summary>
    /// <param name="uniqueIdentifier">Ezzel az egyedi azonosítóval rendelkező klienst keressük.</param>
    /// <returns>A megtalált kliens adata vagy null, ha nincs megadott azonosítójú kliens.</returns>
    public ClientData GetClientDataByUniqueIdentifier(string uniqueIdentifier) {
        foreach (ClientData clientData in listOfClients)
            if (clientData.uniqueIdentifier == uniqueIdentifier) 
                return clientData;

        return null;
    }

    /// <summary>
    /// Megkeresi azt a klienst amelyikhez a megadott tanuló van hozzárendelve.
    /// Ha nem találja meg null a vissza adott érték.
    /// </summary>
    /// <param name="studentID">Ezzel a tanulói azonsítóval rendelkező klienst keressük.</param>
    /// <returns>A megtalált kliens adata vagy null, ha nincs megadott azonosítójú kliens.</returns>
    public ClientData GetClientDataByStudentID(int studentID)
    {
        foreach (ClientData clientData in listOfClients)
            if (clientData.studentData != null)
                if (clientData.studentData.id == studentID)
                    return clientData;

        return null;
    }

    /// <summary>
    /// Összekapcsol egy tanulót egy tablettel.
    /// </summary>
    /// <param name="tabletID">A tablet azonosítója, amit a rendszertől kapott azonosító kéréskor.</param>
    /// <param name="studentID">A tanuló azonosítója</param>
    public void ConnectTableIDAndStudentID(int tabletID, int studentID) {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="LessonMosaicIndex"></param>
    public void Initialize(int LessonMosaicIndex) {
        listOfClients.Clear();
    }

    public void StartFirstLessonMosaic(int connectionId, LessonMosaicData lessonMosaicData) {


        // Ha bemelegítés folyik, azaz az első óramozaikban vagyunk
        if (gameMode == GameMode.WarmUp)
        {
            // Megkeressük azt a kliens csoportot amiben a megadott kapcsolat azonosító szerepel
            // Ha már korábban játékban volt a kliens, akkor valamelyik csoportban meg kell találnia a kapcsolat azonosítóját
            ClientGroup clientGroup = null;
            foreach (ClientGroup foreachClientGroup in listOfClientGroup)
            {
                if (foreachClientGroup.ContainClient(connectionId))
                {
                    clientGroup = foreachClientGroup;
                    break;
                }
            }

            // Ha egyik kliens csopotban sincs a megadott kapcsolat azonosítójú kliens, 
            // akkor létrehoz számára egy csoportot és elindítja a játékot
            if (clientGroup == null)
            {
                // Ez a rész felesleges, mert ha korábban már létre van hozva egy csoport számára, akkor 
                // nem csinálunk semmit, mert a csoportban található GameControl, majd automatikusan elindítja
                // a félbehagyott játékot újra
                /*
                // Ha létrehoztunk már ennek a kapcsolat azonosítójú kliensnek egy játékot, akkor azt töröljük
                // Ez akkor lehetséges, ha megszakadt a kapcsolat és újra csatlakozott és ugyan azt a kapcsolat azonossítót kapta mint korábban
                for (int i = listOfClientGroup.Count - 1; i >= 0; i--)
                {
                    if (listOfClientGroup[i].ContainClient(connectionId))
                        listOfClientGroup.RemoveAt(i);
                }
                */

                // Létrehozunk egy új csoportot a megadott kapcsolat azonosítójú kliensnek
                clientGroup = new ClientGroup(sendMessage: Common.HHHnetwork.SendMessage, neverEnding: true);
                clientGroup.AddClient(GetClientDataByConnectionID(connectionId));

                // Elindítjuk a játékot
                clientGroup.StartLessonMosaic(lessonMosaicData);

                listOfClientGroup.Add(clientGroup);

                // Ha le van állítva a játék, akkor elküldjük a pause eseményt is
                if (pause)
                    SendPauseStatusToClient(GetClientDataByConnectionID(connectionId));
            }
        }
        else {
            // Valószínű, hogy a kliens már valamelyik csoportban szerepel
            ClientData clientData = GetClientDataByConnectionID(connectionId);

            // Ha megtalálta
            if (clientData != null) {
                SendGroupIDForClient(connectionId, clientData.groupID, false);
            }
        }
    }

    /// <summary>
    /// Létrehozzuk a megfelelő csoportszámot
    /// </summary>
    /// <param name="studentNumberInGroups"></param>
    public void Grouping(LessonMosaicData lessonMosaicData, int studentNumberInGroups = 1) {
        this.lessonMosaicData = lessonMosaicData;

        switch (lessonMosaicData.multiPlayerGameMode)
        {
            case LessonMosaicData.MultiPlayerGameMode.Single:
                gameMode = GameMode.Single;
                break;
            case LessonMosaicData.MultiPlayerGameMode.MultiSingle:
                gameMode = GameMode.Multi_Single;
                break;
            case LessonMosaicData.MultiPlayerGameMode.MultiOneAfterAnother:
                gameMode = GameMode.Multi_OneAfterAnother;
                break;
        }

        //gameMode = GameMode.Single;
        //gameMode = lessonMosaicData.multiPlayerGameMode;

        DestroyClientGroups();

        // Csak akkor hozunk létre csoportokat ha legalább egy kliens csatlakozott
        if (listOfClients.Count > 0)
        {
            switch (lessonMosaicData.multiPlayerGameMode)
            {
                case LessonMosaicData.MultiPlayerGameMode.Single:
                    foreach (ClientData clientData in listOfClients)
                    {
                        // Létrehozzuk a kliens csoportot
                        ClientGroup clientGroup = new ClientGroup(-1, Common.HHHnetwork.SendMessage);
                        clientGroup.AddClient(clientData);

                        // Elindítjuk a játékot
                        //clientGroup.StartLessonMosaic(lessonMosaicData);

                        listOfClientGroup.Add(clientGroup);

                        // Elküldjük a kliensnek, hogy melyik csoportba került
                        SendGroupIDForClient(clientData.connectionID, clientData.groupID, false);
                    }
                    break;

                case LessonMosaicData.MultiPlayerGameMode.MultiSingle:
                case LessonMosaicData.MultiPlayerGameMode.MultiOneAfterAnother:

                    // Csoportokba szervezzük a klienseket
                    // Kiszámoljuk hány csoport legyen
                    int needGroup = (listOfClients.Count - 1) / studentNumberInGroups + 1;
                    if (needGroup > Common.configurationController.groupColors.Length)
                        needGroup = Common.configurationController.groupColors.Length;

                    // Létrehozzuk a csoportokat
                    for (int i = 0; i < needGroup; i++)
                    {
                        listOfClientGroup.Add(new ClientGroup(i, Common.HHHnetwork.SendMessage));
                    }

                    // Feltöltjük a csoportokba a tanulókat
                    int[] indexes = Common.GetRandomNumbers(listOfClients.Count);

                    for (int i = 0; i < indexes.Length; i++)
                    {
                        listOfClientGroup[i % needGroup].AddClient(listOfClients[indexes[i]]);
                    }

                    /*
                    // A kapott csoportokat elindítjuk
                    foreach (ClientGroup clientGroup in listOfClientGroup)
                    {
                        clientGroup.StartLessonMosaic(lessonMosaicData);
                    }
                    */

                    // Elküldjük a klienseknek, hogy melyik csoportba kerültek
                    foreach (ClientData client in listOfClients)
                    {
                        SendGroupIDForClient(client.connectionID, client.groupID, true);

                        // Létrehozzuk a json-t amit elküldünk a kliensnek.
                        /*
                        JSONClass jsonData = new JSONClass();
                        jsonData[C.JSONKeys.dataContent] = C.JSONValues.groupID;
                        jsonData[C.JSONKeys.clientGroupID].AsInt = client.groupID;
                        jsonData[C.JSONKeys.clientGroupID].AsInt = client.groupID;
                        jsonData[C.JSONKeys.clientGroupScreenShow].AsBool = true;

                        Common.HHHnetwork.SendMessage(client.connectionID, jsonData);
                        */
                    }

                    break;
            }
        }
    }

    void SendGroupIDForClient(int connectionID, int groupID, bool screenShow) {
        JSONClass jsonData = new JSONClass();
        jsonData[C.JSONKeys.dataContent] = C.JSONValues.groupID;
        jsonData[C.JSONKeys.clientGroupID].AsInt = groupID;
        jsonData[C.JSONKeys.clientGroupScreenShow].AsBool = screenShow;

        Common.HHHnetwork.SendMessage(connectionID, jsonData);
    }

    /// <summary>
    /// Elindítja a megadott lessonMosaicData-ban tárolt óra-mozaikot. 
    /// A tanulókból megadott létszámú csoportokat hoz létre.
    /// </summary>
    /// <param name="lessonMosaicData">A lejátszandó óra-mozaik</param>
    /// <param name="studentNumberInGroups">Hány tanuló legyen egy csoportban.</param>
    public void StartLessonMosaic() { //, int studentNumberInGroups = 1) {

        switch (lessonMosaicData.multiPlayerGameMode)
        {
            case LessonMosaicData.MultiPlayerGameMode.Single:
                foreach (ClientGroup clientGroup in listOfClientGroup)
                {
                    // Elindítjuk a játékot
                    clientGroup.StartLessonMosaic(lessonMosaicData);
                }
                break;

            case LessonMosaicData.MultiPlayerGameMode.MultiSingle:
            case LessonMosaicData.MultiPlayerGameMode.MultiOneAfterAnother:

                // A kapott csoportokat elindítjuk
                foreach (ClientGroup clientGroup in listOfClientGroup)
                {
                    clientGroup.StartLessonMosaic(lessonMosaicData);
                }

                break;
        }



        /*
        gameMode = GameMode.Single;





        // Lehet, hogy a korábbi játékot be kell fejezni
        // Küldünk minden csoportnak egy vége jelet, hogy lépjen ki az aktuális játékból
        foreach (ClientGroup clientGroup in listOfClientGroup)
        {
            //clientGroup.EndGame();
        }







        listOfClientGroup.Clear();
        switch (lessonMosaicData.multiPlayerGameMode)
        {
            case LessonMosaicData.MultiPlayerGameMode.Single:
                foreach (ClientData clientData in listOfClients)
                {
                    // Létrehozzuk a kliens csoportot
                    ClientGroup clientGroup = new ClientGroup(-1, Common.HHHnetwork.SendMessage);
                    clientGroup.AddClient(clientData);

                    // Elindítjuk a játékot
                    clientGroup.StartLessonMosaic(lessonMosaicData);

                    listOfClientGroup.Add(clientGroup);
                }
                break;

            case LessonMosaicData.MultiPlayerGameMode.MultiSingle:
            case LessonMosaicData.MultiPlayerGameMode.MultiOneAfterAnother:

                // Csoportokba szervezzük a klienseket
                // Kiszámoljuk hány csoport legyen
                int needGroup = (listOfClients.Count - 1) / studentNumberInGroups + 1;
                if (needGroup > Common.configurationController.groupColors.Length)
                    needGroup = Common.configurationController.groupColors.Length;

                // Létrehozzuk a csoportokat
                for (int i = 0; i < needGroup; i++)
                {
                    listOfClientGroup.Add(new ClientGroup(i, Common.HHHnetwork.SendMessage));
                }

                // Feltöltjük a csoportokba a tanulókat
                int[] indexes = Common.GetRandomNumbers(listOfClients.Count);

                for (int i = 0; i < indexes.Length; i++)
                {
                    listOfClientGroup[i % needGroup].AddClient(listOfClients[indexes[i]]);
                }

                // A kapott csoportokat elindítjuk
                foreach (ClientGroup clientGroup in listOfClientGroup)
                {
                    clientGroup.StartLessonMosaic(lessonMosaicData);
                }

                break;
        }
        */
    }

    /// <summary>
    /// Beállítja a játék pause státuszát a megadott értéknek megfelelően. Ha igaz értéket adunk, a játék le lesz állítva, egyébként el lesz indítva.
    /// </summary>
    /// <param name="pause"></param>
    public void SetPause(bool pause) {
        // Az összes kliensnek elküldjük a pause eseményt
        this.pause = pause;

        // Elkészítjük a json-t amit elküldünk a kliensnek
        //JSONClass jsonData = new JSONClass();
        //jsonData[C.JSONKeys.dataContent] = (pause) ? C.JSONValues.pauseOn : C.JSONValues.pauseOff;

        foreach (ClientData clientData in listOfClients)
        {
            //Common.HHHnetwork.SendMessage(clientData.connectionID, jsonData);

            SendPauseStatusToClient(clientData);
        }
    }

    void SendPauseStatusToClient(ClientData clientData) {
        // Elkészítjük a json-t amit elküldünk a kliensnek
        JSONClass jsonData = new JSONClass();
        jsonData[C.JSONKeys.dataContent] = (pause) ? C.JSONValues.pauseOn : C.JSONValues.pauseOff;

        Common.HHHnetwork.SendMessage(clientData.connectionID, jsonData);
    }

    /// <summary>
    /// Befejezzük az óramozaikot és megmutatjuk a kliensek eredményeit az Badge képernyőn.
    /// </summary>
    public void EvaluateLessonMosaic() {

        // Ki kell számolni, hogy ki gyűjtötte a legtöbb pontot
        int maxGroupStars = 0;
        foreach (ClientData clientData in listOfClients)
            if (clientData.groupStars > 0) 
                maxGroupStars = clientData.groupStars;

        //  Elküldjük az eredményeket a klienseknek
        foreach (ClientData clientData in listOfClients)
        {
            // Csak akkor küldjük el ha van aktív kapcsolata
            if (clientData.connectionID >= 0)
            {
                // Létrehozzuk a json-t amit elküldünk a kliensnek.
                JSONClass jsonData = new JSONClass();
                jsonData[C.JSONKeys.dataContent] = C.JSONValues.EvaluationScreen;

                jsonData[C.JSONKeys.multi].AsBool = false;

                jsonData[C.JSONKeys.onlyBadge].AsBool = true;
                jsonData[C.JSONKeys.evaluate].AsBool = true;

                jsonData[C.JSONKeys.allStar].AsInt = clientData.starNumber;

                //jsonData[C.JSONKeys.result].AsInt = result;
                //jsonData[C.JSONKeys.resultNew].AsInt = starNumber;
                jsonData[C.JSONKeys.groupStar].AsInt = clientData.groupStars;
                //jsonData[C.JSONKeys.groupStarNew].AsInt = clientData.groupStarsNew;
                //jsonData[C.JSONKeys.itWasLastGame].AsBool = (gameIndex + 1 >= lessonMosaicData.listOfGames.Count);

                jsonData[C.JSONKeys.allGroupStar].AsInt = clientData.allGroupStar;
                jsonData[C.JSONKeys.monsterOrder] = Common.ArrayToJSON(clientData.monsterOrder);

                jsonData[C.JSONKeys.bestTeamMember].AsBool = clientData.groupStars == maxGroupStars && maxGroupStars > 0;
                jsonData[C.JSONKeys.cleverestStudent].AsBool = false;
                jsonData[C.JSONKeys.fastestStudent].AsBool = false;
                jsonData[C.JSONKeys.longest3StarSeries].AsInt = clientData.longes3StarSeries;
                //jsonData[C.JSONKeys.showTime].AsInt = 30;
                jsonData[C.JSONKeys.levelBorder].AsInt = Common.configurationController.levelBorder;

                Common.HHHnetwork.SendMessage(clientData.connectionID, jsonData);
            }
        }

        // Töröljük a játékokat
        DestroyClientGroups();
    }

    /// <summary>
    /// Vége az óratervnek. Minden kliensnek elküldjük, hogy ügyes volt, befejezte az óratervet.
    /// </summary>
    public void LessonPlanEnd() {
        //  Elküldjük az eredményeket a klienseknek
        foreach (ClientData clientData in listOfClients)
        {
            // Csak akkor küldjük el ha van aktív kapcsolata
            if (clientData.connectionID >= 0)
            {
                // Létrehozzuk a json-t amit elküldünk a kliensnek
                JSONNode jsonNode = CollectEvaluateData(clientData);

                jsonNode[C.JSONKeys.lessonPlanEnd].AsBool = true;

                Common.HHHnetwork.SendMessage(clientData.connectionID, jsonNode);
            }
        }
    }

    /// <summary>
    /// A ClientData-ban található összes információt beírjuk egy jsonData-ba.
    /// </summary>
    /// <param name="clientData"></param>
    /// <returns></returns>
    public JSONNode CollectEvaluateData(ClientData clientData) {
        // Létrehozzuk a json-t amit elküldünk a kliensnek
        JSONClass jsonData = new JSONClass();
        jsonData[C.JSONKeys.dataContent] = C.JSONValues.EvaluationScreen;

        jsonData[C.JSONKeys.multi].AsBool = false;

        jsonData[C.JSONKeys.onlyBadge].AsBool = false;
        jsonData[C.JSONKeys.evaluate].AsBool = false;

        jsonData[C.JSONKeys.allStar].AsInt = clientData.starNumber;

        jsonData[C.JSONKeys.groupStar].AsInt = clientData.groupStars;

        jsonData[C.JSONKeys.allGroupStar].AsInt = clientData.allGroupStar;
        jsonData[C.JSONKeys.monsterOrder] = Common.ArrayToJSON(clientData.monsterOrder);

        jsonData[C.JSONKeys.bestTeamMember].AsBool = false;
        jsonData[C.JSONKeys.cleverestStudent].AsBool = false;
        jsonData[C.JSONKeys.fastestStudent].AsBool = false;
        jsonData[C.JSONKeys.longest3StarSeries].AsInt = clientData.longes3StarSeries;
        //jsonData[C.JSONKeys.showTime].AsInt = 30;
        jsonData[C.JSONKeys.levelBorder].AsInt = Common.configurationController.levelBorder;

        return jsonData;
    }

    /// <summary>
    /// Törli az összes kliens csoportot és leállítja a futásukat, nem küldhetnek a továbbiakban üzeneteket
    /// </summary>
    void DestroyClientGroups()
    {
        // Küldünk minden csoportnak egy vége jelet, hogy lépjen ki az aktuális játékból
        foreach (ClientGroup clientGroup in listOfClientGroup)
        {
            // A kliens csoportban található összes játékot leállítja.
            clientGroup.GameDestroy();
        }

        listOfClientGroup.Clear();
    }

    /// <summary>
    /// Üzenet érkezett a kliensektől.
    /// </summary>
    /// <param name="networkEventType"></param>
    /// <param name="connectionID"></param>
    /// <param name="jsonNodeMessage"></param>
    public void MessageArrived(NetworkEventType networkEventType, int connectionId, JSONNode jsonNodeMessage)
    {
        // Ha a kliensen vagyunk
        switch (networkEventType)
        {
            // Adathalmaz érkezett
            case NetworkEventType.DataEvent:
                switch (jsonNodeMessage[C.JSONKeys.dataContent])
                {
                    case C.JSONValues.gameEvent:


                        break;
                }

                break;
            case NetworkEventType.ConnectEvent:
                break;
            case NetworkEventType.DisconnectEvent:
                // Ha megszakadt a kapcsolat, akkor a connectionID-t -1 -re állítjuk.
                ClientData clientData = GetClientDataByConnectionID(connectionId);
                if (clientData != null)
                {
                    clientData.connectionID = -1;
                    clientData.clientInGame = false;
                }

                break;
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.BroadcastEvent:
                break;
        }

        // A bejövő eseményt tovább küldjük
        SendMessageForClientGroup(networkEventType, connectionId, jsonNodeMessage);
    }

    /// <summary>
    /// Üzenetet küld annak a ClientGroup-nak amelyik a megadott connectionID-val foglalkozik.
    /// </summary>
    /// <param name="networkEventType"></param>
    /// <param name="connectionId"></param>
    /// <param name="jsonNodeMessage"></param>
    void SendMessageForClientGroup(NetworkEventType networkEventType, int connectionId, JSONNode jsonNodeMessage) {
        foreach (ClientGroup clientGroup in listOfClientGroup)
        {
            if (clientGroup.ContainClient(connectionId))
                clientGroup.MessageArrived(networkEventType, connectionId, jsonNodeMessage);
        }
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	public void Update () {
        if (!pause)
        {
            foreach (ClientGroup clientGroup in listOfClientGroup)
                clientGroup.Update();

            // Megnézzük, hogy minden csoport befejezte-e a játékot
            allGroupEnd = true;
            foreach (ClientGroup clientGroup in listOfClientGroup)
            {
                if (!clientGroup.lessonMosaicEnd)
                {
                    allGroupEnd = false;
                    break;
                }
            }
        }
    }
}
