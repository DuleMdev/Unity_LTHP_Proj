using UnityEngine;
using System.Collections;
using SimpleJSON;

/*

Egy óra idó adatait tartalmazza.

- Melyik osztállyal történik az óra
- Mikor kezdődik az óra.
- Mikor végződik az óra.

*/

public class TimeData  {

    public int classID;

    public int startInMinute;   // Az óra hol kezdődik hétfő nulla órától percben
    public int endInMinute;     // Az óra hol végződik hétfő nulla órától percben

    public int day;
    public int startHour;
    public int startMinute;
    public int endHour;
    public int endMinute;

    public bool error;

    public TimeData(JSONNode jsonData) {
        startInMinute = 0;
        endInMinute = 0;

        error = false;

        try
        {
            classID = jsonData[C.JSONKeys.classid].AsInt;

            day = jsonData[C.JSONKeys.day].AsInt;
            startHour = jsonData[C.JSONKeys.startHour].AsInt;
            startMinute = jsonData[C.JSONKeys.startMinute].AsInt;
            endHour = jsonData[C.JSONKeys.endHour].AsInt;
            endMinute = jsonData[C.JSONKeys.endMinute].AsInt;

            startInMinute = ConvertInMinute(day, startHour, startMinute);
            endInMinute = ConvertInMinute(day, endHour, endMinute);
        }
        catch (System.Exception)
        {
            // Valamilyen hiba történt a feldolgozáskor
            error = true;
        }
    }

    public static int ConvertInMinute(int day, int hour, int minute) {
        return day * 1440 + hour * 60 + minute;
    }

    /// <summary>
    /// json-ba menti az osztály tartalmát.
    /// </summary>
    /// <returns>Egy json, ami tartalmazza az osztály adatait.</returns>
    public JSONNode SaveDataToJSON()
    {
        JSONClass jsonData = new JSONClass();

        // Elmentjük az osztály azonosítóját
        jsonData[C.JSONKeys.classid].AsInt = classID;

        // Elmentjük a start time-ot
        jsonData[C.JSONKeys.day].AsInt = day;
        jsonData[C.JSONKeys.startHour].AsInt = startHour;
        jsonData[C.JSONKeys.startMinute].AsInt = startMinute;

        jsonData[C.JSONKeys.endHour].AsInt = endHour;
        jsonData[C.JSONKeys.endMinute].AsInt = endMinute;

        return jsonData;
    }
}
