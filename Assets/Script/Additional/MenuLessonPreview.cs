using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class MenuLessonPreview : MonoBehaviour {

    Text textGameID;
    Text textLabels;
    Text textScreenNumbers;

    Transform bottomLeft;   // A preview képernyő megjelenítésének bal alsó koordinátája
    Transform topRight;     // A preview képernyő megjelenítésének jobb felső koordinátája

    Image nextButton;       // A képernyők közötti lapozó gombokat néha el kell tüntetni, ha már a nyíl által meghatározott irányba nem lehet tovább menni
    Image previousButton;   //

    Image imageFade;        // Képernyő váltásoknál a fadeEffect-et végrehajtó kép

    GameData gameData;      // A preview melyik játék képernyőit mutatja

    GameObject actScreen;   // Az aktuálisan mutatott játék képernyő

    //bool previewScreenBuild;    // Éppen az előnézeti kép készítése folyik

	// Use this for initialization
	void Awake () {
        textGameID = Common.SearchGameObject(gameObject, "TextGameID").GetComponent<Text>();
        textLabels = Common.SearchGameObject(gameObject, "TextLabels").GetComponent<Text>();
        textScreenNumbers = Common.SearchGameObject(gameObject, "TextScreenNumbers").GetComponent<Text>();

        bottomLeft = Common.SearchGameObject(gameObject, "BottomLeft").transform;
        topRight = Common.SearchGameObject(gameObject, "TopRight").transform;

        nextButton = Common.SearchGameObject(gameObject, "NextButton").GetComponent<Image>();
        previousButton = Common.SearchGameObject(gameObject, "PreviousButton").GetComponent<Image>();

        imageFade = Common.SearchGameObject(gameObject, "ImageFade").GetComponent<Image>();
    }

    /// <summary>
    /// Egy játékot és a képernyőit mutatja meg.
    /// </summary>
    public IEnumerator ShowGame(GameData gameData) {
        this.gameData = gameData;

        // Beállítjuk a Játék azonosító és a Cimkék text mező tartalmait
        textGameID.text = Common.languageController.Translate("GameID") + ": " + gameData.name;
        textLabels.text = Common.languageController.Translate("Labels") + ": " + gameData.GetLabels();

        yield return StartCoroutine(ShowScreen());
    }

    /// <summary>
    /// Megmutatja a actScreenNumber által megadott képernyőt.
    /// </summary>
    IEnumerator ShowScreen() {

        Debug.Log("ShowScreenStart");

        float effectSpeed = 0.2f;

        // Beállítjuk a előző / következő gomb láthatóságát
        nextButton.color = nextButton.color.SetA((Common.menuLessonPlan.screenIndex < gameData.screens.Count - 1) ? 1 : 0.2f);
        previousButton.color = previousButton.color.SetA((Common.menuLessonPlan.screenIndex > 0) ? 1 : 0.2f);

        // Kiírjuk az összes és az aktuális képernyő számát
        textScreenNumbers.text = Common.languageController.Translate("Screen") + ": " + (Common.menuLessonPlan.screenIndex + 1) + "/" + gameData.screens.Count;

        // Kitakarjuk fadeEffectel a korábbi képernyőt
        iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", 1, "time", effectSpeed, "easetype", iTween.EaseType.linear, "onupdate", "ImageFadeUpdate", "onupdatetarget", gameObject));
        yield return new WaitForSeconds(effectSpeed);

        // töröljük a korább képernyőt ha van
        if (actScreen != null)
        {
            Destroy(actScreen.gameObject);
            actScreen = null;
        }

        GameObject newScreen = null;

        switch (gameData.gameEngine)
        {
            case GameData.GameEngine.TrueOrFalse:
                newScreen = Common.screenController.GetGameScreenCopyByName("TrueOrFalseGame");
                break;
            case GameData.GameEngine.Bubble:
                newScreen = Common.screenController.GetGameScreenCopyByName("BubbleGame3");
                break;
            case GameData.GameEngine.Sets:
                newScreen = Common.screenController.GetGameScreenCopyByName("SetGame");
                break;
            case GameData.GameEngine.MathMonster:
                newScreen = Common.screenController.GetGameScreenCopyByName("MathMonsterGame");
                break;
            case GameData.GameEngine.Millionaire:
                newScreen = Common.screenController.GetGameScreenCopyByName("MillionaireGame");
                break;
            case GameData.GameEngine.Fish:
                newScreen = Common.screenController.GetGameScreenCopyByName("FishGame");
                break;
            case GameData.GameEngine.Affix:
                newScreen = Common.screenController.GetGameScreenCopyByName("AffixGame");
                break;
            case GameData.GameEngine.Boom:
                newScreen = Common.screenController.GetGameScreenCopyByName("BoomGame");
                break;
            case GameData.GameEngine.Hangman:
                newScreen = Common.screenController.GetGameScreenCopyByName("HangmanGame");
                break;
            case GameData.GameEngine.Read:
                newScreen = Common.screenController.GetGameScreenCopyByName("ReadGame");
                break;
        }

        // Ha megtaláltuk a megfelelő képernyőt
        if (newScreen != null) {
            actScreen = newScreen;

            newScreen.gameObject.SetActive(true);

            // Átadjuk a képernyőnek a megjelenítésre szánt adatokat
            yield return StartCoroutine(newScreen.GetComponent<HHHScreen>().Preview(gameData.screens[Common.menuLessonPlan.screenIndex]));

            // Beállítjuk a szülőjét és beállítjuk a legelső elemnek a hierarhiába
            newScreen.transform.SetParent(gameObject.transform, false);
            newScreen.transform.SetAsFirstSibling();

            // Megkeressük a background komponenst és megkérjük, hogy állítsa be a méretarányokat úgy, hogy pontosan kitöltse a megadott tartományt
            newScreen.GetComponentInChildren<Background>(true).Refresh(topRight.localPosition - bottomLeft.localPosition);
            // A képernyő közepére pozícionáljuk
            newScreen.transform.position = (bottomLeft.position + topRight.position) / 2;
        }

        // Láthatóvá tesszük az új képernyőt
        iTween.ValueTo(gameObject, iTween.Hash("from", 1, "to", 0, "time", effectSpeed, "easetype", iTween.EaseType.linear, "onupdate", "ImageFadeUpdate", "onupdatetarget", gameObject));
        yield return new WaitForSeconds(effectSpeed);

        Debug.Log("ShowScreenEnd");

    }

    // iTween ValueTo metódusa hívja meg, amikor változtatni kell az szín értékét
    void ImageFadeUpdate(float value)
    {
        imageFade.color = imageFade.color.SetA(value);
    }

    public void ClickButton(string buttonName)
    {
        switch (buttonName)
        {
            case "Previous":
                if (Common.menuLessonPlan.screenIndex > 0) {
                    Common.menuLessonPlan.screenIndex--;
                    StartCoroutine(ShowScreen());
                }
                break;
            case "Next":
                if (Common.menuLessonPlan.screenIndex < gameData.screens.Count - 1) {
                    Common.menuLessonPlan.screenIndex++;
                    StartCoroutine(ShowScreen());
                }
                break;
            case "PreviewPlay":

                switch (gameData.gameEngine)
                {
                    /*
                    case GameData.GameType.Sets:
                        Common.taskControllerOld.PlayQuestionList(gameData, Common.menuLessonPlan.screenIndex, true, () => {
                            Common.screenController.ChangeScreen("MenuLessonPlan");
                        });

                        break;
                        */
                    case GameData.GameEngine.TrueOrFalse:
                    case GameData.GameEngine.Millionaire:
                    case GameData.GameEngine.Boom:
                    case GameData.GameEngine.Hangman:
                    case GameData.GameEngine.Bubble:
                    case GameData.GameEngine.Read:
                    case GameData.GameEngine.MathMonster:
                    case GameData.GameEngine.Fish:
                    case GameData.GameEngine.Affix:
                    case GameData.GameEngine.Sets:
                        Common.taskController.PlayGameInServer(gameData, Common.menuLessonPlan.screenIndex, () => {
                            Common.screenController.ChangeScreen("MenuLessonPlan");
                        } );
                        break;
                }




                
                break;
        }
        Debug.Log("Panel : " + buttonName);
    }
}