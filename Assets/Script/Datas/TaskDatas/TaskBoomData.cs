using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*

    {
        "question" : "Ez a képernyő kérdés helye",

        "goodAnswers" : [
            {
                "is_image" : "0",   // A kérdés képet tartalmaz-e (igazából nem használt, a képek meghatározására az answer változót használjuk lásd ott hogyan)
                "answer" : "Peti",  // A kérdés, lehet szöveg vagy képnév (képnél az első karakter #)
                "time" : "0.5",     // Mennyi ideig látszódjon a kérdés
                "isCorrect" : "1"   // Talán a jó és a rossz válaszok megkülönböztetésére lett létrehozva (igazából nem használt, mivel külön tömbben vannak a jó és a rossz válaszok)
            },

            {
                "is_image" : "0",
                "answer" : "#pitt",
                "time" : "0.5",
                "isCorrect" : "1"
            }
        ]

        "wrongAnswers" : [
            {
                "is_image" : "0",
                "answer" : "#aniston",
                "time" : "1.5",
                "isCorrect" : "1"
            },

            {
                "is_image" : "0",
                "answer" : "Sári",
                "time" : "0.5",
                "isCorrect" : "1"
            }
        ]
    }

*/

public class TaskBoomData : TaskAncestor
{
    public class AnswerData
    {
        public string answerID;
        public bool isImage;
        public string answer;
        public float time;
        public bool isCorrect;

        public AnswerData(JSONNode node)
        {
            InitData(node);
        }

        public void InitData(JSONNode node)
        {
            answerID = node[C.JSONKeys.answerID];
            isImage = node[C.JSONKeys.is_image].Value == "1";

            answer = node[C.JSONKeys.answer];
            if (!Common.configurationController.isServer2020 && answer[0] == '#')
            {
                answer = answer.Substring(1);
                isImage = true;
            }

            time = node[C.JSONKeys.time].AsFloat;
            isCorrect = node[C.JSONKeys.is_correct].Value == "1";
        }
    }

    public string question;                 // A kérdés

    // Old server
    public List<AnswerData> goodAnswers;    // A jó válaszok
    public List<AnswerData> wrongAnswers;   // A rossz válaszok

    // Server2020
    public List<AnswerData> answers;        // Az összes lehetséges válasz

    public float showTime;                  // Mennyi ideig látszik egy kérdés
    public float breakTime;                 // Két kérdés között mennyi a szünet
    public int maxRound;                    // Hányszor menjen körbe a válaszok 0 = végtelen
    public bool random;                     // A kártyák véletlenszerű sorrendben jöjjenek, vagy a megadott sorrendben?
    public bool needGoodAnswers;            // A jó válaszokat kell kiválasztani vagy a rosszakat?
    public int goodAnswerPiece;             // Hány jó választ kell adni


    // Játék vezérlés -------------------------------------------------------------------------------------------
    enum TVStatus
    {
        ChannelError,       // Villog a képernyő
        Show,               // Mutat a tévé egy lehetséges választ
        Switch,             // Megnyomták a kapcsolót
    }

    TVStatus tvStatus;

    float remainTime;       // a következő állapot váltásig hátralevő idő
    int actQuestion;        // Melyik a következő kérdés
    int round;              // Hányadik körnél tartunk a "kártyák" mutogatásában
    int goodAnswersCounter; // Hány jó választ adtunk már

    List<AnswerData> questionList;  // Az összes még ki nem választott kérdés
    List<AnswerData> questionOrder; // A kérdéseknek milyen sorrendben kell felvillanniuk

    int succesfullTask;     // A jó helyre húzott elemek száma

    override public float resultPercent
    {
        get
        {
            float result = ((float)goodAnswersCount / (wrongAnswersCount + GetQuestionNumber())) * 100;
            //if (differentAnswerFeedbackEnabled && Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately)
            //    result = ((float)goodAnswersCount / GetQuestionNumber()) * 100;

            return result;
        }
    }

    public TaskBoomData(JSONNode jsonNode) {
        taskType = TaskType.Boom;
        differentAnswerFeedbackEnabled = true;
        InitDatas(jsonNode);
    }

    override protected void InitTaskDatas(JSONNode jsonNode)
    {
        error = false;

        question = jsonNode[C.JSONKeys.question];

        // Old server
        goodAnswers = new List<AnswerData>();
        for (int i = 0; i < jsonNode[C.JSONKeys.goodAnswers].Count; i++)
        {
            goodAnswers.Add(new AnswerData(jsonNode[C.JSONKeys.goodAnswers][i]));
            goodAnswers[goodAnswers.Count - 1].isCorrect = true;
        }

        wrongAnswers = new List<AnswerData>();
        for (int i = 0; i < jsonNode[C.JSONKeys.wrongAnswers].Count; i++)
        {
            wrongAnswers.Add(new AnswerData(jsonNode[C.JSONKeys.wrongAnswers][i]));
            wrongAnswers[wrongAnswers.Count - 1].isCorrect = false;
        }

        // Server2020
        answers = new List<AnswerData>();
        goodAnswerPiece = 0;
        for (int i = 0; i < jsonNode[C.JSONKeys.answers].Count; i++)
        {
            AnswerData answerData = new AnswerData(jsonNode[C.JSONKeys.answers][i]);
            answers.Add(answerData);

            if (answerData.isCorrect)
                goodAnswerPiece++;
        }

        showTime = (jsonNode.ContainsKey("showtime")) ? jsonNode["showtime"].AsFloat : 1.5f;
        breakTime = (jsonNode.ContainsKey("breaktime")) ? jsonNode["breaktime"].AsFloat : 0.5f;

        maxRound = jsonNode["round"].AsInt;
        random = jsonNode["random"].AsBool;

        needGoodAnswers = (jsonNode.ContainsKey(C.JSONKeys.goodAnswerNeed)) ? jsonNode[C.JSONKeys.goodAnswerNeed].AsBool : true;

        waitBeforeStart = waitScreenChange + GetAnswers().Count * animSpeed2 + animSpeed1 * 4;
        //waitBetweenQuestion = 1.2f;
    }

    // Vissza adjuk a lehetséges válaszokat
    public List<AnswerData> GetAnswers()
    {
        // Összegyűjtjük a lehetséges válaszokat
        List<AnswerData> list = new List<AnswerData>();
        if (Common.configurationController.isServer2020)
        {
            // Az új szerveren a válaszok egy tömbbe vannak
            list.AddRange(answers);
        }
        else
        {
            // A régi szerveren a válaszok
            answers.AddRange(goodAnswers);
            answers.AddRange(wrongAnswers);
        }

        // Megkeverjük őket
        list.Shuffle();

        return list;
    }

    /// <summary>
    /// A játék task-ok feladat specifikus indítása.
    /// </summary>
    override protected void StartTask()
    {
        remainTime = waitBeforeStart; // 2f;
        round = 0;
        goodAnswersCounter = 0;

        // Lekérdezzük a kérdéseket
        questionList = GetAnswers();

        SetQuestionOrder();
    }

    public string GetTextFromID(string ID)
    {
        for (int i = 0; i < answers.Count; i++)
            if (answers[i].answerID == ID)
                return answers[i].answer;

        return "bad ID : " + ID;
    }

    public AnswerData GetAnswerDataFromID(string ID)
    {
        for (int i = 0; i < answers.Count; i++)
            if (answers[i].answerID == ID)
                return answers[i];

        return null;
    }


    public bool IsAnswerGood(string answerID)
    {
        for (int i = 0; i < answers.Count; i++)
            if (answers[i].answerID == answerID)
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
        // Ha akkor érintették meg a tv képernyőjét amikor kép volt rajta
        if (tvStatus == TVStatus.Show) {
            bool answerCorrect = questionOrder[actQuestion].isCorrect;  //goodAnswers.Contains(questionOrder[actQuestion]); // tvScreen.GetText());
                                                                                            //answerCorrect = (answerCorrect && taskData.needGoodAnswers);
            answerCorrect = answerCorrect == needGoodAnswers;

            tvStatus = TVStatus.Switch;
            remainTime = 2;

            if (answerCorrect)
            {
                // elhelyezzük a jsonData-ban a választ és azt, hogy hányadik jó válasz volt már, az első a nulladik
                if (Common.configurationController.isServer2020)
                    jsonData[C.JSONKeys.selectedAnswer] = questionOrder[actQuestion].answerID;
                else
                    jsonData[C.JSONKeys.selectedAnswer] = questionOrder[actQuestion].answer;

                jsonData[C.JSONKeys.answerCount].AsInt = goodAnswersCounter;

                // A megadott választ eltávolítjuk a kérdések listájából
                questionList.Remove(questionOrder[actQuestion]);
                goodAnswersCounter++;

                return C.JSONValues.evaluateIsTrue;
            }
            else
            {
                jsonData[C.JSONKeys.selectedAnswer] = questionOrder[actQuestion].answerID;

                // Ha teszt módban vagyunk, akkor a rossz választ is töröljük a kérdések listájából
                if (Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately)
                {
                    questionList.Remove(questionOrder[actQuestion]);
                }

                return C.JSONValues.evaluateIsFalse;
            }
        }
        else
            return C.JSONValues.evaluateIsIgnore;
    }

    override public void Update()
    {
        if (!taskIsEnd && !replayMode)
        {
            remainTime -= Time.deltaTime;

            if (remainTime <= 0)
            {
                JSONClass jsonData;

                switch (tvStatus)
                {
                    case TVStatus.ChannelError:
                        // Bekapcsoljuk a TV-n a soron következő elemet

                        // Elkészítjük a json-t amit elküldünk a kliensnek
                        jsonData = new JSONClass();

                        jsonData[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
                        jsonData[C.JSONKeys.gameEventType] = C.JSONValues.show;
                        jsonData[C.JSONKeys.showItem] = questionOrder[actQuestion].answer;
                        jsonData[C.JSONKeys.showItemIsPicture].AsBool = questionOrder[actQuestion].isImage;

                        // Elküldjük a klienseknek 
                        sendMessageAllClient(jsonData);

                        remainTime = questionOrder[actQuestion].time; // showTime

                        tvStatus = TVStatus.Show;
                        break;
                    case TVStatus.Show:
                    case TVStatus.Switch:
                        // Kikapcsoljuk a tv-n a látható elemet

                        // Elkészítjük a json-t amit elküldünk a kliensnek
                        jsonData = new JSONClass();

                        jsonData[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
                        jsonData[C.JSONKeys.gameEventType] = C.JSONValues.show;
                        jsonData[C.JSONKeys.showItem] = "";

                        // Elküldjük a klienseknek 
                        sendMessageAllClient(jsonData);

                        // Ha nincs már több elem, akkor sose járjon le a breakTime
                        remainTime = (questionList.Count != 0) ? breakTime : float.MaxValue;

                        /*if (goodAnswersCounter == goodAnswers.Count)
                        {
                            gameEnd = true;
                        }
                        else */{
                            if (++actQuestion == questionOrder.Count)
                            {
                                // Ha nincs már következő kérdés, akkor új kérdés sorrendet határozunk meg
                                round++;

                                if (round < maxRound || maxRound == 0)
                                {
                                    //actQuestion = 0;
                                    SetQuestionOrder();
                                }
                                else {
                                    taskMustEnd = true;
                                }
                            };
                        }

                        tvStatus = TVStatus.ChannelError;
                        break;
                }
            }
        }
    }

    // Meghatározzuk a kérdések sorrendjét
    void SetQuestionOrder()
    {
        actQuestion = 0;
        questionOrder = new List<AnswerData>(questionList);

        if (random)
            Common.Shuffle(questionOrder);
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
