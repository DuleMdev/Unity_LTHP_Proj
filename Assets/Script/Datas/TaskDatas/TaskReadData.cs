using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*

    {
        "text_title" : "Ez a szöveg címe",
        "text_content" : "Itt van a szöveg tartalma",
        "questions" : [
            {
                "question" : "?? és a ?? törpe",
                "answerGroups" : [
                    [ "Piroska", "Hófehérke" ],
                    [ "hét", "három", "nyolc" ],
                ]
            },
        ]
    }

*/
public class TaskReadData : TaskAncestor {

    public class QuestionData
    {
        public string questionID;
        public string question;                     // A kérdés szövege
        public List<List<string>> answerGroups;     // A válaszok csoportjai (minden csoportban az első válasznak kell a helyesnek lennie)

        public QuestionData(JSONNode node)
        {
            InitData(node);
        }

        public void InitData(JSONNode node)
        {
            question = node[C.JSONKeys.question];
            questionID = node[C.JSONKeys.questionID];

            answerGroups = new List<List<string>>();
            for (int i = 0; i < node[C.JSONKeys.answerGroups].Count; i++)
            {
                List<string> answers = new List<string>();
                for (int j = 0; j < node[C.JSONKeys.answerGroups][i].Count; j++)
                {
                    answers.Add(node[C.JSONKeys.answerGroups][i][j]);
                }
                answerGroups.Add(answers);
            }
        }
    }

    public string textTitle;                // A történet címe
    public string textContent;              // Maga a történet

    public List<QuestionData> questions;    // Kérdések

    // A megadott JSON alapján inicializálja a változóit
    public TaskReadData(JSONNode jsonNode)
    {
        taskType = TaskType.Read;
        differentAnswerFeedbackEnabled = true;
        InitDatas(jsonNode);
    }

    override protected void InitTaskDatas(JSONNode jsonNode)
    {
        error = false;

        id = jsonNode[C.JSONKeys.questionID];

        textTitle = jsonNode[C.JSONKeys.text_title];
        textContent = jsonNode[C.JSONKeys.text_content]; //  Common.Base64Decode(jsonNode["story"]);

        questions = new List<QuestionData>();
        for (int i = 0; i < jsonNode[C.JSONKeys.questions].Count; i++)
            questions.Add(new QuestionData(jsonNode[C.JSONKeys.questions][i]));

        waitBeforeStart =
            waitScreenChange + // Képernyőváltás sebessége
            animSpeed1; // Megjelenik a szöveg

        waitBetweenQuestion = 1.2f;
        waitUntilHideElements = animSpeed1;
    }

    override protected int GetQuestionNumber()
    {
        int questionNumber = 0;
        foreach (QuestionData questionData in questions)
            questionNumber += questionData.answerGroups.Count;

        return questionNumber;
    }

    override protected string AnswerEvaluation(JSONNode jsonData)
    {
        return (questions[jsonData[C.JSONKeys.selectedQuestion].AsInt].answerGroups[jsonData[C.JSONKeys.selectedSubQuestion].AsInt][0] == jsonData[C.JSONKeys.selectedAnswer].Value) ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
    }

    override public JSONNode GetEvaluations()
    {
        if (!replayMode)
        {
            for (int i = 0; i < evaluations.answers.Count; i++)
            {
                int index = evaluations.answers[i][C.JSONKeys.questionID].AsInt;
                evaluations.answers[i][C.JSONKeys.questionID] = questions[index].questionID;

                evaluations.answers[i][C.JSONKeys.answer].AsInt = questions[index].answerGroups[evaluations.answers[i][C.JSONKeys.subQuestionID].AsInt].IndexOf(evaluations.answers[i][C.JSONKeys.answer].Value);
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
            replayAnswer[C.JSONKeys.selectedQuestion].AsInt = GetQuestionIndexFromID(evaluations.answers[replayIndex][C.JSONKeys.questionID]);
            replayAnswer[C.JSONKeys.selectedSubQuestion].AsInt = evaluations.answers[replayIndex][C.JSONKeys.questionIndex].AsInt;
            replayAnswer[C.JSONKeys.selectedAnswer] = questions[replayAnswer[C.JSONKeys.selectedQuestion].AsInt].answerGroups[replayAnswer[C.JSONKeys.selectedSubQuestion].AsInt][evaluations.answers[replayIndex][C.JSONKeys.answerIndex].AsInt];
            replayAnswer[C.JSONKeys.evaluateAnswer] = evaluations.answers[replayIndex][C.JSONKeys.isGood].AsBool ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;

            //replayAnswer = evaluations.answers[replayIndex];
            replayIndex++;
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

    /*
    string GetAnswerFromAnswerIndex(int questionIndex, int subQuestionIndex, int answerIndex)
    {
        for (int i = 0; i < questions[questionIndex].an length; i++)
        {

        }
    }
    */
}
