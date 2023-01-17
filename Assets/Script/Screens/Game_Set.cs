using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.UI;

public class Game_Set : GameDragAncestor
{
    [Tooltip("Halmazok közötti távolság")]
    public float distanceBetweenSets = 0.05f;
    [Tooltip("Mennyi időközönként kövessék egymást a halmaz elemek")]
    public float waitTimeSetItemShow = 0.2f;

    SetItemCollector originalSetItemCollector; // Halmaz elem gyűjtő amit klónozni fogunk
    SetItem originalSetItem;    // Halmaz elem amit klónozni fogunk

    GameObject upperLeft;       // A halmazok számára fenntartott terület bal felső sarka
    GameObject bottomRight;     // A halmazok számára fenntartott terület jobb alsó sarka
    GameObject itemRight;       // Hol kezdődnek a halmaz elemek jobbról
    GameObject itemLeft;        // Meddig tarthatnak a halmaz elemek

    GameObject questionTextMove;        // A kérdés előugrásához kell
    Text questionText;                  // A kérdés szövegének kiírásához    
    TEXDraw questionTEXDraw;            // A kérdés szövegének kiírásához

    //Text verifyButtonText;              // Az önellenőrzés gomb kezeléséhez
    Text piecesLeftText;                // Ide írjuk, hogy mennyit tettünk már helyre

    TaskSetsData taskData;

    List<SetItemCollector> listOfSetItemCollector = new List<SetItemCollector>();
    List<DragItem> listOfSetItem = new List<DragItem>();

    //DragAndDropControl dragAndDropControl;

    DragItem zoomedGrabItem;            // A kinagyított grab item

    Vector3 grabSetItemPosInWorldSpace; // Hol volt a setItem a megfogás pillanatában
    Vector3 grabMousePosInWorldSpace;   // Hol volt az egér a megfogás pillanatában

    override public void Awake()
    {
        base.Awake();

        originalSetItemCollector = Common.SearchGameObject(gameObject, "SetItemCollector").GetComponent<SetItemCollector>();
        originalSetItemCollector.gameObject.SetActive(false);
        originalSetItem = Common.SearchGameObject(gameObject, "SetItem").GetComponent<SetItem>();
        originalSetItem.gameObject.SetActive(false);

        upperLeft = Common.SearchGameObject(gameObject, "UpperLeft", originalSetItem.gameObject, originalSetItemCollector.gameObject).gameObject;
        bottomRight = Common.SearchGameObject(gameObject, "BottomRight", originalSetItem.gameObject, originalSetItemCollector.gameObject).gameObject;
        itemRight = Common.SearchGameObject(gameObject, "ItemRight").gameObject;
        itemLeft = Common.SearchGameObject(gameObject, "ItemLeft").gameObject;

        questionTextMove = Common.SearchGameObject(gameObject, "questionTextMove").gameObject;
        questionText = Common.SearchGameObject(gameObject, "QuestionText").GetComponent<Text>();
        questionTEXDraw = Common.SearchGameObject(gameObject, "QuestionTEXDraw").GetComponent<TEXDraw>();

        //verifyButtonText = gameObject.SearchChild("VerifyButtonText").GetComponent<Text>();
        piecesLeftText = gameObject.SearchChild("PiecesLeftText").GetComponent<Text>();

        // Zoomer feedBack eseményeinek beállítása
        zoomer.beforeZoomBackStartCallBack = () => {
            zoomer.animSetLocalPos = true;
            zoomer.goMagnifyOriginalPos = Vector3.zero;
            zoomer.itemZoomedPos = zoomer.goMagnify.transform.localPosition;
        };

        zoomer.zoomBackFinishCallBack = () =>
        {
            zoomedGrabItem.SetOrderInLayer(zoomedGrabItem.OrderInLayer);
        };

        // DragAndDropControl feedBack eseményeinek beállítása
        // Rákattintottak egy elemre
        dragAndDropControl.itemClick = (DragItem dragItem) => {
            zoomedGrabItem = dragItem;

            // Megvizsgáljuk, hogy a kattintott elem kép-e és engedélyezve van-e a megfogása, mivel lehet, hogy már a helyén van
            if (/*((SetItem)dragItem).isPicture &&*/ dragItem.enabledGrab)
            {
                // Az összes dragitem-nek beállítjuk a rétegsorrendjét alapra
                //foreach (DragItem item in listOfSetItem)
                //    if (item.enabledGrab)
                //        item.SetOrderInLayer(item.OrderInLayer);

                // Kinagyítjuk az elemet a rétegsorrendját is beállítjuk, hogy lehetőleg takarjon mindent
                zoomer.Zoom(dragItem.gameObject.SearchChild("move"), 3, true);
                dragItem.SetOrderInLayer(400);
            }
        };

        // Megfogtak egy elemet
        dragAndDropControl.itemDrag = (DragItem dragItem) => {
        };

        // Az elemet elengedtük nem egy célpont felett
        dragAndDropControl.itemReleased = (DragItem dragItem) => { dragItem.MoveBasePos(); };

        // Elengedtük az elemet egy célpont felett, még nem tudjuk, hogy jó helyen vagy rossz helyen
        dragAndDropControl.itemReleasedOverADragTarget = (DragItem dragItem, DragTarget dragTarget) => {
            listOfSetItem.Remove(dragItem);
        };

        // Az elemet rossz helyre helyeztük
        dragAndDropControl.itemPutWrongPlace = (DragItem dragItem, DragTarget dragTarget) => {
            dragItem.enabledGrab = false;
            ((SetItem)dragItem).FlashingAndGoOut(new Vector3(Camera.main.orthographicSize * -Camera.main.aspect - dragItem.GetWidth(), itemRight.transform.position.y));
            listOfSetItem.Add(dragItem);
            ItemDispenser();
        };

        // Az elemet jó helyre helyeztük
        dragAndDropControl.itemPutGoodPlace = (DragItem dragItem, DragTarget dragTarget) => {
            //grabSetItem.SetBasePos(grabSetItem.MoveTransform GetMovePos);
            ItemDispenser();
        };

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
        ClearSetsAndSetItems();

        exitButton.SetActive(false); // Common.configurationController.deviceIsServer);

        // A verify gomb szövegének beállítása nyelv függően
        // Ebben a játékban nincs értelme a következő gombnak mivel minden elemet valamelyik halmazba kell tenni
        // Tehát akkor van vége a játéknak ha elfogytak az elemek
        //verifyButtonText.text = Common.languageController.Translate(C.Texts.Verify);
        //verifyButtonText.transform.parent.gameObject.SetActive(Common.configurationController.answerFeedback == ConfigurationController.AnswerFeedback.ShelfCheckButton);

        // Beállítjuk a kérdést
        questionTEXDraw.text = taskData.question;
        questionText.text = taskData.question;
        questionText.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text;
        questionTEXDraw.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.TEXDraw;

        questionTextMove.transform.localScale = Vector3.zero;

        // Beállítjuk a rendelkezésre álló időt
        clock.timeInterval = taskData.time;
        clock.Reset(0);

        // Elhelyezzük a halmazokat
        Vector2 allSize = new Vector2(bottomRight.transform.localPosition.x - upperLeft.transform.localPosition.x, upperLeft.transform.localPosition.y - bottomRight.transform.localPosition.y);
        //Vector2 allSize = new Vector2(bottomRight.transform.position.x - upperLeft.transform.position.x, upperLeft.transform.position.y - bottomRight.transform.position.y);

        Debug.Log("allSize : " + allSize.x + ":" + allSize.y);

        float setWidth = allSize.x; // A halmaz szélessége
        float setHeight = allSize.y; // A halmaz magassága
        int newLine = 10; // Hányadik halmaz után kezdjen újsort

        // Meghatározzuk a halmazok méretét
        switch (taskData.sets.Count)
        {
            case 2:
                setWidth = (allSize.x - distanceBetweenSets) / 2;
                break;
            case 3:
                setWidth = (allSize.x - distanceBetweenSets * 2) / 3;
                break;
            case 4:
                setWidth = (allSize.x - distanceBetweenSets) / 2;
                setHeight = (allSize.y - distanceBetweenSets) / 2;
                newLine = 2;
                break;
            case 5:
            case 6:
                setWidth = (allSize.x - distanceBetweenSets * 2) / 3;
                setHeight = (allSize.y - distanceBetweenSets) / 2;
                newLine = 3;
                break;
        }

        Debug.Log("setWidth : " + setWidth);
        Debug.Log("setHeight : " + setHeight);

        // Ténylegesen létrehozzuk a halmazokat
        float xPos = setWidth / 2 + upperLeft.transform.localPosition.x;
        float yPos = setHeight / -2 + upperLeft.transform.localPosition.y;
        for (int i = 0; i < taskData.sets.Count; i++)
        {
            if (i == newLine)
            {
                xPos = setWidth / 2 + upperLeft.transform.localPosition.x;
                if (taskData.sets.Count == 5) // Ha öt halmaz van, akkor a következő sort egy kicsit beljebb kell kezdeni
                    xPos += setWidth / 2 + distanceBetweenSets / 2;

                yPos -= (setHeight + distanceBetweenSets);
            }

            SetItemCollector collector = Instantiate(originalSetItemCollector).GetComponent<SetItemCollector>();
            collector.transform.SetParent(background.transform, false);
            collector.gameObject.SetActive(true);

            collector.transform.localPosition = new Vector3(xPos, yPos, 0);
            //collector.canvasRectTransform.transform.localPosition = new Vector3(0, -3, 0);
            collector.canvasRectTransform.transform.position = collector.canvasRectTransform.transform.position.AddY(-Camera.main.orthographicSize * 2);

            collector.questionIndex = i;

            //yield return collector.InitializeCoroutine(taskData.sets[i].name, taskData.sets[i].pictureName, Color.green, taskData.sets[i].items, new Vector2(setWidth * background.transform.lossyScale.x, setHeight * background.transform.lossyScale.y));
            collector.InitializeCoroutine(taskData.sets[i].name, taskData.gameData.GetSprite(taskData.sets[i].pictureName), Color.green, taskData.sets[i].items, new Vector2(setWidth * background.transform.lossyScale.x, setHeight * background.transform.lossyScale.y));

            Debug.Log("Scale " + i + " : " + collector.transform.lossyScale);
            Debug.Log("localScale " + i + " : " + collector.transform.localScale);

            listOfSetItemCollector.Add(collector);

            xPos = xPos + distanceBetweenSets + setWidth;
        }

        // Létrehozzuk a halmaz elemeket
        int posIndex = 0;
        foreach (string answerText in taskData.GetAnswersShuffle())
        {
            SetItem setItem = Instantiate(originalSetItem).GetComponent<SetItem>();
            setItem.gameObject.SetActive(true);
            setItem.transform.SetParent(background.transform, false);
            setItem.transform.localPosition = Vector3.zero;

            //setItem.gameObject.SetActive(false);
            setItem.enabledGrab = false;
            setItem.answerIndex = taskData.answerOrder[posIndex];

            yield return setItem.InitializeCoroutine(taskData, answerText, Color.yellow);
            //StartCoroutine(setItem.InitializeCoroutine(taskData, answerText, Color.yellow));
            setItem.gameObject.SetActive(false);

            listOfSetItem.Add(setItem);

            posIndex++;
        }

        DrawPiecesLeft();

        yield return null;
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        status = Status.Init;

        // Lekérdezzük a feladatot
        taskData = (TaskSetsData)Common.taskController.task;

        yield return StartCoroutine(PrepareTask());
    }

    /// <summary>
    /// Töröljük a korábbi halmazokat és halmaz elemeket.
    /// </summary>
    void ClearSetsAndSetItems() {
        // Töröljük a halmazokat
        foreach (SetItemCollector setItemCollector in listOfSetItemCollector)
            Destroy(setItemCollector.gameObject);
        listOfSetItemCollector.Clear();

        // Töröljük a halmaz elemeket
        foreach (SetItem setItem in listOfSetItem)
            Destroy(setItem.gameObject);
        listOfSetItem.Clear();
    }

    /*
    Halmaz elemeket adagolja
    Annyit jelenít meg, amennyi kifér, illetve odébb mozgatja őket szükség szerint
    
    Egy új elemet jelenít meg ha az elem még nem aktív
    Viszont ha már aktív, akkor csak odébb mozgatja
    */
    void ItemDispenser() {

        float aktPos = itemRight.transform.position.x;
        bool firstItem = true;  // Egy elemet mindenféleképpen belekell tenni, akkor is ha olyan széles, hogy az egész rendelkezésre álló helyet elfoglalja
        float waitTime = 0;     // Mennyit várakozzon míg elindul a mozgatás

        foreach (SetItem setItem in listOfSetItem)
        {
            float itemWidth = setItem.GetWidth();

            // Ma már nem fér ki, akkor befejezzük az adagolást
            if (!firstItem && aktPos - itemWidth < itemLeft.transform.position.x) break;

            if (setItem.gameObject.activeSelf) {
                // Ha már be van kapcsolva, akkor csak odébb mozgatjuk
                if (setItem.SetBasePos(new Vector3(aktPos - itemWidth / 2, itemRight.transform.position.y), waitTime)) {
                    waitTime += waitTimeSetItemShow;

                    if (setItem.transform.position.y == setItem.GetMovePos.y)
                        setItem.SetOrderInLayer(1);
                }  
            }
            else {
                // Ha még nem volt bekapcsolva, akkor kipozícionáljuk a képernyőből, majd bekapcsoljuk
                setItem.transform.position = new Vector3(Camera.main.orthographicSize * -Camera.main.aspect - itemWidth, itemRight.transform.position.y);
                setItem.gameObject.SetActive(true);
                setItem.SetOrderInLayer(1);

                // Beállítjuk az új bázis pozícióját
                setItem.SetBasePos(new Vector3(aktPos - itemWidth / 2, itemRight.transform.position.y), waitTime);
                waitTime += waitTimeSetItemShow;
            }

            setItem.enabledGrab = true;

            aktPos -= (itemWidth + distanceBetweenSets * background.transform.localScale.x); 

            firstItem = false;
        }

        DrawPiecesLeft();

        //piecesLeftText.text = listOfSetItem.Count + "/" + taskData.allAnswers.Count;
    }

    // Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        // Megjelenítjük a kérdés szövegét ha van
        if (!string.IsNullOrEmpty(taskData.question))
        {
            iTween.ScaleTo(questionTextMove, iTween.Hash("islocal", true, "scale", Vector3.one, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutElastic));
            Common.audioController.SFXPlay("boing");
            yield return new WaitForSeconds(taskData.animSpeed1);
        }

        // A halmazok feljönnek
        foreach (SetItemCollector setItemCollector in listOfSetItemCollector)
        {
            iTween.MoveTo(setItemCollector.canvasRectTransform.gameObject, iTween.Hash("islocal", true, "position", Vector3.zero, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutCirc));
            yield return new WaitForSeconds(taskData.animSpeed1 * 0.2f);
        }
        yield return new WaitForSeconds(taskData.animSpeed1 * 0.8f);

        Common.HHHnetwork.messageProcessingEnabled = true;
        status = Status.Play;

        ItemDispenser();

        Common.audioController.SetBackgroundMusic(1, 0.05f, 4); // Elindítjuk a háttérzenét
    }

    // Az kérdéshez tartozó játék elemeket kell eltávolítani, ha meghívják ezt a metódust. A TaskController fogja új feladatnál ha az új feladatot ugyan annak a képernyőnek kell megjelenítenie.
    override public IEnumerator HideGameElement()
    {
        clock.Reset(1);

        // Leesnek a halmaz elemek
        foreach (SetItem setItem in listOfSetItem)
        {
            if (setItem.isActiveAndEnabled)
            {
                iTween.MoveAdd(setItem.gameObject, iTween.Hash("y", -3, "easetype", iTween.EaseType.easeInCubic));
                yield return new WaitForSeconds(0.2f);
            }
        }

        // Leesnek a halmazok
        for (int i = listOfSetItemCollector.Count - 1; i >= 0; i--)
        {
            iTween.MoveAdd(listOfSetItemCollector[i].gameObject, iTween.Hash("y", -3, "easetype", iTween.EaseType.easeInCubic));
            yield return new WaitForSeconds(0.2f);
        }

        // Kérdés összezsugorodik
        iTween.ScaleTo(questionTextMove, iTween.Hash("islocal", true, "scale", Vector3.zero, "time", taskData.animSpeed1, "easeType", iTween.EaseType.linear));
        yield return new WaitForSeconds(taskData.animSpeed1);

        ClearSetsAndSetItems();
    }

    /*
    // Vissza adja, hogy a kurzor pozíciójában milyen SetItem található
    // Null értéket ad vissza ha nincs ott semmi
    public SetItem GetSetItemInCursorPos()
    {
        // Egy sugarat kell kibocsájtani a kamerából
        Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Retrieve all raycast hits from the click position and store them in an array called "hits".
        RaycastHit2D[] hits = Physics2D.LinecastAll(mousePositionInWorld, mousePositionInWorld);

        foreach (RaycastHit2D raycastHit in hits)
            if (raycastHit.collider.gameObject.tag == "setItem") {
                SetItem setItem = raycastHit.collider.transform.parent.parent.GetComponent<SetItem>();
                if (setItem.enabledGrab) return setItem;
            }

        return null;
    }

    // Vissza adja, hogy a kurzor pozíciójában milyen SetItemCollector található
    // Null értéket ad vissza ha nincs ott semmi
    public SetItemCollector GetSetItemCollectorInCursorPos()
    {
        // Egy sugarat kell kibocsájtani a kamerából
        Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Retrieve all raycast hits from the click position and store them in an array called "hits".
        RaycastHit2D[] hits = Physics2D.LinecastAll(mousePositionInWorld, mousePositionInWorld);

        foreach (RaycastHit2D raycastHit in hits)
            if (raycastHit.collider.gameObject.tag == "setItemCollector")
                return raycastHit.collider.transform.parent.GetComponent<SetItemCollector>();

        return null;
    }
    */

    // Játéknak vége letelt az idő, vagy elfogytak a halmaz elemek
    override public IEnumerator GameEnd() {
        status = Status.Result;
        clock.Stop();

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
        taskData = (TaskSetsData)task;

        yield return StartCoroutine(PrepareTask());

        // Megjelenítjük a kérdés szövegét
        questionTextMove.transform.localScale = Vector3.one;

        // A halmazok feljönnek
        foreach (SetItemCollector setItemCollector in listOfSetItemCollector)
        {
            setItemCollector.canvasRectTransform.localPosition = Vector3.zero;
        }

        ItemDispenser();
    }

    void DrawPiecesLeft()
    {
        piecesLeftText.text = taskData.allAnswers.Count - listOfSetItem.Count + "/" + taskData.allAnswers.Count;
    }

    /*
    IEnumerator ExitCoroutine()
    {
        status = Status.Exit;

        clock.Stop();

        Common.taskController.GameExit();

        //Common.audioController.MuteBackgroundMusic();
        //yield return new WaitForSeconds(2f);

        //Common.screenController.ChangeScreen("MainMenu");
        yield return null;

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
                dragTarget = ((SetItemCollector)listOfSetItemCollector[questionIndex]);
                //}

                DragItem dragItem = null;

                int answerIndex = jsonNodeMessage[C.JSONKeys.selectedAnswer].AsInt;

                foreach (SetItem setItem in listOfSetItem)
                {
                    if (setItem.answerIndex == answerIndex)
                    {
                        dragItem = setItem;
                        dragItem.gameObject.SetActive(true);
                        break;
                    }
                }

                dragAndDropControl.MessageArrived(dragTarget, dragItem, jsonNodeMessage);

                //Ha válasz történt leállítjuk az idő számlálást
                if (jsonNodeMessage[C.JSONKeys.gameEventType].Value == C.JSONValues.answer)
                {
                    status = Status.Result;
                }
            }
        }
    }

    /*
    public void UIButtonClick(string button)
    {
        if (userInputIsEnabled)
        {
            switch (button)
            {
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
    */

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
            }
        }
    }
    */
}
