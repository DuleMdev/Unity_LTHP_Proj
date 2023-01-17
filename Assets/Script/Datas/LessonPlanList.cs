using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*
Ez az objektum tárolja a lecketervek fejléc adatait, amit a teacherConfig fájlba írunk majd ki.

*/

public class LessonPlanList {

    List<LessonPlanData> listOfLessonPlan = new List<LessonPlanData>();

    public int Count { get { return listOfLessonPlan.Count; } }

    public LessonPlanData this[int index] { get { return listOfLessonPlan[index]; } }

    public LessonPlanList(JSONNode jsonData = null) {
        if (jsonData != null)
        {
            Add(jsonData);
        }
    }

    /// <summary>
    /// A lecketervek listájához hozzáadunk további leckéket.
    /// </summary>
    /// <param name="jsonData">Ebben a json-ban vannak a további leckék tárolva.</param>
    public void Add(JSONNode jsonData, string lessonPlanDir = null)
    {
        if (jsonData != null)
        {
            for (int i = 0; i < jsonData.Count; i++)
            {
                DeleteByID(jsonData[i][C.JSONKeys.lessonid].AsInt);

                // Feldolgozzuk a soron következő lecketervet
                LessonPlanData lessonPlanData = new LessonPlanData();
                lessonPlanData.LoadDataFromJSONNode(jsonData[i], false);

                // Ha nem volt hiba a feldolgozás során akkor tároljuk a lecketervet
                if (!lessonPlanData.error)
                {
                    listOfLessonPlan.Add(lessonPlanData);

                    // Tároljuk a háttértáron az óratervet, ha megadtak egy directory nevet
                    if (lessonPlanDir != null)
                    {
                        // Időbélyeget adunk a lecketervhez
                        jsonData[i][C.JSONKeys.lessonSynchronizeTime] = Common.TimeStamp();

                        // Tároljuk a lecketervet
                        string fileName = System.IO.Path.Combine(lessonPlanDir, jsonData[i][C.JSONKeys.lessonid].AsInt.ToString());
                        System.IO.File.WriteAllText(fileName + ".json", jsonData[i].ToString(" "));
                    }
                }
            }
        }
    }

    /// <summary>
    /// A megadott azonosítóval rendelkező lecketervet kitörli a listából.
    /// </summary>
    /// <param name="id">A törlendő lecketerv azonosítója.</param>
    public void DeleteByID(int id)
    {
        for (int i = 0; i < listOfLessonPlan.Count; i++)
        {
            if (listOfLessonPlan[i].id == id)
            {
                listOfLessonPlan.RemoveAt(i);
                break;
            }
        }
    }

    /// <summary>
    /// Visszaadja a megadott azonosítóju lecketervet.
    /// </summary>
    /// <param name="lessonPlanID">A keresett lecketerv azonosítója.</param>
    /// <returns></returns>
    public LessonPlanData GetLessonPlanByName(int lessonPlanID)
    {
        foreach (LessonPlanData lessonPlanData in listOfLessonPlan)
            if (lessonPlanData.id == lessonPlanID)
                return lessonPlanData;

        return null;
    }

    /*
    public void Load(JSONNode jsonData)
    {
        for (int i = 0; i < jsonData.Count; i++)
        {
            Add(jsonData[i]);

            //listOfLessonPlan.Add(new LessonPlanData(jsonData[i], false));
        }
    }
    */

    public JSONNode SaveDataToJSON()
    {
        JSONArray jsonData = new JSONArray();

        for (int i = 0; i < listOfLessonPlan.Count; i++)
        {
            jsonData.Add("", listOfLessonPlan[i].SaveDataToJSON());
        }

        return jsonData;
    }



}
