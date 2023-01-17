using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*
    {
        "question" : "A kérdés szövege",
        "image" : "",
        "imageBorder" : false,
        "goodAnswers" : [ "jó válasz 1", "jó válasz 2" ],
        "wrongAnswers" : [ "rossz válasz 1", "rossz válasz 2" ],
        "goodAnswerNeed" : true,
        "goodAnswerPiece" : 2,
    }


*/

public class TaskMillionaireData : TaskAncestor
{
    public class AnswerData
    {
        public string id;
        public string answer;
        public bool isCorrect;

        public AnswerData(JSONNode json)
        {
            id = json[C.JSONKeys.answerID];
            answer = json[C.JSONKeys.answer];
            isCorrect = json[C.JSONKeys.is_correct].Value == "1";
        }
    }


    public string question;             // A kérdés
    public string questionPicture;      // A kérdéshez tartozó kép
    public bool questionPictureBorder;  // A kérdés képe körül legyen-e keret
    public List<string> goodAnswers;    // A lehetséges jó válaszok Régi szerver
    public List<string> wrongAnswers;   // A lehetséges rossz válaszok Régi szerver
    public List<AnswerData> answers;    // A lehetséges összes válasz Server2020
    public bool goodAnswerNeed;         // Ha ez true, akkor a jó válaszokat kell adni, ha false, akkor az összes rossz választ kell megadni
    public int goodAnswerPiece;         // Hány jó választ kell adni
    //public int lifeNumber;              // Hány élete van a játékosnak
    //public int time;                    // Mennyi idő van a játék megoldására, ha -1, akkor nincs időmérés


    // Extra információ
    int[] answerOrder;

    // Vége a játéknak ha minden kérdésre válaszoltunk vagy ha rontottunk már annyit amennyi az életek száma vagy lejárt az idő
    /*  Nem muszáj felülírni, mivel ugyan az mint a TaskAncestor-ban
    override public bool taskIsEnd
    {
        get
        {
            return (GetQuestionNumber() == goodAnswersCount && Common.configurationController.answerFeedback == ConfigurationController.AnswerFeedback.Immediately) ||
                (GetAnswerNumber() == goodAnswersCount + wrongAnswersCount && Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately) ||
                (tries > 0 && tries <= wrongAnswersCount) ||
                outOfTime ||
                taskMustEnd;
        }
    }
    */

    // A feladatot hány százalékosan sikerült megoldani
    override public float resultPercent
    {
        get
        {
            return ((float)goodAnswersCount / (wrongAnswersCount + GetQuestionNumber())) * 100;
        }
    }

    // A megadott JSON alapján inicializálja a változóit
    public TaskMillionaireData(JSONNode jsonNode)
    {
        taskType = TaskType.Millionaire;
        differentAnswerFeedbackEnabled = true;
        InitDatas(jsonNode);
    }

    override protected void InitTaskDatas(JSONNode jsonNode)
    {
        error = false;

        question = jsonNode[C.JSONKeys.question];
        questionPicture = jsonNode[C.JSONKeys.image];
        if (string.IsNullOrEmpty(questionPicture))
            questionPicture = "";

        questionPictureBorder = jsonNode[C.JSONKeys.imageBorder].AsBool;

        // Old server
        goodAnswers = new List<string>();
        for (int i = 0; i < jsonNode[C.JSONKeys.goodAnswers].Count; i++)
            goodAnswers.Add(jsonNode[C.JSONKeys.goodAnswers][i]);

        wrongAnswers = new List<string>();
        for (int i = 0; i < jsonNode[C.JSONKeys.wrongAnswers].Count; i++)
            wrongAnswers.Add(jsonNode[C.JSONKeys.wrongAnswers][i]);

        // Server2020
        goodAnswerPiece = 0;
        answers = new List<AnswerData>();
        for (int i = 0; i < jsonNode[C.JSONKeys.answers].Count; i++)
        {
            AnswerData answerData = new AnswerData(jsonNode[C.JSONKeys.answers][i]);
            answers.Add(answerData);

            if (answerData.isCorrect)
                goodAnswerPiece++;
        }

        goodAnswerNeed = jsonNode[C.JSONKeys.goodAnswerNeed].AsBool;
        //goodAnswerPiece = jsonNode[C.JSONKeys.goodAnswerPiece].AsInt;

        waitBeforeStart = waitScreenChange + GetAnswers().Count * animSpeed2 + animSpeed1 * 2 + ((questionPicture != "") ? animSpeed1 : 0);
        waitBetweenQuestion = 0.1f;
    }

    override public JSONNode GetExtraInfo()
    {
        /*
        JSONClass jsonData = new JSONClass();

        int[] answerOrder = Common.GetRandomNumbers(GetAnswers().Count);

        for (int i = 0; i < answerOrder.Length; i++)
            jsonData[C.JSONKeys.answerOrder][i].AsInt = answerOrder[i];
        
        return jsonData;
        */

        return Common.ArrayToJSON(Common.GetRandomNumbers(GetAnswers().Count));
    }

    override public void AddExtraInfo(JSONNode jsonNode) {
        try
        {
            jsonNode = jsonNode[C.JSONKeys.answerOrder];
            // Ha nem egyezik a kérdések sorrendje tömb hossza a feladatban található kérdések számával, akkor dobunk egy hiba üzenetet
            if (jsonNode.Count != GetAnswers().Count)
                throw new System.Exception();

            answerOrder = Common.JSONToArray(jsonNode[C.JSONKeys.answerOrder]);
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

    public List<string> GetAnswersShuffle()
    {
        List<string> answers = GetAnswers();

        if (answerOrder == null)
            answers.Shuffle();
        else
            answers.ShuffleByIndexArray(answerOrder);

        return answers;

        /*
        // Összegyűjtjük a lehetséges válaszokat
        List<string> list = new List<string>(goodAnswers);
        list.AddRange(wrongAnswers);

        // Megkeverjük őket
        list.Shuffle();

        return list;
        */
    }

    List<string> GetAnswers() {
        // Összegyűjtjük a lehetséges válaszokat
        List<string> answers = new List<string>();
        if (Common.configurationController.isServer2020)
        {
            // Az új szerveren a válaszok egy tömbbe vannak
            for (int i = 0; i < this.answers.Count; i++)
                answers.Add(this.answers[i].id);
        }
        else
        {
            // A régi szerveren a válaszok
            answers.AddRange(goodAnswers);
            answers.AddRange(wrongAnswers);
        }

        return answers;
    }

    int GetAnswerNumber()
    {
        if (Common.configurationController.isServer2020)
            return answers.Count;
        else
            return goodAnswers.Count + wrongAnswers.Count;
    }


    public string GetTextFromID(string ID)
    {
        for (int i = 0; i < answers.Count; i++)
            if (answers[i].id == ID)
                return answers[i].answer;

        return "bad ID : " + ID;
    }

    public bool IsAnswerGood(string answerID)
    {
        for (int i = 0; i < answers.Count; i++)
            if (answers[i].id == answerID)
                return answers[i].isCorrect;

        return false;
    }

    override protected int GetQuestionNumber()
    {
        if (Common.configurationController.isServer2020)
            return goodAnswerPiece;
        else
            return goodAnswers.Count;
    }



    override protected string AnswerEvaluation(JSONNode jsonData)
    {
        if (Common.configurationController.isServer2020)
            return IsAnswerGood(jsonData[C.JSONKeys.selectedAnswer].Value) ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
        else
            return goodAnswers.Contains(jsonData[C.JSONKeys.selectedAnswer].Value) ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
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
            replayAnswer[C.JSONKeys.selectedQuestion].AsInt = 0;
            replayAnswer[C.JSONKeys.selectedSubQuestion].AsInt = 0;
            replayAnswer[C.JSONKeys.selectedAnswer] = evaluations.answers[replayIndex][C.JSONKeys.answerID];
            replayAnswer[C.JSONKeys.evaluateAnswer] = evaluations.answers[replayIndex][C.JSONKeys.isGood].AsBool ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;

            //replayAnswer = evaluations.answers[replayIndex];
            replayIndex++;
        }

        return replayAnswer;
    }
}
