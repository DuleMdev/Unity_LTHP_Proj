using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*
    {
        "question" : "A kérdés szövege",
        "sets" : [
            {
                "id" : "28",
                "name" : "Kettővel osztható számok",
                "image" : "A kép neve",
                "items" : [ "4", "8", "10" ]
            },
            {
                "id" : "29",
                "name" : "Páratlan számok",
                "image" : "A kép neve",
                "items" : [ "4", "8", "10" ]
            },
        ]
    }


*/

public class TaskSetsData : TaskAncestor {

    // Az alábbi osztály egy darab halmaz adatait tartalmazza
    public class Set
    {
        public class ItemData
        {
            public string answerID;
            public string answer;
            public bool isImage;

            public ItemData(JSONNode json)
            {
                answerID = json[C.JSONKeys.answerID];
                answer = json[C.JSONKeys.answer];
                isImage = json[C.JSONKeys.is_image].Value == "1";
            }
        }

        public string id;
        public string name;             // halmaz neve
        public string pictureName;      // A halmazban látható kép neve

        public List<string> items;      // a halmaz elemei
        public List<ItemData> itemDatas;      // a halmaz elemei Server2020

        public Set(JSONNode node) {
            InitData(node);
        }

        public void InitData(JSONNode node) {
            //JSONNode node = JSON.Parse(jsonText);

            id = node[C.JSONKeys.setsGroupID];
            name = node[C.JSONKeys.name];
            pictureName = node[C.JSONKeys.image];

            items = new List<string>();
            itemDatas = new List<ItemData>();
            for (int i = 0; i < node[C.JSONKeys.items].Count; i++)
                if (Common.configurationController.isServer2020)
                    itemDatas.Add(new ItemData(node[C.JSONKeys.items][i]));
                else
                    items.Add(node[C.JSONKeys.items][i]);
        }

        public List<string> GetItems()
        {
            if (Common.configurationController.isServer2020)
            {
                List<string> items = new List<string>();
                foreach (ItemData item in itemDatas)
                    items.Add(item.answerID);

                return items;
            }
            else
                return items;
        }

        public bool ContainAnswerID(string answerID)
        {
            foreach (ItemData item in itemDatas)
                if (item.answerID == answerID)
                    return true;

            return false;
        }
    }

    public string question;             // A kérdés
    public List<Set> sets;              // Halmazok

    public List<string> allAnswers;     // Az összes lehetséges válasz jók és rosszak együtt

    // Extra információ
    public int[] answerOrder; // A válaszok sorrendje

    // Vége a játéknak ha minden kérdésre válaszoltunk vagy ha rontottunk már annyit amennyi az életek száma vagy lejárt az idő
//    override public bool gameIsEnd
//    {
//        get
//        {
//            return (GetQuestionNumber() == goodAnswersCount && Common.configurationController.answerFeedback == ConfigurationController.AnswerFeedback.Immediately) ||
//                (GetQuestionNumber() == goodAnswersCount + wrongAnswersCount && Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately) ||
//                (tries > 0 && tries <= wrongAnswersCount) ||
//                outOfTime ||
//                gameEnd;
//        }
//
//        get
//        {
//            return base.gameIsEnd ||
//                (GetQuestionNumber() == goodAnswersCount + wrongAnswersCount && Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately);
//        }
//    }

    // A megadott JSON alapján inicializálja a változóit
    public TaskSetsData(JSONNode jsonNode)
    {
        taskType = TaskType.Sets;
        differentAnswerFeedbackEnabled = true;
        InitDatas(jsonNode);
    }

    override protected void InitTaskDatas(JSONNode jsonNode)
    {
        error = false;

        id = jsonNode[C.JSONKeys.questionID];

        question = jsonNode[C.JSONKeys.question];

        sets = new List<Set>();
        for (int i = 0; i < jsonNode[C.JSONKeys.sets].Count; i++)
            sets.Add(new Set(jsonNode[C.JSONKeys.sets][i]));

        allAnswers = new List<string>();

        for (int i = 0; i < sets.Count; i++)
            allAnswers.AddRange(sets[i].GetItems());

        waitBetweenQuestion = 0.1f;
    }

    public List<string> GetAnswersShuffle()
    {
        List<string> answers = GetAnswers();

        if (answerOrder == null)
            answerOrder = Common.GetRandomNumbers(allAnswers.Count);

        answers.ShuffleByIndexArray(answerOrder);

        return answers;
    }

    List<string> GetAnswers() {
        return new List<string>(allAnswers);
    }

    override public JSONNode GetExtraInfo()
    {
        answerOrder = Common.GetRandomNumbers(allAnswers.Count);

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
            if (jsonNode.Count != allAnswers.Count)
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
        return allAnswers.Count;
    }

    public Set.ItemData GetItemDataFromAnswerID(string answerID)
    {
        for (int i = 0; i < sets.Count; i++)
            for (int j = 0; j < sets[i].itemDatas.Count; j++)
                if (sets[i].itemDatas[j].answerID == answerID)
                    return sets[i].itemDatas[j];

        return null;
    }

    override protected string AnswerEvaluation(JSONNode jsonData)
    {
        int questionIndex = jsonData[C.JSONKeys.selectedQuestion].AsInt; // Melyik halmazba akarjuk tenni
        int answerIndex = jsonData[C.JSONKeys.selectedAnswer].AsInt;    // Mit akarunk a halmazba tenni

        Debug.Log("Válasz : " + questionIndex + "|" + answerIndex + "|" + allAnswers[answerIndex]);

        if (Common.configurationController.isServer2020)
            return sets[questionIndex].ContainAnswerID(allAnswers[answerIndex]) ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
        else
            return (sets[questionIndex].items.Contains(allAnswers[answerIndex])) ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
    }

    override public JSONNode GetEvaluations()
    {
        if (Common.configurationController.isServer2020)
        {
            for (int i = 0; i < evaluations.answers.Count; i++)
            {
                int questionIndex = evaluations.answers[i][C.JSONKeys.questionID].AsInt;
                evaluations.answers[i][C.JSONKeys.questionID] = sets[questionIndex].id;

                int answerIndex = evaluations.answers[i][C.JSONKeys.answer].AsInt;
                evaluations.answers[i][C.JSONKeys.answer] = allAnswers[answerIndex];
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
            replayAnswer[C.JSONKeys.selectedQuestion].AsInt = GetSetIndexFromID(evaluations.answers[replayIndex][C.JSONKeys.setsGroupID]);
            replayAnswer[C.JSONKeys.selectedSubQuestion].AsInt = 0;
            replayAnswer[C.JSONKeys.selectedAnswer].AsInt = GetAnswerIndexFromID(evaluations.answers[replayIndex][C.JSONKeys.answerID]);
            replayAnswer[C.JSONKeys.evaluateAnswer] = evaluations.answers[replayIndex][C.JSONKeys.isGood].AsBool ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
            replayAnswer[C.JSONKeys.replayMode].AsBool = true;

            //replayAnswer = evaluations.answers[replayIndex];
            replayIndex++;

            Debug.LogWarning("replayAnswer : \n" + replayAnswer.ToString());
        }

        return replayAnswer;
    }

    int GetSetIndexFromID(string id)
    {
        for (int i = 0; i < sets.Count; i++)
        {
            if (sets[i].id == id)
                return i;
        }

        return -1;
    }

    int GetAnswerIndexFromID(string id)
    {
        int index = 0;

        for (int i = 0; i < sets.Count; i++)
            for (int j = 0; j < sets[i].itemDatas.Count; j++)
            {
                if (sets[i].itemDatas[j].answerID == id)
                    return index;

                index++;
            }

        return -1;
    }

}
