using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

    {
        "psycho_instruction" : "Ez valamilyen instrukció",
        "psycho_question" : "A kérdés ... , válaszolj vazzeeee",

        "prevAnswers" : [
            {
                "gameEnd" : "Itt valamilyen dátum áll",
                "answer" : "Nem tok mit mondani, kérem kapcsolja ki.",
            },
            {
                "gameEnd" : "Itt valamilyen dátum áll",
                "answer" : "Őőőőőőő, izé.",
            },
        ]
    }

*/

public class TaskPsychoData : TaskAncestor
{
    public class PreAnswer
    {
        public string date;
        public string answer;

        public PreAnswer(JSONNode jsonNode)
        {
            date = jsonNode[C.JSONKeys.gameEnd];
            answer = jsonNode[C.JSONKeys.answer];
        }
    }

    public string instructions;
    public string question;
    public List<PreAnswer> preAnswers;

    public string answer;   // A jelenlegi válasz

    // A megadott JSON alapján inicializálja a változóit
    public TaskPsychoData(JSONNode jsonNode)
    {
        taskType = TaskType.Psycho;
        InitDatas(jsonNode);
    }

    override protected void InitTaskDatas(JSONNode jsonNode)
    {
        error = false;

        instructions = jsonNode[C.JSONKeys.psycho_instructions];
        question = jsonNode[C.JSONKeys.psycho_question];

        preAnswers = new List<PreAnswer>();
        for (int i = 0; i < jsonNode[C.JSONKeys.prevAnswers].Count; i++)
            preAnswers.Add(new PreAnswer(jsonNode[C.JSONKeys.prevAnswers][i]));

        // Beállítjuk az időket
        waitBeforeStart = waitScreenChange + animSpeed1;
        waitBetweenQuestion = 0;
        waitAtGameEnd = 1;
    }

    override protected string AnswerEvaluation(JSONNode jsonData)
    {
        taskMustEnd = true;
        return C.JSONValues.evaluateIsIgnore;
    }

    override public void FinalMessage(JSONNode jsonNode)
    {
        base.FinalMessage(jsonNode);

        answer = jsonNode[C.JSONKeys.answer].Value;
    }

    //override protected void CollectExtraData()
    //{
    //    extraData = new JSONClass();
    //    extraData[C.JSONKeys.type] = "psychoAnswer";
    //    extraData[C.JSONKeys.data] = answer;
    //}

    override public JSONNode GetEvaluations()
    {
        evaluations.AddEvaluation("0", "0", answer, true, answerTime.ToString());

        return evaluations.answers;
    }
}
