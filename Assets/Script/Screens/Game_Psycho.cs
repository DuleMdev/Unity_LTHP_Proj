using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_Psycho : TrueGameAncestor
{
    TaskPsychoData taskData;

    Text textInstruction;

    RectTransform rectTransformScrollQuestion; // A kérdést tartalmazó scrollBox (A tartalomból látható méret lekérdezéséhez kell, mennyire kell kimozgatni a kérdés text-jét, hogy ne látszódjon)
    RectTransform rectTransformContentQuestion; // A kérdést tartalmazó scrollBox Content-je (A szöveg méretéhez állítjuk a content méretét, hogy scrollozható legyen a teljes szöveg)
    Text textQuestionContent;   // A kérdés megjelenítéséhez a scrollBox-ban

    Text textAnswer;    // A "válasz" felírat megjelenítéséhez
    InputField inputFieldAnswer;    // A válasz megadásához
    Text textInputFieldAnswer;  // Az inputField szövegének elhalványításához

    RectTransform rectTransformHistoryMove; // Előzmények mozgatásához
    Text textHistory;   // Az "előzmények" felírat megjelenítéséhez
    Text textHistoryContent; // Az előzmények megjelenítéséhez a scrollBox-ban
    GameObject gameObjectScrollPreviousAnswers; // Az előzmény scrollBox kikapcsolásához, ha nincs előzmény
    RectTransform rectTransformContentHistory;  // Az előzményeket tartalmazó scrollBox Content-je (Az előzmények méretéhet állítjuk a content méretét, hogy scrollozható legyen a teljes előzmény)

    UnityEngine.UI.Button button;
    Text textButton;    // Küldés és továbblépés gomb szövegének kiírásához

    // Use this for initialization
    override public void Awake()
    {
        base.Awake(); // Meghívjuk az ős osztály Awake metódusát

        textInstruction = gameObject.SearchChild("TextInstruction").GetComponent<Text>();

        rectTransformScrollQuestion = gameObject.SearchChild("ScrollQuestion").GetComponent<RectTransform>();
        rectTransformContentQuestion = gameObject.SearchChild("ContentQuestion").GetComponent<RectTransform>();
        textQuestionContent = gameObject.SearchChild("TextQuestionContent").GetComponent<Text>();

        textAnswer = gameObject.SearchChild("TextAnswer").GetComponent<Text>();
        inputFieldAnswer = gameObject.SearchChild("InputFieldAnswer").GetComponent<InputField>();
        textInputFieldAnswer = gameObject.SearchChild("TextInputFieldAnswer").GetComponent<Text>();

        rectTransformHistoryMove = gameObject.SearchChild("HistoryMove").GetComponent<RectTransform>();
        textHistory = gameObject.SearchChild("TextHistory").GetComponent<Text>();

        textHistoryContent = gameObject.SearchChild("TextHistoryContent").GetComponent<Text>();
        gameObjectScrollPreviousAnswers = gameObject.SearchChild("ScrollPreviousAnswers").gameObject;
        rectTransformContentHistory = gameObject.SearchChild("ContentHistory").GetComponent<RectTransform>();

        button = gameObject.SearchChild("Button").GetComponent<UnityEngine.UI.Button>();
        textButton = gameObject.SearchChild("TextButton").GetComponent<Text>();
    }

    /// <summary>
    /// Felkészülünk a feladat megmutatására.
    /// </summary>
    /// <returns></returns>
    override public IEnumerator PrepareTask()
    {
        menu.Reset();
        menu.gameObject.SetActive(false);

        exitButton.SetActive(false); // Common.configurationController.deviceIsServer);

        clock.timeInterval = taskData.time;
        clock.Reset(0);

        textInstruction.text = taskData.instructions;

        textQuestionContent.text = "\n" + taskData.question;
        rectTransformContentQuestion.sizeDelta = new Vector2(0, textQuestionContent.preferredHeight);

        textAnswer.text = Common.languageController.Translate(C.Texts.Answer) + ":";
        inputFieldAnswer.text = "";

        textHistory.text = Common.languageController.Translate(C.Texts.History) + ":";

        // Összeállítjuk az előzmény stringet
        string allHistory = "";
        for (int i = 0; i < taskData.preAnswers.Count; i++)
        {
            if (i != 0)
                allHistory += "\n";

            allHistory += "<b>" + taskData.preAnswers[i].date + "</b>\n";
            allHistory += taskData.preAnswers[i].answer + "\n";
        }
        textHistoryContent.text = allHistory;

        rectTransformContentHistory.sizeDelta = new Vector2(0, textHistoryContent.preferredHeight);

        // Ha nincs előzmény, akkor eltüntetjük a megjelenítéséhez szükséges komponenseket
        textHistory.gameObject.SetActive(taskData.preAnswers.Count != 0);
        gameObjectScrollPreviousAnswers.SetActive(taskData.preAnswers.Count != 0);

        textButton.text = Common.languageController.Translate(C.Texts.SendAndNext);

        SetColor(1);

        // Összezsugorítjuk a feladat leírást
        textInstruction.transform.localScale = Vector3.one * 0.001f;

        // Kipozícionáljuk a képernyőből a kérdést és az előzményeket
        SetQuestionPos(-rectTransformScrollQuestion.sizeDelta.y);
        SetHistoryPos(-rectTransformHistoryMove.sizeDelta.y);

        clock.SetPictures();

        iTween.Stop(gameObject);

        yield return null;
    }

    override public IEnumerator InitCoroutine()
    {
        status = Status.Init;

        // Lekérdezzük a feladatot
        taskData = (TaskPsychoData)Common.taskController.task;

        // Felkészülünk a feladat megjelenítésére
        yield return StartCoroutine(PrepareTask());
    }

    override public IEnumerator ScreenShowStartCoroutine()
    {
        StartCoroutine(base.ScreenShowStartCoroutine());
        // Eltüntetjük a tartalmaz, hogy a ScreenShowFinish-ben előtudjanak bukkanni

        // Összezsugorítjuk a feladat leírást
        textInstruction.transform.localScale = Vector3.one * 0.001f;

        // Kipozícionáljuk a képernyőből a kérdést és az előzményeket
        SetQuestionPos(-rectTransformScrollQuestion.sizeDelta.y);
        SetHistoryPos(-rectTransformHistoryMove.sizeDelta.y);

        //textQuestionContent.rectTransform.sizeDelta = new Vector2(0, rectTransformScrollQuestion.sizeDelta.y);
        //rectTransformHistoryMove.anchoredPosition = new Vector2(0, -rectTransformHistoryMove.sizeDelta.y);

        yield break;
    }

    // Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        // Megjelenítjük a feladat leírást
        iTween.ScaleTo(textInstruction.gameObject, iTween.Hash("islocal", true, "scale", Vector3.one, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutElastic));
        Common.audioController.SFXPlay("boing");
        yield return new WaitForSeconds(taskData.animSpeed1);

        // Beúszik a kérdés
        iTween.ValueTo(gameObject, iTween.Hash("from", textQuestionContent.rectTransform.anchoredPosition.y, "to", 0, "time", taskData.animSpeed1, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "SetQuestionPos", "onupdatetarget", gameObject));
        yield return new WaitForSeconds(taskData.animSpeed1 / 2); 
        float waitTime = taskData.animSpeed1 / 2;

        // Beúsznak az előzmények, ha van
        if (taskData.preAnswers.Count > 0) {
            iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformHistoryMove.anchoredPosition.y, "to", 0, "time", taskData.animSpeed1, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "SetHistoryPos", "onupdatetarget", gameObject));
            yield return new WaitForSeconds(taskData.animSpeed1 / 2);

            waitTime = taskData.animSpeed1;
        }

        yield return new WaitForSeconds(waitTime);

        status = Status.Play;
        Common.HHHnetwork.messageProcessingEnabled = true;
    }

    public override IEnumerator HideGameElement()
    {
        base.HideGameElement();

        // Elhalványítjuk a feladat leírását és a kérdés szövegét
        iTween.ValueTo(gameObject, iTween.Hash("from", 1, "to", 0, "time", taskData.animSpeed1, "easetype", iTween.EaseType.linear, "onupdate", "SetColor", "onupdatetarget", gameObject));

        // Az előzmények kimennek a képernyőből, ha van
        if (taskData.preAnswers.Count > 0)
        {
            iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformHistoryMove.anchoredPosition.y, "to", -rectTransformHistoryMove.sizeDelta.y, "time", taskData.animSpeed1, "easetype", iTween.EaseType.easeInCubic, "onupdate", "SetHistoryPos", "onupdatetarget", gameObject));
        }

        yield return new WaitForSeconds(taskData.animSpeed1);
    }

    void SetColor(float opacity) {
        textInstruction.color = textInstruction.color.SetA(opacity);
        textQuestionContent.color = textQuestionContent.color.SetA(opacity);
    }

    void SetQuestionPos(float pos) {
        textQuestionContent.rectTransform.anchoredPosition = new Vector2(0, pos);
    }

    void SetHistoryPos(float pos) {
        rectTransformHistoryMove.anchoredPosition = new Vector2(0, pos);
    }

    // A menüből kiválasztották a kilépést a játékból
    /*
    IEnumerator ExitCoroutine()
    {
        status = Status.Exit;
        clock.Stop();

        Common.taskController.GameExit();
        yield return null;
    }
    */

    /*
    override protected void ButtonClick(Button button)
    {
        if (userInputIsEnabled)
        {
            switch (button.buttonType)
            {
                case Button.ButtonType.Exit:
                    StartCoroutine(ExitCoroutine());
                    break;

                case Button.ButtonType.SwitchLayout: // Megnyomták a layout váltó gombot
                    //layoutManager.ChangeLayout();
                    //SetPictures();
                    break;
            }
        }
    }
    */

    public void SendButton()
    {
        // Elküldünk egy hamis választ a szervernek
        // A szerveren levő psychoGameData nem fogja kiértékelni csak véget vett a játéknak és majd a végső üzenetben fogjuk elküldeni ténylegesen a választ
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
        jsonClass[C.JSONKeys.gameEventType] = C.JSONValues.answer;

        Common.taskController.SendMessageToServer(jsonClass);
    }

    override public void CollectFinalMessage()
    {
        base.CollectFinalMessage();

        finalMessageJson[C.JSONKeys.answer] = inputFieldAnswer.text;
    }

    // Update is called once per frame
    override public void Update()
    {
        base.Update();

        // A küldés gomb csak akkor aktív, ha van legalább egy karakter az input mezőben
        button.interactable = inputFieldAnswer.text.Length > 0;
    }
}
