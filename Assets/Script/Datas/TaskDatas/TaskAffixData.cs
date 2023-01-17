using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class TaskAffixData : TaskAncestor {

    public class QuestionDataOld
    {
        public string question;             // A kérdés
        public List<string> answers;        // A kérdésre a válasz(ok)

        public QuestionDataOld(JSONNode node)
        {
            InitData(node);
        }

        public void InitData(JSONNode node)
        {
            question = node[C.JSONKeys.question];

            answers = new List<string>();
            for (int i = 0; i < node[C.JSONKeys.solutions].Count; i++)
                answers.Add(node[C.JSONKeys.solutions][i]);
        }
    }

    public class TextWithID
    {
        public string id;
        public string text;
    }

    public class QuestionData
    {
        public string id;
        public string question;             // A kérdés
        public List<TextWithID> answers;        // A kérdésre a válasz(ok)

        public QuestionData(JSONNode node)
        {
            InitData(node);
        }

        public void InitData(JSONNode node)
        {
            question = node[C.JSONKeys.question];
            id = node[C.JSONKeys.treeItemID];

            answers = new List<TextWithID>();
            for (int i = 0; i < node[C.JSONKeys.solutions].Count; i++)
                answers.Add(
                    new TextWithID()
                    {
                        id = node[C.JSONKeys.solutions][i][C.JSONKeys.solutionID],
                        text = node[C.JSONKeys.solutions][i][C.JSONKeys.solution],
                    }
                    );
        }

        public List<string> GetAnswersText()
        {
            List<string> answer = new List<string>();

            foreach (TextWithID item in answers)
                answer.Add(item.text);

            return answer;
        }

        static public int GetIndexFromID(List<QuestionData> list, string id)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].id == id)
                    return i;
            }

            return -1;
        }
    }

    const int maxQuestionNumber = 4;        // Maximum 4 kérdés van
    const int maxAnswerNumber = 7;          // Maximum 7 válasz van

    public string screenQuestion;           // A kérpernyőn megjelenő kérdés.

    public List<string> allAnswers;         // Az összes lehetséges válasz jók és rosszak együtt

    // Régi szerverhez tartozó adatok
    public List<QuestionDataOld> questionsOld;    // Kérdések
    public List<string> distractorsOld;       // A lehetséges válaszok

    // Server2020-hoz tartozó adatok
    public List<QuestionData> questions;    // Kérdések
    public List<TextWithID> distractors;       // A lehetséges válaszok

    // Extra információ
    public int[] questionOrder;
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

    public TaskAffixData(JSONNode jsonNode)
    {
        taskType = TaskType.Affix;
        differentAnswerFeedbackEnabled = true;
        InitDatas(jsonNode);
    }

    override protected void InitTaskDatas(JSONNode jsonNode)
    {
        error = false;

        id = jsonNode[C.JSONKeys.questionID];

        screenQuestion = jsonNode[C.JSONKeys.question]; // screenQuestion];

        if (Common.configurationController.isServer2020)
        {
            questions = new List<QuestionData>();
            for (int i = 0; i < jsonNode[C.JSONKeys.questions].Count; i++)
                questions.Add(new QuestionData(jsonNode[C.JSONKeys.questions][i]));

            distractors = new List<TextWithID>();
            for (int i = 0; i < jsonNode[C.JSONKeys.distractors].Count; i++)
                distractors.Add(
                    new TextWithID()
                    {
                        id = jsonNode[C.JSONKeys.distractors][i][C.JSONKeys.distractorID],
                        text = jsonNode[C.JSONKeys.distractors][i][C.JSONKeys.distractor],
                    }
                    );
        }
        else
        {
            questionsOld = new List<QuestionDataOld>();
            for (int i = 0; i < jsonNode[C.JSONKeys.questions].Count; i++)
                questionsOld.Add(new QuestionDataOld(jsonNode[C.JSONKeys.questions][i]));

            distractorsOld = new List<string>();
            for (int i = 0; i < jsonNode[C.JSONKeys.distractors].Count; i++)
                distractorsOld.Add(jsonNode[C.JSONKeys.distractors][i]);
        }

        waitBetweenQuestion = 0.1f;



        //GetExtraInfo();
    }

    // Vissza adjuk a lehetséges válaszokat
    public List<string> GetAnswers()
    {
        allAnswers = new List<string>();

        if (Common.configurationController.isServer2020)
        {
            // Az új szerveren a szövegek azonosítóját gyűjtjük össze a listába
            // Összegyűjtjük az összes lehetséges válasz azonosítóját és egy 'a' betűt (Answer) teszünk az elejére, hogy meg tudjuk különböztetni a distractor-októl
            for (int i = 0; i < questions.Count; i++)
                for (int j = 0; j < questions[i].answers.Count; j++)
                    allAnswers.Add("a" + questions[i].answers[j].id);

            // Csak maximum hét válasz lehetőség lehet
            // Ha a válaszok száma nagyobb mint maxAnswerNumber, akkor törlünk annyit, hogy annyi legyen
            while (allAnswers.Count > maxAnswerNumber)
                allAnswers.RemoveAt(maxAnswerNumber); // Random.Range(0, list.Count));

            // Ha a válaszok száma kisebb mint maxAnswerNumber, akkor helytelen válaszokkal bővítjük a válasz lehetőségeket az elejére egy d betűt (Distractor) teszünk
            int answerIndex = 0;
            //int[] answerOrder = Common.GetRandomNumbers(answers.Count);
            while (allAnswers.Count < maxAnswerNumber && answerIndex < distractors.Count)
            {
                allAnswers.Add("d" + distractors[answerIndex].id);
                //list.Add(answers[answerOrder[answerIndex]]);
                answerIndex++;
            }
        }
        else
        {
            // A régi szerveren konkrétan a szövegeket gyűjtjük össze
            // Összegyűjtjük az összes lehetséges válaszokat
            for (int i = 0; i < questionsOld.Count; i++)
                allAnswers.AddRange(questionsOld[i].answers);

            // Csak maximum hét válasz lehetőség lehet
            // Ha a válaszok száma nagyobb mint maxAnswerNumber, akkor törlünk annyit, hogy annyi legyen
            while (allAnswers.Count > maxAnswerNumber)
                allAnswers.RemoveAt(maxAnswerNumber); // Random.Range(0, list.Count));

            // Ha a válaszok száma kisebb mint maxAnswerNumber, akkor helytelen válaszokkal bővítjük a válasz lehetőségeket
            int answerIndex = 0;
            //int[] answerOrder = Common.GetRandomNumbers(answers.Count);
            while (allAnswers.Count < maxAnswerNumber && answerIndex < distractorsOld.Count)
            {
                allAnswers.Add(distractorsOld[answerIndex]);
                //list.Add(answers[answerOrder[answerIndex]]);
                answerIndex++;
            }
        }

        return allAnswers;
    }

    override public JSONNode GetExtraInfo()
    {
        questionOrder = Common.GetRandomNumbers(maxQuestionNumber);
        answerOrder = Common.GetRandomNumbers(maxAnswerNumber);

        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.questionOrder] = Common.ArrayToJSON(questionOrder);
        jsonClass[C.JSONKeys.answerOrder] = Common.ArrayToJSON(answerOrder);

        return jsonClass;
    }

    override public void AddExtraInfo(JSONNode jsonNode)
    {
        questionOrder = Common.JSONToArray(jsonNode[C.JSONKeys.questionOrder]);
        answerOrder = Common.JSONToArray(jsonNode[C.JSONKeys.answerOrder]);
    }

    override protected int GetQuestionNumber()
    {


        return Common.configurationController.isServer2020 ? questions.Count : questionsOld.Count;
    }

    /// <summary>
    /// Vissza adja a szövegét a megadott azonosítónak.
    /// A string első karaktere mutatja, hogy az answer-ek között kell keresni az azonosítót vagy a distractor-ok között
    /// a = answer
    /// d = distractor
    /// pl. a458, d1478
    /// </summary>
    /// <param name="ID"></param>
    /// <returns></returns>
    public string GetTextFromID(string ID)
    {
        char firstChar = ID[0];
        ID = ID.Substring(1);

        if (firstChar == 'd')
        {
            // Először megnézzük a rossz válaszok között
            for (int i = 0; i < distractors.Count; i++)
                if (distractors[i].id == ID)
                    return distractors[i].text;
        }

        if (firstChar == 'a')
        {
            // Ha nincs meg a rossz válaszok között, akkor a kérdésekre adható jó válaszok között keressük
            for (int i = 0; i < questions.Count; i++)
                for (int j = 0; j < questions[i].answers.Count; j++)
                    if (questions[i].answers[j].id == ID)
                        return questions[i].answers[j].text;
        }

        return null;
    }

    override protected string AnswerEvaluation(JSONNode jsonData)
    {
        int questionIndex = jsonData[C.JSONKeys.selectedQuestion].AsInt;
        int answerIndex = jsonData[C.JSONKeys.selectedAnswer].AsInt;

        if (allAnswers == null)
            GetAnswers();

        if (Common.configurationController.isServer2020)
            return (questions[questionIndex].GetAnswersText().Contains(GetTextFromID(allAnswers[answerIndex]))) ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
        else
            return (questionsOld[questionIndex].answers.Contains(allAnswers[answerIndex])) ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
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
                    evaluations.answers[i][C.JSONKeys.questionID] = questions[questionIndex].id;

                    int answerIndex = evaluations.answers[i][C.JSONKeys.answer].AsInt;
                    string answer = allAnswers[answerIndex];
                    char firstChar = answer[0];
                    answer = answer.Substring(1);

                    if (firstChar == 'a')
                    {
                        evaluations.answers[i][C.JSONKeys.answer] = answer;
                        evaluations.answers[i][C.JSONKeys.subQuestionID] = "-1";
                    }
                    else
                    {
                        evaluations.answers[i][C.JSONKeys.answer] = "-1";
                        evaluations.answers[i][C.JSONKeys.subQuestionID] = answer;
                    }
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
            replayAnswer[C.JSONKeys.selectedQuestion].AsInt = QuestionData.GetIndexFromID(questions, evaluations.answers[replayIndex][C.JSONKeys.treeItemID].Value);
            replayAnswer[C.JSONKeys.selectedSubQuestion].AsInt = 0;
            replayAnswer[C.JSONKeys.selectedAnswer].AsInt = GetAnswerIndexFromID(evaluations.answers[replayIndex][C.JSONKeys.solutionID]);
            replayAnswer[C.JSONKeys.evaluateAnswer] = evaluations.answers[replayIndex][C.JSONKeys.isGood].AsBool ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;

            //replayAnswer = evaluations.answers[replayIndex];
            replayIndex++;

            Debug.LogWarning("replayAnswer : \n" + replayAnswer.ToString());
        }

        return replayAnswer;
    }

    int GetAnswerIndexFromID(string id)
    {
        int index = 0;
        // Az új szerveren a szövegek azonosítóját gyűjtjük össze a listába
        // Összegyűjtjük az összes lehetséges válasz azonosítóját és egy 'a' betűt (Answer) teszünk az elejére, hogy meg tudjuk különböztetni a distractor-októl
        for (int i = 0; i < questions.Count; i++)
            for (int j = 0; j < questions[i].answers.Count; j++)
            {
                if (questions[i].answers[j].id == id)
                    return index;

                index++;
            }

        for (int i = 0; i < distractors.Count; i++)
        {
            if (distractors[i].id == id)
                return index;

            index++;
        }

        return -1;
    }
}
