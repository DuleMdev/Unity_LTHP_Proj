using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class LessonMosaicData {

    public enum MultiPlayerGameMode
    {
        Single,                 // SinglePlayer nincsenek csoportok
        MultiSingle,            // MultiPlayer, de továbbra is mindenki külön játszik, az eredmények átlagolva lesznek
        MultiOneAfterAnother,   // Mindenki ugyan azt a feladatot oldja meg, de egyszerre csak egy játékos válaszolhat

        /*
        EveryoneAlone,      // Mindenki egyedül
        ChangeForTask,      // Mindenki egy külön feladatot kap, az aktív játékos megoldja az egész feladatot míg a többiek nézik
        ChangeForSubTask,   // All feladatonként csere - A játékból mindenki csak egy rész feladatot oldhat meg
        EveryoneTogether,   // Mindenki játszik egyszerre - Egy feladat van aki gyorsabb az foghatja meg az elemet és mozgathatja
        */
    }

    public string name;     // az óra mozaik neve
    public MultiPlayerGameMode multiPlayerGameMode; // Az óra mozaikot milyen játékmódban kell lejátszani

    public int fixGames { get; private set; }

    public List<GameData> listOfGames;  // játékok listája az óra mozaikban

    public bool autoContinuation;   // Az óra mozaik után automatikusan jön a következő óra mozaik vagy sem

    public bool neverEnding;        // MenuLessonPlan objektum állítja be - Az óra-mozaikban található játékokat végtelenítve kell játszani

    public LessonMosaicData()
    {
        listOfGames = new List<GameData>();
    }

    public LessonMosaicData(JSONNode jsonNode)
    {
        InitDatas(jsonNode);
    }

    public void Add(GameData gameData) {
        listOfGames.Add(gameData);
    }

    public void InitDatas(JSONNode jsonNode)
    {
        autoContinuation = true;

        name = jsonNode[C.JSONKeys.lessonMosaicName];
        multiPlayerGameMode = (MultiPlayerGameMode)jsonNode[C.JSONKeys.multiPlayer].AsInt; // (MultiPlayerGameMode)System.Enum.Parse(typeof(MultiPlayerGameMode), jsonNode["multiPlayerGameMode"]);

        //multiPlayerGameMode = MultiPlayerGameMode.Single;

        //if (jsonNode.ContainsKey(C.JSONKeys.fixGames)
        fixGames = jsonNode[C.JSONKeys.games].Count;
        if (jsonNode.ContainsKey(C.JSONKeys.fixGames))
            fixGames = jsonNode[C.JSONKeys.fixGames].AsInt;
        
        listOfGames = new List<GameData>();
        for (int i = 0; i < jsonNode[C.JSONKeys.games].Count; i++)
        {
            GameData gameData = new GameData(jsonNode[C.JSONKeys.games][i]);

            if (gameData.screens.Count != 0)
                listOfGames.Add(gameData);
        }
    }
}
