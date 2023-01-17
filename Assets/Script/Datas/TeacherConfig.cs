using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;

/*

Egy tanárhoz tartozó adatok vannak itt tárolva.

- 








*/

public class TeacherConfig {

    TeacherData teacherData;

    public TimeList timeList = new TimeList();

    public ClassList classList = new ClassList();

    public LessonPlanList lessonPlanList = new LessonPlanList();

    public bool error;  // Hiba történt a feldolgozás alatt 

    public TeacherConfig(TeacherData teacherData) {
        this.teacherData = teacherData;

        Load();
    }

    /// <summary>
    /// Feldolgozza a megadott json objektumot.
    /// </summary>
    /// <remarks>
    /// Kiolvassa a json objektumból a tanárhoz rendelt osztályokat illetve leckéket és elmenti a háttértárra.
    /// </remarks>
    /// <param name="jsonData">A feldolgozandó json.</param>
    public void AllDataProcessor(JSONNode jsonData)
    {
        error = false;

        try
        {
            // Osztályok és az órarend feldolgozása
            if (jsonData.ContainsKey(C.JSONKeys.classes))
            {
                classList.Clear();
                timeList.Clear();

                for (int i = 0; i < jsonData[C.JSONKeys.classes].Count; i++)
                {
                    classList.Add(jsonData[C.JSONKeys.classes][i]);

                    // Tároljuk a háttértáron az osztály adatait.
                    string fileName = System.IO.Path.Combine(Common.configurationController.GetClassDirectory(teacherData), jsonData[C.JSONKeys.classes][i][C.JSONKeys.classid].AsInt.ToString());
                    System.IO.File.WriteAllText(fileName + ".json", jsonData[C.JSONKeys.classes][i].ToString(" "));

                    timeList.AddTimeDatas(jsonData[C.JSONKeys.classes][i][C.JSONKeys.times], jsonData[C.JSONKeys.classes][i][C.JSONKeys.classid].AsInt);
                }
            }

            // Töröljük a lecketerv directory-jának tartalmát
            Common.DeleteDirectoryContent(Common.configurationController.GetLessonDirectory(teacherData));

            // Lecke tervek foldolgozása
            lessonPlanList = new LessonPlanList();
            if (jsonData.ContainsKey(C.JSONKeys.lessons))
            {
                lessonPlanList.Add(jsonData[C.JSONKeys.lessons], Common.configurationController.GetLessonDirectory(teacherData));
            }

        }
        catch (System.Exception)
        {
            error = true;
        }
    }

    public void Load() {
        string fileName = System.IO.Path.Combine(Common.configurationController.GetTeacherDirectory(teacherData), C.DirFileNames.teacherConfigFileName);

        if (File.Exists(fileName))
        {
            try
            {
                JSONNode jsonData = JSON.Parse(System.IO.File.ReadAllText(fileName));

                if (jsonData != null)
                {
                    // Konfigurációs beállítások betöltése
                    timeList = new TimeList(jsonData[C.JSONKeys.times]);
                    classList = new ClassList(jsonData[C.JSONKeys.classes]);
                    lessonPlanList = new LessonPlanList(jsonData[C.JSONKeys.lessons]);
                }
            }
            catch (System.Exception)
            {

            }
        }
    }

    /// <summary>
    /// Az adatokat elmentjük a háttértárra.
    /// </summary>
    public void Save() {
        JSONClass jsonData = new JSONClass();

        jsonData[C.JSONKeys.times] = timeList.SaveDataToJSON();
        jsonData[C.JSONKeys.classes] = classList.SaveDataToJSON(false);
        jsonData[C.JSONKeys.lessons] = lessonPlanList.SaveDataToJSON();

        string fileName = System.IO.Path.Combine(Common.configurationController.GetTeacherDirectory(teacherData), C.DirFileNames.teacherConfigFileName);
        System.IO.File.WriteAllText(fileName, jsonData.ToString(" "));
    }
}
