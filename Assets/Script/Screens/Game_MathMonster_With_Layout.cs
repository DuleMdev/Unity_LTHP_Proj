using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.UI;

public class Game_MathMonster_With_Layout : GameDragAncestor {

    [Tooltip("A kérdés prefabja")]
    public GameObject questionPrefab;
    [Tooltip("A mozgatható elem prefabja")]
    public GameObject bubbleGrabPrefab;
    [Tooltip("Kérdések közti távolság")]
    public float distanceBetweenQuestion;

    LayoutManager layoutManager;    // A különböző layoutokhoz tartozó képeket tartalmazza

    SpriteRenderer foreground;

    //public float animSpeed = 1f;

    Text textScreenQuestion;

    SpriteRenderer monsterMove;         // A szörnyet mozgató GameObject
    GameObject questionsAreaCenter;     // A kérdéseknek hol kell megjelenniük
    GameObject questionAreaBottomLeft;  // Honnan kezdődhetnek a kérdések (Ha a kérdések nem férnek el a questionCenter és a questionLeft távolságának kétszeresen, akkor skálázni kell)

    GameObject actQuestionRoot;         // Az aktuális kérdéseket tartalmazó gameObject
    GameObject oldQuestionRoot;         // Az előző kérdéseket tartalmazó gameObject

    List<MonoBehaviour> questions;  // A kérdések gameObjectje Maximum két kérdés lehet

    List<Transform> bubblePosList;  // A buborékok pozícióját mutató Transform komponensek
    List<BubbleDrag> bubbleList;    // A létrehozott buborékok

    GameObject bubblesRoot;         // A létrehozott buborékokat tartalmazó GameObject

    //DragAndDropControl dragAndDropControl;

    TaskMathMonsterData taskData;   // A feladat adatait tartalmazó objektum

    float monsterOutPos = -3.5f;    // A szörny lokális y pozíciója mikor nem látszik a képernyőn

    int taskNumber;                 // A feladatok száma
    int succesfullTask;             // A jól megoldott feladatok száma

    Arranger arranger;              // 

    override public void Awake()
    {
        base.Awake();

        layoutManager = GetComponentInChildren<LayoutManager>();

        foreground = transform.Find("background/foreground").GetComponent<SpriteRenderer>();

        monsterMove = Common.SearchGameObject(gameObject, "MonsterMove").GetComponent<SpriteRenderer>();

        // DragAndDropControl feedBack eseményeinek beállítása
        //dragAndDropControl = gameObject.GetComponent<DragAndDropControl>();
        dragAndDropControl.itemReleased = (DragItem dragItem) => { dragItem.MoveBasePos(0); };                  // Az elemet elengedtük nem egy célpont felett
        dragAndDropControl.itemReleasedOverADragTarget = (DragItem dragItem, DragTarget dragTarget) => { };    // Elengedtük az elemet egy célpont felett, még nem tudjuk, hogy jó helyen vagy rossz helyen
        dragAndDropControl.itemPutWrongPlace = (DragItem dragItem, DragTarget dragTarget) => { dragItem.MoveBasePos(); };               // Az elemet rossz helyre helyeztük
        dragAndDropControl.itemPutGoodPlace = 
            (DragItem dragItem, DragTarget dragTarget) => {
                /*
            succesfullTask++;
            if (succesfullTask == taskNumber) // if (succesfullTask == taskData.questions.Count)
                StartCoroutine(GameEnd());
                */
        };                // Az elemet jó helyre helyeztük

        textScreenQuestion = gameObject.SearchChild("textQuestion").GetComponent<Text>();

        // Összeszedjük a buborékok pozícióit
        bubblePosList = new List<Transform>();
        int index = 1;
        while (true)
        {
            GameObject go = Common.SearchGameObject(gameObject, "Pos" + index);
            if (go == null) break;
            bubblePosList.Add(go.transform);
            index++;
        }

        // Kikapcsoljuk a szörnyet
        monsterMove.gameObject.SetActive(false);

        questionsAreaCenter = Common.SearchGameObject(gameObject, "QuestionAreaCenter").gameObject;
        questionAreaBottomLeft = Common.SearchGameObject(gameObject, "QuestionAreaBottomLeft").gameObject;

        // Gombok szkriptjének beállítása
        foreach (Button button in GetComponentsInChildren<Button>())
        {
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

        // Kiírjuk a kérdést
        textScreenQuestion.text = taskData.taskDescription;
        textScreenQuestion.transform.localScale = Vector3.one * 0.001f;

        // Töröljük az esetleg létező buborékokat
        if (bubblesRoot != null)
            Destroy(bubblesRoot);

        // Létrehozzuk a buborékokat tartalmazó gameObject-et
        bubblesRoot = new GameObject();
        bubblesRoot.name = "BubblesRoot";
        bubblesRoot.transform.parent = monsterMove.transform;
        bubblesRoot.transform.localScale = Vector3.one;
        bubblesRoot.transform.localPosition = Vector3.zero;

        // Létrehozzuk a buborékokat
        bubbleList = new List<BubbleDrag>();
        int bubbleIndex = 0;
        foreach (string itemName in taskData.GetAnswersShuffle())
        {
            GameObject bubble = Instantiate(bubbleGrabPrefab);
            bubble.transform.parent = bubblesRoot.transform;
            bubble.transform.position = bubblePosList[bubbleIndex].position;
            bubble.transform.localScale = Vector3.one;

            BubbleDrag bubbleDrag = bubble.GetComponentInChildren<BubbleDrag>();

            Color newColor = new Color(
                (float)(Common.random.NextDouble() * 0.6 + 0.2),
                (float)(Common.random.NextDouble() * 0.6 + 0.2),
                (float)(Common.random.NextDouble() * 0.6 + 0.2),
                1f
            );

            bubbleDrag.Initialize(itemName, newColor, taskData.answerOrder[bubbleIndex]);

            bubbleDrag.SetPicture(
                layoutManager.GetSprite("bubble"),
                layoutManager.GetSprite("gleam"), layoutManager.GetColor("gleam")
                );

            bubbleList.Add(bubbleDrag); // bubble.GetComponentInChildren<BubbleDrag>());

            bubbleIndex++;
        }

        // A szörnyet kipozícionáljuk a képenyőből.
        monsterMove.transform.localPosition = new Vector3(0, monsterOutPos, 0);

        // Létrehozzuk a kérdéseket tartalmazó gameObject-et
        actQuestionRoot = new GameObject();
        actQuestionRoot.name = "questionRoot";
        actQuestionRoot.transform.parent = questionsAreaCenter.transform;
        actQuestionRoot.transform.localPosition = Vector3.zero;
        actQuestionRoot.transform.localScale = Vector3.one;

        // Létrehozzuk a kérdéseket ??????????????????????????????????
        //questionsCenter.transform.localScale = Vector3.one; // Nem erre tesszük a kérdéseket, hanem egy külön létrehozott gameObjectre

        // Létrehozzuk a DragTarget-ek tárolására használt listát
        List<DragTarget> listOfDragTarget = new List<DragTarget>();

        // Létrehozzuk az összes létrehozott objektumok tárolására a listát
        List<MonoBehaviour> listOfAllItems = new List<MonoBehaviour>();

        taskNumber = 0;
        questions = new List<MonoBehaviour>();
        for (int i = 0; i < taskData.questions.Count; i++)
        {
            TextMeshWithDragTarget textMeshWithDragTarget = Instantiate(questionPrefab).GetComponent<TextMeshWithDragTarget>();
            textMeshWithDragTarget.transform.parent = actQuestionRoot.transform;
            textMeshWithDragTarget.transform.localPosition = Vector3.zero;
            textMeshWithDragTarget.transform.localScale = Vector3.one;
            //textMeshWithDragTarget.transform.localScale = Vector3.one;

            textMeshWithDragTarget.Initialize(taskData.questions[i], i, layoutManager.GetSprite("answer"));
            taskNumber += textMeshWithDragTarget.subQuestionNumber;

            questions.Add((MonoBehaviour)textMeshWithDragTarget);

            // Összeszedjük a DragTarget-eket
            listOfDragTarget.AddRange(textMeshWithDragTarget.listOfDragTarget);

            // Összeszedjük az összes létrehozott elemet
            listOfAllItems.AddRange(textMeshWithDragTarget.listItems);
        }

        // Az összeszedett DragTarget-eket átadjuk a dragAndDropControl-nak
        dragAndDropControl.ListOfDragTarget = listOfDragTarget;

        // Kiszámoljuk az elemek pozícióját és igazítjuk
        arranger = new Arranger(listOfAllItems, 0.05f, 0.05f);
        arranger.Arrange((questionsAreaCenter.transform.position - questionAreaBottomLeft.transform.position) * 2, false);

        // Méretezzük a tartalmazó objektumot
        //questionsCenter.transform.localScale = Vector3.one * arranger.ratio;
        actQuestionRoot.transform.localScale = Vector3.one * arranger.ratio;


        /*
        // Igazítjuk a kérdéseket a kérdések középpontjához
        float fullLength = Common.PositionToBase(actQuestionRoot, questions, distanceBetweenQuestion);
        float maxLength = (questionsAreaCenter.transform.position.x - questionAreaBottomLeft.transform.position.x) * 2;

        // Ha a kérdés túl hosszú, akkor skálázzuk a questionCenter objektumot
        if (fullLength > maxLength)
        {
            actQuestionRoot.transform.localScale = Vector3.one * maxLength / fullLength;
        }
        */



        // A kérdést balra kipozícionáljuk a képernyőből egy képernyőnyit
        actQuestionRoot.transform.position = new Vector3(Camera.main.aspect * -2, actQuestionRoot.transform.position.y, actQuestionRoot.transform.position.z);

        SetPictures();

        yield return null;
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát
    override public IEnumerator InitCoroutine()
    {
        status = Status.Init;

        // Lekérdezzük a feladat adatait
        taskData = (TaskMathMonsterData)Common.taskController.task;

        yield return StartCoroutine(PrepareTask());

        succesfullTask = 0;

        yield return null;
    }

    // Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        // Megjelenítjük a kérdés szövegét ha van
        if (!string.IsNullOrEmpty(taskData.taskDescription))
        {
            iTween.ScaleTo(textScreenQuestion.gameObject, iTween.Hash("islocal", true, "scale", Vector3.one, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutElastic));
            Common.audioController.SFXPlay("boing");
            yield return new WaitForSeconds(taskData.animSpeed1);
        }

        // Bejön az új kérdés balról miközben a régi kimegy jobbra ha volt régi
        iTween.MoveTo(actQuestionRoot, iTween.Hash("islocal", true, "position", Vector3.zero, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutBack));
        if (oldQuestionRoot != null)
            iTween.MoveTo(oldQuestionRoot, iTween.Hash("x", Camera.main.aspect * 2, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutBack));

        // Miután ez megtörtént a régi kérdést töröljük
        yield return new WaitForSeconds(taskData.animSpeed1);
        Destroy(oldQuestionRoot);
        oldQuestionRoot = actQuestionRoot;

        // Megjelenik a szörny
        monsterMove.gameObject.SetActive(true);
        iTween.MoveTo(monsterMove.gameObject, iTween.Hash("islocal", true, "position", Vector3.zero, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutCirc));

        yield return new WaitForSeconds(taskData.animSpeed1); // Várunk amíg az animáció befejeződik

        Common.audioController.SetBackgroundMusic(1, 0.05f, 4); // Elindítjuk a háttérzenét

        status = Status.Play;
        Common.HHHnetwork.messageProcessingEnabled = true;
    }

    // A kérdéshez tartozó játék elemeket kell eltávolítani, ha meghívják ezt a metódust. A TaskController fogja új feladatnál ha az új feladatot ugyan annak a képernyőnek kell megjelenítenie
    // Meglehet adni neki egy callBack függvényt, amit akkor hív meg ha végzet a játék elemek elrejtésével, mivel ez sokáig is eltarthat és addig nem kéne tovább menni az új feladatra.
    override public IEnumerator HideGameElement()
    {
        clock.Reset(1); // Az órát alaphelyzetbe állítja

        // A szörny lebukik
        iTween.MoveTo(monsterMove.gameObject, iTween.Hash("islocal", true, "y", monsterOutPos, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeInCirc));

        yield return new WaitForSeconds(taskData.animSpeed1); // Várunk amíg az animáció befejeződik
        monsterMove.gameObject.SetActive(false);

        // A többit az Initialize megcsinálja

        yield return null;
    }

    // A képernyő teljesen eltünt. Csinálhatunk valamit ilyenkor ha szükséges
    // De figyelni kell, mivel a következő pillanatban már inaktív lesz az egész képernyő. 
    // Tehát azonnal meg kell tennünk amit akarunk nem indíthatunk a képernyőn egy coroutine-t mivel úgy sem fog lefutni.
    // Kikapcsolt gameObject-eken nem fut a coroutine.
    // Ezt a ScreenController hívja meg képernyő váltásnál
    override public IEnumerator ScreenHideFinish()
    {
        StartCoroutine(base.ScreenHideFinish());

        Debug.Log("ScreenHideFinish");
        Destroy(actQuestionRoot);
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
        taskData = (TaskMathMonsterData)task;

        yield return StartCoroutine(PrepareTask());

        // Megjelenítjük a kérdés szövegét
        textScreenQuestion.transform.localScale = Vector3.one;

        // Megjelenik a kérdés
        actQuestionRoot.transform.localPosition = Vector3.zero;

        // Miután ez megtörtént a régi kérdést töröljük
        Destroy(oldQuestionRoot);
        oldQuestionRoot = actQuestionRoot;

        // Megjelenik a szörny
        monsterMove.gameObject.SetActive(true);
        monsterMove.transform.localPosition = Vector3.zero;
    }

    IEnumerator ExitCoroutine()
    {
        status = Status.Exit;
        clock.Stop();

        Common.taskController.GameExit();
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
            if (jsonNodeMessage[C.JSONKeys.dataContent].Value == C.JSONValues.gameEvent)
            {
                // Megkeressük a json-ban található DragTarget és a DragItem objektumokat
                DragTarget dragTarget = null;
                //if (jsonNodeMessage.ContainsKey(C.JSONKeys.selectedQuestion))
                //{
                    int questionIndex = jsonNodeMessage[C.JSONKeys.selectedQuestion].AsInt;
                    int subQuestionIndex = jsonNodeMessage[C.JSONKeys.selectedSubQuestion].AsInt;
                    dragTarget = ((TextMeshWithDragTarget)questions[questionIndex]).listOfDragTarget[subQuestionIndex];
                //}

                DragItem dragItem = null;

                int answerIndex = jsonNodeMessage[C.JSONKeys.selectedAnswer].AsInt;

                foreach (BubbleDrag bubbleDrag in bubbleList)
                {
                    if (bubbleDrag.answerIndex == answerIndex)
                        dragItem = bubbleDrag;
                }
                //dragItem = bubbleList[answerIndex];

                dragAndDropControl.MessageArrived(dragTarget, dragItem, jsonNodeMessage);

                //Ha válasz történt leállítjuk az idő számlálást
                if (jsonNodeMessage[C.JSONKeys.gameEventType].Value == C.JSONValues.answer) {
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

    /// <summary>
    /// A megadott layout-nak megfelelően beállítja a képeket.
    /// Meghívása előtt a LayoutManager-ben ki kell választani a megfelelő képi világot.
    /// </summary>
    void SetPictures()
    {
        background.GetComponent<SpriteRenderer>().sprite = layoutManager.GetSprite("background");
        foreground.sprite = layoutManager.GetSprite("foreground");

        textScreenQuestion.color = layoutManager.GetColor("questionTextColor");

        monsterMove.sprite = layoutManager.GetSprite("monster");

        // Kicseréljük a kérdőjelek képét a kérdésekben
        foreach (MonoBehaviour question in questions)
        {
            ((TextMeshWithDragTarget)question).SetPictures(layoutManager.GetSprite("answer"));
        }

        // Pozícionáljuk a buborékokat tartalmazó gameObject-et a design-nak megfelelően
        bubblesRoot.transform.localPosition = bubblesRoot.transform.localPosition.SetX(
            (LayoutManager.actLayoutSetName == "1") ? 0 : -0.74f
            );

        // Kicseréljük a létrehozott buborékokat
        foreach (BubbleDrag bubbleDrag in bubbleList)
        {
            bubbleDrag.SetPicture(
                layoutManager.GetSprite("bubble"),
                layoutManager.GetSprite("gleam"), layoutManager.GetColor("gleam")
                );
        }

        // beállítjuk az óra layout-ját is
        clock.SetPictures();
    }
}