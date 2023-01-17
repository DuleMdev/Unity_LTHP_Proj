using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using SimpleJSON;

/*
Egy listát tárol a tanárhoz tartozó osztályokról.

Csak az osztályok adatait tárolja a diákokét nem.

*/

public class ClassList {

    List<ClassRoster> listOfClass = new List<ClassRoster>();

    public int Count { get { return listOfClass.Count; } }

    public ClassRoster this[int index] { get { return listOfClass[index]; } }

    public ClassList(JSONNode jsonData = null) {
        if (jsonData != null) {
            for (int i = 0; i < jsonData.Count; i++)
                Add(jsonData[i]);
                //listOfClass.Add(new ClassRoster(jsonData[i].ToString()));
        }
    }

    public void Add(JSONNode jsonData) {
        // Beolvassuk a jsonData-ban található adatokat, de csak az osztályra vonatkozókat a diákokra vonatkozókat nem
        ClassRoster classRoster = new ClassRoster();
        classRoster.LoadFromJSON(jsonData, false);

        // Megnézzük, hogy volt-e már tárolva ilyen azonosítójú osztály, mert ha igen, akkor azt kitöröljük
        DeleteByID(classRoster.id);

        // Rögzítjük az új osztályt
        listOfClass.Add(classRoster);
    }

    /// <summary>
    /// A megadott azonosítóval rendelkező osztályt kitörli a listából.
    /// </summary>
    /// <param name="id">A törlendő osztály azonosítója.</param>
    public void DeleteByID(int id) {
        for (int i = 0; i < listOfClass.Count; i++)
            if (listOfClass[i].id == id) {
                listOfClass.RemoveAt(i);
                break;
            }
    }

    /// <summary>
    /// Vissza adja a megadott osztálynév alapján az osztály azonosítóját. -1 -et ad vissza, ha nincs meadott osztálynév.
    /// </summary>
    /// <param name="name">A keresett osztálynév.</param>
    /// <returns>Az keresett osztály azonosítója. -1 ha nincs keresett nevű osztály.</returns>
    public int GetClassIDByName(string name) {
        for (int i = 0; i < listOfClass.Count; i++)
        {
            if (listOfClass[i].name == name)
            {
                return listOfClass[i].id;
            }
        }

        return -1;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Clear() {
        listOfClass.Clear();
    }

    /// <summary>
    /// json-ba menti az osztály tartalmát.
    /// </summary>
    /// <returns>Egy json, ami tartalmazza az osztály adatait.</returns>
    public JSONNode SaveDataToJSON(bool full = true) {
        // Egy json tömbbe mentjük az osztályok adatait
        JSONArray jsonDataArray = new JSONArray();
        foreach (var item in listOfClass)
            jsonDataArray.Add("", item.SaveDataToJSON(full));

        return jsonDataArray;
    }
}
