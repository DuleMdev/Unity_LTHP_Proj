using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using SimpleJSON;

public class StudentData {

    // Az egyes játékokban a tanuló milyen eredményeket ért el.
    // Ha egy játékban már kapott valahány csillagot az a legközelebbi játékba nem lesz beleszámítva.
    class GameResults
    {
        int lessonID;       
        int lessonMosaicID;
        int gameID;

        int stars;
    }

    public bool isNeedUpdate;   // Ha változás történt a tanuló adataiban, akkor feltöltés szükséges

    // Alap adatok
    public int id;          // A tanuló azonosítója
    public string name;     // Mi a tanuló neve
    public string notes;    // Megjegyzés a tanulóhoz

    // Játék adatok
    public int stars;       // Hány csilagot gyűjtött
    public int actStars;    // Hány csillagot gyűjtött az aktuális óratervben (ez még nincs hozzáadva a stars-hoz)
    public float points;      // Hány pontot gyűjtött
    public float actPoints;   // Hány pontot gyűjtött az aktuális óratervben (ez még nincs hozzáadva a points-hoz)

    public string tabletUniqueIdentifier { get; set; }    // A kliens készülék egyedi azonosítója amivel a diák utoljára játszott

    public int[] monsterOrder;   // A megnyerhető szörnyek sorrendje

    public ClientData clientData;    // A diák melyik tablettel lett összerendelve

    List<GameResults> listOfGameResults = new List<GameResults>();

    /*
    public StudentData(string name, int stars, int points) {
        this.name = name;
        this.stars = stars;
        this.points = points;
    }
    */

    public StudentData(JSONNode jsonData) {

        if (jsonData.ContainsKey(C.JSONKeys.studentDataUploadNeed))
            isNeedUpdate = jsonData[C.JSONKeys.studentDataUploadNeed].AsBool;

        if (jsonData.ContainsKey(C.JSONKeys.studentID))
            id = jsonData[C.JSONKeys.studentID].AsInt;

        if (jsonData.ContainsKey(C.JSONKeys.studentName))
            name = jsonData[C.JSONKeys.studentName].Value;

        if (jsonData.ContainsKey(C.JSONKeys.studentNotes))
            notes = jsonData[C.JSONKeys.studentNotes].Value;

        if (jsonData.ContainsKey(C.JSONKeys.studentStars))
            stars = jsonData[C.JSONKeys.studentStars].AsInt;

        if (jsonData.ContainsKey(C.JSONKeys.studentPoints))
            points = jsonData[C.JSONKeys.studentPoints].AsFloat;

        tabletUniqueIdentifier = "";
        if (jsonData.ContainsKey(C.JSONKeys.studentTabletUniqueIdentifier))
            tabletUniqueIdentifier = jsonData[C.JSONKeys.studentTabletUniqueIdentifier].Value;

        if (jsonData.ContainsKey(C.JSONKeys.monsterOrder) && jsonData[C.JSONKeys.monsterOrder].Count > 0)
            monsterOrder = Common.JSONToArray(jsonData[C.JSONKeys.monsterOrder]);
        else
            monsterOrder = GetMonsterOrder();
    }

    /// <summary>
    /// A studentData adatait a JSON osztályba menti.
    /// </summary>
    /// <returns>A visszaadott érték a JSON osztály.</returns>
    public JSONNode SaveDataToJSON() {
        JSONClass jsonData = new JSONClass();

        jsonData[C.JSONKeys.studentDataUploadNeed].AsBool = isNeedUpdate || actPoints != 0;
        jsonData[C.JSONKeys.studentID].AsInt = id;
        jsonData[C.JSONKeys.studentName] = name ?? "";
        jsonData[C.JSONKeys.studentNotes] = notes ?? "";
        jsonData[C.JSONKeys.studentStars].AsInt = stars + actStars;
        jsonData[C.JSONKeys.studentPoints].AsFloat = points + actPoints;
        jsonData[C.JSONKeys.studentTabletUniqueIdentifier] = tabletUniqueIdentifier;

        jsonData[C.JSONKeys.monsterOrder] = Common.ArrayToJSON(monsterOrder);

        return jsonData;
    }

    /// <summary>
    /// Report-hoz a studentData adatait a JSON osztályba menti.
    /// </summary>
    /// <returns>A visszaadott érték a JSON osztály.</returns>
    public JSONNode SaveReportToJSON()
    {
        JSONClass jsonData = new JSONClass();

        jsonData[C.JSONKeys.studentID].AsInt = id;
        jsonData[C.JSONKeys.studentStars].AsInt = stars + actStars;
        jsonData[C.JSONKeys.studentPoints].AsFloat = points + actPoints;
        jsonData[C.JSONKeys.monsterOrder] = Common.ArrayToJSON(monsterOrder);
        jsonData[C.JSONKeys.studentTabletUniqueIdentifier] = tabletUniqueIdentifier;

        return jsonData;
    }

    /// <summary>
    /// Vissza ad egy tömböt ami meghatározza a megnyerhető szörnyek sorrendjét
    /// </summary>
    /// <returns></returns>
    public static int[] GetMonsterOrder()
    {
        int[] monsterOrder = new int[9];

        for (int i = 0; i < 3; i++)
        {
            int[] randomInt = Common.GetRandomNumbers(3);
            for (int j = 0; j < 3; j++)
                monsterOrder[i * 3 + j] = i * 3 + randomInt[j];
        }

        return monsterOrder;
    }
}
