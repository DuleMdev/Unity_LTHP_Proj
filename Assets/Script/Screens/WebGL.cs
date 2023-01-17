using UnityEngine;
using System.Collections;
using SimpleJSON;

/// <summary>
/// Ha először jelenik meg a start képernyő, akkor küldünk egy UnityLoaded hívást a böngésző 
/// javascriptjéhez, ekkor az elküldheti a lejátszandó játékot.
/// </summary>

public class WebGL : HHHScreen
{
    //GameObject playButton;

    GameData gameData;

    public TextAsset task;

    //bool firstTime = true;  // Ha először jelenik meg a start képernyő, akkor küldünk egy UnityLoaded hívást a böngésző javascriptjéhez

    string testToken;
    string replayToken;

    bool userInputEnabled = false;

	// Use this for initialization
	void Awake ()
    {
        testToken = "";
        replayToken = "";

        if (URLParameterReader.KeyIsExists(C.JSONKeys.testToken))
            testToken = URLParameterReader.GetParameter(C.JSONKeys.testToken);

        if (URLParameterReader.KeyIsExists(C.JSONKeys.replayToken))
            replayToken = URLParameterReader.GetParameter(C.JSONKeys.replayToken);


#if UNITY_EDITOR
        if (Common.configurationController.testTokenTest)
            testToken = Common.configurationController.testToken;

        if (Common.configurationController.replayTokenTest)
            replayToken = Common.configurationController.replayToken;

        /*
        testToken = "ZShWfxTBleoNur4PJDxI8tyO5uqWG13j";
        testToken = "9kwcz7A1E9B8Uzt7jzX24mA6BLZvM1K2";
        testToken = "iPqgzeIxcUwXTeZrpclm0LYf295TfqiS";
        testToken = "ZLmASTLKKZkadFZHgbYWRtpa8EMf9SZF";
        testToken = "Z88jIKyLZepIZXu1XjcRls3vQLQp2sac";
        testToken = "ra7fnSz1ZE9TZHCe4E2a7KnVwu0ixltp";
        testToken = "WUKtRV20VJACxFBHLQ3bTGj0O8AhDtaK";
        testToken = "hZcH9VXy1EO1PVRWoVdkZbLHw4xdQTo2";

        testToken = "Opl0RqUByIWZ5NhKWCgmitlr8OejngEy"; // Millionaire - Ez a két testháló lehet-e ugyan annak a kockának a testhálója?
        testToken = "tEiz7lIJGUVeP313SeilkX8AbAF2YOMQ"; // Buborékos - 
        //testToken = "yJOtQ7yTJGkm4r7QFAscLvShYRUFaSKx"; // Buborékos - 
        //testToken = "RntbfMuQUBFNAXLhwKTBvsDvvzJM6CGr"; // MathMonster - 


        //testToken = "oPaVEDMaeGkehSg1MpG9UYTyzE7CPy4q";
        testToken = "DOrhxQUniWEY2ReES5Xp70w5AI8gZ2pw";
        testToken = "HG9dBbI08xPzNin86Jmx77oYcW01w2ch";
        testToken = "NV5dslTdU2sCLD6vMEj940tAllB3RSjV";
        testToken = "jl3GIGWyQ6YRpPVDkNQx0jsryo5JJ5fV";
        testToken = "oqCnqOZyuDsOCcYrYDEIAdSITI7iJrpU";
        testToken = "7nrR3PalPNmOGhqQDXSy2rU2u0nCZDlp";
        testToken = "0jJ48XIz6BovnfM7SoPcTeL2y0fng0x7";
        testToken = "U2BpQXbOT2zR5pwGEP5sTde15MsdXZ5E";
        testToken = "uCFIApKhmQ4j5tYaZEoi1bWX2iaO0B9N";

        */



#endif

        //playButton = transform.Find("background/PlayButton").gameObject;
        //playButton.transform.localScale = Vector3.zero;

        // Gombok szkriptjének beállítása
        foreach (Button button in GetComponentsInChildren<Button>())
            button.buttonClick = ButtonClick;

        //playButton.SetActive(false);


    }

    void Start() {

    }

    public override IEnumerator ScreenShowFinishCoroutine()
    {
        /*
        if (firstTime) {
            Application.ExternalCall("UnityLoaded");
            firstTime = false;

#if UNITY_EDITOR
            //SendGameData("{\"id\" : \"714\",\"gameType\" : \"Millionaire\",\"gameID\" : \"4 -BE-3-MT-magyar\",\"labels\" : \"MT; Alg; logika; százas; összeadás; rendezés\",\"screens\" : [{\"question\" : \"Melyik szám hiányzik a sorozatból? \n 12, 24, …, 48, 60\",\"goodAnswers\" : [\"36\"],\"wrongAnswers\" : [\"25\",\"35\",\"37\",\"12\",\"34\"]}], \"pictures\" : []}");
            //SendGameData("{ \"id\" : \"11\",\"engine\" : \"Fish\",\"name\" : \"halTest\",\"labels\" : \"test\",\"gameDifficulty\" : \"50.000\",\"avgPlayTime\" : \"0.000\",\"screens\" : [{\"id\" : \"34\",\"question\" : \"hal kerdes\", \"time\" : \"90.5\", \"questions\" : [ \"hal mondat ?(x|z) y ?(x|z)\", \"masik hal mondat ?(a) b ?(c)\"], \"answers\":[ \"x\", \"z\", \"a\", \"c\", \"asd\", \"dsa\"]}], \"lastGamePercent\" : \"null\"}");
            if (task != null)
                SendGameData(task.text);
#endif
        }
        */

        userInputEnabled = true;

        yield return null;
    }

    /// <summary>
    /// Ezt a metódust a unity WebGL-be ágyazott javascript fogja meghívni.
    /// </summary>
    /// <param name="message">Az üzenet egy json-ba csomagolt játék. (akár több képernyővel)</param>
    //void SendGameData(string message) {
    //    //gameData = new GameData(JSON.Parse(message));
    //    gameData = new GameData(JSON.Parse(message)[C.JSONKeys.answer][C.JSONKeys.gameData]);
    //    gameData.lessonMosaicIndex = 0;
    //    gameData.gameIndex = 0;
    //
    //    LessonPlanData lessonPlanData = new LessonPlanData();
    //    LessonMosaicData lessonMosaicData = new LessonMosaicData();
    //    lessonMosaicData.Add(gameData);
    //    lessonPlanData.lessonMosaicsList.Add(lessonMosaicData);
    //
    //    Common.configurationController.lessonPlanData = lessonPlanData;
    //
    //    // A Play gombot megjelenítjük
    //    //playButton.transform.localScale = Vector3.zero;
    //    //playButton.SetActive(true);
    //    //iTween.ScaleTo(playButton, iTween.Hash("islocal", true, "scale", Vector3.one, "time", 1, "easeType", iTween.EaseType.easeOutElastic));
    //    //Common.audioController.SFXPlay("boing");
    //}

    //void ApplicationQuit() {
    //    Application.Quit();
    //}

    // Ha rákattintottak a buborékra, akkor meghívódik ez az eljárás a buborékon levő Button szkript által
    void ButtonClick(Button button)
    {
        // Ha még nem jelent meg teljsen a képernyő, akkor a felhasználói input kezelés nem engedélyezett
        if (!userInputEnabled)
            return;

        switch (button.buttonType)
        {
            //case Button.ButtonType.Exit:
            //    Application.ExternalCall("UnityPlayerExit"); // A beágyazott weboldalon kell lennie egy UnityPlayerExit függvénynek ami meghívásra kerül ez által
            //    Application.Quit();
            //    break;

            case Button.ButtonType.Play:

                userInputEnabled = false;


#if UNITY_EDITOR
                Common.configurationController.WebGLTestLink = "https://cm.classyedu.eu/app/";

#endif


                Common.CallBack_In_Bool_JSONNode responseProcessor = (bool success, JSONNode response) =>
                {
                    if (success)
                    {
                        // A json-ban található játékot feldolgozzuk
                        gameData = new GameData(response[C.JSONKeys.answer]);

                        GameMenu.instance.SetPreviousButton(false);
                        GameMenu.instance.SetNextButton(false);

                        Common.configurationController.WaitCoroutine(ServerPlay.instance.PlayGame(gameData), () =>
                        {
                            if (!string.IsNullOrWhiteSpace(testToken))
                            {
                                Common.configurationController.WaitCoroutine(WaitAClickCoroutine(), () =>
                                    {
                                        Common.screenController.ChangeScreen("WebGLScreen");
                                    }
                                );
                            }
                            else
                                Common.screenController.ChangeScreen("WebGLScreen");
                        });
                    }
                    else
                    {
                        userInputEnabled = true;
                    }
                };


                if (!string.IsNullOrWhiteSpace(testToken)) //  Common.configurationController.link == ConfigurationController.Link.Server2020Link)
                    ClassYServerCommunication.instance.getGameForTest(testToken, responseProcessor);
                else
                    ClassYServerCommunication.instance.getReplayData(replayToken, responseProcessor);


                /*
                // Lekérdezzük a szervertől a lejátszandó tananyagot
                ClassYServerCommunication.instance.getGameForTest(
                    testToken,
                    (bool success, JSONNode response) =>
                    {
                        if (success)
                        {
                            // A json-ban található játékot feldolgozzuk
                            gameData = new GameData(response[C.JSONKeys.answer]);

                            GameMenu.instance.SetPreviousButton(false);
                            GameMenu.instance.SetNextButton(false);

                            Common.configurationController.WaitCoroutine(ServerPlay.instance.PlayGame(gameData), () =>
                            {
                                // Itt várakozni kell egy kattintásra
                                // ...
                                //StartCoroutine(WaitAClickCoroutine());

                                Common.configurationController.WaitCoroutine(WaitAClickCoroutine(), () =>
                                {
                                    Common.screenController.ChangeScreen("WebGLScreen");
                                });

                                //Common.screenController.ChangeScreen("WebGLScreen");
                            });

                            //gameData.lessonMosaicIndex = 0;
                            //gameData.gameIndex = 0;
                            //
                            //LessonPlanData lessonPlanData = new LessonPlanData();
                            //LessonMosaicData lessonMosaicData = new LessonMosaicData();
                            //lessonMosaicData.Add(gameData);
                            //lessonPlanData.lessonMosaicsList.Add(lessonMosaicData);
                            //
                            //Common.configurationController.lessonPlanData = lessonPlanData;
                            //
                            //// lejátszuk a játékot
                            //Common.taskController.PlayGameInServer(gameData, 0, () =>
                            //{
                            //    Common.screenController.ChangeScreen("WebGLScreen");
                            //});
                        }
                    }
                );
                */

                break;
        }
    }

    IEnumerator WaitAClickCoroutine()
    {
        // Várunk amíg felengedik az egér gombját
        while (Input.GetMouseButton(0))
        {
            yield return null;
        }

        // Várunk amíg nem nyomják le az egér gombját
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }
    }
}
