using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*

 {
    "screenQuestion" : "Ez itt a képernyő kérdés helye",
    "question" : "Kérdés",
    "image" : "A megjelenítendő kép neve",
    "imageBorder" : "false",

    "goodAnswers" : [
        "Aniston"
    ],

    "wrongAnswers" : [
        "pitt", "Peti", "Feri", "Gáspár",
    ],

    "goodAnswerNeed" : "false",             // A jó vagy a rossz válaszokat kell megadni (Nem használt)
    "goodAnswerPiece" : "2",
 }

 Server_2020
 { 
    "id" : "236", 
    "image" : "3/b6737eb9eb460b567900ed9927209cd9.jpg", 
    "question" : "DUCK?", 
    "task" : "Pukkaszd ki a rossz választ tartalmazó buborékokat!", 
    "time" : "180", 
    "answers" : [ 
       { 
          "answerID" : "759", 
          "answer" : "3/0cbb5e2f3616c6ddf57e4b4b2a73ca61.jpg", 
          "is_image" : "1", 
          "is_correct" : "1"
       }, 
       { 
          "answerID" : "760", 
          "answer" : "NO DUCK", 
          "is_image" : "0", 
          "is_correct" : "0"
       }
    ]
 }

*/

public class TaskBubbleData : TaskAncestor
{
    public string taskText;             // Képernyőn megjelenő utasítás (Pukkaszd ki a rossz választ tartalmazó buborékokat). Ez az alapértelmezett (kölönböző nyelvekre lefordított) ha nincs más megadva

    public string question;             // A kérdés
    public string questionPicture;      // A kérdéshez tartozó kép
    public bool questionPictureBorder;  // A kérdés képe körül legyen-e keret
    public List<string> goodAnswers;    // A lehetséges jó válaszok
    public List<string> wrongAnswers;   // A lehetséges rossz válaszok
    public bool goodAnswerNeed;         // Ha ez true, akkor a jó válaszokat kell adni, ha false, akkor az összes rossz választ kell megadni
    public int goodAnswerPiece;         // Hány jó választ kell adni

    List<int> allAnswers;            // Hány buborékot pukasztottak már ki

    override public float waitUntilHideElements { get { return allAnswers.Count * 0.5f + animSpeed1; } protected set { } } // Mennyi várjon az elemek eltüntetésére

    bool solved;                      // A feladat meg van oldva
    int reBorn;                         // Hányszor születtek újjá a buborékok

    ConfigurationController.AnswerFeedback originalGameEnding;

    // Server_2020 adatok
    public class AnswerData
    {
        public int answerID;
        public string answer;
        public bool isImage;
        public bool isCorrect;
    }

    List<AnswerData> answersData;

    // Vége a játéknak ha rontottunk már annyit amennyi az életek száma vagy lejárt az idő vagy helyes a válasz (Ha helyes a válasz, akkor a screenMustEnd változót igazra állítjuk az értékelő helyen)
    override public bool taskIsEnd
    {
        get
        {
            return (tries > 0 && tries <= reBorn) ||
                outOfTime ||
                taskMustEnd;
        }
    }

    // A feladatot hány százalékosan sikerült megoldani
    override public float resultPercent
    {
        get
        {
            // Ha teszt módban futtatjuk az alkalmazást
            if (Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately)
            {
                // meg kell számolni a megmaradt buborékok közül, hogy hány jó és rossz van
                goodAnswersCount = 0;
                wrongAnswersCount = 0;

                foreach (int item in allAnswers)
                    if (!GetAnswerDataByAnswerID(item).isCorrect)
                        // Ha van egy olyan buborék amit ki kellett volna pukkasztani, de nem lett az hiba
                        wrongAnswersCount++;
                    else
                        goodAnswersCount++;

                return (float)goodAnswersCount / (wrongAnswersCount + goodAnswerPiece) * 100;
            }

            // Ha normál módban fut, akkor így számoljuk az eredményt
            return ((float)((solved) ? 1 : 0) / (reBorn + 1)) * 100;
        }
    }

    // A megadott JSON alapján inicializálja a változóit
    public TaskBubbleData(JSONNode jsonNode)
    {
        taskType = TaskType.Bubble;
        InitDatas(jsonNode);
    }

    override protected void InitTaskDatas(JSONNode jsonNode)
    {
        error = false;

        taskText = jsonNode[C.JSONKeys.task].Value;

        question = jsonNode[C.JSONKeys.question];
        questionPicture = jsonNode[C.JSONKeys.image];
        questionPictureBorder = jsonNode.ContainsKey(C.JSONKeys.imageBorder) ? jsonNode[C.JSONKeys.imageBorder].AsBool : true;

        if (Common.configurationController.isServer2020)
        {
            goodAnswerPiece = 0;
            answersData = new List<AnswerData>();
            for (int i = 0; i < jsonNode[C.JSONKeys.answers].Count; i++)
            {
                AnswerData answer = new AnswerData();
                answer.answerID = jsonNode[C.JSONKeys.answers][i][C.JSONKeys.answerID].AsInt;
                answer.answer = jsonNode[C.JSONKeys.answers][i][C.JSONKeys.answer].Value;
                answer.isImage = jsonNode[C.JSONKeys.answers][i][C.JSONKeys.is_image].Value != "0";
                answer.isCorrect = jsonNode[C.JSONKeys.answers][i][C.JSONKeys.is_correct].Value != "0";

                answersData.Add(answer);

                if (answer.isCorrect) // Megszámoljuk hány jó válasz van (ennyinek kell maradnia, ha kipukkasztottuk a buborékokat)
                    goodAnswerPiece++;
            }
        }
        else
        {
            goodAnswers = new List<string>();
            for (int i = 0; i < jsonNode[C.JSONKeys.goodAnswers].Count; i++)
                goodAnswers.Add(jsonNode[C.JSONKeys.goodAnswers][i]);

            wrongAnswers = new List<string>();
            for (int i = 0; i < jsonNode[C.JSONKeys.wrongAnswers].Count; i++)
                wrongAnswers.Add(jsonNode[C.JSONKeys.wrongAnswers][i]);

            goodAnswerPiece = jsonNode[C.JSONKeys.goodAnswerPiece].AsInt;
        }

        goodAnswerNeed = jsonNode[C.JSONKeys.goodAnswerNeed].AsBool;

        // Idők beállításai
        animSpeed2 = 0.1f;

        if (Common.configurationController.isServer2020)
            waitBeforeStart =
                waitScreenChange + // Képernyőváltás sebessége
                ((!string.IsNullOrEmpty(questionPicture)) ? animSpeed1 : 0) + // Ha tartozik a kérdéshez kép, akkor az megjelenik
                animSpeed1 + // Megjelenik a kérdés
                answersData.Count * animSpeed2 + animSpeed1; // Megjelennek a buborékok
        else
            waitBeforeStart =
                waitScreenChange + // Képernyőváltás sebessége
                ((!string.IsNullOrEmpty(questionPicture)) ? animSpeed1 : 0) + // Ha tartozik a kérdéshez kép, akkor az megjelenik
                animSpeed1 + // Megjelenik a kérdés
                (goodAnswers.Count + wrongAnswers.Count) * animSpeed2 + animSpeed1; // Megjelennek a buborékok

        waitBetweenQuestion = 0.5f;
    }

    public List<int> GetAnswers()
    {
        // Összegyűjtjük a lehetséges válaszokat
        //List<string> list = new List<string>(goodAnswers);
        //list.AddRange(wrongAnswers);
        //
        //// Megkeverjük őket
        //list.Shuffle();

        List<int> answers = new List<int>();

        if (Common.configurationController.isServer2020)
        {
            // A lista a válaszok azonosítóját fogja tárolni
            foreach (AnswerData answerData in answersData)
                answers.Add(answerData.answerID);
        }
        else
        {
            answers = new List<int>(Common.GetRandomNumbers(goodAnswers.Count + wrongAnswers.Count));
        }

        answers.Shuffle(); // Megkeverjük a válaszokat

        return answers; // new List<int>(Common.GetRandomNumbers(goodAnswers.Count + wrongAnswers.Count)); // list;
    }

    public AnswerData GetAnswerDataByAnswerID(int answerID)
    {
        foreach (AnswerData answerData in answersData)
        {
            if (answerData.answerID == answerID)
                return answerData;
        }

        return null;
    }

    /// <summary>
    /// A játék task-ok feladat specifikust indítása.
    /// </summary>
    override protected void StartTask()
    {
        allAnswers = GetAnswers();
        solved = false;
        reBorn = 0;

        // Ha replay módban játszuk le a játékot, akkor meg kell határozni, hogy vajon a játékot normál vagy teszt módban 
        // játszották eredetileg
        // Bizzunk abban, hogy a szerver a gameEnding-et olyan értékkel küldi amilyennel a lejátszás lezajlott.
        // Persze ha nem, akkor ha megváltoztatják a gameEnding-et, akkor rosszúl fogja visszajátszani a játékot.
        // Ezért figyelni kell a következőkre:
        // Ha a userAnswers-ben több válasz van mint a lehetséges válaszok száma, akkor normál mód volt biztos.
        // Illetve ha egy answerID többször is szerepel a válaszok között, akkor is normál volt.

        originalGameEnding = (ConfigurationController.AnswerFeedback)gameData.gameEnding - 1;

        //int possibleMaxAnswersInTestMode = 0;
        //foreach (var item in answersData)
        //    if (item.isCorrect)
        //        possibleMaxAnswersInTestMode++;

        bool doubleAnswersID = false;
        List<string> answersIDs = new List<string>();
        for (int i = 0; i < evaluations.answers.Count; i++)
        {
            if (answersIDs.Contains(evaluations.answers[i][C.JSONKeys.answerID]))
            {
                doubleAnswersID = true;
                break;
            }

            answersIDs.Add(evaluations.answers[i][C.JSONKeys.answerID]);
        }

        if (answersData.Count < evaluations.answers.Count || doubleAnswersID)
            originalGameEnding = ConfigurationController.AnswerFeedback.Immediately;
    }

    override protected string AnswerEvaluation(JSONNode jsonData)
    {
        if (Common.configurationController.isServer2020)
            return AnswerEvaluationServer2020(jsonData);
        else
            return AnswerEvaluationServerOld(jsonData);
    }

    string AnswerEvaluationServerOld(JSONNode jsonData)
    {
        waitBetweenQuestion = 0.2f;

        // Az számít jó válasznak, ha a felhasználó egy rossz választ (mivel azokat kell neki kipukkasztania
        bool answerIsGood = jsonData[C.JSONKeys.selectedAnswer].AsInt >= goodAnswers.Count; //  wrongAnswers.Contains(jsonData[C.JSONKeys.selectedAnswer].Value);

        //int answerID = answerIsGood ? wrongAnswers.IndexOf()

        allAnswers.Remove(jsonData[C.JSONKeys.selectedAnswer].AsInt);

        if (allAnswers.Count == goodAnswers.Count) {

            waitBetweenQuestion = 1.3f; // A villogás ideje, amíg a jó vagy a rossz válasz villog

            // Akkor megnézzük, hogy a még létező buborékok a jó válaszok közül valóak-e
            bool goodAnswer = true;
            foreach (int item in allAnswers)
                if (item >= goodAnswers.Count)
                {
                    goodAnswer = false;
                    break;  // Megszakítjuk a ciklust, felesleges tovább futnia, mert találtunk egy rossz választ
                }

            // Elkészítjük a json-t amit elküldünk a kliensnek
            JSONClass newData;
            newData = new JSONClass();
            newData[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;

            if (goodAnswer)
            { // Ha igen, akkor vége
                newData[C.JSONKeys.gameEventType] = C.JSONValues.goodSolution;

                solved = true;
                taskMustEnd = true;
            }
            else
            { // Ha nem újra termeljük a buborékokat
                newData[C.JSONKeys.gameEventType] = C.JSONValues.wrongSolution;

                reBorn++;
                allAnswers = GetAnswers();
            }

            // Várunk egy tized másodpercet, majd elküldjük a végeredményt is a klienseknek
            // Azért várunk mert előbb a kiértékelés eredményét küldjük el, vagyis, hogy melyik buborékot kell kipukkasztani
            // Majd azt hogy a maradék buborék azok jók vagy nem jók, milyen színnel villogjon és ha pirossal, akkor újra termelődnek a villogás után a buborékok
            Common.configurationController.WaitTime(0.1f, () =>
            {
                // Elküldjük a klienseknek 
                sendMessageAllClient(newData);
            });
        }

        return (answerIsGood) ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
    }

    string AnswerEvaluationServer2020(JSONNode jsonData)
    {
        waitBetweenQuestion = 0.2f;

        allAnswers.Remove(jsonData[C.JSONKeys.selectedAnswer].AsInt);

        // Ha már csak annyi buborék van mint a jó válaszok száma, akkor megnézzük, hogy csak a jó válaszok maradtak-e
        // Ha igen, akkor vége a játéknak, ha nem, akkor újra termeljük a buborékokat
        if (Common.configurationController.answerFeedback == ConfigurationController.AnswerFeedback.Immediately)
        {
            if (allAnswers.Count == goodAnswerPiece)
            {
                bool goodAnswer = evaluation();

                if (goodAnswer)
                { // Ha igen, akkor vége
                    solved = true;
                    taskMustEnd = true;
                }
                else
                { // Ha nem újra termeljük a buborékokat
                    reBorn++;
                }
            }
        }

        // Akkor jó a válasz, ha egy rossz buborékot pukkasztottunk ki
        return !GetAnswerDataByAnswerID(jsonData[C.JSONKeys.selectedAnswer].AsInt).isCorrect ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
    }

    bool evaluation()
    {
        if (allAnswers.Count == goodAnswerPiece)
        {
            waitBetweenQuestion = 1.3f; // A villogás ideje, amíg a jó vagy a rossz válasz villog

            // Akkor megnézzük, hogy a még létező buborékok a jó válaszok közül valóak-e
            bool goodAnswer = true;
            foreach (int item in allAnswers)
                if (!GetAnswerDataByAnswerID(item).isCorrect)
                {
                    goodAnswer = false;
                    break;  // Megszakítjuk a ciklust, felesleges tovább futnia, mert találtunk egy rossz választ
                }

            // Elkészítjük a json-t amit elküldünk a kliensnek
            JSONClass newData;
            newData = new JSONClass();
            newData[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;

            if (goodAnswer)
            { // Ha igen, akkor vége
                newData[C.JSONKeys.gameEventType] = C.JSONValues.goodSolution;
            }
            else
            { // Ha nem újra termeljük a buborékokat
                newData[C.JSONKeys.gameEventType] = C.JSONValues.wrongSolution;
                allAnswers = GetAnswers();
            }

            // Várunk egy tized másodpercet, majd elküldjük a végeredményt is a klienseknek
            // Azért várunk mert előbb a kiértékelés eredményét küldjük el, vagyis, hogy melyik buborékot kell kipukkasztani
            // Majd azt hogy a maradék buborék azok jók vagy nem jók, milyen színnel villogjon és ha pirossal, akkor újra termelődnek a villogás után a buborékok
            Common.configurationController.WaitTime(0.1f, () =>
            {
                // Elküldjük a klienseknek 
                sendMessageAllClient(newData);
            });

            return goodAnswer;
        }

        return false;
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

            if (originalGameEnding == ConfigurationController.AnswerFeedback.Immediately)
            {
                allAnswers.Remove(replayAnswer[C.JSONKeys.selectedAnswer].AsInt);
                evaluation();
            }
        }

        return replayAnswer;
    }
}
