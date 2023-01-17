using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*

{ 
   "error" : "false", 
   "answer" : { 
      "pathData" : { 
         "flowStyle" : "simple", 
         "gameIsolation" : "2020-02-17 17:16:33", 
         "currentLevel" : "4", 
         "levelIsolation" : "2020-02-05 17:03:43", 
         "userID" : "3", 
         "learnRoutePathID" : "84", 
         "learnRoutePathStart" : "2020-02-05 17:03:37", 
         "subjectID" : "19", 
         "topicID" : "107", 
         "courseID" : "286", 
         "curriculumID" : "499", 
         "curriculumIsolationTime" : "2020-02-05 17:03:43"
      }, 
      "gameData" : { 
         "id" : "6590", 
         "engine" : "Texty", 
         "name" : "TEXTY", 
         "labels" : "TEST", 
         "difficulty" : "1", 
         "keyset" : "HU", 
         "screens" : [ 
            { 
               "id" : "525", 
               "task" : "DUCK!", 
               "text" : "Duck duck duck!", 
               "time" : "180", 
               "questions" : [ 
                  { 
                     "questionID" : "565", 
                     "question" : "ADUCK [?]", 
                     "inOrder" : "1", 
                     "answers" : [ 
                        "DUCK"
                     ]
                  }, 
                  { 
                     "questionID" : "566", 
                     "question" : "ADUCK [?] BDUCK [?] CDUCK [?]", 
                     "inOrder" : "1", 
                     "answers" : [ 
                        "DUCK", 
                        "DUCK", 
                        "duck"
                     ]
                  }, 
                  { 
                     "questionID" : "567", 
                     "question" : "\\frac{1}{2} = [?]", 
                     "inOrder" : "1", 
                     "answers" : [ 
                        "0.5"
                     ]
                  }, 
                  { 
                     "questionID" : "568", 
                     "question" : "[#/block]\\frac{1}{2} + \\frac{1}{2}[#/block] = [?]", 
                     "inOrder" : "1", 
                     "answers" : [ 
                        "1"
                     ]
                  }, 
                  { 
                     "questionID" : "569", 
                     "question" : "[#block]\\frac{1}{2} + \\frac{1}{2} =[#/block] [?]", 
                     "inOrder" : "1", 
                     "answers" : [ 
                        "\\frac{2}{2}"
                     ]
                  }, 
                  { 
                     "questionID" : "570", 
                     "question" : "[#block]\\frac{1}{2} + \\frac{1}{2}[#block] = [?]", 
                     "inOrder" : "1", 
                     "answers" : [ 
                        "1"
                     ]
                  }
               ]
            }
         ]
      }
   }
}

*/
public class TaskTextyData : TaskAncestor
{
    public enum AnswerType
    {
        text,
        fraction,
        root,
    }

    public class QuestionData
    {
        public string questionID;
        public string question;          // A kérdés szövege
        public bool inOrder;            // Ha igaz, akkor nem cserélhetőek fel a válaszok
        public List<string> answers;     // A kérdésekre adható válaszok
        public List<string> allAnswers;  // Felcserélhető válaszoknál innen törli ki a már megadott jó válaszokat. A továbbiakban a maradék közül kerülhet ki a jó válasz

        public QuestionData(JSONNode node)
        {
            InitData(node);
        }

        public void InitData(JSONNode node)
        {
            questionID = node[C.JSONKeys.questionID];
            question = node[C.JSONKeys.question];
            inOrder = node[C.JSONKeys.inOrder].Value == "1";

            answers = new List<string>();
            for (int i = 0; i < node[C.JSONKeys.answers].Count; i++)
                answers.Add(node[C.JSONKeys.answers][i]);
        }

        public AnswerType GetAnswerType(string s)
        {
            return AnswerType.text;
        }
    }

    public string textTitle;                // A történet címe
    public string textContent;              // Maga a történet

    public List<QuestionData> questions;    // Kérdések

    // A megadott JSON alapján inicializálja a változóit
    public TaskTextyData(JSONNode jsonNode)
    {
        taskType = TaskType.Texty;
        differentAnswerFeedbackEnabled = true;
        InitDatas(jsonNode);
    }

    override protected void InitTaskDatas(JSONNode jsonNode)
    {
        error = false;

        textTitle = jsonNode[C.JSONKeys.task];
        textContent = jsonNode[C.JSONKeys.text];

        questions = new List<QuestionData>();
        for (int i = 0; i < jsonNode[C.JSONKeys.questions].Count; i++)
            questions.Add(new QuestionData(jsonNode[C.JSONKeys.questions][i]));

        waitBeforeStart =
            waitScreenChange + // Képernyőváltás sebessége
            animSpeed1; // Megjelenik a szöveg

        waitBetweenQuestion = 0; // 1.2f;
        waitUntilHideElements = animSpeed1;
    }


    /// <summary>
    /// A játék task-ok feladat specifikus indítása.
    /// </summary>
    override protected void StartTask()
    {
        // Feltöltjük a QuestionData-ban az allAnswers tömböt az answers-ekkel. Ezt a felcserélhető válaszok megadásánál lesz segítségünkre
        for (int i = 0; i < questions.Count; i++)
            questions[i].allAnswers = new List<string>(questions[i].answers);
    }

    override protected int GetQuestionNumber()
    {
        int questionNumber = 0;
        foreach (QuestionData questionData in questions)
            questionNumber += questionData.answers.Count;

        return questionNumber;
    }

    override protected string AnswerEvaluation(JSONNode jsonData)
    {
        int questionIndex = jsonData[C.JSONKeys.selectedQuestion].AsInt;
        int subQuestionIndex = jsonData[C.JSONKeys.selectedSubQuestion].AsInt;
        string answer = jsonData[C.JSONKeys.selectedAnswer].Value;

        if (questions[questionIndex].inOrder)
        {
            // Nem cserélhetőek fel a válaszok
            return SpecialInputPopUp.IsEqual(questions[questionIndex].answers[subQuestionIndex], answer) ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
            //return questions[questionIndex].answers[subQuestionIndex] == answer ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
        }
        else
        {
            bool answerIsGood = false;
            for (int i = 0; i < questions[questionIndex].allAnswers.Count; i++)
            {
                if (SpecialInputPopUp.IsEqual(questions[questionIndex].allAnswers[i], answer))
                {
                    answerIsGood = true;
                    questions[questionIndex].allAnswers.RemoveAt(i);
                    break;
                }
            }

            return answerIsGood ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;

            /*
            // Felcserélhető válaszok
            bool answerIsGood = questions[questionIndex].allAnswers.Contains(answer);
            if (answerIsGood)
            {
                // Kitöröljük az összes válasz közül a most megadottat
                questions[questionIndex].allAnswers.Remove(answer);
                return C.JSONValues.evaluateIsTrue;
            }
            else
            {
                return C.JSONValues.evaluateIsFalse;
            }
            */
        }
    }

    override public JSONNode GetEvaluations()
    {
        if (!replayMode)
        {
            if (Common.configurationController.isServer2020)
            {
                for (int i = 0; i < evaluations.answers.Count; i++)
                {
                    int questionIndex = evaluations.answers[i][C.JSONKeys.questionID].AsInt;
                    evaluations.answers[i][C.JSONKeys.questionID] = questions[questionIndex].questionID;
                }
            }
        }

        return evaluations.answers;
    }

    override public JSONNode GetNextReplayAnswer()
    {
        Debug.LogWarning("GetNextReplayAnswer");

        // Ha ez volt az utolsó válasz
        taskMustEnd = replayIndex >= evaluations.answers.Count;

        JSONNode replayAnswer = null;
        if (replayIndex < evaluations.answers.Count)
        {
            replayAnswer = new JSONClass();
            replayAnswer[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
            replayAnswer[C.JSONKeys.gameEventType] = C.JSONValues.answer;
            replayAnswer[C.JSONKeys.selectedQuestion].AsInt = GetQuestionIndexByQuestinIndex(evaluations.answers[replayIndex][C.JSONKeys.questionID].Value);
            //evaluations.answers[replayIndex][C.JSONKeys.questionID].Value = replayAnswer[C.JSONKeys.selectedQuestion].Value;
            replayAnswer[C.JSONKeys.selectedSubQuestion].AsInt = evaluations.answers[replayIndex][C.JSONKeys.questionIndex].AsInt;
            replayAnswer[C.JSONKeys.selectedAnswer] = evaluations.answers[replayIndex][C.JSONKeys.answer];
            replayAnswer[C.JSONKeys.evaluateAnswer] = evaluations.answers[replayIndex][C.JSONKeys.isGood].AsBool ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;

            //replayAnswer = evaluations.answers[replayIndex];
            replayIndex++;
        }

        return replayAnswer;
    }

    int GetQuestionIndexByQuestinIndex(string questionID)
    {
        for (int i = 0; i < questions.Count; i++)
        {
            if (questions[i].questionID == questionID)
                return i;
        }

        return -1;
    }

}
