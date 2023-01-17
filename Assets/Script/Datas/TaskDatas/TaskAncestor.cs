using UnityEngine;
using System.Collections;
using SimpleJSON;

/*
Ez az osztály minden feladatosztály őse, minden feladatosztály ebből származik.
Azért van, hogy egy tömbbe tudjam tenni a különböző feladatokat és tudjam, hogy melyik feladat milyen típusú.

GetExtraInfo - A feladathoz tartozó extra információkat adja meg. MultiPlayer miatt lett létrehozva, hogy
minden résztvevőnél a játék a lehetőségekhez képest ugyan úgy nézzen ki.

Start - Indítjuk a játékot, alaphelyzetbe állítja az értékeket

Answer - megadhatunk egy válasz json formában
{
    "selectedQuestion" : "valami",
    "selectedAnswer" : "bármi",
}
A vissza adott érték mutatja a válasz helyességét.

gameIsEnd - mutatja, hogy vége van-e a játéknak már. Több okból is vége lehet a játéknak, bővebben lásd ott.

resultPercent - mutatja a feladat megoldás hatékonyságát százalékban. Akkor célszerű megnézni, ha vége van már a feladatnak.


A származtatott osztályokban felül kell írni az alábbi metódusokat.

InitData            - beolvassa az aktuális játék a játék specifikus adatokat.
GetExtraInfo        - Ha a játékokban több kérdés vagy több válasz van, akkor itt meghatározhatjuk az index sorrendjüket.
GetQuestionNumber   - Meg kell határozni, hogy hány kérdésből áll a képernyő.
AnswerEvaluation    - A felhasználó által adott válaszról el kell dönteni, hogy helyes vagy helytelen.

*/

public class TaskAncestor  {

    public class Evaluations
    {
        public JSONArray answers;

        public Evaluations()
        {
            answers = new JSONArray();
        }

        public void AddEvaluation(string questionID, string subQuestionID, string answerID, bool isGood, string time)
        {
            JSONClass jsonClass = new JSONClass();
            jsonClass[C.JSONKeys.questionID] = string.IsNullOrEmpty(questionID) ? "" : questionID;
            jsonClass[C.JSONKeys.subQuestionID] = string.IsNullOrEmpty(subQuestionID) ? "" : subQuestionID;
            jsonClass[C.JSONKeys.answer] = answerID;
            jsonClass[C.JSONKeys.isGood].AsBool = isGood;
            jsonClass[C.JSONKeys.time] = time;

            AddEvaluation(jsonClass);
        }

        public void AddEvaluation(JSONNode evaluation)
        {
            answers.Add(evaluation);
        }

        //public void AddTimeValue(string time)
        //{
        //    // Az utolsó bejegyzésnek megváltoztatjuk az idejét
        //    answers[answers.Count - 1][C.JSONKeys.time] = time;
        //}
    }

    public enum TaskType
    {
        Unknown,            // Ismeretlen (nincs még beállítva)
        TrueOrFalse,        // Igaz hamis játék
        Bubble,             // Buborékos játék
        Sets,               // Halmazos játék
        MathMonster,        // Matek szörny játék
        Affix,              // Toldalékos játék
        Boom,               // Boom játék (TV-s)
        Fish,               // Halas játék
        Hangman,            // Akasztófás játék
        Read,               // Olvasás értéses játék
        Millionaire,        // Milliomos játék
        Texty,              // Matematikai szövegértéses játék

        FreePlay,           // A játékokból véletlenszerűen választ

        PDF,
        YouTube,
        Psycho,
    }

    public bool differentAnswerFeedbackEnabled = false; // Figyelembe vegye-e a ConfigurationController.AnswerFeedback beállítását vagy ne, ha nem engedélyezzük, akkor csak egy féle az azonnali lehetséges. Ez ki hat a játék végének meghatározására, illetve az eredmény számításra.

    public GameData gameData;       // A feladat (screen) melyik játékhoz tartozik

    public string info;

    public string id;               // A képernyő azonosítója

    public int tries { get; protected set; } // Hány élete van a játékosnak, ha nulla akkor végtelen sok van
    public float time { get; protected set; }     // Mennyi idő van a játék megoldására, ha -1, akkor nincs időmérés

    public TaskType taskType { get; protected set; }
    public bool error;              // Történt hiba az adatok feldolgozása közben?

    public Common.CallBack_In_JSONNode sendMessageAllClient; // Ha az Update metódusban üzenetet akarunk küldeni a klienseknek, akkor ezen keresztűl megtehetjük

    // Feldolgozás után kiolvasható értékek   
    //public int questionNumber { get; protected set; }       // Hány kérdést tartalmaz a képernyő
    public int goodAnswersCount { get; protected set; }     // Hány kérdésre válaszoltak már helyesen
    public int wrongAnswersCount { get; protected set; }    // Hány kérdésre válaszoltak már rosszúl

    public float waitScreenChange { get; protected set; }   // Mennyit várjon a képernyő váltásra, a ScreenController-ben a képernyő váltás sebessége
    public float waitBeforeStart { get; protected set; }    // Mennyit várjon amíg az első választ meg lehet adni (Ez az az idő amíg a játék felépül az indítása után, tehát a játék elemek a helyükre kerülnek)
    public float waitBetweenQuestion { get; protected set; }// Mennyit várjon válaszadás után míg jöhet a következő válasz (itt a jó vagy a rossz válasz szerint villogtatunk bizonyos elemeket)
    public float waitAtGameEnd { get; protected set; }      // Mennyit várjon ha befejeződött a feladat, tehát meddig mutassa a végeredményt

    virtual public float waitUntilHideElements { get; protected set; } // Mennyi várjon az elemek eltüntetésére
    /*
    public float _waitUntilHideElements;
    float GetWaitUntilHideElemenets() {
        return
    }*/

    public float animSpeed1 { get; protected set; }    // A megjelenés és az eltüntetés animációinak sebessége
    public float animSpeed2 { get; protected set; }    // 

    public float gameRemainTime { get; protected set; }    // Mennyi idő maradt a játékból

    protected bool taskMustEnd; // Ha azt akarjuk, hogy vége legyen a játéknak az alábbi okokon kívűl

    protected Evaluations evaluations = new Evaluations();

    public bool replayMode; // A feladatot replay módban kell játszani
    public int replayIndex; // Mutatja, hogy melyik válasz a következő

    protected float answerTime;

    // Vége a játéknak ha minden kérdésre válaszoltunk vagy ha rontottunk már annyit amennyi az életek száma vagy lejárt az idő
    virtual public bool taskIsEnd {
        get {
            return (!replayMode && GetQuestionNumber() == goodAnswersCount && (!differentAnswerFeedbackEnabled || Common.configurationController.answerFeedback == ConfigurationController.AnswerFeedback.Immediately)) ||
                //(differentAnswerFeedbackEnabled && GetQuestionNumber() == goodAnswersCount + wrongAnswersCount && Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately) ||
                (tries > 0 && tries <= wrongAnswersCount) ||
                outOfTime ||
                taskMustEnd;
        }
    }

    // Lejárt az idő
    public bool outOfTime { get { return (time > 0 && gameRemainTime <= 0); } }

    // A feladatot hány százalékosan sikerült megoldani
    virtual public float resultPercent {
        get
        {
            float result = ((float)goodAnswersCount / (wrongAnswersCount + GetQuestionNumber())) * 100;
            if (differentAnswerFeedbackEnabled && Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately)
                result = ((float)goodAnswersCount / GetQuestionNumber()) * 100;

            return result;
        }
    } 

    virtual public float elapsedGameTime { get; set; }

    protected JSONNode extraData; // Ebben állítjuk össze az extraData-t

    virtual public void InitDatas(JSONNode jsonNode)
    {
        error = true;

        waitScreenChange = 0.5f;
        waitBeforeStart = 2;
        waitBetweenQuestion = 1;
        waitAtGameEnd = 0;
        waitUntilHideElements = 0;

        animSpeed1 = 0.5f;
        animSpeed2 = 0.1f;

        time = 180; // Az alap értelmezett idő 3 perc
        tries = 0;

        id = jsonNode[C.JSONKeys.id];

        if (jsonNode.ContainsKey(C.JSONKeys.time))
            time = jsonNode[C.JSONKeys.time].AsFloat;
        if (jsonNode.ContainsKey(C.JSONKeys.tries))
            tries = jsonNode[C.JSONKeys.tries].AsInt;

        if (jsonNode.ContainsKey(C.JSONKeys.info) && !string.IsNullOrWhiteSpace(jsonNode[C.JSONKeys.info]))
            info = jsonNode[C.JSONKeys.info];


        Debug.LogWarning("InitDatas");
        // Ha vannak már válaszok, akkor visszajátszás lesz
        if (jsonNode.ContainsKey(C.JSONKeys.userAnswers))
        {
            time = 0;
            replayMode = true;
            replayIndex = 0;

            // Beolvassuk a visszajátszandó válaszokat
            for (int i = 0; i < jsonNode[C.JSONKeys.userAnswers][C.JSONKeys.answers].Count; i++)
            {
                JSONNode json = jsonNode[C.JSONKeys.userAnswers][C.JSONKeys.answers][i];
                json[C.JSONKeys.subQuestionID] = json[C.JSONKeys.questionIndex];
                json[C.JSONKeys.evaluateAnswer] = json[C.JSONKeys.isGood];

                evaluations.AddEvaluation(json);
            }

            // Ha nincs egy válasz sem, akkor egyből leállítjuk a játékot
            if (jsonNode[C.JSONKeys.userAnswers].Count == 0)
                taskMustEnd = true;
        }

        InitTaskDatas(jsonNode);

        //Start();
    }

    virtual protected void InitTaskDatas(JSONNode jsonNode) {

    }

    virtual public void AddExtraInfo(JSONNode jsonNode) {

    }

    virtual public JSONNode GetExtraInfo() {
        JSONClass jsonData = new JSONClass();

        return jsonData;
    }

    /// <summary>
    /// Elindítjuk a játékot. Az értékeket alaphelyzetbe állítjuk.
    /// </summary>
    virtual public void Start() {
        // Meghatározzuk, hogy hány kérdést tartalmaz a képernyő
        //questionNumber = GetQuestionNumber();

        if (replayMode)
            Common.configurationController.answerFeedback = ConfigurationController.AnswerFeedback.Immediately;
        else 
            evaluations = new Evaluations();


        answerTime = 0;

        // Nullázzuk a jó és a rossz válaszok számát
        goodAnswersCount = 0;
        wrongAnswersCount = 0;

        gameRemainTime = time;

        taskMustEnd = false;

        StartTask();
    }

    /// <summary>
    /// A játék task-ok feladat specifikust indítása.
    /// </summary>
    virtual protected void StartTask() {

    }

    virtual protected int GetQuestionNumber()
    {
        return 1;
    }

    public void LetTheTaskEnd()
    {
        taskMustEnd = true;
    }

    /// <summary>
    /// Válaszoltak egy kérdésre.
    /// </summary>
    /// <param name="jsonData">A jsonData-ban található, hogy mire válaszoltak és mit.</param>
    /// <returns>A vissza adott érték, hogy a válasz megfelelő-e.</returns>
    virtual public string Answer(JSONNode jsonData) 
    {
        string result = C.JSONValues.evaluateIsIgnore;
        if (!replayMode)
            result = AnswerEvaluation(jsonData);

        // A kiértékelés eredményét elhelyezzük a json-ban
        jsonData[C.JSONKeys.evaluateAnswer] = result;

        switch (result)
        {
            case C.JSONValues.evaluateIsFalse:
                wrongAnswersCount++;
                // A válasz már ki van értékelve, rögzítjük ezt az eseményt
                evaluations.AddEvaluation(jsonData[C.JSONKeys.selectedQuestion].Value, jsonData[C.JSONKeys.selectedSubQuestion], jsonData[C.JSONKeys.selectedAnswer].Value, false, answerTime.ToString());
                answerTime = 0;
                break;

            case C.JSONValues.evaluateIsTrue:
                goodAnswersCount++;
                evaluations.AddEvaluation(jsonData[C.JSONKeys.selectedQuestion].Value, jsonData[C.JSONKeys.selectedSubQuestion], jsonData[C.JSONKeys.selectedAnswer].Value, true, answerTime.ToString());
                answerTime = 0;
                break;

            case C.JSONValues.evaluateIsIgnore:

                break;
        }

        return result;
    }

    //void AddAnswerTime()
    //{
    //    evaluations.AddTimeValue(answerTime.ToString());
    //    answerTime = 0;
    //}

    virtual protected string AnswerEvaluation(JSONNode jsonData)
    { 
        return C.JSONValues.evaluateIsIgnore;
    }

    virtual public void FinalMessage(JSONNode jsonNode)
    {
        elapsedGameTime = jsonNode[C.JSONKeys.elapsedGameTime].AsFloat;
    }

    virtual public void Update() {

    }

    /// <summary>
    /// Az idő megy.
    /// </summary>
    public void GoTime() {
        // Az idő számlálása
        gameRemainTime = Mathf.Clamp(gameRemainTime -Time.deltaTime, 0, time);
        answerTime += Time.deltaTime;
    }

    public JSONNode GetExtraData()
    {
        extraData = null;
        CollectExtraData();
        return extraData;
    }

    virtual protected void CollectExtraData()
    {

    }

    virtual public JSONNode GetEvaluations()
    {
        return evaluations.answers;

        // Elvileg az evaluations nem lehet null, mivel a deklaráció helyén létrehozzuk az objektumot újabban.
        //return evaluations != null ? evaluations.answers : new JSONArray();
    }

    virtual public JSONNode GetNextReplayAnswer()
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
            replayAnswer[C.JSONKeys.selectedQuestion].AsInt = evaluations.answers[replayIndex][C.JSONKeys.questionID].AsInt;
            replayAnswer[C.JSONKeys.selectedSubQuestion].AsInt = evaluations.answers[replayIndex][C.JSONKeys.questionIndex].AsInt;
            replayAnswer[C.JSONKeys.selectedAnswer] = evaluations.answers[replayIndex][C.JSONKeys.answer];
            replayAnswer[C.JSONKeys.evaluateAnswer] = evaluations.answers[replayIndex][C.JSONKeys.isGood].AsBool ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;

            //replayAnswer = evaluations.answers[replayIndex];
            replayIndex++;
        }

        return replayAnswer;
    }

    public TaskAncestor Clone() {
        TaskAncestor cloneTaskAncestor = (TaskAncestor)this.MemberwiseClone();

        return cloneTaskAncestor;
    }
}
