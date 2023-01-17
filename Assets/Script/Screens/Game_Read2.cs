using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using SimpleJSON;
using UnityEngine.Networking;

public class Game_Read2 : TrueGameAncestor
{
    /// <summary>
    /// A kérdések gombja, amit ki kell tölteni
    /// </summary>
    [Tooltip("A kérdések gombja, amit ki kell tölteni")]
    GameObject inputFieldPrefab;
    /// <summary>
    /// A kérdés szövege szavanként, amit nem lehet megnyomni
    /// </summary>
    [Tooltip("A kérdés szövege szavanként, amit nem lehet megnyomni")]
    GameObject textWordPrefab;

    /// <summary>
    /// Hány pixel van két szó között
    /// </summary>
    [Tooltip("Hány pixel van két szó között")]
    public float distanceBetweenWords;

    /// <summary>
    /// Hány pixel van két sor között
    /// </summary>
    [Tooltip("Hány pixel van két sor között")]
    public float distanceBetweenLines;
    /// <summary>
    /// Hány pixel van két kérdés között
    /// </summary>
    [Tooltip("Hány pixel van két kérdés között")]
    public float distanceBetweenQuestions;

    /// <summary>
    /// Hány pixel van a felső és alsó margó és a kérdések között
    /// </summary>
    [Tooltip("Hány pixel van a felső és alsó margó és a kérdések között")]
    public float distanceBetweenMargo;

    CanvasGroup canvasGroup;            // A teljes canvas tartalom fade-eléséhez amikor megjelenik a szöveg

    GameObject storyShow;               // Ezt a gameObject-et kell bekapcsolni, ha a történetet szeretnénk látni
    GameObject questionsShow;           // Ezt a gameObject-et kell bekapcsolni, ha a kérdéseket

    RectTransform rectTransformContentStory;    // A tartalom rectTransformja be kell állítani a méretét a benne levő tartalomnak megfelelően
    RectTransform rectTransformContentStory2;   // A tartalom rectTransformja be kell állítani a méretét a benne levő tartalomnak megfelelően
    RectTransform rectTransformContentQuestion; // A tartalom rectTransformja be kell állítani a méretét a benne levő tartalomnak megfelelően

    Text storyText;     // Az első képernyőn levő történet
    Text storyText2;    // A második képernyőn levő ugyan az a történet

    GameObject questionHoldingPrefab;   // A kérdéseket tartó gameObject (ebből csinálunk egy másolatot és a másolatra tesszük a kérdéseket, hogy könnyebb legyen törölni)
    GameObject actQuestionHolding;      // A kérdéseket valóban tartalmazó gameObject, amit könnyen törölhetünk

    RectTransform verifyButton;         // Az ellenőrző gomb beállításához

    TaskRead2Data taskData;              // A feladatot tartalmazó objektum

    List<UIRead2InputField> listOfInputFields;    // A térténet kérdéseinek gombjai
    List<object> listOfItems;           // szövegek, kérdések és a soremelés = null objektumai

    //bool storyShowed;                   // Ha true, akkor a történet van mutatva, ha false akkor a kérdések

    float questionHeight;               // A kérdések magassága
    float answerBottomPos;              // Hol van a kérdésre adható válaszok alja

    override public void Awake()
    {
        base.Awake();

        // Megkeressük a játékban használt prefabokat
        inputFieldPrefab = gameObject.SearchChild("UIRead2InputField").gameObject;
        textWordPrefab = gameObject.SearchChild("UIReadTextWord").gameObject;
        // Kikapcsoljuk a prefab-okat
        inputFieldPrefab.SetActive(false);
        textWordPrefab.SetActive(false);

        canvasGroup = Common.SearchGameObject(gameObject, "CanvasGroup").GetComponent<CanvasGroup>();

        storyShow = gameObject.SearchChild("Story").gameObject;
        questionsShow = gameObject.SearchChild("Questions").gameObject;

        //scrollBar = Common.SearchGameObject(gameObject, "Scrollbar Vertical").GetComponent<Scrollbar>();
        rectTransformContentStory = Common.SearchGameObject(gameObject, "ContentStory").GetComponent<RectTransform>();
        rectTransformContentStory2 = Common.SearchGameObject(gameObject, "ContentStory2").GetComponent<RectTransform>();
        rectTransformContentQuestion = Common.SearchGameObject(gameObject, "ContentQuestions").GetComponent<RectTransform>();
        storyText = Common.SearchGameObject(gameObject, "StoryText").GetComponent<Text>();
        storyText2 = Common.SearchGameObject(gameObject, "StoryText2").GetComponent<Text>();
        questionHoldingPrefab = Common.SearchGameObject(gameObject, "QuestionsHolding").gameObject;
        verifyButton = gameObject.SearchChild("VerifyButton").GetComponent<RectTransform>();

        // Gombok szkriptjének beállítása
        foreach (Button button in GetComponentsInChildren<Button>(true))
            button.buttonClick = ButtonClick;
    }

    /// <summary>
    /// Felkészülünk a feladat megmutatására.
    /// </summary>
    /// <returns></returns>
    override public IEnumerator PrepareTask()
    {
        exitButton.SetActive(false); // Common.configurationController.deviceIsServer);

        clock.timeInterval = taskData.time;
        clock.Reset(0);

        canvasGroup.alpha = 0;

        // A gombok szövegeinek nyelvi fordításai
        gameObject.SearchChild("QuestionButtonText").GetComponent<Text>().text = Common.languageController.Translate(C.Texts.Questions);
        gameObject.SearchChild("TaskButtonText").GetComponent<Text>().text = Common.languageController.Translate(C.Texts.Task);
        if (Common.configurationController.answerFeedback == ConfigurationController.AnswerFeedback.NoFeedback)
            gameObject.SearchChild("VerifyButtonText").GetComponent<Text>().text = Common.languageController.Translate(C.Texts.Next);
        if (Common.configurationController.answerFeedback == ConfigurationController.AnswerFeedback.ShelfCheckButton)
            gameObject.SearchChild("VerifyButtonText").GetComponent<Text>().text = Common.languageController.Translate(C.Texts.Verify);

        // A történetet és a címét elhelyezzük a Text componensben
        string text = "\n";

        if (!string.IsNullOrEmpty(taskData.textTitle)) // Ha van cím hozzá adjuk
            text += "<b>" + taskData.textTitle + "</b>\n\n";

        text += "     " + taskData.textContent + "\n";

        storyText.text = text;
        storyText2.text = text;

        // Kérdések létrehozása -------------------------------------------------------------------------------------

        // Az esetlegesen létező előző kérdéseket eltávolítjuk
        Destroy(actQuestionHolding);

        // Készítünk egy másolatot a kérdések tárolására készített gameObject-ből
        actQuestionHolding = Instantiate(questionHoldingPrefab);
        actQuestionHolding.transform.SetParent(questionHoldingPrefab.transform.parent, false);

        float fullWidth = questionHoldingPrefab.GetComponent<LayoutElement>().preferredWidth;
        listOfInputFields = new List<UIRead2InputField>();
        listOfItems = new List<object>();
        for (int i = 0; i < taskData.questions.Count; i++)
        {
            // Feldaraboljuk a kérdést a szóközök mentén
            string[] substrings = taskData.questions[i].question.Split(' ');
            int questionCounter = 0; // Kérdéseket számolja a szövegben, hogy tudjam melyik kérdéshez melyik lehetőségeket kell társítani

            // Végig megyünk a kapott rész stringeken
            foreach (string item in substrings)
            {
                // Ha valamelyik szó ??, akkor az egy kérdést jelent
                if (item == "??")
                {
                    UIRead2InputField button = Instantiate(inputFieldPrefab, actQuestionHolding.transform, false).GetComponent<UIRead2InputField>();
                    button.gameObject.SetActive(true);
                    button.Init(taskData.questions[i].answerGroups[questionCounter], InputFieldAnswer, i, questionCounter, fullWidth);

                    questionCounter++;
                    listOfInputFields.Add(button);

                    listOfItems.Add(button);
                }
                else
                {   // Egyébként létrehozzuk a szót ha nem üres string
                    if (string.IsNullOrEmpty(item)) continue;

                    UIReadTextWord textWord = Instantiate(textWordPrefab, actQuestionHolding.transform, false).GetComponent<UIReadTextWord>();
                    textWord.gameObject.SetActive(true);
                    textWord.Init(item);

                    listOfItems.Add(textWord);
                }

            }

            listOfItems.Add(null);
        }

        ShowStory(true);

        ArrangeItems();
        yield break;
    }

    void InputFieldEndEdit()
    {

    }

//    /// <summary>
//    /// Felkészülünk a feladat megmutatására.
//    /// </summary>
//    /// <returns></returns>
//    override public IEnumerator PrepareTask()
//    {
//        exitButton.SetActive(false); // Common.configurationController.deviceIsServer);
//
//        clock.timeInterval = taskData.time;
//        clock.Reset(0);
//
//        canvasGroup.alpha = 0;
//
//        // A gombok szövegeinek nyelvi fordításai
//        gameObject.SearchChild("QuestionButtonText").GetComponent<Text>().text = Common.languageController.Translate(C.Texts.Questions);
//        gameObject.SearchChild("TaskButtonText").GetComponent<Text>().text = Common.languageController.Translate(C.Texts.Task);
//        gameObject.SearchChild("VerifyButtonText").GetComponent<Text>().text = Common.languageController.Translate(C.Texts.Verify);
//
//        // A történetet és a címét elhelyezzük a Text componensben
//        string text = "\n";
//
//
//        if (!string.IsNullOrEmpty(taskData.textTitle)) // Ha van cím hozzá adjuk
//            text += "<b>" + taskData.textTitle + "</b>\n\n";
//
//        text += "     " + taskData.textContent + "\n";
//
//        storyText.text = text;
//        storyText2.text = text;
//
//        // Kérdések létrehozása -------------------------------------------------------------------------------------
//
//        // Az esetlegesen létező előző kérdéseket eltávolítjuk
//        Destroy(actQuestionHolding);
//
//        // Készítünk egy másolatot a kérdések tárolására készített gameObject-ből
//        actQuestionHolding = Instantiate(questionHoldingPrefab);
//        actQuestionHolding.transform.SetParent(questionHoldingPrefab.transform.parent, false);
//
//        float fullWidth = questionHoldingPrefab.GetComponent<LayoutElement>().preferredWidth;
//
//        float cursorX = 0;
//        float cursorY = -distanceBetweenMargo; // Az y érték lefele csökken, ezért megyünk mínuszba
//
//        listOfButton = new List<UIReadWriteButton>();
//        listOfItems = new List<object>();
//        for (int i = 0; i < taskData.questions.Count; i++)
//        {
//            cursorX = 0; //distanceBetweenMargo;
//
//            // Feldaraboljuk a kérdést a szóközök mentén
//            string[] substrings = taskData.questions[i].question.Split(' ');
//            int questionCounter = 0; // Kérdéseket számolja a szövegben, hogy tudjam melyik kérdéshez melyik lehetőségeket kell társítani
//
//            // Végig megyünk a kapott rész stringeken
//            foreach (string item in substrings)
//            {
//                float gameObjectWidth = 0;
//                GameObject newGameObject = null;
//
//                // Ha valamelyik szó ??, akkor az egy kérdést jelent
//                if (item == "??")
//                {
//                    UIReadWriteButton button = Instantiate(buttonTextPrefab, actQuestionHolding.transform, false).GetComponent<UIReadWriteButton>();
//                    button.gameObject.SetActive(true);
//                    button.Init(taskData.questions[i].answerGroups[questionCounter], ButtonQuestionClick, i, questionCounter);
//
//                    gameObjectWidth = button.GetWidth();
//                    newGameObject = button.gameObject;
//
//                    questionCounter++;
//                    listOfButton.Add(button);
//                    listOfItems.Add(button);
//                }
//                else
//                {   // Egyébként létrehozzuk a szót ha nem üres string
//                    if (string.IsNullOrEmpty(item)) continue;
//
//                    UIReadTextWord textWord = Instantiate(textWordPrefab, actQuestionHolding.transform, false).GetComponent<UIReadTextWord>();
//                    textWord.gameObject.SetActive(true);
//                    textWord.Init(item);
//
//                    gameObjectWidth = textWord.GetWidth();
//                    newGameObject = textWord.gameObject;
//
//                    listOfItems.Add(textWord);
//                }
//
//                // Elhelyezzük az objektumot
//                // Ha nem fér el már az adott sorba, akkor következő sorba lépünk ha már van a sorba legalább egy elem
//                if (cursorX + gameObjectWidth > fullWidth && cursorX > distanceBetweenMargo)
//                {
//                    cursorX = 0;
//                    cursorY += distanceBetweenLines;
//                }
//
//                newGameObject.transform.localPosition = new Vector2(cursorX, cursorY);
//                cursorX += gameObjectWidth + distanceBetweenWords;
//            }
//
//            listOfItems.Add(null);
//            cursorY += distanceBetweenQuestions;
//        }
//
//        // Ha nem azonnali a visszajelzés, akkor egy gombot kell elhelyezni a kérdések alatt
//        if (Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately)
//        {
//            // Az ellenőrző gomb mindig a scrollBox alján legyen, akkor is ha kevés szöveg volt előtte.
//            verifyButton.localPosition = new Vector2(fullWidth / 2, cursorY > -550 ? -550 : cursorY);
//            verifyButton.gameObject.SetActive(true);
//            cursorY -= 50;
//        }
//        else
//        {
//            verifyButton.gameObject.SetActive(false);
//        }
//
//
//        questionHeight = Mathf.Abs(cursorY);
//
//        ShowStory(true);
//
//        selectedButton = null;
//        succesfullTask = 0;
//
//        // Az ellenőrző gomb beállítása, ha szükséges egyáltalán
//
//        yield break;
//    }


    // Igazítja a létrehozott elemeket
    void ArrangeItems()
    {
        float cursorX = 0;
        float cursorY = -distanceBetweenMargo; // Az y érték lefele csökken, ezért megyünk mínuszba
        float fullWidth = questionHoldingPrefab.GetComponent<LayoutElement>().preferredWidth;

        for (int i = 0; i < listOfItems.Count; i++)
        {
            float gameObjectWidth = 0;

            if (listOfItems[i] == null) // A null soremelést jelent
            {
                cursorY += distanceBetweenQuestions;
                cursorX = 0;
            }
            else if (listOfItems[i] is IWidthHeight) // Egyébként valamilyen elemnek kell ott lennie, aminek van szélessége
            {
                gameObjectWidth = (listOfItems[i] as IWidthHeight).GetWidth();

                // Elhelyezzük az objektumot
                // Ha nem fér el már az adott sorba, akkor következő sorba lépünk ha már van a sorba legalább egy elem
                if (cursorX + gameObjectWidth > fullWidth && cursorX > distanceBetweenMargo)
                {
                    cursorX = 0;
                    cursorY += distanceBetweenLines;
                }

                (listOfItems[i] as MonoBehaviour).transform.localPosition = new Vector2(cursorX, cursorY);
                cursorX += gameObjectWidth + distanceBetweenWords;
            }
        }

        // Ha nem azonnali a visszajelzés, akkor egy gombot kell elhelyezni a kérdések alatt
//        if (Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately)
//        {
//            // Az ellenőrző gomb mindig a scrollBox alján legyen, akkor is ha kevés szöveg volt előtte.
//            cursorY -= 50;
//            verifyButton.localPosition = new Vector2(fullWidth / 2, cursorY > -750 ? -750 : cursorY);
//            verifyButton.gameObject.SetActive(true);
//            cursorY -= 100;
//        }
//        else
        {
            verifyButton.gameObject.SetActive(false);
        }

        questionHeight = Mathf.Abs(cursorY);
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát
    override public IEnumerator InitCoroutine()
    {
        status = Status.Init;

        // Lekérdezzük a feladat adatait
        taskData = (TaskRead2Data)Common.taskController.task;

        yield return StartCoroutine(PrepareTask());



        yield return null;
    }

    // Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        // A szöveg előtűnik
        iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", 1, "time", taskData.animSpeed1, "easetype", iTween.EaseType.linear, "onupdate", "FadeCanvasUpdate", "onupdatetarget", gameObject));
        
        yield return new WaitForSeconds(taskData.animSpeed1); // Várunk amíg az animáció befejeződik

        Common.audioController.SetBackgroundMusic(1, 0.05f, 4); // Elindítjuk a háttérzenét

        status = Status.Play;
        Common.HHHnetwork.messageProcessingEnabled = true;

        yield return null;
    }

    // iTween ValueTo call this
    public void FadeCanvasUpdate(float value)
    {
        canvasGroup.alpha = value;
    }

    // A kérdéshez tartozó játék elemeket kell eltávolítani, ha meghívják ezt a metódust. A TaskController fogja új feladatnál ha az új feladatot ugyan annak a képernyőnek kell megjelenítenie
    // Meglehet adni neki egy callBack függvényt, amit akkor hív meg ha végzet a játék elemek elrejtésével, mivel ez sokáig is eltarthat és addig nem kéne tovább menni az új feladatra.
    override public IEnumerator HideGameElement()
    {
        clock.Reset(1); // Az órát alaphelyzetbe állítja

        iTween.ValueTo(gameObject, iTween.Hash("from", 1, "to", 0, "time", taskData.animSpeed1, "easetype", iTween.EaseType.linear, "onupdate", "FadeCanvasUpdate", "onupdatetarget", gameObject));

        yield return new WaitForSeconds(taskData.animSpeed1); // Várunk amíg az animáció befejeződik
    }

    /// <summary>
    /// A tanári tablet óraterv előnézeti képernyője hívja meg ha meg kell mutatni a játék előnézetét.
    /// A task paraméter tartalmazza a játék képernyőjének adatait.
    /// </summary>
    /// <param name="task">A megjelenítendő képernyő adata</param>
    override public IEnumerator Preview(TaskAncestor task)
    {
        taskData = (TaskRead2Data)task;

        yield return StartCoroutine(PrepareTask());

        canvasGroup.alpha = 1;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        rectTransformContentStory.sizeDelta = new Vector2(0, storyText.preferredHeight);
        rectTransformContentStory2.sizeDelta = new Vector2(0, storyText2.preferredHeight);
        rectTransformContentQuestion.sizeDelta = new Vector2(0, Mathf.Max(questionHeight, answerBottomPos));
    }

    // Játéknak vége letelt az idő, vagy a játék befejeződött
    override public IEnumerator GameEnd()
    {
        status = Status.Result;
        //clock.Stop();

        yield return new WaitForSeconds(2);

        // Tájékoztatjuk a feladatkezelőt, hogy vége a játéknak és átadjuk a játékos eredményeit
        Common.taskControllerOld.TaskEnd(null);
    }

    // A menüből kiválasztották a kilépést a játékból
    IEnumerator ExitCoroutine()
    {
        status = Status.Exit;
        clock.Stop();

        Common.taskController.GameExit();
        yield return null;
    }

    /// <summary>
    /// Beállíthatjuk, hogy a történetet akarjuk látni vagy a kérdéseket.
    /// Ha a Show változó true, akkor a történetet.
    /// </summary>
    /// <param name="show">Ha true, akkor a történet lesz látható egyébként a kérdések.</param>
    void ShowStory(bool show)
    {
        // Beállítjuk melyik tartalom legyen látható
        storyShow.SetActive(show);
        questionsShow.SetActive(!show);

        //storyShowed = show;
    }

    /// <summary>
    /// Létrehozza a buttonID változóban megadott gomb válasz lehetőségeit.
    /// </summary>
    /// <param name="buttonID"></param>
//    void ShowAnswer(int buttonID) {
//
//        UIReadButton questionButton = listOfButton[buttonID];
//
//        questionButton.Selected(true);
//
//        DeleteAnswer();
//        listOfAnswers = new List<UIReadButton>();
//
//        float fullWidth = questionHoldingPrefab.GetComponent<LayoutElement>().preferredWidth;
//        float questionButtonCenter = questionButton.transform.localPosition.x + questionButton.GetWidth() / 2;
//        float cursorX = 0;
//        float cursorY = questionButton.transform.localPosition.y + distanceBetweenAnswers;
//        int firstButtonIndexActualLine = 0;
//        foreach (string answer in questionButton.GetAnswers())
//        {
//            UIReadButton answerButton = Instantiate(buttonTextPrefab, actQuestionHolding.transform, false).GetComponent<UIReadButton>();
//            answerButton.gameObject.SetActive(true);
//            //answerButton.transform.SetParent(actQuestionHolding.transform, false);
//            answerButton.Init(answer, ButtonAnswerClick);
//
//            // Elhelyezzük az új válasz gombot
//            // Ha nem fér el már az adott sorba, akkor következő sorba lépünk
//            if (cursorX + answerButton.GetWidth() > fullWidth && cursorX > 0)
//            {
//                AdjustAnswerButtons(questionButtonCenter, cursorX, fullWidth, firstButtonIndexActualLine, listOfAnswers);
//
//                cursorX = 0;
//                cursorY += distanceBetweenAnswers;
//
//                firstButtonIndexActualLine = listOfAnswers.Count;
//            }
//
//            answerButton.transform.localPosition = new Vector2(cursorX, cursorY);
//            cursorX += answerButton.GetWidth() + distanceBetweenWords;
//
//            listOfAnswers.Add(answerButton);
//            //SetPicturesOnReadButton(answerButton);
//        }
//
//        AdjustAnswerButtons(questionButtonCenter, cursorX, fullWidth, firstButtonIndexActualLine, listOfAnswers);
//
//        answerBottomPos = Mathf.Abs(cursorY + distanceBetweenAnswers - distanceBetweenMargo);
//
//        selectedButton = questionButton;
//    }

    /// <summary>
    /// A létrehozott válasz gombokat a kérdés gomb alá igazítja.
    /// </summary>
    /// <param name="questionButtonCenter">Hol van a kérdés gomb közepe.</param>
    /// <param name="width">A létrehozott válasz gomb sor milyen széles.</param>
    /// <param name="fullWidth">Mennyi a rendelkezésre álló hely teljes szélessége.</param>
    /// <param name="firstButtonIndexActualLine">Mi az indexe az aktuális sor első válasz gombjának.</param>
    /// <param name="listOfAnswers">A létrehozott válasz gombok.</param>
//    void AdjustAnswerButtons(float questionButtonCenter, float width, float fullWidth, int firstButtonIndexActualLine, List<UIReadButton> listOfAnswers) {
//        // A maradék helyett elosztjuk, és a válaszokat a kérdés alá igazítjuk
//        if (width > 0)
//            width -= distanceBetweenWords;
//
//        // Hol kell lennie az első válasznak
//        float basePosX = questionButtonCenter - width / 2;
//
//        if (basePosX < 0)
//            basePosX = 0;
//
//        if (basePosX + width > fullWidth)
//            basePosX = fullWidth - width;
//
//        // Végig megyünk az aktuális sorban létrehozott gombokon és igazítjuk az X koordinátáját
//        for (int i = firstButtonIndexActualLine; i < listOfAnswers.Count; i++)
//        {
//            Vector2 pos = listOfAnswers[i].transform.localPosition;
//            listOfAnswers[i].transform.localPosition = new Vector2(pos.x + basePosX, pos.y);
//        }
//    }

    /// <summary>
    /// Kiértékeli a program a megadott választ.
    /// </summary>
    /// <param name="answer">A kérdésre a válasz.</param>
    IEnumerator EvaluateAnswerCoroutine(JSONNode jsonData)
    {
        // Megkeressük a villogtatni kívánt inputField-et
        UIRead2InputField inputField = null;
        int questionIndex = jsonData[C.JSONKeys.selectedQuestion].AsInt;
        int subQuestionIndex = jsonData[C.JSONKeys.selectedSubQuestion].AsInt;

        for (int i = 0; i < listOfInputFields.Count; i++)
            if (listOfInputFields[i].questionIndex == questionIndex &&
                listOfInputFields[i].subQuestionIndex == subQuestionIndex)
            {
                inputField = listOfInputFields[i];
                break;
            }

        if (Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately)
        {
            // Ha beírtunk valamit egy mezőbe, akkor már nem lehet megváltoztatni, akkor sem ha később rájövünk, hogy rossz
            inputField.Interactable(false); 
        }
        else
        {
            switch (jsonData[C.JSONKeys.evaluateAnswer].Value)
            {
                case C.JSONValues.evaluateIsTrue:
                    // Helyes a válasz
                    Common.audioController.SFXPlay("positive");
                    inputField.Flashing(true);
                    //yield return new WaitForSeconds(selectedButton.Flashing(true));

                    break;

                case C.JSONValues.evaluateIsFalse:
                    // A válasz helytelen
                    Common.audioController.SFXPlay("negative");
                    inputField.Flashing(false);
                    break;

                case C.JSONValues.evaluateIsSilent:

                    break;
            }
        }

        yield return null;
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
                    StartCoroutine(EvaluateAnswerCoroutine(jsonNodeMessage));

                    break;

                case C.JSONValues.nextPlayer:
                    status = Status.Play;

                    break;
            }
        }
    }

//
//    // Rákattintottak egy kérdésre
//    void ButtonQuestionClick(Object o)
//    {
//        if (userInputIsEnabled)
//        {
//            UIReadWriteButton button = (UIReadWriteButton)o;
//
//            // Megnyitjuk a készülék billentyűzetét
//            TouchScreenKeyboard deviceKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, true);
//
//            /*
//            if (selectedButton == null)
//            {
//                ShowAnswer(listOfButton.IndexOf(button));
//            }
//            else if (selectedButton == button)
//            {
//                selectedButton.Selected(false);
//                selectedButton = null;
//                DeleteAnswer();
//            }
//            */
//        }
//    }

    // Megadtak egy választ, de lehet, hogy csak igazítani kell
    void InputFieldAnswer(Object o)
    {
        if (userInputIsEnabled)
        {
            UIRead2InputField button = (UIRead2InputField)o;

            if (button.wasAnswer) // Ha válasz volt akkor kiértékeljük azt
            {
                // Ha játékmódban vagyunk, akkor elküldjük a játékos választását
                JSONClass jsonClass = new JSONClass();
                jsonClass[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
                jsonClass[C.JSONKeys.gameEventType] = C.JSONValues.answer;
                jsonClass[C.JSONKeys.selectedQuestion].AsInt = button.questionIndex;
                jsonClass[C.JSONKeys.selectedSubQuestion].AsInt = button.subQuestionIndex;
                jsonClass[C.JSONKeys.selectedAnswer] = button.GetText();

                Common.taskController.SendMessageToServer(jsonClass);
            }
        }

        ArrangeItems();
    }

    public void UIButtonClick(string button)
    {
        if (userInputIsEnabled)
        {
            switch (button)
            {
                case "GoQuestions":
                    ShowStory(false);
                    break;

                case "GoStory":
                    ShowStory(true);
                    break;

                case "SelfVerify": // ön ellenőrzés vagy következő tanegység
                    if (Common.configurationController.answerFeedback == ConfigurationController.AnswerFeedback.NoFeedback)
                    {
                        // Következő tanegység
                        GameMenu.instance.ButtonClick(C.Program.GameMenuNext);
                    }
                    else
                    {
                        // Önellenőrzés
                        // ??? Hogyan?
                    }
                    break;
            }
        }
    }

    // Ha rákattintottak egy gombra, akkor meghívódik ez az eljárás a Button szkript által
    // Egyenlőre csak az exit gombot tartalmazza amit csak a tanári tabletten használunk mikor megnézi az óramozaikot
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
}

