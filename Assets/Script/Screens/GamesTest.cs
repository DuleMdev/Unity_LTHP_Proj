using UnityEngine;
using System.Collections;

using UnityEngine.Networking;
using SimpleJSON;

/*



Már nem használt.



*/

public class GamesTest : HHHScreen {

    public TextAsset TrueOrFalseGameDataJSON;
    public TextAsset BubbleGameDataJSON;
    public TextAsset SetsGameDataJSON;
    public TextAsset MathMonsterGameDataJSON;
    public TextAsset FishGameDataJSON;
    public TextAsset AffixGameDataJSON;
    public TextAsset BoomGameDataJSON;
    public TextAsset HangManGameDataJSON;
    public TextAsset ReadGameDataJSON;
    public TextAsset MillionaireGameDataJSON;

    // Use this for initialization
    void Awake () {
        //((HHHScreen)this).Awake(); // Meghívjuk az ős osztály Awake metódusát

        // Gombok szkriptjének beállítása
        foreach (Button button in GetComponentsInChildren<Button>())
            button.buttonClick = ButtonClick;

        // Teszt jelleggel
         MakeBigJSON();
    }

    // A megadott JSON fájlokat össze gyúrja egy naggyá
    void MakeBigJSON() {
        JSONClass newJSON = new JSONClass();

        newJSON["name"] = "Single Player";

        for (int i = 0; i < 10; i++)
        {
            JSONClass mosaicClass = new JSONClass();
            mosaicClass["name"] = "óra-mozaik";
            mosaicClass["multiPlayerGameMode"] = "ChangeForSubTask";

            switch (i)
            {
                case 0: mosaicClass["games"][0] = JSON.Parse(TrueOrFalseGameDataJSON.text); break;
                case 1: mosaicClass["games"][0] = JSON.Parse(BubbleGameDataJSON.text); break;
                case 2: mosaicClass["games"][0] = JSON.Parse(SetsGameDataJSON.text); break;
                case 3: mosaicClass["games"][0] = JSON.Parse(MathMonsterGameDataJSON.text); break;
                case 4: mosaicClass["games"][0] = JSON.Parse(FishGameDataJSON.text); break;
                case 5: mosaicClass["games"][0] = JSON.Parse(AffixGameDataJSON.text); break;
                case 6: mosaicClass["games"][0] = JSON.Parse(BoomGameDataJSON.text); break;
                case 7: mosaicClass["games"][0] = JSON.Parse(HangManGameDataJSON.text); break;
                case 8: mosaicClass["games"][0] = JSON.Parse(ReadGameDataJSON.text); break;
                case 9: mosaicClass["games"][0] = JSON.Parse(MillionaireGameDataJSON.text); break;
            }

            newJSON["lessonMosaics"][i] = mosaicClass;
        }

        /*
        newJSON["flows"][0] = JSON.Parse(TrueOrFalseGameDataJSON.text);
        newJSON["flows"][1] = JSON.Parse(BubbleGameDataJSON.text);
        newJSON["flows"][2] = JSON.Parse(SetsGameDataJSON.text);
        newJSON["flows"][3] = JSON.Parse(MathMonsterGameDataJSON.text);
        newJSON["flows"][4] = JSON.Parse(FishGameDataJSON.text);
        newJSON["flows"][5] = JSON.Parse(AffixGameDataJSON.text);
        newJSON["flows"][6] = JSON.Parse(BoomGameDataJSON.text);
        newJSON["flows"][7] = JSON.Parse(HangManGameDataJSON.text);
        newJSON["flows"][8] = JSON.Parse(ReadGameDataJSON.text);
        newJSON["flows"][9] = JSON.Parse(MillionaireGameDataJSON.text);
        */

        System.IO.File.WriteAllText("mainmenugame.json", newJSON.ToString(" "));
    }

    override public IEnumerator InitCoroutine()
    {
        // A hálózati eseményekről értesítést szeretnénk kapni
        Common.HHHnetwork.callBackNetworkEvent = ReceivedNetworkEvent;

        yield return null;
    }

    // Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        Common.HHHnetwork.messageProcessingEnabled = true;

        yield return null;
    }

    // A menü saját magát láthatóvá teszi
    void ShowMySelf() {
        Common.screenController.ChangeScreen("MainMenu");
    }

    // Ha rákattintottak a buborékra, akkor meghívódik ez az eljárás a buborékon levő Button szkript által
    void ButtonClick(Button button)
    {
        if (!Common.screenController.changeScreenInProgress)
        { // Ha játékmódban vagyunk, akkor 
            /*
            switch (button.buttonType)
            {
                case Button.ButtonType.Game_TrueOrFalse:
                    Common.taskController.PlayQuestionList(TrueOrFalseGameDataJSON.text, true, () => { ShowMySelf(); } );
                    break;
                case Button.ButtonType.Game_Bubble:
                    Common.taskController.PlayQuestionList(BubbleGameDataJSON.text, true, () => { ShowMySelf(); });
                    break;
                case Button.ButtonType.Game_Sets:
                    Common.taskController.PlayQuestionList(SetsGameDataJSON.text, true, () => { ShowMySelf(); });
                    break;
                case Button.ButtonType.Game_Math_Monster:
                    Common.taskController.PlayQuestionList(MathMonsterGameDataJSON.text, true, () => { ShowMySelf(); });
                    break;
                case Button.ButtonType.Game_Fish:
                    Common.taskController.PlayQuestionList(FishGameDataJSON.text, true, () => { ShowMySelf(); });
                    break;
                case Button.ButtonType.Game_Affix:
                    Common.taskController.PlayQuestionList(AffixGameDataJSON.text, true, () => { ShowMySelf(); });
                    break;
                case Button.ButtonType.Game_Boom:
                    Common.taskController.PlayQuestionList(BoomGameDataJSON.text, true, () => { ShowMySelf(); });
                    break;
                case Button.ButtonType.Game_Hangman:
                    Common.taskController.PlayQuestionList(HangManGameDataJSON.text, true, () => { ShowMySelf(); });
                    break;
                case Button.ButtonType.Game_Read:
                    Common.taskController.PlayQuestionList(ReadGameDataJSON.text, true, () => { ShowMySelf(); });
                    break;
                case Button.ButtonType.Game_Millionaire:
                    Common.taskController.PlayQuestionList(MillionaireGameDataJSON.text, true, () => { ShowMySelf(); });
                    break;
            }
            */
        }
    }

    /// <summary>
    /// Esemény érkezett a hálózaton.
    /// </summary>
    /// <param name="networkEventType">A hálózati esemény típusa.(connect, data, disconnect, stb.)</param>
    /// <param name="connectionID">Melyik kapcsolat azonosítón jött be.</param>
    /// <param name="receivedData">A fogadott adat JSONNode formájában.</param>
    void ReceivedNetworkEvent(NetworkEventType networkEventType, int connectionID, JSONNode receivedData)
    {
        switch (networkEventType)
        {
            case NetworkEventType.DataEvent:
                string dataContent = receivedData["dataContent"];
                // Ha le kell állítani a játékot, akkor vissza lépünk a kliens várakozó képernyőre
                if (dataContent == "playStop")
                    Common.screenController.ChangeScreen("CanvasScreenClientWaitStart");
                break;
        }
    }
}
