using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*
    {
        "question" : "Ez itt a képernyő kérdés helye",
        "questions" : [
            "Peti szereti az ?(alma|körte) befőttet",
            "Mese szám a ?(három|hét|tizenhárom)"
        ],

        "answers" : [
            "alma", "körte",
            "narancs", "eper",

            "három", "hét", "tizenhárom",
            "egy", "öt"
        ],
    }
*/

public class TaskFishData : TaskAncestor {

    public string screenQuestion;
    public List<TextMeshWithDragTargetData> questions;      // Kérdések (Maximum kettő)
    public List<string> answers;        // A lehetséges összes válasz (a rossz és a jó válaszok együtt)



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
    public TaskFishData(JSONNode jsonNode)
    {
        taskType = TaskType.Fish;
        differentAnswerFeedbackEnabled = true;
        InitDatas(jsonNode);
    }

    override protected void InitTaskDatas(JSONNode jsonNode)
    {
        error = false;

        id = jsonNode[C.JSONKeys. questionID];

        screenQuestion = jsonNode[C.JSONKeys.question].Value;

        questions = new List<TextMeshWithDragTargetData>();
        for (int i = 0; i < jsonNode[C.JSONKeys.questions].Count; i++)
            questions.Add(new TextMeshWithDragTargetData(jsonNode[C.JSONKeys.questions][i]));

        /*
        questions = new List<TextMeshWithDragTargetData>();
        questions.Add(new TextMeshWithDragTargetData("Ez egy nagyon ?(uszony) ú kérdés."));
        questions.Add(new TextMeshWithDragTargetData("Itt, itt itt itt a Itt ?(kopoltyúk) itt itt itt itt a válasz a kopoltyúk bizony ám és ha nem hiszed az nagyon gáz rád nézve. Na ez az utolsó sor."));
        */

        answers = new List<string>();
        for (int i = 0; i < jsonNode[C.JSONKeys.answers].Count; i++)
            answers.Add(jsonNode[C.JSONKeys.answers][i]);

        // Beállítjuk az időket
        waitBeforeStart = waitScreenChange + animSpeed1 * 3;
    }

    public List<string> GetAnswers()
    {
        // Összegyűjtjük a lehetséges válaszokat
        List<string> list = new List<string>(answers); // Hozzáadjuk az összes választ

        // Megkeverjük őket
        // Nem kell megkeverni úgy is össze-vissza rakja ki a képernyőre
        // list.Shuffle();

        return list; // new List<string>(answers);
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

        string result = (questions[questionIndex].listOfAnswerGroups[subQuestionIndex].Contains(answers[answerIndex])) ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;

        //jsonData[C.JSONKeys.selectedQuestion] = questions[questionIndex].questionID;

        return result;
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
            replayAnswer[C.JSONKeys.selectedQuestion].AsInt = GetQuestionIndexFromID(evaluations.answers[replayIndex][C.JSONKeys.fishSentenceID]);
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