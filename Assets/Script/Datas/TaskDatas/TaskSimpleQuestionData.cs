using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;


public class TaskSimpleQuestionData : TaskAncestor {

    public string question;             // A kérdés
    public List<string> goodAnswers;    // A lehetséges jó válaszok
    public List<string> wrongAnswers;   // A lehetséges rossz válaszok
    public bool goodAnswerNeed;         // Ha ez true, akkor a jó válaszokat kell adni, ha false, akkor az összes rossz választ kell megadni
    public int goodAnswerPiece;         // Hány jó választ kell adni

    // A megadott JSON alapján inicializálja a változóit
    public TaskSimpleQuestionData(string jsonText) {
        taskType = TaskType.Bubble;

        InitDatas(jsonText);
    }

    void InitDatas(string jsonText) {
        JSONNode node = JSON.Parse(jsonText);

        question = node["question"];

        goodAnswers = new List<string>();
        for (int i = 0; i < node["goodAnswers"].Count; i++)
            goodAnswers.Add(node["goodAnswers"][i]);

        wrongAnswers = new List<string>();
        for (int i = 0; i < node["wrongAnswers"].Count; i++)
            wrongAnswers.Add(node["wrongAnswers"][i]);

        goodAnswerNeed = node["goodAnswerNeed"].AsBool;
        goodAnswerPiece = node["goodAnswerPiece"].AsInt;
        tries = node["lifeNumber"].AsInt;
    }
}
