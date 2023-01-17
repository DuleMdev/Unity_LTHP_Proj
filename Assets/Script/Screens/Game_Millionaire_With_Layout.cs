using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;
using SimpleJSON;
using UnityEngine.Networking;

public class Game_Millionaire_With_Layout : TrueGameAncestor
{
    //SpriteRenderer questionSpriteRenderer;  // A kérdés képének animálásához
    LayoutManager layoutManager;    // A különböző layoutokhoz tartozó képeket tartalmazza

    Zoomer zoomer;

    SpriteRenderer foreground;

    Image imageQuestion;            // A kérdés képe ide kerül
    GameObject questionPictureMove;  // A kérdés képének előugrásához
    GameObject questionPictureCanvas;

    MilionairePanel questionPanel;

    Transform answerNarrow_A_Column;
    Transform answerNarrow_B_Column;
    Transform answerWide_A_Column;
    Transform answerWide_B_Column;

    //Transform A_Column;                     // A válaszok első oszlopa
    //Transform B_Column;                     // A válaszok második oszlopa



    //Panel questionPanel;        // A kérdés szövegének megjelenítéséhez
    List<MilionairePanel> listOfPanel; // A válaszok megjelenítéséhez
    TaskMillionaireData taskData;     // A feladatot tartalmazó objektum

    int succesfullTask;         // A megadott jó válaszok száma

    //GameEngine_Question questionEngine;

    override public void Awake()
    {
        base.Awake();

        layoutManager = GetComponentInChildren<LayoutManager>();

        zoomer = GetComponentInChildren<Zoomer>();

        foreground = transform.Find("background/foreground").GetComponent<SpriteRenderer>();

        imageQuestion = gameObject.SearchChild("ImageQuestion").GetComponent<Image>();

        questionPictureMove = gameObject.SearchChild("QuestionPictureMove").gameObject;
        questionPictureCanvas = questionPictureMove.SearchChild("Canvas").gameObject;

        questionPanel = gameObject.SearchChild("Question").GetComponent<MilionairePanel>();

        answerNarrow_A_Column = gameObject.SearchChild("AnswerNarrow_A_Column").transform;
        answerNarrow_B_Column = gameObject.SearchChild("AnswerNarrow_B_Column").transform;
        answerWide_A_Column = gameObject.SearchChild("AnswerWide_A_Column").transform;
        answerWide_B_Column = gameObject.SearchChild("AnswerWide_B_Column").transform;

        // Gombok szkriptjének beállítása
        foreach (Button button in GetComponentsInChildren<Button>())
        {
            // Ha a gomb a menün található, akkor nem állítjuk be a buttonClick-jét
            if (Common.IsDescendant(menu.transform, button.transform)) continue;
            button.buttonClick = ButtonClick;
        }
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

        bool pictureIsPresent = taskData.questionPicture != "";

        // A kérdés képét elhelyezzük és összezsugorítjuk
        if (pictureIsPresent)
        {
            imageQuestion.sprite = taskData.gameData.GetSprite(taskData.questionPicture);

            yield return null;

            //yield return Common.pictureController.LoadSpriteFromFileSystemCoroutine(taskData.questionPicture);
            //imageQuestion.sprite = Common.pictureController.resultSprite;
        }

        questionPictureMove.transform.localScale = Vector3.one * 0.001f;

        // beállítjuk a kérdést panelt
        questionPanel.SetText(taskData.question, taskData.question);           // Beállítjuk a kérdés szövegét
        questionPanel.SetLocalMovePos(new Vector3(0, 5));   // Beállítjuk, hogy honnan jöjjön be a kérdés
        questionPanel.gameObject.SetActive(false);          // Kikapcsoljuk a panelt, hogy még ne látszódjon

        // Lekérdezzük a lehetséges válaszokat
        List<string> listOfAnswers = taskData.GetAnswersShuffle(); //  questionEngine.GetAnswers();

        // Kiszámoljuk a sorok számát (soronként két válasz lehetséges, előfordulhat, hogy az utolsó sorban csak egy válasz van)
        int rowNumber = (listOfAnswers.Count + 1) / 2;

        // Kiszámoljuk a pozíciókat
        float center = (taskData.questionPicture == "") ?
            (answerWide_A_Column.localPosition.x + answerWide_B_Column.localPosition.x) / 2 : // Ha páratlan a válaszok száma, akkor hová kell esnie a képernyőn az utolsónak
            (answerNarrow_A_Column.localPosition.x + answerNarrow_B_Column.localPosition.x) / 2;
        float A_RowXPos = (taskData.questionPicture == "") ? answerWide_A_Column.localPosition.x : answerNarrow_A_Column.localPosition.x;
        float B_RowXPos = (taskData.questionPicture == "") ? answerWide_B_Column.localPosition.x : answerNarrow_B_Column.localPosition.x;
        float rowDistance = Mathf.Abs(answerWide_A_Column.localPosition.y - answerNarrow_A_Column.localPosition.y);  // Mennyi a sorok közti távolság
        float aktRowPosY = answerWide_A_Column.localPosition.y; // Meghatározzuk az első sor pozícióját

        // Létrehozzuk az objektumokat

        // Ha léteznek már válasz panelek, akkor azokat megsemmisítjük
        if (listOfPanel != null)
            foreach (MilionairePanel panel in listOfPanel)
                GameObject.Destroy(panel.gameObject);

        bool left = true;   // A válasz melyik oldalra kell tenni

        listOfPanel = new List<MilionairePanel>();

        // Létrehozunk annyi válasz panelt amennyire szükség van
        for (int i = 0; i < listOfAnswers.Count; i++)
        {
            string answerText = listOfAnswers[i];

            // Létrehozunk egy válasz panelt
            MilionairePanel answerPanel = GameObject.Instantiate((pictureIsPresent) ? answerNarrow_A_Column : answerWide_A_Column).GetComponent<MilionairePanel>();
            answerPanel.transform.parent = background.transform;
            answerPanel.transform.localScale = Vector3.one;

            // beállítjuk a válasz panelt
            answerPanel.SetText(answerText, taskData.GetTextFromID(answerText));
            answerPanel.SetPicture(layoutManager.GetSprite("answer"));
            answerPanel.SetColor(layoutManager.GetColor("answer"));
            answerPanel.SetTextColor(layoutManager.GetColor("textColor"));

            // Beállítjuk a panel pozícióját
            if (i == listOfAnswers.Count - 1 && left)
                // Ha ez az utolsó eleme és bal oldalra kerülne, akkor inkább középre tesszük
                answerPanel.transform.localPosition = new Vector2((A_RowXPos + B_RowXPos) / 2, aktRowPosY);
            else
                answerPanel.transform.localPosition = new Vector2((left) ? A_RowXPos : B_RowXPos, aktRowPosY);

            answerPanel.SetLocalMovePos(new Vector2(0, -5));
            answerPanel.gameObject.SetActive(false);

            //answerPanel.GetComponentInChildren<Button>().buttonClick = ButtonClick; // Beállítjuk a válaszra kattintást feldolgozó metódust
            answerPanel.panelClick = PanelClick;

            // Az elkészített panel hozzáadjuk a listához
            listOfPanel.Add(answerPanel);

            // Cserélgetjük a bal oldalra rajzolást és a jobb oldalra rajzolást
            if (left)
            {
                left = false;
            }
            else { // jobb oldalra rajzolás után új sor következik
                left = true;
                aktRowPosY -= rowDistance;
            }
        }

        SetPictures();
    }

    override public IEnumerator InitCoroutine()
    {
        status = Status.Init;
        paused = false;

        // Lekérdezzük a feladat adatait
        taskData = (TaskMillionaireData)Common.taskController.task;

        yield return StartCoroutine(PrepareTask());
    }

    // Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        float animSpeed = 1;

        // Megjelenítjük a kérdés szövegét
        questionPanel.gameObject.SetActive(true);
        questionPanel.Move(Vector3.zero, animSpeed, iTween.EaseType.easeOutBack);
        //iTween.MoveTo(questionPanel.moveTransform.gameObject, iTween.Hash("islocal", true, "position", Vector3.zero, "time", animSpeed, "easetype", iTween.EaseType.easeOutBack));
        yield return new WaitForSeconds(animSpeed);

        // Megjelenítjük a kérdés képét ha van
        if (taskData.questionPicture != "")
        {
            Common.audioController.SFXPlay("boing");
            iTween.ScaleTo(questionPictureMove, iTween.Hash("islocal", true, "scale", Vector3.one, "time", animSpeed, "easeType", iTween.EaseType.easeOutElastic));
            yield return new WaitForSeconds(animSpeed);
        }

        // Megjelenítjük a válaszokat
        foreach (MilionairePanel panel in listOfPanel)
        {
            panel.gameObject.SetActive(true);
            panel.Move(Vector3.zero, animSpeed, iTween.EaseType.easeOutCirc);
            //iTween.MoveTo(panel.moveTransform.gameObject, iTween.Hash("islocal", true, "position", Vector3.zero, "time", animSpeed, "easetype", iTween.EaseType.easeInOutCirc));

            // Várunk egy keveset
            yield return new WaitForSeconds(0.2f);
        }

        // Várunk amíg az utolsó animáció is befejeződik
        yield return new WaitForSeconds(animSpeed);

        succesfullTask = 0;

        status = Status.Play;
        Common.HHHnetwork.messageProcessingEnabled = true;

        Common.audioController.SetBackgroundMusic(1, 0.05f, 4); // Elindítjuk a háttérzenét
    }

    // A kérdéshez tartozó játék elemeket kell eltávolítani, ha meghívják ezt a metódust. A TaskController fogja új feladatnál ha az új feladatot ugyan annak a képernyőnek kell megjelenítenie
    // Meglehet adni neki egy callBack függvényt, amit akkor hív meg ha végzet a játék elemek elrejtésével, mivel ez sokáig is eltarthat és addig nem kéne tovább menni az új feladatra.
    override public IEnumerator HideGameElement()
    {
        clock.Reset(1); // Az órát alaphelyzetbe állítja

        float animSpeed = 1;

        // Összezsugorítjuk a kérdés képét ha van
        if (taskData.questionPicture != "")
        {
            iTween.ScaleTo(questionPictureMove, iTween.Hash("islocal", true, "scale", Vector3.zero * 0.001f, "time", animSpeed, "easeType", iTween.EaseType.linear));
            yield return new WaitForSeconds(animSpeed);
        }

        // A válaszok lepotyognak
        for (int i = listOfPanel.Count - 1; i >= 0; i--)
        {
            MilionairePanel panel = listOfPanel[i];
            panel.Move(new Vector3(0, -5), animSpeed, iTween.EaseType.easeInCirc);
            Destroy(panel.gameObject, animSpeed);
            yield return new WaitForSeconds(0.2f);
        }
        listOfPanel.Clear();

        yield return new WaitForSeconds(animSpeed); // Várunk amíg az animáció befejeződik

        // Ki megy a kérdés is a képernyőből
        questionPanel.Move(new Vector3(0, 5), animSpeed, iTween.EaseType.easeInCirc);

        yield return new WaitForSeconds(animSpeed); // Várunk amíg az animáció befejeződik
    }

    // Update is called once per frame
    /*
    new void Update () {
        menu.menuEnabled = (status == Status.Play);

        if (status == Status.Play && !paused) // Ha megy a játék, akkor megy az óra
            clock.Go();
        else
            clock.Stop();
    }
    */

    /*
    // Játéknak vége letelt az idő, vagy a játékot sikeresen megoldottuk
    override public IEnumerator GameEnd()
    {
        status = Status.Result;
        //clock.Stop();

        yield return new WaitForSeconds(2);

        // Tájékoztatjuk a feladatkezelőt, hogy vége a játéknak és átadjuk a játékos eredményeit
        Common.taskControllerOld.TaskEnd(null);
    }
    */

    /// <summary>
    /// A tanári tablet óraterv előnézeti képernyője hívja meg ha meg kell mutatni a játék előnézetét.
    /// A task paraméter tartalmazza a játék képernyőjének adatait.
    /// </summary>
    /// <param name="task">A megjelenítendő képernyő adata</param>
    override public IEnumerator Preview(TaskAncestor task)
    {
        taskData = (TaskMillionaireData)task;

        yield return StartCoroutine(PrepareTask());

        // Megjelenítjük a kérdés szövegét
        questionPanel.gameObject.SetActive(true);
        questionPanel.SetLocalMovePos(Vector3.zero);

        //questionPanel.Move(Vector3.zero, 0, iTween.EaseType.easeOutBack);

        // Megjelenítjük a kérdés képét ha van
        if (taskData.questionPicture != "")
            questionPictureMove.transform.localScale = Vector3.one;
        //iTween.ScaleTo(questionPictureMove, iTween.Hash("islocal", true, "scale", Vector3.one, "time", 0, "easeType", iTween.EaseType.easeOutElastic));

        // Megjelenítjük a válaszokat
        foreach (MilionairePanel panel in listOfPanel)
        {
            panel.gameObject.SetActive(true);
            panel.SetLocalMovePos(Vector3.zero);
            //panel.Move(Vector3.zero, 0, iTween.EaseType.easeOutCirc);
        }
    }

    // A menüből kiválasztották a kilépést a játékból
    IEnumerator ExitCoroutine()
    {
        status = Status.Exit;
        clock.Stop();

        Common.taskController.GameExit();
        yield return null;
    }

    IEnumerator EvaluateCoroutine(JSONNode jsonData)
    {
        // Megkeressük azt a panelt amin a megadott szöveg szerepel
        MilionairePanel panel = null;

        foreach (MilionairePanel item in listOfPanel)
        {
            if (item.GetText() == jsonData[C.JSONKeys.selectedAnswer].Value)
            {
                panel = item;
                break;
            }
        }

        // Ha megtaláltuk a panelt, akkor ellenőrzizzük a válasz helyességét
        if (panel != null)
        {
            switch (jsonData[C.JSONKeys.evaluateAnswer].Value)
            {
                case C.JSONValues.evaluateIsTrue:
                    // Helyes a válasz
                    Common.audioController.SFXPlay("positive");
                    yield return new WaitForSeconds(panel.Flashing(true));

                    break;

                case C.JSONValues.evaluateIsFalse:
                    // A válasz helytelen
                    Common.audioController.SFXPlay("negative");
                    yield return new WaitForSeconds(panel.Flashing(false));

                    break;
            }
        }

        /*
        if (panel != null) {

            if (taskData.goodAnswers.Contains(panel.GetText()))
            {   // Helyes a válasz
                Common.audioController.SFXPlay("positive");
                yield return new WaitForSeconds(panel.Flashing(true));
                succesfullTask++;

                Common.taskControllerOld.GameEventHappend(TaskControllerOld.GameEvent.GoodAnswer);
            }
            else
            {   // A válasz helytelen
                Common.audioController.SFXPlay("negative");
                yield return new WaitForSeconds(panel.Flashing(false));

                Common.taskControllerOld.GameEventHappend(TaskControllerOld.GameEvent.WrongAnswer);
            }

            // panel.SetColor((taskData.goodAnswers.Contains(panel.GetText())) ? Color.green : Color.red);
        }
        */

        if (status == Status.Result)
            status = Status.Play;

        // ha az összes kérdésre helyesen válaszoltunk, akkor vége a játéknak
        //if (succesfullTask == taskData.goodAnswers.Count)
        //    StartCoroutine(GameEnd());
    }

    /// <summary>
    /// Üzenet érkezett a hálózaton, amit a TaskController továbbított.
    /// </summary>
    /// <param name="networkEventType"></param>
    /// <param name="connectionID"></param>
    /// <param name="jsonNodeMessage"></param>
    override public void MessageArrived(NetworkEventType networkEventType, int connectionId, JSONNode jsonNodeMessage)
    {
        // Ős osztálynak is elküldjük a bejövő üzenetet
        base.MessageArrived(networkEventType, connectionId, jsonNodeMessage);

        if (networkEventType == NetworkEventType.DataEvent)
        {
            switch (jsonNodeMessage[C.JSONKeys.gameEventType])
            {
                case C.JSONValues.answer:
                    status = Status.Result;
                    StartCoroutine(EvaluateCoroutine(jsonNodeMessage));

                    break;

                case C.JSONValues.nextPlayer:
                    status = Status.Play;

                    break;
            }
        }
    }

    // Elavult ***********************************************************
    /*
    // Task controller hívja meg ha történt valamilyen esemény
    // jsonNode változóban taláható a történt esemény
    override public void EventHappened(JSONNode jsonNode)
    {
        switch (jsonNode[C.JSONKeys.gameEvent])
        {
            case C.NetworkGameEvent.SelectAnswer:
                status = Status.Result;
                StartCoroutine(EvaluateCoroutine(jsonNode[C.JSONKeys.gameEventData]));
                break;
            case C.NetworkGameEvent.OutOfTime:
                Common.audioController.SFXPlay("negative");

                Common.taskControllerOld.GameEventHappend(TaskControllerOld.GameEvent.OutOfTime);
                StartCoroutine(GameEnd());
                break;
        }
    }
    */

    /*
/// <summary>
/// Egy eseményt küldünk a TaskManagernek
/// </summary>
/// <param name="Event"></param>
/// <param name="data"></param>
void SendMessage(string Event, string data = null)
{
    JSONClass jsonClass = new JSONClass();
    jsonClass[C.JSONKeys.dataContent] = C.JSONKeys.gameEvent;
    jsonClass[C.JSONKeys.gameEventType] = Event;

    if (data != null)
        jsonClass[C.JSONKeys.gameEventData] = data;

    Common.taskController.SendMessageToServer(jsonClass);
}
*/

    /// <summary>
    /// Ez a metódus hívódik meg ha rákattintottak a képre
    /// </summary>
    public void PictureClick()
    {
        zoomer.Zoom(questionPictureCanvas, 2);
    }

    // Ha rákattintottak egy válaszra, akkor meghívódik ez az eljárás a válaszpanelen levő Button szkript által
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

    // Ha rákattintottak egy válaszra, akkor meghívódik ez az eljárás a válaszpanelen levő Button szkript által
    void PanelClick(MilionairePanel panel)
    {
        if (userInputIsEnabled)
        {
            // Ha játékmódban vagyunk, akkor elküldjük a játékos választását
            JSONClass jsonClass = new JSONClass();
            jsonClass[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
            jsonClass[C.JSONKeys.gameEventType] = C.JSONValues.answer;
            jsonClass[C.JSONKeys.selectedAnswer] = panel.GetText();

            Common.taskController.SendMessageToServer(jsonClass);
        }

        //SendMessage(C.JSONValues.answer, panel.GetText());
    }

    /// <summary>
    /// A megadott layout-nak megfelelően beállítja a képeket.
    /// Meghívása előtt a LayoutManager-ben ki kell választani a megfelelő képi világot.
    /// </summary>
    void SetPictures()
    {
        background.GetComponent<SpriteRenderer>().sprite = layoutManager.GetSprite("background");
        foreground.sprite = layoutManager.GetSprite("foreground");

        questionPanel.SetPicture(layoutManager.GetSprite("question"));
        questionPanel.SetColor(layoutManager.GetColor("question"));
        questionPanel.SetTextColor(layoutManager.GetColor("textColor"));

        foreach (MilionairePanel panel in listOfPanel)
        {
            panel.SetPicture(layoutManager.GetSprite("answer"));
            panel.SetColor(layoutManager.GetColor("answer"));
            panel.SetTextColor(layoutManager.GetColor("textColor"));
        }

        // beállítjuk az óra layout-ját is
        clock.SetPictures();
    }
}

