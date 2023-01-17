using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameEngine_Question {

    /*
    string Question;
    List<string> goodAnswers;
    List<string> wrongAnswers;
    bool goodAnswerNeed;    // Ha ez true, akkor a jó válaszokat kell adni, ha false, akkor az összes rossz választ kell megadni
    int goodAnswerPiece;    // Hány jó választ kell adni
    int lifeNumber;         // Hány élete van a játékosnak
    */

    public TaskSimpleQuestionData taskSimpleQuestionData;

    // Az adott válaszokat tartalmazza. A kulcs a választ tartalmazza (vagy egy lehetőséget), az érték a válasz, hogy jónak tartjuk-e (true) vagy hamisnak (false)
    // Az AddGoodAnswer a válaszhoz igaz értéket állit be, az AddWrongAnswer pedig hamisat
    Dictionary<string, bool> answers = new Dictionary<string, bool>();  

    int wrongAnswerCounter; // Hány rossz választ adott a játékos

    public bool isDead { get { return (wrongAnswerCounter >= taskSimpleQuestionData.tries); } } // A játékosnak elfogytak a próbálkozásai vagy nem

    //public int GetAnswerNumber { get { return questionData.goodAnswers.Count + questionData.wrongAnswers.Count; }  } // Vissza adja az összes lehetséges válasz számát

    // Az objektum constructora
    public GameEngine_Question(TaskSimpleQuestionData taskSimpleQuestionData) {
        this.taskSimpleQuestionData = taskSimpleQuestionData;
        Reset();
    }

    public void Reset() {
        wrongAnswerCounter = 0;
        answers.Clear();
    }

    /*
    // A játékosnak elfogytak a próbálkozásai vagy nem
    public bool IsDead() {
        return (wrongAnswerCounter >= lifeNumber);
    }
    */
    
    // Rögzíti a választ és megmondja, hogy helyes-e
    public bool AddGoodAnswer(string answer) {

        bool answerIsGood = AnswerIsGood(answer);

        answers.Add(answer, true);

        if (!answerIsGood) 
            wrongAnswerCounter++;

        return answerIsGood;
    }

    // Előfordulhat, hogy a rossz válaszokat kell eltávolítani
    // Ebben az esetben akkor ad igaz értéket vissza ha a választ valóban rossz volt
    public bool AddWrongAnswer(string answer) {
        bool answerIsGood = AnswerIsGood(answer);

        answers.Add(answer, false);

        if (answerIsGood)
            wrongAnswerCounter++;

        return !answerIsGood;
    }

    // Megadja, hogy a megfejtés helye volt-e
    public bool Check() {

        // Megnézzük, hogy a jó válaszokat kellett-e megadni, vagy a rosszakat
        //if (nee)


        return false;
    }

    // A lehetséges válaszokat megkeveri és visszaadja egy tömbben
    public List<string> GetAnswers() {
        // Összegyűjtjük a lehetséges válaszokat
        List<string> list = new List<string>(taskSimpleQuestionData.goodAnswers);
        list.AddRange(taskSimpleQuestionData.wrongAnswers);

        // Megkeverjük őket
        list.Shuffle();

        return list;
    }

    // Megvizsgáljuk, hogy a válasz a jó válaszok között van-e
    bool AnswerIsGood(string answer) {
        foreach (string goodAnswer in taskSimpleQuestionData.goodAnswers)
            if (answer == goodAnswer) return true;

        return false;
    }

}
