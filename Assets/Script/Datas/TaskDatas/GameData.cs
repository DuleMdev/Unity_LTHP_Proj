using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class GameData {
    public enum GameEngine
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
        Read2,
        Millionaire,        // Milliomos játék
        Texty,              // Matematikai szövegértéses játék

        PDF,
        YouTube,
        Psycho,

        FreePlay,           // A játékokból véletlenszerűen választ
    }

    public GameEngine gameEngine;   // Milyen játékról van szó

    public int ID;                  // Egy szám azonosító amit a szerver adott a játéknak

    public string name;             // Egy név amit a játékot készítő adott a játéknak

    public float gameDifficulty;    // A játékot átlagosan hány százalékra oldják meg  pl. 12.8%
    public float avgPlayTime;       // A játékot átlagosan mennyi idő alatt oldják meg
    public float lastGamePercent;   // Ha már lejátszották az utolsó tananyag indítás óta, akkor it van eredmény

    public bool played;            // Az aktuális tananyag indításnál lejátszották már

    public List<string> labels;     // A játékhoz tartozó cimkék listája
    //public string labels;     // A játékhoz tartozó cimkék listája

    public int gameTheme { get; private set; }
    public int gameEnding { get; private set; }
    public bool playAnimation { get; private set; }
    public bool isLatex { get; private set; }

    public string keyset;

    /*

    */
    // A képernyők a játékban követhetik egymást sorban vagy véletlenszerűen.
    // Itt lehetőség van megadni a fix képernyőket, vagyis, hogy az első x képernyő sorban jön
    // majd a többi véletlenszerűen.
    public int fixScreens { get; private set; }

    public List<TaskAncestor> screens;  // A játék képernyőinek listája

    Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    public int lessonMosaicIndex;   // Hányadik óra-mozaikhoz tartozik
    public int gameIndex;           // Hányadik játék az óra-mozaikban

    public GameData()
    {
        screens = new List<TaskAncestor>();
    }

    public GameData(JSONNode jsonNode)
    {
        InitDatas(jsonNode);
    }

    public void Add(TaskAncestor game)
    {

    }

    public void InitDatas(JSONNode jsonNode)
    {
        Debug.Log("GameEngine : " + jsonNode[C.JSONKeys.engine]);

        gameEngine = (GameEngine)System.Enum.Parse(typeof(GameEngine), jsonNode[C.JSONKeys.engine]);

        // Ha PDF vagy YouTube játékról van szó, akkor egy kicsit trükközni kell
        // Mivel nincs screens tömb, ezért létre kell hozni és a megfelelő tartalmat elhelyezni benne
        if (gameEngine == GameEngine.PDF || gameEngine == GameEngine.YouTube) {
            jsonNode[C.JSONKeys.screens][0][C.JSONKeys.pdf] = jsonNode[C.JSONKeys.pdf];
            jsonNode[C.JSONKeys.screens][0][C.JSONKeys.pdfLinks] = jsonNode[C.JSONKeys.pdfLinks];
            jsonNode[C.JSONKeys.screens][0][C.JSONKeys.link] = jsonNode[C.JSONKeys.link];
            jsonNode[C.JSONKeys.screens][0][C.JSONKeys.subtitle] = jsonNode[C.JSONKeys.subtitle];
        }


        /*
        // Ha PDF vagy YouTube játékról van szó, akkor egy kicsit trükközni kell
        // Mivel nincs screens tömb, ezért létre kell hozni és a megfelelő tartalmat elhelyezni benne
        if (gameEngine == GameEngine.PDF || gameEngine == GameEngine.YouTube)
        {
            if (jsonNode.ContainsKey(C.JSONKeys.pdf))
                jsonNode[C.JSONKeys.screens][0][C.JSONKeys.pdf] = jsonNode[C.JSONKeys.pdf];
            if (jsonNode.ContainsKey(C.JSONKeys.pdfLinks))
                jsonNode[C.JSONKeys.screens][0][C.JSONKeys.pdfLinks] = jsonNode[C.JSONKeys.pdfLinks];
            if (jsonNode.ContainsKey(C.JSONKeys.link))
                jsonNode[C.JSONKeys.screens][0][C.JSONKeys.link] = jsonNode[C.JSONKeys.link];
            if (jsonNode.ContainsKey(C.JSONKeys.subtitle))
                jsonNode[C.JSONKeys.screens][0][C.JSONKeys.subtitle] = jsonNode[C.JSONKeys.subtitle];
        }
        */

        ID = jsonNode[C.JSONKeys.id].AsInt;

        name = jsonNode[C.JSONKeys.gameID].Value;

        gameDifficulty = jsonNode[C.JSONKeys.gameDifficulty].AsFloat;
        avgPlayTime = jsonNode[C.JSONKeys.avgPlayTime].AsFloat;

        played = jsonNode[C.JSONKeys.lastGamePercent].Value != "null";
        lastGamePercent = (played) ? jsonNode[C.JSONKeys.lastGamePercent].AsFloat : 0;

        gameTheme = jsonNode.ContainsKey(C.JSONKeys.gameTheme) ? jsonNode[C.JSONKeys.gameTheme].AsInt : 3;
        gameEnding = jsonNode.ContainsKey(C.JSONKeys.gameEnding) ? jsonNode[C.JSONKeys.gameEnding].AsInt : 1;
        playAnimation = jsonNode.ContainsKey(C.JSONKeys.playAnimation) ? jsonNode[C.JSONKeys.playAnimation].AsBool : true;
        isLatex = jsonNode.ContainsKey(C.JSONKeys.isLaTeX) ? jsonNode[C.JSONKeys.isLaTeX].AsBool : true;
        keyset = jsonNode.ContainsKey(C.JSONKeys.keyset) ? jsonNode[C.JSONKeys.keyset] : "";

        //gameTheme = jsonNode[C.JSONKeys.gameTheme].AsInt;
        //gameEnding = jsonNode[C.JSONKeys.gameEnding].AsInt;

        // Feldolgozzuk a játékhoz kapcsolt cimkéket
        //labels = jsonNode[C.JSONKeys.labels];
        labels = new List<string>();
        for (int i = 0; i < jsonNode[C.JSONKeys.labels].Count; i++) {
            labels.Add(jsonNode[C.JSONKeys.labels][i].Value);
        }

        // Meghatározzuk, hogy hány képernyő legyen fix a többi véletlenszerűen jön
        fixScreens = jsonNode[C.JSONKeys.screens].Count;
        // Ha nincs megadva a json-ban, akkor fix lesz az egész
        if (jsonNode.ContainsKey(C.JSONKeys.fixGames))
            fixScreens = jsonNode[C.JSONKeys.fixGames].AsInt;

        // Feldolgozzuk a játék képernyőit
        screens = new List<TaskAncestor>();

        for (int i = 0; i < jsonNode[C.JSONKeys.screens].Count; i++) {
            TaskAncestor task = null;

            JSONNode screenJSON = jsonNode[C.JSONKeys.screens][i];

            switch (gameEngine)
            {
                case GameEngine.TrueOrFalse:
                    task = new TaskTrueOrFalseData(screenJSON);
                    break;
                case GameEngine.Bubble:
                    task = new TaskBubbleData(screenJSON);
                    break;
                case GameEngine.Sets:
                    task = new TaskSetsData(screenJSON);
                    break;
                case GameEngine.MathMonster:
                    task = new TaskMathMonsterData(screenJSON);
                    break;
                case GameEngine.Affix:
                    task = new TaskAffixData(screenJSON);
                    break;
                case GameEngine.Boom:
                    task = new TaskBoomData(screenJSON);
                    break;
                case GameEngine.Fish:
                    task = new TaskFishData(screenJSON);
                    break;
                case GameEngine.Hangman:
                    task = new TaskHangmanData(screenJSON);
                    break;
                case GameEngine.Read:
                    task = new TaskReadData(screenJSON);
                    break;
                case GameEngine.Read2:
                    task = new TaskRead2Data(screenJSON);
                    break;
                case GameEngine.Millionaire:
                    task = new TaskMillionaireData(screenJSON);
                    break;
                case GameEngine.FreePlay:
                    task = new TaskFreePlayData(screenJSON);
                    break;
                case GameEngine.PDF:
                    task = new TaskPDF(screenJSON);
                    break;
                case GameEngine.YouTube:
                    task = new TaskYouTube(screenJSON);
                    break;
                case GameEngine.Psycho:
                    task = new TaskPsychoData(screenJSON);
                    break;
                case GameEngine.Texty:
                    task = new TaskTextyData(screenJSON);
                    break;
                default:
                    Debug.Log("Unknow game type : " + jsonNode[C.JSONKeys.engine]);
                    break;
            }

            if (task != null && !task.error)
            {
                task.gameData = this;
                screens.Add(task);
//#if UNITY_EDITOR
//                if (task.taskType != TaskAncestor.TaskType.PDF && task.taskType != TaskAncestor.TaskType.YouTube)
//                    screens.Add(task);
//#endif
            }
        }

        // Beolvassuk a képeket a játékhoz
        sprites.Clear();

        foreach (string item in jsonNode[C.JSONKeys.images].Keys)
        {
            try
            {
                sprites.Add(item, Common.MakeSpriteFromByteArray(System.Convert.FromBase64String(jsonNode[C.JSONKeys.images][item].Value)));

                //Texture2D texture = new Texture2D(2, 2);
                //
                //texture.LoadImage(System.Convert.FromBase64String(jsonNode[C.JSONKeys.images][item].Value));
                //sprites.Add(item, Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));

                // Kiszámoljuk hány százaléknál tart a program a képek feldolgozásában
                //picturesCount++;
                //progressPercent = 1.0f / allPictures * picturesCount;
            }
            catch (System.Exception e)
            {
                // Debug.Log("json pictures error : " + e.Message);
                //errorMessage = "Picture error : " + e.Message;
                //error = true;
                //throw;
            }

            //yield return null;
        }



        /*
        for (int i = 0; i < jsonNode["pictures"].Count; i++)
        {
            foreach (string item in jsonNode["pictures"][i].Keys)
            {
                try
                {
                    Texture2D texture = new Texture2D(2, 2);

                    texture.LoadImage(System.Convert.FromBase64String(jsonNode["pictures"][i][item].Value));
                    pictures.Add(item, Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));

                    // Kiszámoljuk hány százaléknál tart a program a képek feldolgozásában
                    //picturesCount++;
                    //progressPercent = 1.0f / allPictures * picturesCount;
                }
                catch (System.Exception e)
                {
                    // Debug.Log("json pictures error : " + e.Message);
                    //errorMessage = "Picture error : " + e.Message;
                    //error = true;
                    //throw;
                }

                //yield return null;
            }
        }
        */




    }

    public Sprite GetSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
            return null;

        return (sprites.ContainsKey(spriteName)) ? sprites[spriteName] : null;
    }

    public string GetLabels() {
        string result = "";

        for (int i = 0; i < labels.Count; i++)
        {
            if (!string.IsNullOrEmpty(result))
                result += ", ";

            result += labels[i];
        }

        return result;
    }

    public JSONNode GetScreensEvaluations()
    {
        // Összeszedjük a válaszokat egy json-ba
        JSONArray jsonScreens = new JSONArray();
        foreach (TaskAncestor screen in screens)
        {
            JSONClass jsonScreen = new JSONClass();
            jsonScreen[C.JSONKeys.screenID] = screen.id.ToString();
            jsonScreen[C.JSONKeys.answers] = screen.GetEvaluations();

            jsonScreens.Add(jsonScreen);
        }

        return jsonScreens;
    }

    public GameData Clone() {
        GameData cloneGameData = (GameData)this.MemberwiseClone();

        cloneGameData.screens = new List<TaskAncestor>();
        foreach (TaskAncestor task in screens)
            cloneGameData.screens.Add(task.Clone());

        return cloneGameData;
    }
}
