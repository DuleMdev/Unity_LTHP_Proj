using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*

Tanárok listáját tartalmazza.

*/

public class TeacherList {

    List<TeacherData> teachers = new List<TeacherData>();

    /// <summary>
    /// Egy új tanár ad a rendszerhez. Ha az azonosító már létezik, akkor csak az adatokat frissíti.
    /// </summary>
    /// <param name="node">A tanár adatait tartalmazó json.</param>
    public void Add(JSONNode node) {
        int id = node[C.JSONKeys.userID].AsInt;

        TeacherData teacherData = GetTeacherByID(id);

        if (teacherData == null) {
            teacherData = new TeacherData(node);
            teachers.Add(teacherData);
        } else {
            teacherData.FillData(node);
        }
    }

    /// <summary>
    /// Visszaadja a megadott azonosítóval rendelkező tanár adatait. Ha nem találta meg, akkor null-t add vissza.
    /// </summary>
    /// <param name="ID">A keresett azonosítóju tanár.</param>
    /// <returns></returns>
    public TeacherData GetTeacherByID(int ID) {
        foreach (var item in teachers)
            if (item.userID == ID)
                return item;

        return null;
    }

    /// <summary>
    /// Visszaadja azt a tanárt aki a megadott névvel és jelszóval rendelkezik.
    /// </summary>
    /// <param name="userName">A keresett tanár neve.</param>
    /// <param name="passMD5">A keresett tanár jelszava.</param>
    /// <returns></returns>
    public TeacherData GetTeacherByNameAndPass(string userName, string passMD5) {
        foreach (var item in teachers)
            if (item.userName == userName && item.passMD5 == passMD5)
                return item;

        return null;
    }

    /// <summary>
    /// Egy JSON tömbbe mentjük a rendszerben található tanárok adatait.
    /// </summary>
    /// <returns>A tanárok adatai JSON tömbben.</returns>
    public JSONNode SaveDataToJSON() {
        JSONArray jsonData = new JSONArray();

        foreach (var item in teachers)
            jsonData.Add("", item.SaveDataToJSON());

        return jsonData;
    }

    /// <summary>
    /// Betölti egy JSON tömbböl a tanárok adatait.
    /// </summary>
    /// <param name="jsonData">A tanárok adatait tartalmazó JSON tömb</param>
    public void LoadDataFromJSON(JSONNode jsonData) {
        teachers.Clear();

        for (int i = 0; i < jsonData.Count; i++)
            teachers.Add(new TeacherData(jsonData[i]));
    }
}
