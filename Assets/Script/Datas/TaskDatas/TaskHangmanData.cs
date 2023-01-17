using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;

/*

    {
        "question" : "Találd ki a szöveget",
        "puzzle" : "A rejtvény szövege",
        "keyset" : "hu",
    }

*/
public class TaskHangmanData : TaskAncestor
{
    public string taskDescription;  // A kérpernyőn megjelenő kérdés.
    public string puzzle;           // rejtvény amit meg kell fejteni 
    public string keyset;           // A betű tábla nyelve

    // A feladatot hány százalékosan sikerült megoldani
    override public float resultPercent { get { return ((float)goodAnswersCount / (wrongAnswersCount + GetQuestionNumber())) * 100; } }

    // A megadott JSON alapján inicializálja a változóit
    public TaskHangmanData(JSONNode jsonNode)
    {
        taskType = TaskType.Hangman;
        InitDatas(jsonNode);
    }

    override protected void InitTaskDatas(JSONNode jsonNode)
    {
        error = false;

        taskDescription = jsonNode[C.JSONKeys.question];
        puzzle = jsonNode[C.JSONKeys.puzzle].Value.ToLower();
        keyset = jsonNode[C.JSONKeys.keyset];

        if (keyset == null)
            // Ha nincs beállítva a keyset, akkor a rendszer nyelvét használjuk
            keyset = Common.languageController.actLanguageID;
        else {
            keyset = keyset.ToLower();
            if (keyset == "hu") keyset = "Hungarian";
            if (keyset == "en") keyset = "English";
            if (keyset == "ro") keyset = "Romanian";
            if (keyset == "sk") keyset = "Slovak";
            if (keyset == "sl") keyset = "Slovenian";
        }

        // Ha az alkalmazás a Storie, akkor fixen az angol nyelvet használjuk
        if (Common.configurationController.appID == ConfigurationController.AppID.Storie)
            keyset = "English";

        animSpeed2 = 0.05f;
    }

    /// <summary>
    /// A játék task-ok feladat specifikust indítása.
    /// </summary>
    override protected void StartTask()
    {
        // Megszámoljuk, hogy összesen hány karakterből áll a kérdés
        int letterNumber = 0;
        for (int i = 0; i < puzzle.Length; i++)
            // Ha az aktuális betű korábban még nem volt és a betű választható az abc tábláról akkor ez egy különböző betű
            if (puzzle[i] != ' ')
                letterNumber++;

        waitBeforeStart =
            waitScreenChange + // Képernyőváltás sebessége
            ((!string.IsNullOrEmpty(taskDescription)) ? animSpeed1 : 0) + // Ha van képernyő kérdés
            letterNumber * animSpeed2 + //animSpeed1 + // Megjelennek a betűk
            //HangmanMonster.MONSTER_SPEED + // Bejön a szörny
            animSpeed1; // Bejön az abc tábla

        //waitAtGameEnd = 10; // A játék végén 10 másodpercet várjon
    }

    override protected int GetQuestionNumber()
    {
        // Annyi kérdés van, ahány különböző betű van a kérdésben, ami választható az abc tábláról
        JSONNode node = JSON.Parse(Common.configurationController.hangmanKeySet.text);
        node = node[keyset]; // Common.languageController.actLanguageID];

        string allLetters = ""; // Az összes választható betű
        for (int i = 0; i < node.Count; i++)
            allLetters += node[i];

        int differentLetterNumber = 0;
        for (int i = 0; i < puzzle.Length; i++)
            // Ha az aktuális betű korábban még nem volt és a betű választható az abc tábláról akkor ez egy különböző betű
            if (!puzzle.Substring(0, i).Contains(puzzle[i].ToString()) && allLetters.Contains(puzzle[i].ToString()))
                differentLetterNumber++;

        return differentLetterNumber;
    }

    override protected string AnswerEvaluation(JSONNode jsonData)
    {
        return (puzzle.Contains(jsonData[C.JSONKeys.selectedAnswer].Value)) ? C.JSONValues.evaluateIsTrue : C.JSONValues.evaluateIsFalse;
    }
}
