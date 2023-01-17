using UnityEngine;
using System.Collections;
using SimpleJSON;

/*

Egy tanuló egy játékban elért eredményeit tárolja

*/

public class ReportEvent {

    public enum GameEndType
    {
        studentEnd,     // A tanuló befejezte a játékot
        outOfTime,      // Lejárt a játékra szánt idő
        teacherBreak,   // A tanár szakította meg a játékot
        other           // ConnectBreak, stb.
    }

    //public int studentID;     // Hol állítódnak be az egyes változók
    //public int mosaicIndex;     // 
    public int gameID;          // ClientData játék elején
    //public int screenIndex;
    public int gameType;        // ClientData játék elején
    public int goodAnswers;     // GameControl
    public int wrongAnswers;    // GameControl
    public float resultPercent; // ClientGroup a játék végén
    public string startTime;    // ClientData játék elején
    public string endTime;      // ClientData játék végén
    public GameEndType gameEndType;



    public JSONNode ToJSON() {
        JSONClass jsonClass = new JSONClass();

        //jsonClass[C.JSONKeys.studentID].AsInt = studentID;
        //jsonClass[C.JSONKeys.lessonMosaicIndex].AsInt = mosaicIndex;
        jsonClass[C.JSONKeys.gameID].AsInt = gameID;
        //jsonClass[C.JSONKeys.screenIndex].AsInt = screenIndex;
        jsonClass[C.JSONKeys.engine].AsInt = gameType;
        jsonClass[C.JSONKeys.goodAnswers].AsInt = goodAnswers;
        jsonClass[C.JSONKeys.wrongAnswers].AsInt = wrongAnswers;
        jsonClass[C.JSONKeys.resultPercent].AsFloat = resultPercent;
        jsonClass[C.JSONKeys.startTime] = startTime;
        jsonClass[C.JSONKeys.endTime] = endTime;
        jsonClass[C.JSONKeys.gameEndType].AsInt = (int)gameEndType;

        return jsonClass;
    }
}
