using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/// <summary>
/// Tananyag adatait tartalmazza
/// </summary>
public class CurriculumData
{
    public string id { get; private set; }
    public string name { get; private set; }

    public string newCurriculumIsolationTimeForContinue { get; private set; }
    public string lastCurriculumIsolationTimeForContinue { get; private set; }

    public List<GameData> plannedGames;     // Minden diák számára kötelezően lejátszandó
    public List<GameData> automatedGames;   // A tervezett játékok közé szúrható egyébb játékok

    public CurriculumData(JSONNode jsonNode)
    {
        InitDatas(jsonNode);
    }

    public void InitDatas(JSONNode jsonNode)
    {
        id = jsonNode[C.JSONKeys.id];
        name = jsonNode[C.JSONKeys.name];

        newCurriculumIsolationTimeForContinue = jsonNode[C.JSONKeys.newCurriculumIsolationTimeForContinue];
        lastCurriculumIsolationTimeForContinue = jsonNode[C.JSONKeys.lastCurriculumIsolationTimeForContinue];

        // Beolvassuk a kötelező játékokat
        plannedGames = new List<GameData>();
        for (int i = 0; i < jsonNode[C.JSONKeys.plannedGames].Count; i++)
        {
            GameData gameData = new GameData(jsonNode[C.JSONKeys.plannedGames][i]);

            if (gameData.screens.Count != 0)
                plannedGames.Add(gameData);
        }

        // Beolvassuk az opcionális játékokat
        automatedGames = new List<GameData>();
        for (int i = 0; i < jsonNode[C.JSONKeys.automatedGames].Count; i++)
        {
            GameData gameData = new GameData(jsonNode[C.JSONKeys.automatedGames][i]);

            if (gameData.screens.Count != 0)
                automatedGames.Add(gameData);
        }
    }
}
