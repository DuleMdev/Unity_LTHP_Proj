using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using SimpleJSON;

public class ClientData {

    class GameResult {
        public int mosaicIndex;    // Melyik óramozaikban
        public int gameID;         // Melyik játékban
        public int starNumber;     // Hány csillagot szerzett

        public GameResult(int mosaicIndex, int gameID, int starNumber) {
            this.mosaicIndex = mosaicIndex;
            this.gameID = gameID;
            this.starNumber = starNumber;
        }
    }

    public class GameResultList {
        List<GameResult> listOfGameResult = new List<GameResult>();

        public void Add(int mosaicIndex, int gameID, int starNumber)
        {
            GameResult gameResult = GetGameResult(mosaicIndex, gameID);

            if (gameResult != null)
            {
                if (gameResult.starNumber < starNumber)
                    gameResult.starNumber = starNumber;
            }
            else
            {
                listOfGameResult.Add(new GameResult(mosaicIndex, gameID, starNumber));
            }
        }

        public int GetStar(int mosaicIndex, int gameID)
        {
            GameResult gameResult = GetGameResult(mosaicIndex, gameID);

            return (gameResult != null) ? gameResult.starNumber : 0;
        }

        GameResult GetGameResult(int mosaicIndex, int gameID)
        {
            foreach (GameResult gameResult in listOfGameResult)
            {
                if (gameResult.mosaicIndex == mosaicIndex && gameResult.gameID == gameID) {
                    return gameResult;
                }
            }

            return null;
        }
    }

    /*
    Az osztály a kapcsolódott kliens adatait tartalmazza
    */

    public GameResultList gameResultList = new GameResultList();   // A kliens eredményeit tárolja az egyes játékokban

    public int connectionID { get; set; }

    public int tabletID { get; private set; }       // Tablet azonosítója amit a szerver tablet adott neki

    public string uniqueIdentifier { get; private set; }    // A kliens készülék egyedi azonosítója
    
    //public string clientID { get; private set; }    // Kliens azonosítója pl. egy sorszám ami a tablet sorszámát tárolja
    //public string userName { get; private set; }    // A tablettet használó játékos neve

    // Az adott kliens csoport adatait tartalmazza
    public int groupID { get; set; }        // Csoport azonosító, melyik csoportba tartozik a felhasználó
    public int indexInGroup { get; private set; }   // Csoporton belül hányadik játékos
    public int groupHeadCount { get; private set; } // Csoport létszám. Hányan vannak a csoportba

    string _name = "";
    public string name {
        get { return (studentData != null) ? studentData.name : _name; }
        set { _name = value; }
    }

    int _starNumber;  // Hány csillagot szerzett eddig a játékos
    public int starNumber {
        get { return _starNumber + ((studentData != null) ? studentData.stars : 0); }
        set { _starNumber = value - ((studentData != null) ? studentData.stars : 0);
            if (studentData != null) studentData.actStars = _starNumber; }
    }

    float _point;       // Hány pontja van a játékosnak
    public float point {
        get { return _point + ((studentData != null) ? studentData.points : 0); }
        set { _point = value - ((studentData != null) ? studentData.points : 0);
            if (studentData != null) studentData.actPoints = _point; }
    }

    public int allGroupStar;    // Hány csillagot szerzett csoport játékokban

    public int groupStars;      // Hány csillagot szerzett az aktuális óramozakban
    public int groupStarsNew;   // Hány új csillagot szerzett az aktuális óramozaikban (a korábban megszerzettek itt nincsenek bele számolva)

    public int rank;        // Hányadik helyen áll a játékos
    public int mosaic;      // Hányadik óramozaiknál tart a játékos
    public int game;        // Az óramozaikon belül hányadik játéknál tart
    public int screen;      // Játékon belül hányadik képernyő (nem biztos, hogy kelleni fog)

    public bool lessonMosaicFinished; // Az aktuális óramozaikban az összes játékot megcsinálta már legalább egyszer

    public bool nextOk;     // A játékos felkészült a következő játék vagy óra-mozaik indítására, vagyis az értékelő képernyőn megnyomta a play gombot vagy lejárt az ideje

    public bool lessomMosaicEnd;    // Igaz értéket tartalmaz, ha befejezte az óra-mozaikot

    public int longes3StarSeries;   // A leghosszabb három csillagos sorozat
    public int act3StarSeries;      // Az aktuális három csillagos sorozat

    public bool clientInGame;   // Van élő kapcsolat a szerver és a kliens között

    public StudentData studentData;  // Az adatok melyik tanulóhoz tartoznak

    //public ReportLessonPlan reportLessonPlan = new ReportLessonPlan();   // 

    public ReportEvent reportEvent; // Egy játék eredményeit tartalmazza
    public List<ReportEvent> listOfReportEvent = new List<ReportEvent>();

    public int[] _monsterOrder; // Amíg nincs tanuló rendelve a klienshez, addig ez határozza meg a szörnyek sorrendjét

    public int[] monsterOrder {
        get {
            if (studentData != null)
                return studentData.monsterOrder;
            else
                return _monsterOrder; // Common.GetIntArray(9);
            }
    }

    public void StartReport(int gameID, int gameType) {
        reportEvent = new ReportEvent();

        reportEvent.gameID = gameID;
        reportEvent.startTime = Common.TimeStamp();
    }

    public void EndReport(float resultPercent, ReportEvent.GameEndType gameEndType)
    {
        reportEvent.resultPercent = resultPercent;
        reportEvent.endTime = Common.TimeStamp();
        reportEvent.gameEndType = gameEndType;

        listOfReportEvent.Add(reportEvent);
    }

    // Létrehoz egy klienset
    public ClientData(int connectionID, int tabletID, string uniqueIdentifier) { //, string clientID, string userName) {
        this.connectionID = connectionID;
        this.tabletID = tabletID;
        this.uniqueIdentifier = uniqueIdentifier;
        //this.clientID = clientID;
        //this.userName = userName;
        clientInGame = true;

        _monsterOrder = StudentData.GetMonsterOrder();

        groupID = -1;
    }

    // Beállítja a kliens csoportját
    public void SetGroup(int groupID, int groupHeadCount, int indexInGroup) {
        this.groupID = groupID;
        this.indexInGroup = indexInGroup;
        this.groupHeadCount = groupHeadCount;
    }

    /*
    // Adatot küld a kliensnek szöveges formában
    public void SendDataToClient(string data) {
        Common.networkTest.SendDataServerToClient(connectionID, data);
    }*/

        /*
    // Adatot küld a kliensnek json adat formájában
    public void SendJSONToClient(JSONNode jsonData) {
        Common.HHHnetwork.SendJSONServerToClient(connectionID, jsonData);
    }
    */
}
