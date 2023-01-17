using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Text.RegularExpressions;

/*

    {
        "question" : "Ez itt a képernyő kérdés helye",
        "questions" : [
            {
                "question" : "Peti szereti az ?(alma|körte) befőttet",
            },
            {
                "question" : "Mese szám a ?(három|hét|tizenhárom)",
            }
        ],

        "wrongAnswers" : [
            "narancs", "eper",
            "egy", "öt"
        ],
    }

*/
public class TaskMathMonsterData : TaskAncestor
{
    public string taskDescription;

    public List<TextMeshWithDragTargetData> questions;    // Kérdések
    public List<string> wrongAnswers;   // A lehetséges rossz válaszok

    // Extra információ
    public int[] answerOrder;

    // Vége a játéknak ha minden kérdésre válaszoltunk vagy ha rontottunk már annyit amennyi az életek száma vagy lejárt az idő
//    override public bool gameIsEnd
//    {
//        get
//        {
//            return base.gameIsEnd ||
//                (GetQuestionNumber() == goodAnswersCount + wrongAnswersCount && Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately);
//        }
//    }

    // A megadott JSON alapján inicializálja a változóit
    public TaskMathMonsterData(JSONNode jsonNode)
    {
        taskType = TaskType.MathMonster;
        differentAnswerFeedbackEnabled = true;
        InitDatas(jsonNode);
    }

    override protected void InitTaskDatas(JSONNode jsonNode)
    {
        error = false;

        id = jsonNode[C.JSONKeys.questionID];

        taskDescription = jsonNode[C.JSONKeys.question];

        questions = new List<TextMeshWithDragTargetData>();
        for (int i = 0; i < jsonNode[C.JSONKeys.questions].Count; i++)
            questions.Add(new TextMeshWithDragTargetData(
                Common.configurationController.isServer2020 ?
                jsonNode[C.JSONKeys.questions][i] : 
                jsonNode[C.JSONKeys.questions][i][C.JSONKeys.question]));

        wrongAnswers = new List<string>();
        for (int i = 0; i < jsonNode[C.JSONKeys.wrongAnswers].Count; i++)
            wrongAnswers.Add(jsonNode[C.JSONKeys.wrongAnswers][i]);

        // Beállítjuk az időket
        waitBeforeStart = waitScreenChange + animSpeed1 * 3;
    }

    public List<string> GetAnswersShuffle()
    {
        List<string> answers = GetAnswers();

        if (answerOrder == null)
            answerOrder = Common.GetRandomNumbers(GetAnswers().Count);

        answers.ShuffleByIndexArray(answerOrder);

        return answers;
    }

    List<string> GetAnswers()
    {
        // Összegyűjtjük a lehetséges válaszokat
        List<string> list = new List<string>(wrongAnswers); // Hozzáadjuk az összes rossz választ

        // A rossz válaszok között vannak a jó válaszok is, tehát nem kell újra hozzáadni
        /*
        foreach (QuestionData question in questions)
            if (!string.IsNullOrEmpty(question.answer))
                list.Add(question.answer); // Hozzáadjuk a jó válaszokat is

        */

        // Max 8 válasz lehet, a többit eltávolítjuk a tömb végéről
        while (list.Count > 8)
            list.RemoveAt(8);

        // Megkeverjük őket
        //list.Shuffle();

        return list;
    }

    override public JSONNode GetExtraInfo()
    {
        answerOrder = Common.GetRandomNumbers(GetAnswers().Count);

        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.answerOrder] = Common.ArrayToJSON(answerOrder);

        return jsonClass;
    }

    override public void AddExtraInfo(JSONNode jsonNode)
    {
        try
        {
            jsonNode = jsonNode[C.JSONKeys.answerOrder];
            // Ha nem egyezik a kérdések sorrendje tömb hossza a feladatban található kérdések számával, akkor dobunk egy hiba üzenetet
            if (jsonNode.Count != GetAnswers().Count)
                throw new System.Exception();

            answerOrder = Common.JSONToArray(jsonNode);
            /*
            answerOrder = new int[jsonNode.Count];

            for (int i = 0; i < jsonNode.Count; i++)
                answerOrder[i] = jsonNode[i].AsInt;
                */
        }
        catch (System.Exception)
        {
            answerOrder = null;
        }
    }

    override protected int GetQuestionNumber()
    {
        int questionCount = 0;

        foreach (TextMeshWithDragTargetData questionData in questions)
            questionCount += questionData.listOfAnswerGroups.Count;

        return questionCount;
    }

    override protected string AnswerEvaluation(JSONNode jsonData)
    {
        int questionIndex = jsonData[C.JSONKeys.selectedQuestion].AsInt;
        int subQuestionIndex = jsonData[C.JSONKeys.selectedSubQuestion].AsInt;
        int answerIndex = jsonData[C.JSONKeys.selectedAnswer].AsInt;

        return (questions[questionIndex].listOfAnswerGroups[subQuestionIndex].Contains(wrongAnswers[answerIndex])) ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
    }

    override public JSONNode GetEvaluations()
    {
        if (!replayMode)
        {
            for (int i = 0; i < evaluations.answers.Count; i++)
            {
                int index = evaluations.answers[i][C.JSONKeys.questionID].AsInt;
                evaluations.answers[i][C.JSONKeys.questionID] = questions[index].questionID;
            }
        }

        return evaluations.answers;
    }

    override public JSONNode GetNextReplayAnswer()
    {
        Debug.LogWarning("GetNextReplayAnswer");

        // Ha nincs több válasz
        taskMustEnd = replayIndex >= evaluations.answers.Count;

        JSONNode replayAnswer = null;
        if (replayIndex < evaluations.answers.Count)
        {
            replayAnswer = new JSONClass();
            replayAnswer[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
            replayAnswer[C.JSONKeys.gameEventType] = C.JSONValues.answer;
            replayAnswer[C.JSONKeys.selectedQuestion].AsInt = GetQuestionIndexFromID(evaluations.answers[replayIndex][C.JSONKeys.mathMonsterSentenceID]);
            replayAnswer[C.JSONKeys.selectedSubQuestion].AsInt = evaluations.answers[replayIndex][C.JSONKeys.questionIndex].AsInt;
            replayAnswer[C.JSONKeys.selectedAnswer] = evaluations.answers[replayIndex][C.JSONKeys.answerIndex];
            replayAnswer[C.JSONKeys.evaluateAnswer] = evaluations.answers[replayIndex][C.JSONKeys.isGood].AsBool ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;

            //replayAnswer = evaluations.answers[replayIndex];
            replayIndex++;

            Debug.LogWarning("replayAnswer : \n" + replayAnswer.ToString());
        }

        return replayAnswer;
    }

    int GetQuestionIndexFromID(string id)
    {
        for (int i = 0; i < questions.Count; i++)
        {
            if (questions[i].questionID == id)
                return i;
        }

        return -1;
    }
}
