using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*

Itt tároljuk a tanár heti óratervét.

Két féleképpen kerülhetnek bele az adatok.
1. Amikor a tanár szinkronizál.
Le lesz töltve a json és az osztályok résznél minden osztályban megtalálható a times rész.
Ezeket a times részeket külön-külön feldolgozzuk az AddTimeDatas metódussal, ahol meg kell adni, hogy melyik
osztályhoz tartozik az adott idő intervallum.

2. A második lehetőség, amikor a tanár config fájlját betöltjük a háttértárról.
Itt már az idő intervallumok össze vannak gyűjtve egy json osztályba, így a TimeList objektum létrehozásánál
megadhatjuk azt a json objektumot amiben az idő intervallumok megtalálhatóak.


- GetNextClassID
Vissza adja annak az osztálynak az azonosítóját, amlyik osztállyal a tanárnak órája van vagy lesz.
-1 -et ad vissza, ha a tanárnak nincs már egyik osztállyal sem órája

*/

public class TimeList {

    List<TimeData> listOfTimeData = new List<TimeData>();

    /// <summary>
    /// Létrehozza az objektumot és a megadott jsonData-ból betölti az órarendet.
    /// </summary>
    /// <remarks>
    /// Akkor van paraméter megadva ha a tanár config fájljából töltjük be, mivel ott már egy osztályba van az összes órerend.
    /// </remarks>
    /// <param name="jsonData">A paraméter tartalmazza az órarendet.</param>
    public TimeList(JSONNode jsonData = null) {
        if (jsonData != null )
            for (int i = 0; i < jsonData.Count; i++) 
                Add(jsonData[i]);
    }

    /// <summary>
    /// A szerverről letöltött és az osztályok objektumában megtalálható times-okat dolgozza fel.
    /// </summary>
    /// <remarks>
    /// A feldolgozáshoz
    /// </remarks>
    /// <param name="jsonData"></param>
    /// <param name="classID"></param>
    public void AddTimeDatas(JSONNode jsonData, int classID) {
        for (int i = 0; i < jsonData.Count; i++) {
            jsonData[i][C.JSONKeys.classid].AsInt = classID;
            Add(jsonData[i]);
        }
    }

    /// <summary>
    /// Egy újabb órát ad a tanár óratervéhez, amennyiben az óratervet tartalmazó json érvényes.
    /// </summary>
    /// <param name="jsonData">Az új órát tartalmazó json.</param>
    public void Add(JSONNode jsonData) {
        TimeData timeData = new TimeData(jsonData);

        // Ha nem volt hiba a feldolgozás során, akkor az új órát az óratervhez adjuk
        if (!timeData.error)
            listOfTimeData.Add(timeData);
    }

    public void Clear() {
        listOfTimeData.Clear();
    }

    /// <summary>
    /// Visszaadja a tanár aktuálisan folyó vagy a következő óra osztályának azonosítóját.
    /// </summary>
    /// <returns>Az aktuálisan folyó vagy a következő óra osztályának azonosítója. -1 -et ad vissza ha nincs már további óraja a tanárnak.</returns>
    public int GetNextClassID() {
        DateTime now = DateTime.Now;

        // Kiszámoljuk, hogy a héten melyik percnél tart a pontos idő
        // Elcsusztatjuk egy nappal az értéket, mert a nulla a játékban a hétfő a DateTime-ban meg a vasárnap
        int weekMinute = TimeData.ConvertInMinute(((int)now.DayOfWeek + 6) % 7, now.Hour, now.Minute);

        int bestClassID = -1;   // Melyik osztály órája van legközelebb a mostani időhöz
        int bestDifferent = int.MaxValue;

        foreach (TimeData timeData in listOfTimeData)
        {
            // Kiszámoljuk a timeData-ban található idő és a pillanatnyi idő között a különbséget
            int different = timeData.endInMinute - weekMinute;

            // Ha a kiszámolt differencia kisebb mint nulla, akkor az már eltelt, nem kell vele foglalkozni
            // Ha nagyobb egyenlő nulla és kisebb mint az eddigi legjobb, akkor találtunk egy közelebbi órát
            if (different >= 0 && different < bestDifferent) {
                bestDifferent = different;
                bestClassID = timeData.classID;
            }
        }

        return bestClassID;
    }

    /// <summary>
    /// Elmenti
    /// </summary>
    /// <returns></returns>
    public JSONNode SaveDataToJSON() {
        JSONArray jsonData = new JSONArray();

        foreach (TimeData timeData in listOfTimeData)
            jsonData.Add("", timeData.SaveDataToJSON());

        return jsonData;
    }

}
