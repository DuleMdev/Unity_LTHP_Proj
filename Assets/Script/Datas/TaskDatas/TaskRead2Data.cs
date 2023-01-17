using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*

{
	"id": "7855",
	"engine": "Read2",
	"name": "Read_1_2_2_1_a",
	"gameDifficulty": "50.000",
	"avgPlayTime": "0.000",
	"screens": [{
		"id": "84",
		"time": "900",
        "text_title" : "Ez a szöveg címe",
        "text_content" : "Itt van a szöveg tartalma \n új sor remélhetőleg\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy\nmég egy",
        "question" : "Peti szereti az ?? beföttet\nA sárkányoknak ?? fejük van\n\n?? ?? ?? négy öt hat hét."
        "answerGroups" : [
             [ "alma", "körte" ],
             [ "hét", "három", "tizenhárom" ],
             [ "egy" ],
             [ "kettő" ],
             [ "három" ],
        ],
	}],
	"lastGamePercent": "null"
}

*/

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
public class TaskRead2Data : TaskAncestor {

    public class QuestionData
    {
        public string question;                     // A kérdés szövege
        public List<List<string>> answerGroups;     // A válaszok csoportjai (minden csoportban az első válasznak kell a helyesnek lennie)
    
        public QuestionData(JSONNode node)
        {
            InitData(node);
        }
    
        public void InitData(JSONNode node)
        {
            question = node[C.JSONKeys.question];
    
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

    public string questionText;             // A történetre vonatkozó kérdések

    public List<QuestionData> questions;    // Kérdések

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
    public TaskRead2Data(JSONNode jsonNode)
    {
        taskType = TaskType.Read;
        differentAnswerFeedbackEnabled = true;
        InitDatas(jsonNode);
    }

    override protected void InitTaskDatas(JSONNode jsonNode)
    {
        error = false;

        textTitle = jsonNode[C.JSONKeys.text_title];
        textContent = jsonNode[C.JSONKeys.text_content]; //  Common.Base64Decode(jsonNode["story"]);

        questions = new List<QuestionData>();
        for (int i = 0; i < jsonNode[C.JSONKeys.questions].Count; i++)
            questions.Add(new QuestionData(jsonNode[C.JSONKeys.questions][i]));

        waitBeforeStart =
            waitScreenChange + // Képernyőváltás sebessége
            1; // Megjelenik a szöveg

        waitBetweenQuestion = 1.2f;
        waitUntilHideElements = 1;
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
        List<string> goodAnswers = questions[jsonData[C.JSONKeys.selectedQuestion].AsInt].answerGroups[jsonData[C.JSONKeys.selectedSubQuestion].AsInt];
        for (int i = 0; i < goodAnswers.Count; i++)
            if (goodAnswers[i] == jsonData[C.JSONKeys.selectedAnswer].Value)
                return C.JSONValues.evaluateIsTrue;

        return C.JSONValues.evaluateIsFalse;

        //return (questions[jsonData[C.JSONKeys.selectedQuestion].AsInt].answerGroups[jsonData[C.JSONKeys.selectedSubQuestion].AsInt][0] == jsonData[C.JSONKeys.selectedAnswer].Value) ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
    }
}
