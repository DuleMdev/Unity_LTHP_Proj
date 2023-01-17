using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.IO;

/*

Ez az objektum összegyűjti a report-olandó adatokat és egy nagy json-t készít belőlük.

*/

public class ReportFull {



    /// <summary>
    /// A megadott tanár directory-ból összegyűjti az elküldendő adatokat.
    /// </summary>
    /// <param name="dirName">A tanár directory-ja.</param>
    /// <returns></returns>
    public JSONNode GetReportDatas(string dirName) {

        // Összegyűjtjük
        string reportDirName = Path.Combine(dirName, C.DirFileNames.reportsDirName);
        string[] files = Directory.GetFiles(reportDirName);

        // Csak akkor folytatjuk, ha vannak elküldendő óraterv adatok
        if (files.Length > 0)
        {
            JSONArray lessonPlansJSONs = new JSONArray();

            foreach (string fileName in files)
            {
                try
                {
                    JSONNode json = JSON.Parse(File.ReadAllText(Path.Combine(reportDirName, fileName)));
                    lessonPlansJSONs.Add(json);
                }
                catch (System.Exception)
                {

                }
            }

            // Ha legalább 1 óraterv report adatot elmentettünk, akkor elmentjük a diákok eredményeit is
            if (lessonPlansJSONs.Count > 0) {
                string classesDirName = Path.Combine(dirName, C.DirFileNames.classesDirName);

                JSONArray classesJSONs = new JSONArray();

                foreach (string fileName in Directory.GetFiles(classesDirName))
                {
                    try
                    {
                        JSONNode json = JSON.Parse(File.ReadAllText(Path.Combine(classesDirName, fileName)));
                        ClassRoster classRoster = new ClassRoster();
                        classRoster.LoadFromJSON(json);

                        if (classRoster.isNeedUpdate)
                            classesJSONs.Add(classRoster.SaveReportToJSON());
                    }
                    catch (System.Exception)
                    {

                    }
                }

                // Összeállítjuk a json-t
                JSONClass allJSON = new JSONClass();
                allJSON[C.JSONKeys.userID].AsInt = Common.configurationController.actTeacher.userID;
                allJSON[C.JSONKeys.classes] = classesJSONs;
                allJSON[C.JSONKeys.lessons] = lessonPlansJSONs;

                // Elküldjük a fájlt a szervernek
                // Lementjük a háttértárra
                File.WriteAllText(Path.Combine(dirName, "Report-" + Common.TimeStampWithSpace() + ".json"), allJSON.ToString(" "));

                return allJSON;
            }
        };

        return null;
    }


}
