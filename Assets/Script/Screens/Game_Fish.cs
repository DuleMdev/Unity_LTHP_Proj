using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.UI;

public class Game_Fish : GameDragAncestor { 

    [Tooltip("A kérdés prefabja")]
    GameObject questionPrefab;
    [Tooltip("A mozgatható hal prefabja")]
    GameObject fishPrefab;

    Transform questionScale;
    Text textScreenQuestion;
    TEXDraw texDrawScreenQuestion;

    GameObject topLeft;
    GameObject bottomRight;
    GameObject questionAreaCenter;  // A kérdéseknek hol kell megjelenniük
    GameObject questionAreaBottomLeft;    // Honnan kezdődhetnek a kérdések (Ha a kérdések nem férnek el a questionCenter és a questionLeft távolságának kétszeresen, akkor skálázni kell)

    GameObject act1QuestionRoot;    // Az aktuális első kérdés root gameObject-je
    GameObject act2QuestionRoot;    // Az aktuális második kérdés root gameObject-je
    GameObject old1QuestionRoot;    // A régi első kérdés root gameObject-je
    GameObject old2QuestionRoot;    // A régi második kérdés root gameObject-je

    GameObject fishAreaMove;        // A halakat tartalmazó gameObject

    GameObject fishRoot;            // A halakat tartalmazó root gameObject-je
    GameObject oldFishRoot;         // Kérdés cserénél a régi halakat tartalmazó root

    List<TextMeshWithDragTarget> questions;  // A kérdések gameObjectje Maximum két kérdés lehet
    List<FishDrag> listOfFishDrag;  // A válasz halakat tartalmazó tömb

    //DragAndDropControl dragAndDropControl;

    TaskFishData taskData;          // A feladatot tartalmazó objektum

    int taskNumber;                 // A feladatok száma
    int succesfullTask;             // A jó helyre húzott elemek száma

    Arranger arranger;              // 

    override public void Awake() {
        base.Awake();

        questionPrefab = gameObject.SearchChild("TextMeshWithRectangleDragTarget").gameObject;
        fishPrefab = gameObject.SearchChild("FishPrefab").gameObject;

        // DragAndDropControl feedBack eseményeinek beállítása
        //dragAndDropControl = gameObject.GetComponent<DragAndDropControl>();
        dragAndDropControl.itemReleased = (DragItem dragItem) => {
            // Ha a elmozgatott elem pozíciója a uszóterületben van, akkor itt legyen az új báizs pozíció
            if (dragItem.MoveTransform.position.x > topLeft.transform.position.x &&
                dragItem.MoveTransform.position.x < bottomRight.transform.position.x &&
                dragItem.MoveTransform.position.y < topLeft.transform.position.y &&
                dragItem.MoveTransform.position.y > bottomRight.transform.position.y)
            {
                dragItem.BaseTransform.position = dragItem.MoveTransform.position;
                dragItem.MoveTransform.localPosition = Vector3.zero;
                dragItem.MoveBasePosEnd();
            }
            else
            {
                dragItem.MoveBasePos(0);
            }
        };                  // Az elemet elengedtük nem egy célpont felett
        dragAndDropControl.itemReleasedOverADragTarget = (DragItem dragItem, DragTarget dragTarget) => { };    // Elengedtük az elemet egy célpont felett, még nem tudjuk, hogy jó helyen vagy rossz helyen
        dragAndDropControl.itemPutWrongPlace = (DragItem dragItem, DragTarget dragTarget) => { dragItem.MoveBasePos(); };               // Az elemet rossz helyre helyeztük
        dragAndDropControl.itemPutGoodPlace =
            (DragItem dragItem, DragTarget dragTarget) => {

                Vector3 tempScale = act1QuestionRoot.transform.localScale;
                act1QuestionRoot.transform.localScale = Vector3.one;
                arranger.Arrange((questionAreaCenter.transform.position - questionAreaBottomLeft.transform.position) * 2, true);
                act1QuestionRoot.transform.localScale = tempScale;

                iTween.ScaleTo(act1QuestionRoot, iTween.Hash("scale", Vector3.one * arranger.ratio, "islocal", true, "easetype", iTween.EaseType.easeOutCubic, "time", 1));


                //act1QuestionRoot.transform.localScale = Vector3.one * arranger.ratio;

                /*
                succesfullTask++;
                if (succesfullTask == taskNumber)
                    StartCoroutine(GameEnd());
                    */
            };                // Az elemet jó helyre helyeztük

        questionScale = gameObject.SearchChild("QuestionScale").transform;
        textScreenQuestion = gameObject.SearchChild("textQuestion").GetComponent<Text>();
        texDrawScreenQuestion = gameObject.SearchChild("TEXDrawQuestion").GetComponent<TEXDraw>();

        fishAreaMove = Common.SearchGameObject(gameObject, "FishAreaMove");
        questionAreaCenter = Common.SearchGameObject(gameObject, "QuestionAreaCenter");
        questionAreaBottomLeft = Common.SearchGameObject(gameObject, "QuestionAreaBottomLeft");
        topLeft = Common.SearchGameObject(gameObject, "TopLeft");
        bottomRight = Common.SearchGameObject(gameObject, "BottomRight");

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
        exitButton.SetActive(false); // Common.configurationController.deviceIsServer);

        clock.timeInterval = taskData.time;
        clock.Reset(0);

        //questionsCenter.transform.localScale = Vector3.one;

        // Kiírjuk a kérdést
        textScreenQuestion.text = taskData.screenQuestion;
        texDrawScreenQuestion.text = taskData.screenQuestion;
        textScreenQuestion.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text;
        texDrawScreenQuestion.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.TEXDraw;

       questionScale.localScale = Vector3.one * 0.001f;

        // Kérdések létrehozása ----------------------------------------------------------------
        // Létrehozzuk a kérdéseket tartalmazó bázis gameObject-eket
        act1QuestionRoot = new GameObject();
        act1QuestionRoot.name = "questionRoot1";
        act1QuestionRoot.transform.parent = questionAreaCenter.transform;
        act1QuestionRoot.transform.localPosition = Vector3.zero;
        act1QuestionRoot.transform.localScale = Vector3.one;

        act2QuestionRoot = new GameObject();
        act2QuestionRoot.name = "questionRoot2";
        act2QuestionRoot.transform.parent = questionAreaCenter.transform;
        act2QuestionRoot.transform.position = new Vector3(questionAreaCenter.transform.position.x, questionAreaBottomLeft.transform.position.y);
        act2QuestionRoot.transform.localScale = Vector3.one;

        // Létrehozzuk a DragTarget-ek tárolására használt listát
        List<DragTarget> listOfDragTarget = new List<DragTarget>();

        // Létrehozzuk az összes létrehozott objektumok tárolására a listát
        List<MonoBehaviour> listOfAllItems = new List<MonoBehaviour>();

        // Létrehozzuk a kérdéseket
        questions = new List<TextMeshWithDragTarget>();

        // Létrehozzuk az első kérdést
        TextMeshWithDragTarget textMeshWithDragTarget = Instantiate(questionPrefab).GetComponent<TextMeshWithDragTarget>();
        textMeshWithDragTarget.transform.parent = act1QuestionRoot.transform;
        textMeshWithDragTarget.transform.localPosition = Vector3.zero;
        textMeshWithDragTarget.transform.localScale = Vector3.one;
        textMeshWithDragTarget.Initialize(taskData.questions[0], 0);
        questions.Add(textMeshWithDragTarget);
        float question1Length = textMeshWithDragTarget.GetWidth();
        taskNumber = textMeshWithDragTarget.subQuestionNumber;

        // Összeszedjük a DragTarget-eket
        listOfDragTarget.AddRange(textMeshWithDragTarget.listOfDragTarget);

        // Összeszedjük az összes létrehozott elemet
        listOfAllItems.AddRange(textMeshWithDragTarget.listItems);

        // Létrehozzuk a második kérdést
        textMeshWithDragTarget = Instantiate(questionPrefab).GetComponent<TextMeshWithDragTarget>();
        textMeshWithDragTarget.transform.parent = act1QuestionRoot.transform;
        textMeshWithDragTarget.transform.localPosition = Vector3.zero;
        textMeshWithDragTarget.transform.localScale = Vector3.one;
        textMeshWithDragTarget.Initialize((taskData.questions.Count > 1) ? taskData.questions[1] : new TextMeshWithDragTargetData(""), 1); // Ha nincs második kérdés, akkor egy üres stringgel inicializáljuk
        questions.Add(textMeshWithDragTarget);
        float question2Length = textMeshWithDragTarget.GetWidth();
        taskNumber += textMeshWithDragTarget.subQuestionNumber;

        // Összeszedjük a DragTarget-eket
        listOfDragTarget.AddRange(textMeshWithDragTarget.listOfDragTarget);
        // és átadjuk őket a dragAndDropControl-nak
        dragAndDropControl.ListOfDragTarget = listOfDragTarget;

        // Összeszedjük az összes létrehozott elemet
        listOfAllItems.Add(null); // A két kérdés elválasztására berakunk egy null elemet
        listOfAllItems.AddRange(textMeshWithDragTarget.listItems);

        // Kiszámoljuk az elemek pozícióját és igazítjuk
        arranger = new Arranger(listOfAllItems, 0.05f, 0.05f);
        arranger.Arrange((questionAreaCenter.transform.position - questionAreaBottomLeft.transform.position) * 2, false);

        // Méretezzük a tartalmazó objektumot
        //questionsCenter.transform.localScale = Vector3.one * arranger.ratio;
        act1QuestionRoot.transform.localScale = Vector3.one * arranger.ratio;






        /*
        // Megvizsgáljuk a kérdések hosszát, ha túl hosszúak, akkor zsugorítjuk őket
        float longestQuestion = Mathf.Max(question1Length, question2Length);
        float maxLength = (questionsCenter.transform.position.x - questionLeft.transform.position.x) * 2;

        // Ha a kérdés túl hosszú, akkor skálázzuk a questionCenter objektumot
        if (longestQuestion > maxLength)
        {
            act1QuestionRoot.transform.localScale = Vector3.one * maxLength / longestQuestion;
            act2QuestionRoot.transform.localScale = Vector3.one * maxLength / longestQuestion;
        }
        */
        // A kérdéseket eltüntetjük a képernyőről
        act1QuestionRoot.transform.position = act1QuestionRoot.transform.position.SetX(Camera.main.aspect * -2);
        act2QuestionRoot.transform.position = act2QuestionRoot.transform.position.SetX(Camera.main.aspect * 2);
        //act1QuestionRoot.transform.position = new Vector3(Camera.main.aspect * -2, act1QuestionRoot.transform.position.y, act1QuestionRoot.transform.position.z);
        //act2QuestionRoot.transform.position = new Vector3(Camera.main.aspect * 2, act2QuestionRoot.transform.position.y, act2QuestionRoot.transform.position.z);

        // Halak létrehozása -------------------------------------------------------------------
        // Létrehozzuk a halakat tartalmazó bázis gameObject-et
        fishRoot = new GameObject();
        fishRoot.name = "fishAreaRoot";
        fishRoot.transform.parent = fishAreaMove.transform;
        fishRoot.transform.localPosition = Vector3.zero;
        fishRoot.transform.localScale = Vector3.one;

        // Létrehozzuk a halakat
        listOfFishDrag = new List<FishDrag>();
        List<string> answers = taskData.GetAnswers();
        for (int i = 0; i < answers.Count; i++)
        {
            FishDrag fishDrag = Instantiate(fishPrefab).GetComponent<FishDrag>();
            //fishDrag.transform.parent = fishRoot.transform;
            fishDrag.transform.SetParent(fishRoot.transform, false);
            fishDrag.Initialize(i % fishDrag.fishNumber, answers[i]);
            fishDrag.answerIndex = i;
            listOfFishDrag.Add(fishDrag);

            // Beállítjuk a halak pozícióját
            float width = bottomRight.transform.position.x - topLeft.transform.position.x;
            float height = topLeft.transform.position.y - bottomRight.transform.position.y;
            fishDrag.transform.position =
                new Vector3(
                    (float)(Common.random.NextDouble() * width + topLeft.transform.position.x),
                    (float)(Common.random.NextDouble() * height + bottomRight.transform.position.y)
                );
        }

        // Kipozícionáljuk a fishAreát a képernyőből
        fishAreaMove.transform.position = new Vector3(Camera.main.aspect * -3 * fishAreaMove.transform.lossyScale.x, fishAreaMove.transform.position.y, fishAreaMove.transform.position.z);

        yield break;
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        status = Status.Init;

        // Lekérdezzük a feladat adatait
        taskData = (TaskFishData)Common.taskController.task;

        yield return StartCoroutine(PrepareTask());

        succesfullTask = 0;

        yield return null;
    }

    // Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        // Ha van korábbi halak, akkor azok kiúsznak
        if (oldFishRoot != null)
        {
            // Jobbra fordítjuk őket
            foreach (FishDrag fishDrag in oldFishRoot.GetComponentsInChildren<FishDrag>())
                fishDrag.Enabled(false);

            iTween.MoveTo(oldFishRoot, iTween.Hash("x", Camera.main.aspect * 2 * oldFishRoot.transform.lossyScale.x, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutCirc));

            yield return new WaitForSeconds(taskData.animSpeed1); // Várunk amíg az animáció befejeződik

            Destroy(oldFishRoot);
        }

        // Megjelenítjük a kérdés szövegét ha van
        if (!string.IsNullOrEmpty(taskData.screenQuestion))
        {
            iTween.ScaleTo(questionScale.gameObject, iTween.Hash("islocal", true, "scale", Vector3.one, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutElastic));
            Common.audioController.SFXPlay("boing");
            yield return new WaitForSeconds(taskData.animSpeed1);
        }

        oldFishRoot = fishRoot;

        // Bejönnek az új kérdések miközben a régi kimegy ha volt régi
        iTween.MoveTo(act1QuestionRoot, iTween.Hash("islocal", true, "x", 0, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutBack));
        iTween.MoveTo(act2QuestionRoot, iTween.Hash("islocal", true, "x", 0, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutBack));
        if (old1QuestionRoot != null)
            iTween.MoveTo(old1QuestionRoot, iTween.Hash("x", Camera.main.aspect * 2, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutBack));
        if (old2QuestionRoot != null)
            iTween.MoveTo(old2QuestionRoot, iTween.Hash("x", Camera.main.aspect * -2, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutBack));

        // Miután ez megtörtént a régi kérdést töröljük
        yield return new WaitForSeconds(taskData.animSpeed1);
        Destroy(old1QuestionRoot);
        Destroy(old2QuestionRoot);
        old1QuestionRoot = act1QuestionRoot;
        old2QuestionRoot = act2QuestionRoot;

        // Beúsznak az új halak
        fishAreaMove.SetActive(true);
        iTween.MoveTo(fishAreaMove, iTween.Hash("islocal", true, "position", Vector3.zero, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutCirc));

        yield return new WaitForSeconds(taskData.animSpeed1); // Várunk amíg az animáció befejeződik

        // Ha beusztak a halak, akkor áttesszük őket a backgroundra
        fishRoot.transform.parent = background.transform;

        Common.audioController.SetBackgroundMusic(1, 0.05f, 4); // Elindítjuk a háttérzenét

        status = Status.Play;
        Common.HHHnetwork.messageProcessingEnabled = true;
    }



    // A képernyő teljesen eltünk. Csinálhatunk valamit ilyenkor ha szükséges
    // De figyelni kell, mivel a következő pillanatban már inaktív lesz az egész képernyő. 
    // Tehát azonnal meg kell tennünk amit akarunk nem indíthatunk a képernyőn egy coroutine-t mivel úgy sem fog lefutni.
    // Kikapcsolt gameObject-eken nem fut a coroutine.
    // Ezt a ScreenController hívja meg képernyő váltásnál
    override public IEnumerator ScreenHideFinish()
    {
        StartCoroutine(base.ScreenHideFinish());

        // Eltüntetjük a kérdéseket és a halakat
        if (old1QuestionRoot != null)
            Destroy(old1QuestionRoot);
        if (old2QuestionRoot != null)
            Destroy(old2QuestionRoot);
        if (oldFishRoot != null)
            Destroy(oldFishRoot);

        yield return null;
    }

    // Update is called once per frame
    /*
    void Update()
    {
        menu.menuEnabled = (status == Status.Play);
        dragAndDropControl.dragAndDropEnabled = (status == Status.Play);
        if (status == Status.Play)
            clock.Go();
        else
            clock.Stop();
    }
    */

    // Játéknak vége letelt az idő, vagy a játék befejeződött
    override public IEnumerator GameEnd()
    {
        status = Status.Result;
        //clock.Stop();

        yield return new WaitForSeconds(2);

        // Tájékoztatjuk a feladatkezelőt, hogy vége a játéknak és átadjuk a játékos eredményeit
        Common.taskControllerOld.TaskEnd(null);
    }

    /// <summary>
    /// A tanári tablet óraterv előnézeti képernyője hívja meg ha meg kell mutatni a játék előnézetét.
    /// A task paraméter tartalmazza a játék képernyőjének adatait.
    /// </summary>
    /// <param name="task">A megjelenítendő képernyő adata</param>
    override public IEnumerator Preview(TaskAncestor task)
    {
        taskData = (TaskFishData)task;

        yield return StartCoroutine(PrepareTask());

        // Megjelenítjük a kérdés szövegét
        questionScale.localScale = Vector3.one;

        // Ha van korábbi halak, akkor törlöm őket azonnal
        if (oldFishRoot != null)
            Destroy(oldFishRoot);

        // Láthatóvá teszem az új kérdéseket
        act1QuestionRoot.transform.localPosition = act1QuestionRoot.transform.localPosition.SetX(0);
        act2QuestionRoot.transform.localPosition = act2QuestionRoot.transform.localPosition.SetX(0);

        // A régi kérdéseket töröljük
        Destroy(old1QuestionRoot);
        Destroy(old2QuestionRoot);
        old1QuestionRoot = act1QuestionRoot;
        old2QuestionRoot = act2QuestionRoot;

        // Beúsznak az új halak
        fishAreaMove.SetActive(true);
        fishAreaMove.transform.localPosition = Vector3.zero;

        // Ha beusztak a halak, akkor áttesszük őket a backgroundra
        fishRoot.transform.parent = background.transform;
        oldFishRoot = fishRoot;
    }

    /*
    IEnumerator ExitCoroutine()
    {
        status = Status.Exit;
        clock.Stop();

        Common.taskController.GameExit();
        yield break;
    }
    */

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
            if (jsonNodeMessage[C.JSONKeys.dataContent].Value == C.JSONValues.gameEvent)
            {
                // Megkeressük a json-ban található DragTarget és a DragItem objektumokat
                DragTarget dragTarget = null;
                //if (jsonNodeMessage.ContainsKey(C.JSONKeys.selectedQuestion))
                //{
                int questionIndex = jsonNodeMessage[C.JSONKeys.selectedQuestion].AsInt;
                int subQuestionIndex = jsonNodeMessage[C.JSONKeys.selectedSubQuestion].AsInt;
                dragTarget = questions[questionIndex].listOfDragTarget[subQuestionIndex];
                //}

                DragItem dragItem = null;

                int answerIndex = jsonNodeMessage[C.JSONKeys.selectedAnswer].AsInt;

                foreach (FishDrag fishDrag in listOfFishDrag)
                {
                    if (fishDrag.answerIndex == answerIndex)
                        dragItem = fishDrag;
                }
                //dragItem = bubbleList[answerIndex];

                dragAndDropControl.MessageArrived(dragTarget, dragItem, jsonNodeMessage);

                //Ha válasz történt leállítjuk az idő számlálást
                if (jsonNodeMessage[C.JSONKeys.gameEventType].Value == C.JSONValues.answer)
                {
                    status = Status.Result;
                }
            }

            /*
            switch (jsonNodeMessage[C.JSONKeys.gameEventType])
            {
                case C.

                case C.JSONValues.answer:
                    status = Status.Result;
                    StartCoroutine(EvaluateCoroutine(jsonNodeMessage));

                    break;

                case C.JSONValues.nextPlayer:
                    status = Status.Play;

                    break;
            }
            */
        }
    }

    // Ha rákattintottak a buborékra, akkor meghívódik ez az eljárás a buborékon levő Button szkript által
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
}
