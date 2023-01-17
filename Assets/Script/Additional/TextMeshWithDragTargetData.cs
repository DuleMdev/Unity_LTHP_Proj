using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Text.RegularExpressions;
using static RegexSplitter;

// Egy kérdés felbontásának adatait tartalmazza

public class TextMeshWithDragTargetData {

    public string question;             // A kérdés

    public List<string> listOfQuestionParts;    // A kérdés feldarabolva szöveg és kérdés részekre
    public List<List<string>> listOfAnswerGroups;   // A kérdés részekhez tartozó válasz lehetőségek

    public string questionID;
    public bool is_swappable;   // A válaszok felcserélhetőek

    /// <summary>
    /// Egy lehetséges bemenet : Peti szereti az ?(alma|körte) befőttet.
    /// </summary>
    /// <param name="question"></param>
    public TextMeshWithDragTargetData(JSONNode question)
    {
        if (Common.configurationController.isServer2020)
            TextMeshWithDragTargetDataServer2020(question);
        else
            TextMeshWithDragTargetDataServerOld(question);

        /*
        this.question = question;

        listOfQuestionParts = new List<string>();
        listOfAnswerGroups = new List<List<string>>();

        // Felbontjuk a szöveget részekre és létrehozzuk a részeket
        // Az egyik rész az ami nem tartalmaz kérdőjelet
        // A másik rész ami igen és így néz ki ?(xxx) Az xxx helyén lehet bármi csak bezárójel nem.
        foreach (Match m in Regex.Matches(question, @"([\s]+|[^?\s]+|\?\(([^)]*)\))"))
        {
            if (!string.IsNullOrEmpty(m.Value.Trim()))
                listOfQuestionParts.Add(m.Value);

            // Ha a találat első karaktere kérdőjel
            if (m.Value[0] == '?')
            {   // Beolvassuk a kérdőjel helyén megadható szavakat
                List<string> answers = new List<string>();

                // Az első regex-el leszedem a ?(xxx) szövegből az elől található ?( jeleket és a végén található ) jelet
                // A második regex-el felbontom a zárójelen belül található részeket | jelre és nem | jelre
                foreach (Match m2 in Regex.Matches(Regex.Match(m.Value, @"\?\((.*)\)").Groups[1].Value, @"[^|]+|\|"))
                {
                    if (m2.Value != "|") // Ha nem elválasztó jel, akkor a megtalált szöveget hozzáadom a jó válaszok tömbjéhez
                        answers.Add(m2.Value);
                }

                listOfAnswerGroups.Add(answers);
            }
        }
        */
    }

    public void TextMeshWithDragTargetDataServerOld(JSONNode question)
    {
        this.question = question;

        listOfQuestionParts = new List<string>();
        listOfAnswerGroups = new List<List<string>>();

        // Felbontjuk a szöveget részekre és létrehozzuk a részeket
        // Az egyik rész az ami nem tartalmaz kérdőjelet
        // A másik rész ami igen és így néz ki ?(xxx) Az xxx helyén lehet bármi csak bezárójel nem.
        foreach (Match m in Regex.Matches(question, @"([\s]+|[^?\s]+|\?\(([^)]*)\))"))
        {
            if (!string.IsNullOrEmpty(m.Value.Trim()))
                listOfQuestionParts.Add(m.Value);

            // Ha a találat első karaktere kérdőjel
            if (m.Value[0] == '?')
            {   // Beolvassuk a kérdőjel helyén megadható szavakat
                List<string> answers = new List<string>();

                // Az első regex-el leszedem a ?(xxx) szövegből az elől található ?( jeleket és a végén található ) jelet
                // A második regex-el felbontom a zárójelen belül található részeket | jelre és nem | jelre
                foreach (Match m2 in Regex.Matches(Regex.Match(m.Value, @"\?\((.*)\)").Groups[1].Value, @"[^|]+|\|"))
                {
                    if (m2.Value != "|") // Ha nem elválasztó jel, akkor a megtalált szöveget hozzáadom a jó válaszok tömbjéhez
                        answers.Add(m2.Value);
                }

                listOfAnswerGroups.Add(answers);
            }
        }
    }

    public void TextMeshWithDragTargetDataServer2020(JSONNode json)
    {
        question = json.ContainsKey(C.JSONKeys.sentence) ? json[C.JSONKeys.sentence].Value : "";
        questionID = json.ContainsKey(C.JSONKeys.fishSentenceID) ? json[C.JSONKeys.fishSentenceID] : json[C.JSONKeys.mathMonsterSentenceID];
        is_swappable = json.ContainsKey(C.JSONKeys.is_swappable) ? json[C.JSONKeys.is_swappable].Value != "0" : false;

        listOfQuestionParts = new List<string>();
        listOfAnswerGroups = new List<List<string>>();

        // Felbontjuk a szöveget részekre és létrehozzuk a részeket
        // Az egyik rész az ami nem tartalmaz kérdőjelet
        // A másik rész ami igen és így néz ki ?(xxx) Az xxx helyén lehet bármi csak bezárójel nem.
        string regString = @"([\s]+|[^?\s]+|\?\(([^)]*)\))";

        string blockStart = "[#block]";
        string blockEnd = "[#/block]";

        blockStart = Common.ConvertStringToRegEx(blockStart);
        blockEnd = Common.ConvertStringToRegEx(blockEnd);

        RegexSplitterResult regexSplitterResult = RegexSplitter.Split(question, 
            blockStart + @"(.*?)" + blockEnd, // Blokk
            @"\?\(([^)]*)\)",  // kérdés
            @"[^\s]+", // Valamilyen egybeföggő karakterek
            @"\s*"); // White space
        Debug.Log(regexSplitterResult.ToString());

        for (int i = 0; i < regexSplitterResult.Count; i++)
        {
            switch (regexSplitterResult[i].index)
            {
                case 1: // Blokk szerkezet
                    listOfQuestionParts.Add(regexSplitterResult[i].match.Groups[1].Value);
                    break;
                case 2: // Kérdés
                    listOfQuestionParts.Add("??");

                    List<string> answers = new List<string>();

                    // Az első regex-el leszedem a ?(xxx) szövegből az elől található ?( jeleket és a végén található ) jelet
                    // A második regex-el felbontom a zárójelen belül található részeket | jelre és nem | jelre
                    foreach (Match m2 in Regex.Matches(Regex.Match(regexSplitterResult[i].value, @"\?\((.*)\)").Groups[1].Value, @"[^|]+|\|"))
                    {
                        if (m2.Value != "|") // Ha nem elválasztó jel, akkor a megtalált szöveget hozzáadom a jó válaszok tömbjéhez
                            answers.Add(m2.Value);
                    }

                    listOfAnswerGroups.Add(answers);
                    break;
                case 3: // Valamilyen egybefüggő karakterek szó
                    listOfQuestionParts.Add(regexSplitterResult[i].value);
                    break;
            }
        }

        /*
        foreach (Match m in Regex.Matches(question, regString))
        {
            if (!string.IsNullOrEmpty(m.Value.Trim()))
                listOfQuestionParts.Add(m.Value);

            // Ha a találat első karaktere kérdőjel
            if (m.Value[0] == '?')
            {   // Beolvassuk a kérdőjel helyén megadható szavakat
                List<string> answers = new List<string>();

                // Az első regex-el leszedem a ?(xxx) szövegből az elől található ?( jeleket és a végén található ) jelet
                // A második regex-el felbontom a zárójelen belül található részeket | jelre és nem | jelre
                foreach (Match m2 in Regex.Matches(Regex.Match(m.Value, @"\?\((.*)\)").Groups[1].Value, @"[^|]+|\|"))
                {
                    if (m2.Value != "|") // Ha nem elválasztó jel, akkor a megtalált szöveget hozzáadom a jó válaszok tömbjéhez
                        answers.Add(m2.Value);
                }

                listOfAnswerGroups.Add(answers);
            }
        }
        */

        // Ha a válaszok felcserélhetőek, akkor minden válaszhelynek beállítom az összes választ
        if (is_swappable)
        {
            // Összegyűjtjük a megadható válaszokat
            List<string> answers = new List<string>();

            foreach (List<string> group in listOfAnswerGroups)
                answers.AddRange(group);

            // Majd az összes csoportba beállítjuk az összegyűjtött válaszokat
            for (int i = 0; i < listOfAnswerGroups.Count; i++)
                listOfAnswerGroups[i] = answers;
        }
    }

    /*
    public void TextMeshWithDragTargetDataServer2020Old(JSONNode json)
    {
        question = json.ContainsKey(C.JSONKeys.sentence) ? json[C.JSONKeys.sentence].Value : "";
        questionID = json.ContainsKey(C.JSONKeys.fishSentenceID) ? json[C.JSONKeys.fishSentenceID] : json[C.JSONKeys.mathMonsterSentenceID];
        is_swappable = json.ContainsKey(C.JSONKeys.is_swappable) ? json[C.JSONKeys.is_swappable].Value != "0" : false;

        listOfQuestionParts = new List<string>();
        listOfAnswerGroups = new List<List<string>>();

        // Felbontjuk a szöveget részekre és létrehozzuk a részeket
        // Az egyik rész az ami nem tartalmaz kérdőjelet
        // A másik rész ami igen és így néz ki ?(xxx) Az xxx helyén lehet bármi csak bezárójel nem.
        string regString = @"([\s]+|[^?\s]+|\?\(([^)]*)\))";
        foreach (Match m in Regex.Matches(question, regString))
        {
            if (!string.IsNullOrEmpty(m.Value.Trim()))
                listOfQuestionParts.Add(m.Value);

            // Ha a találat első karaktere kérdőjel
            if (m.Value[0] == '?')
            {   // Beolvassuk a kérdőjel helyén megadható szavakat
                List<string> answers = new List<string>();

                // Az első regex-el leszedem a ?(xxx) szövegből az elől található ?( jeleket és a végén található ) jelet
                // A második regex-el felbontom a zárójelen belül található részeket | jelre és nem | jelre
                foreach (Match m2 in Regex.Matches(Regex.Match(m.Value, @"\?\((.*)\)").Groups[1].Value, @"[^|]+|\|"))
                {
                    if (m2.Value != "|") // Ha nem elválasztó jel, akkor a megtalált szöveget hozzáadom a jó válaszok tömbjéhez
                        answers.Add(m2.Value);
                }

                listOfAnswerGroups.Add(answers);
            }
        }

        // Ha a válaszok felcserélhetőek, akkor minden válaszhelynek beállítom az összes választ
        if (is_swappable)
        {
            // Összegyűjtjük a megadható válaszokat
            List<string> answers = new List<string>();

            foreach (List<string> group in listOfAnswerGroups)
                answers.AddRange(group);

            // Majd az összes csoportba beállítjuk az összegyűjtött válaszokat
            for (int i = 0; i < listOfAnswerGroups.Count; i++)
                listOfAnswerGroups[i] = answers;
        }
    }
    */
}
