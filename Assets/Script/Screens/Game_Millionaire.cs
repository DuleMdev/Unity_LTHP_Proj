using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;
using SimpleJSON;
using UnityEngine.Networking;

public class Game_Millionaire : TrueGameAncestor
{
    //SpriteRenderer foreground;

    Image imageQuestion;            // A kérdés képe ide kerül
    GameObject questionPictureMove;  // A kérdés képének előugrásához
    RectTransform canvasQuestionPicture; // Kérdés képének nagyításához

    MilionairePanel questionPanel;

    Transform answerNarrow_A_Column;    // A válaszok bal oldalának oszlopa ha van kép
    Transform answerNarrow_B_Column;    // A válaszok jobb oldalának oszlopa ha van kép
    Transform answerWide_A_Column;      // A válaszok bal oldalának oszlopa ha nincs kép
    Transform answerWide_B_Column;      // A válaszok jobb oldalának oszlop ha nincs kép

    Transform answerPanelPrefab;  // A válaszok ide kerülnek

    GridLayoutGroup gridLayoutGroup;
    RectTransform content;


    //Text verifyButtonText;              // Az önellenőrzés gomb kezeléséhez

    List<MilionairePanel> listOfPanel; // A válaszok megjelenítéséhez
    TaskMillionaireData taskData;     // A feladatot tartalmazó objektum

    override public void Awake() {
        base.Awake();

        //foreground = gameObject.SearchChild("foreground").GetComponent<SpriteRenderer>();

        imageQuestion = gameObject.SearchChild("ImageQuestion").GetComponent<Image>();

        questionPictureMove = gameObject.SearchChild("QuestionPictureMove").gameObject;
        canvasQuestionPicture = questionPictureMove.SearchChild("Canvas").GetComponent<RectTransform>();

        questionPanel = gameObject.SearchChild("Question").GetComponent<MilionairePanel>();

        answerNarrow_A_Column = gameObject.SearchChild("AnswerNarrow_A_Column").transform;
        answerNarrow_B_Column = gameObject.SearchChild("AnswerNarrow_B_Column").transform;
        answerWide_A_Column = gameObject.SearchChild("AnswerWide_A_Column").transform;
        answerWide_B_Column = gameObject.SearchChild("AnswerWide_B_Column").transform;

        answerPanelPrefab = gameObject.SearchChild("AnswerPanel").transform;
        answerPanelPrefab.gameObject.SetActive(false);

        gridLayoutGroup = gameObject.GetComponentInChildren<GridLayoutGroup>();
        content = gameObject.SearchChild("Content").GetComponent<RectTransform>();

        //verifyButtonText = gameObject.SearchChild("VerifyButtonText").GetComponent<Text>();

        // Gombok szkriptjének beállítása
        //foreach (Button button in GetComponentsInChildren<Button>())
        //    button.buttonClick = ButtonClick;
    }

    /// <summary>
    /// Felkészülünk a feladat megmutatására.
    /// </summary>
    /// <returns></returns>
    override public IEnumerator PrepareTask()
    {
        yield return StartCoroutine(base.PrepareTask());

        exitButton.SetActive(false); // Common.configurationController.deviceIsServer);

        clock.timeInterval = taskData.time;
        clock.Reset(0);

        bool pictureIsPresent = taskData.questionPicture != "";

        // A kérdés képét elhelyezzük és összezsugorítjuk
        if (pictureIsPresent)
        {
            imageQuestion.sprite = taskData.gameData.GetSprite(taskData.questionPicture);

            // Módosítjuk a Canvas méretét, hogy a kép méretarányos maradjon
            Rect spriteSize = imageQuestion.sprite.rect;
            float ratioX = canvasQuestionPicture.sizeDelta.x / spriteSize.width;
            float ratioY = canvasQuestionPicture.sizeDelta.y / spriteSize.height;
            float ratio = Mathf.Min(ratioX, ratioY);

            canvasQuestionPicture.sizeDelta = new Vector2(spriteSize.width * ratio, spriteSize.height * ratio);
        }

        questionPictureMove.transform.localScale = Vector3.one * 0.001f;

        // beállítjuk a kérdés panelt
        questionPanel.SetText(taskData.question, taskData.question);           // Beállítjuk a kérdés szövegét
        questionPanel.SetLocalMovePos(new Vector3(0, 5));   // Beállítjuk, hogy honnan jöjjön be a kérdés
        questionPanel.gameObject.SetActive(false);          // Kikapcsoljuk a panelt, hogy még ne látszódjon

        // Lekérdezzük a lehetséges válaszokat
        List<string> listOfAnswers = taskData.GetAnswersShuffle(); //  questionEngine.GetAnswers();

        /*
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
        */

        // Létrehozzuk az objektumokat

        // Ha léteznek már válasz panelek, akkor azokat megsemmisítjük
        if (listOfPanel != null)
            foreach (MilionairePanel panel in listOfPanel)
                GameObject.Destroy(panel.gameObject);

        //bool left = true;   // A válasz melyik oldalra kell tenni

        listOfPanel = new List<MilionairePanel>();

        gridLayoutGroup.cellSize = new Vector3(pictureIsPresent ? 555 : 720, 170, 0);

        // Létrehozunk annyi válasz panelt amennyire szükség van
        for (int i = 0; i < listOfAnswers.Count; i++)
        {
            string answerText = listOfAnswers[i];

            // Létrehozunk egy válasz panelt
            MilionairePanel answerPanel = GameObject.Instantiate(answerPanelPrefab).GetComponent<MilionairePanel>();
            answerPanel.transform.parent = content;
            answerPanel.transform.localScale = Vector3.one;
            answerPanel.gameObject.SetActive(true);

            // beállítjuk a válasz panelt
            answerPanel.SetText(answerText, taskData.GetTextFromID(answerText));

            /*
            // Beállítjuk a panel pozícióját
            if (i == listOfAnswers.Count - 1 && left)
                // Ha ez az utolsó eleme és bal oldalra kerülne, akkor inkább középre tesszük
                answerPanel.transform.localPosition = new Vector2((A_RowXPos + B_RowXPos) / 2, aktRowPosY);
            else
                answerPanel.transform.localPosition = new Vector2((left) ? A_RowXPos : B_RowXPos, aktRowPosY);
            */

            answerPanel.SetLocalMovePos(new Vector2(0, -5000));
            answerPanel.gameObject.SetActive(false);

            //answerPanel.GetComponentInChildren<Button>().buttonClick = ButtonClick; // Beállítjuk a válaszra kattintást feldolgozó metódust
            answerPanel.panelClick = PanelClick;

            // Az elkészített panel hozzáadjuk a listához
            listOfPanel.Add(answerPanel);

            /*
            // Cserélgetjük a bal oldalra rajzolást és a jobb oldalra rajzolást
            if (left)
            {
                left = false;
            }
            else { // jobb oldalra rajzolás után új sor következik
                left = true;
                aktRowPosY -= rowDistance;
            }
            */
        }

        yield break;
    }

    /*override public IEnumerator PrepareTask()
    {
        yield return StartCoroutine(base.PrepareTask());

        exitButton.SetActive(false); // Common.configurationController.deviceIsServer);

        clock.timeInterval = taskData.time;
        clock.Reset(0);

        // A verify gomb szövegének beállítása nyelv függően
        //if (Common.configurationController.answerFeedback == ConfigurationController.AnswerFeedback.NoFeedback)
        //    verifyButtonText.text = Common.languageController.Translate(C.Texts.Next);
        //if (Common.configurationController.answerFeedback == ConfigurationController.AnswerFeedback.ShelfCheckButton)
        //    verifyButtonText.text = Common.languageController.Translate(C.Texts.Verify);
        //verifyButtonText.transform.parent.gameObject.SetActive(Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately);

        bool pictureIsPresent = taskData.questionPicture != "";

        // A kérdés képét elhelyezzük és összezsugorítjuk
        if (pictureIsPresent)
        {
            imageQuestion.sprite = taskData.gameData.GetSprite(taskData.questionPicture);

            // Módosítjuk a Canvas méretét, hogy a kép méretarányos maradjon
            Rect spriteSize = imageQuestion.sprite.rect;
            float ratioX = canvasQuestionPicture.sizeDelta.x / spriteSize.width;
            float ratioY = canvasQuestionPicture.sizeDelta.y / spriteSize.height;
            float ratio = Mathf.Min(ratioX, ratioY);

            canvasQuestionPicture.sizeDelta = new Vector2(spriteSize.width * ratio, spriteSize.height * ratio);
        }

        questionPictureMove.transform.localScale = Vector3.one * 0.001f;

        // beállítjuk a kérdés panelt
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
            else
            { // jobb oldalra rajzolás után új sor következik
                left = true;
                aktRowPosY -= rowDistance;
            }
        }

        yield break;
    }
    */
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
        //float animSpeed = 1;

        // Megjelenítjük a kérdés szövegét
        questionPanel.gameObject.SetActive(true);
        questionPanel.Move(Vector3.zero, taskData.animSpeed1, iTween.EaseType.easeOutBack);
        //iTween.MoveTo(questionPanel.moveTransform.gameObject, iTween.Hash("islocal", true, "position", Vector3.zero, "time", animSpeed, "easetype", iTween.EaseType.easeOutBack));
        yield return new WaitForSeconds(taskData.animSpeed1);

        // Megjelenítjük a kérdés képét ha van
        if (taskData.questionPicture != "")
        {
            Common.audioController.SFXPlay("boing");
            iTween.ScaleTo(questionPictureMove, iTween.Hash("islocal", true, "scale", Vector3.one, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutElastic));
            yield return new WaitForSeconds(taskData.animSpeed1);
        }

        // Megjelenítjük a válaszokat
        foreach (MilionairePanel panel in listOfPanel)
        {
            panel.gameObject.SetActive(true);
            panel.Move(Vector3.zero, taskData.animSpeed1, iTween.EaseType.easeOutCirc);
            //iTween.MoveTo(panel.moveTransform.gameObject, iTween.Hash("islocal", true, "position", Vector3.zero, "time", animSpeed, "easetype", iTween.EaseType.easeInOutCirc));

            // Várunk egy keveset
            yield return new WaitForSeconds(0.2f);
        }

        // Várunk amíg az utolsó animáció is befejeződik
        yield return new WaitForSeconds(taskData.animSpeed1);

        //succesfullTask = 0;

        status = Status.Play;
        Common.HHHnetwork.messageProcessingEnabled = true;

        Common.audioController.SetBackgroundMusic(1, 0.05f, 4); // Elindítjuk a háttérzenét
    }

    // A kérdéshez tartozó játék elemeket kell eltávolítani, ha meghívják ezt a metódust. A TaskController fogja új feladatnál ha az új feladatot ugyan annak a képernyőnek kell megjelenítenie
    // Meglehet adni neki egy callBack függvényt, amit akkor hív meg ha végzet a játék elemek elrejtésével, mivel ez sokáig is eltarthat és addig nem kéne tovább menni az új feladatra.
    override public IEnumerator HideGameElement()
    {
        clock.Reset(1); // Az órát alaphelyzetbe állítja

        //float animSpeed = 1;

        // Összezsugorítjuk a kérdés képét ha van
        if (taskData.questionPicture != "")
        {
            iTween.ScaleTo(questionPictureMove, iTween.Hash("islocal", true, "scale", Vector3.zero * 0.001f, "time", taskData.animSpeed1, "easeType", iTween.EaseType.linear));
            yield return new WaitForSeconds(taskData.animSpeed1);
        }

        // A válaszok lepotyognak
        for (int i = listOfPanel.Count - 1; i >= 0; i--)
        {
            MilionairePanel panel = listOfPanel[i];
            panel.Move(new Vector3(0, -5000), taskData.animSpeed1, iTween.EaseType.easeInCirc);
            Destroy(panel.gameObject, taskData.animSpeed1);
            yield return new WaitForSeconds(0.2f);
        }
        listOfPanel.Clear();

        yield return new WaitForSeconds(taskData.animSpeed1); // Várunk amíg az animáció befejeződik

        // Ki megy a kérdés is a képernyőből
        questionPanel.Move(new Vector3(0, 5), taskData.animSpeed1, iTween.EaseType.easeInCirc);

        yield return new WaitForSeconds(taskData.animSpeed1); // Várunk amíg az animáció befejeződik
    }

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

        // Megjelenítjük a kérdés képét ha van
        if (taskData.questionPicture != "")
            questionPictureMove.transform.localScale = Vector3.one;

        // Megjelenítjük a válaszokat
        foreach (MilionairePanel panel in listOfPanel)
        {
            panel.gameObject.SetActive(true);
            panel.SetLocalMovePos(Vector3.zero);
        }
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

    IEnumerator EvaluateCoroutine(JSONNode jsonData)
    {
        // Megkeressük azt a panelt amin a megadott szöveg szerepel
        MilionairePanel panel = null;

        foreach (MilionairePanel item in listOfPanel)
        {
            if (item.GetText() == jsonData[C.JSONKeys.selectedAnswer].Value) {
                panel = item;
                break;
            }
        }
        
        // Ha megtaláltuk a panelt, akkor ellenőrzizzük a válasz helyességét
        if (panel != null)
        {
            if (Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately)
            {
                panel.Marking();
            }
            else
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

                    case C.JSONValues.evaluateIsSilent:

                        break;
                }
            }
        }

        if (status == Status.Result)
            status = Status.Play;
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

    /// <summary>
    /// Ez a metódus hívódik meg ha rákattintottak a képre
    /// </summary>
    public void PictureClick()
    {
        zoomer.Zoom(canvasQuestionPicture.gameObject); // , goPicture: canvasQuestionPicture.gameObject);
    }

    // Ha rákattintottak egy válaszra, akkor meghívódik ez az eljárás a válaszpanelen levő Button szkript által
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
            }
        }
    }
    */

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
    }
}
