using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*

Egy lecketervben a felhasználóknak a játékokban elért eredményeit tárolja.

*/

public class ReportLessonPlan {
    int lessonID;
    string startTime;
    string endTime;

    List<ReportEvent> reportEvents = new List<ReportEvent>();
    
    public ReportLessonPlan(int lessonID) {
        this.lessonID = lessonID;
        startTime = Common.TimeStamp();

        reportEvents = new List<ReportEvent>();
    }
    
    /*
    public void AddEvent(ReportEvent reportEvent) {
        reportEvents.Add(reportEvent);
    }
    */

    public void AddEvents(List<ReportEvent> listOfReportEvent) {
        reportEvents.AddRange(listOfReportEvent);
    }

    /// <summary>
    /// Vége az óramozaiknak, az óramozaik elmentődik a tanár mappájába.
    /// </summary>
    public void End() {
        endTime = Common.TimeStamp();

        // Elkészítjük a json fájlt
        JSONClass jsonClass = new JSONClass();

        jsonClass[C.JSONKeys.lessonid].AsInt = lessonID;
        jsonClass[C.JSONKeys.startTime] = startTime;
        jsonClass[C.JSONKeys.endTime] = endTime;

        JSONArray eventArray = new JSONArray();
        jsonClass[C.JSONKeys.events] = eventArray;

        foreach (ReportEvent reportEvent in reportEvents)
        {
            eventArray.Add(reportEvent.ToJSON());
        }

        // Az eredményt elmentjük a tanár mappájába
        string fileName = System.IO.Path.Combine(Common.configurationController.GetReportDirectory(), startTime);
        System.IO.File.WriteAllText(fileName + ".json", jsonClass.ToString(" "));
    }
}
