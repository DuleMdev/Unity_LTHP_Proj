using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*
 
    {
        "question" : "Ez itt a kérdés. Egyébként a hamis a jó válasz",
        "true_or_false" : "0"
    }

*/
public class TaskTrueOrFalseData : TaskAncestor
{
    public string question;             // A kérdés
    public bool goodAnswer;             // Melyik a jó válasz az igaz vagy a hamis
    //public int time;                    // Mennyi idő van a játék megoldására, ha -1, akkor nincs időmérés

    override public bool taskIsEnd {
        get {
            return (!replayMode && (!differentAnswerFeedbackEnabled || Common.configurationController.answerFeedback == ConfigurationController.AnswerFeedback.Immediately) && (goodAnswersCount > 0 || wrongAnswersCount > 0)) ||
                (tries > 0 && tries <= wrongAnswersCount) ||
                outOfTime ||
                taskMustEnd;
        }
    }

    // A megadott JSON alapján inicializálja a változóit
    public TaskTrueOrFalseData(JSONNode jsonNode)
    {
        taskType = TaskType.TrueOrFalse;
        differentAnswerFeedbackEnabled = true;
        InitDatas(jsonNode);
    }

    override protected void InitTaskDatas(JSONNode node)
    {
        error = false;

        question = node[C.JSONKeys.question];
        goodAnswer = node[C.JSONKeys.true_or_false].Value == "1";
    }

    override protected string AnswerEvaluation(JSONNode jsonData)
    {
        return (goodAnswer == jsonData[C.JSONKeys.selectedAnswer].AsBool) ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
    }
}
