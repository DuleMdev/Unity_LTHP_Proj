using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using SimpleJSON;

/*

Egy osztály adatait tárolja.

A tanuló nevekkel és eredményekkel együtt.

*/

public class ClassRoster {

    public bool isNeedUpdate;   // Ha változás történt valamelyik tanuló adataiban, akkor feltöltés szükséges

    public int id;             // Az osztály azonosítója
    public string name;        // Az osztály neve
    public string schoolName;  // Az iskola neve
    public string notes;       // Megjegyzés az osztályhoz

    public List<StudentData> studentList = new List<StudentData>();  // A kiválasztott osztály névsora

    public bool error;

    public int Count { get { return studentList.Count; } }

    public StudentData this[int index] { get { return studentList[index]; } }

    /*
    public ClassRoster(int id, string name) {
        this.id = id;
        this.name = name;
    }
    */

    /// <summary>
    /// Beolvassa az osztály egy fájlból.
    /// </summary>
    /// <param name="fileName">A fájl neve amiből az osztályt be kell olvasni.</param>
    /// <param name="full">Ha ez false, akkor a tanulók adatait nem olvassa ki a json-ból.</param>
    public void LoadFromFile(string fileName, bool full = true) {
        error = false;

        try
        {
            DataProcessor(JSON.Parse(System.IO.File.ReadAllText(fileName + ".json")), full);
        }
        catch (System.Exception)
        {
            error = true;
        }
    }

    
    /// <summary>
    /// Beolvassa az osztály és a tanulók adatait a megadott jsonData változóból.
    /// </summary>
    /// <param name="jsonData">Az osztályok és a tanulók adatait tároló json.</param>
    /// <param name="full">Ha ez false, akkor a tanulók adatait nem olvassa ki a json-ból.</param>
    public void LoadFromJSON(JSONNode jsonData, bool full = true) {
        DataProcessor(jsonData, full);
    }


    void DataProcessor(JSONNode jsonData, bool full = true)
    {
        if (jsonData.ContainsKey(C.JSONKeys.classid))
            id = jsonData[C.JSONKeys.classid].AsInt;

        if (jsonData.ContainsKey(C.JSONKeys.className))
            name = jsonData[C.JSONKeys.className].Value;

        if (jsonData.ContainsKey(C.JSONKeys.schoolName))
            schoolName = jsonData[C.JSONKeys.schoolName].Value;

        if (jsonData.ContainsKey(C.JSONKeys.classNotes))
            notes = jsonData[C.JSONKeys.classNotes].Value;

        if (full)
        {
            if (jsonData.ContainsKey(C.JSONKeys.classStudents))
                for (int i = 0; i < jsonData[C.JSONKeys.classStudents].Count; i++)
                {
                    StudentData studentData = new StudentData(jsonData[C.JSONKeys.classStudents][i]);

                    if (studentData.isNeedUpdate)
                        isNeedUpdate = true;

                    studentList.Add(studentData);
                }
        }
    }

    /// <summary>
    /// Megkeresi azt a diák adatot amelynek az azonosítója megegyezik a megadottal.
    /// Ha nem találja meg null a vissza adott érték.
    /// </summary>
    /// <param name="studentID">Ezzel a tanulói azonsítóval rendelkező StudentData-t keressük.</param>
    /// <returns>A megtalált StudentData vagy null, ha nincs megadott azonosítójú diák.</returns>
    public StudentData GetStudentDataByStudentID(int studentID) {
        foreach (StudentData studentData in studentList)
            if (studentData.id == studentID)
                return studentData;

        return null;
    }

    /// <summary>
    /// Megkeresi azt a diák adatot amelyik utoljára a megadott egyedi azonosítóju tablethez volt kapcsolva az osztályból.
    /// Ha nem találja meg null a vissza adott érték.
    /// </summary>
    /// <param name="studentID">Ezzel a tanulói azonsítóval rendelkező StudentData-t keressük.</param>
    /// <returns>A megtalált StudentData vagy null, ha nincs megadott azonosítójú diák.</returns>
    public StudentData GetStudentDataByTabletUniqueIdentifier(string tabletUniqueIdentifier)
    {
        foreach (StudentData studentData in studentList)
            if (studentData.tabletUniqueIdentifier == tabletUniqueIdentifier)
                return studentData;

        return null;
    }

    /// <summary>
    /// json-ba menti az osztály tartalmát.
    /// A tanulói paramétereket automatikusan menti, ha nem adunk meg paramétert, viszont ha hamis értéket
    /// adunk meg paraméterként, akkor a tanulói adatok nem lesznek elmentve
    /// </summary>
    /// <param name="full">Ha ez false, akkor a tanulók adatait nem menti a json-ba.</param>
    /// <returns>Egy json, ami tartalmazza az osztály adatait.</returns>
    public JSONNode SaveDataToJSON(bool full = true) {
        JSONClass jsonData = new JSONClass();

        jsonData[C.JSONKeys.classid].AsInt = id;
        jsonData[C.JSONKeys.className] = name ?? "";
        jsonData[C.JSONKeys.schoolName] = schoolName ?? "";
        jsonData[C.JSONKeys.classNotes] = notes ?? "";

        if (full) {
            // Egy json tömbbe mentjük a tanulói adatokat
            JSONArray jsonDataArray = new JSONArray();
            foreach (var item in studentList)
                jsonDataArray.Add("", item.SaveDataToJSON());

            // Hozzáadjuk az előzőleg elkészített json tömböt a json osztályhoz
            jsonData[C.JSONKeys.classStudents] = jsonDataArray;
        }

        return jsonData;
    }

    /// <summary>
    /// json-ba menti az osztály tartalmát reporthoz.
    /// </summary>
    /// <remarks>
    /// Az osztályból csak az azonosítókat menti.
    /// Tanulókból az azonosítójukat és az elért eredményeiket.
    /// </remarks>
    /// <returns>Egy json, ami tartalmazza az osztály adatait.</returns>
    public JSONNode SaveReportToJSON()
    {
        JSONClass jsonData = new JSONClass();

        jsonData[C.JSONKeys.classid].AsInt = id;

        // Egy json tömbbe mentjük a tanulói adatokat
        JSONArray jsonDataArray = new JSONArray();
        foreach (var item in studentList)
            if (item.isNeedUpdate)
                jsonDataArray.Add(item.SaveReportToJSON());

        // Hozzáadjuk az előzőleg elkészített json tömböt a json osztályhoz
        jsonData[C.JSONKeys.classStudents] = jsonDataArray;

        return jsonData;
    }

    /// <summary>
    /// Elmenti az osztály adatait a megadott directory-ba.
    /// </summary>
    /// <param name="dirName"></param>
    public void SaveToFile(string dirName) {
        // Tároljuk a háttértáron az osztály adatait.
        string fileName = System.IO.Path.Combine(dirName, id.ToString());
        System.IO.File.WriteAllText(fileName + ".json", SaveDataToJSON().ToString(" "));
    }

}
