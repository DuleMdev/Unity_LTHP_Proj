using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;

public class Game_Affix_With_Layout : GameDragAncestor {

    [Tooltip("A fán levő tábla prefabja")]
    public GameObject tablePrefab;      // A fán levő tábla
    [Tooltip("A szörnymacska kezében levő tábla prefabja")]
    public GameObject tableGrabPrefab;  // A macska kezében levő tábla

    LayoutManager layoutManager;    // A különböző layoutokhoz tartozó képeket tartalmazza

    SpriteRenderer foreground;

    Lair lair;

    Text textScreenQuestion;

    //DragAndDropControl dragAndDropControl;

    GameObject catMonsterMove;                  // A szörnymacska mozgatásának gameObject-je
    SpriteRenderer cat;
    SpriteRenderer catShadow;

    List<Transform> tableTargetPosList;         // A fán megjelenő táblák pozíciója
    List<Transform> tableDragPosList;           // A macska kezének pozíicói
    List<TableDrag> tableDragList;              // A létrehozott macska kezek
    List<TableDragTarget> tableTargetList;      // A fán létrehozott táblák

    TaskAffixData taskData;         // A feladatot tartalmazó objektum

    GameObject catArmsBase;         // A macska kezeit tartalmazó gameObject

    override public void Awake() {
        base.Awake();

        layoutManager = GetComponentInChildren<LayoutManager>();

        foreground = transform.Find("background/foreground").GetComponent<SpriteRenderer>();

        lair = Common.SearchGameObject(gameObject, "lair").GetComponent<Lair>();

        textScreenQuestion = gameObject.SearchChild("textQuestion").GetComponent<Text>();

        catMonsterMove = Common.SearchGameObject(gameObject, "CatMonsterMove").gameObject;
        cat = catMonsterMove.GetComponent<SpriteRenderer>();
        catShadow = Common.SearchGameObject(gameObject, "CatMonsterShadow").GetComponent<SpriteRenderer>();

        // DragAndDropControl feedBack eseményeinek beállítása
        dragAndDropControl = gameObject.GetComponent<DragAndDropControl>();
        dragAndDropControl.itemReleased = (DragItem dragItem) => { dragItem.MoveBasePos(0); };                  // Az elemet elengedtük nem egy célpont felett
        dragAndDropControl.itemReleasedOverADragTarget = (DragItem dragItem, DragTarget dragTarget) => { };    // Elengedtük az elemet egy célpont felett, még nem tudjuk, hogy jó helyen vagy rossz helyen
        dragAndDropControl.itemPutWrongPlace = (DragItem dragItem, DragTarget dragTarget) => { dragItem.MoveBasePos(); };               // Az elemet rossz helyre helyeztük
        dragAndDropControl.itemPutGoodPlace =
            (DragItem dragItem, DragTarget dragTarget) => {
                /*
                succesfullTask++;
                if (succesfullTask == taskData.questions.Count)
                    StartCoroutine(GameEnd());
                    */
            };                // Az elemet jó helyre helyeztük
        
        // Összeszedjük a TableTarget pozíciókat
        tableTargetPosList = new List<Transform>();
        int index = 1;
        while (true)
        {
            GameObject go = Common.SearchGameObject(gameObject, "Pos" + index);
            if (go == null) break;
            tableTargetPosList.Add(go.transform);
            index++;
        }

        // Összeszedjük a TableTarget pozíciókat
        tableDragPosList = new List<Transform>();
        index = 1;
        while (true)
        {
            GameObject go = Common.SearchGameObject(gameObject, "DragPos" + index);
            if (go == null) break;
            tableDragPosList.Add(go.transform);
            index++;
        }

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

        // Beállítjuk az odú képeit
        lair.SetPictures(
            layoutManager.GetSprite("lairCover"), 
            layoutManager.GetSprite("squirrel"), 
            layoutManager.GetSprite("owl")
            );

        // Beállítjuk a képernyő kérdést
        textScreenQuestion.text = taskData.screenQuestion;
        textScreenQuestion.transform.localScale = Vector3.one * 0.001f;

        // Kiszámoljuk a képernyő szélességét, azért kell, hogy ki tudjuk pozíciónálni az elemeket a képernyőből
        float halfScreenWidth = Camera.main.aspect / background.transform.localScale.x;

        // Ha a macska nem látható, akkor kipozícionáljuk a képernyőből
        if (!catMonsterMove.activeSelf)
            catMonsterMove.transform.localPosition = new Vector3(halfScreenWidth, 0);

        // Beállítjuk a macska képét a layoutnak megfelelően
        cat.sprite = layoutManager.GetSprite("cat");
        cat.color = layoutManager.GetColor("cat");
        catShadow.sprite = layoutManager.GetSprite("catShadow");
        catShadow.color = layoutManager.GetColor("catShadow");

        // Beállítjuk az odu képét a layoutnak megfelelően


        // Létrehozzuk a fán a kérdéseket, méretük majdnem nulla 0.001f
        tableTargetList = new List<TableDragTarget>();
        for (int i = 0; i < taskData.questionsOld.Count; i++)
        {
            GameObject table = Instantiate(tablePrefab);
            table.transform.parent = background.transform;
            table.transform.position = tableTargetPosList[taskData.questionOrder[i]].position;
            table.transform.localScale = Vector3.one * 0.01f;
            //table.transform.localScale = Vector3.one * 0.001f;

            TableDragTarget tableTarget = table.GetComponentInChildren<TableDragTarget>();
            tableTarget.InitializeTable(taskData.questionsOld[i].question, taskData.questionsOld[i].answers);
            tableTarget.SetPictures(
                layoutManager.GetSprite("tableLeft"), layoutManager.GetColor("tableLeft"), 
                layoutManager.GetSprite("tableLeftShadow"), layoutManager.GetColor("tableLeftShadow"), 
                layoutManager.GetSprite("tableLeftGlow"), layoutManager.GetColor("tableLeftGlow")
                );
            tableTarget.ChangeTextColor(layoutManager.GetColor("textColor"));
            tableTarget.tableMove.localScale = Vector3.one * 0.001f;
            /*
            // Minden kérdést többször írunk ki egymás után a fára, hogy tesztelni tudjam a hosszú szövegek viselkedését
            string question = "";
            for (int j = 0; j < 6; j++)
            {
                question += taskData.questions[i].question;
            }
            tableTarget.InitializeTable(question, taskData.questions[i].answers);
            */

            tableTarget.questionIndex = i;
            tableTarget.maxItem = 1;

            // Ha a tábla kilóg a képernyőből, akkor módosítjuk a pozícióját
            float halfTableWidth = tableTarget.GetWidth() / 2;
            if (table.transform.position.x - halfTableWidth < -Camera.main.aspect) {
                table.transform.position = table.transform.position.SetX(-Camera.main.aspect + halfTableWidth);
            }

            // Összenyomjuk a táblát, hogy előtudjon ugorni
            tableTarget.tableMove.localScale = Vector3.one * 0.001f;


            //tableTargetList.Add(table.GetComponentInChildren<TableDragTarget>());
            Common.ListAdd(tableTargetList, taskData.questionOrder[i], tableTarget, null);
        }

        // Létrehozzunk egy gameObject-et amiben a macska kezeit fogjuk tárolni
        catArmsBase = new GameObject();
        catArmsBase.name = "catArmsBase";
        catArmsBase.transform.parent = background.transform;
        catArmsBase.transform.localPosition = Vector3.zero;
        catArmsBase.transform.localScale = Vector3.one;

        // Létrehozzuk a macska kezeit és kipozícionáljuk a képernyőből őket
        tableDragList = new List<TableDrag>();
        int posIndex = 0;
        foreach (string answer in taskData.GetAnswers())
        {
            GameObject table = Instantiate(tableGrabPrefab);
            table.transform.parent = catArmsBase.transform;
            table.transform.position = tableDragPosList[taskData.answerOrder[posIndex]].position;
            table.transform.localScale = Vector3.one;

            TableDrag tableDrag = table.GetComponentInChildren<TableDrag>();
            tableDrag.InitializeTable(answer, Common.configurationController.isServer2020 ? taskData.GetTextFromID(answer) : null);

            tableDrag.SetPictures(
                layoutManager.GetSprite("tableRight"), layoutManager.GetColor("tableRight"), 
                layoutManager.GetSprite("tableRightShadow"), layoutManager.GetColor("tableRightShadow"), 
                layoutManager.GetSprite("tableRightGlow"), layoutManager.GetColor("tableRightGlow"),
                layoutManager.GetSprite("arm"), layoutManager.GetColor("arm"),
                layoutManager.GetSprite("armShadow"), layoutManager.GetColor("armShadow")
                );
            tableDrag.SetTextColor(layoutManager.GetColor("textColor"));
            tableDrag.answerIndex = posIndex;
            tableDragList.Add(tableDrag);

            // Kikapcsoljuk és kipozícionáljuk a képernyőből
            //table.SetActive(false);
            tableDrag.armMove.position /* localPosition*/ = tableDrag.GetGlobalHideArmMovePos(); //  new Vector2(halfScreenWidth, 0);

            posIndex++;
        }

        SetPictures();

        yield break;
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        status = Status.Init;

        // Lekérdezzük a feladat adatait
        taskData = (TaskAffixData)Common.taskController.task;

        yield return StartCoroutine(PrepareTask());
    }

    // Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        // Megjelenítjük a kérdés szövegét ha van
        if (!string.IsNullOrEmpty(taskData.screenQuestion))
        {
            iTween.ScaleTo(textScreenQuestion.gameObject, iTween.Hash("islocal", true, "scale", Vector3.one, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutElastic));
            Common.audioController.SFXPlay("boing");
            yield return new WaitForSeconds(taskData.animSpeed1);
        }

        // Előugranak a fán levő táblák
        foreach (var item in tableTargetList)
        {
            if (item != null) {
                iTween.ScaleTo(item.tableMove.gameObject, iTween.Hash("islocal", true, "scale", Vector3.one, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutElastic));
                yield return new WaitForSeconds(0.2f);
            }
        }

        // Ha a macska nem látható, akkor beúszik a képbe
        if (!catMonsterMove.activeSelf) {
            catMonsterMove.SetActive(true);
            iTween.MoveTo(catMonsterMove, iTween.Hash("islocal", true, "position", Vector3.zero, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutCirc));
            yield return new WaitForSeconds(taskData.animSpeed1);
        }

        // Beusztatjuk a macska kezeit
        int[] indexes = Common.GetRandomNumbers(tableDragList.Count);
        int posIndex = 0;
        for (int i = 0; i < tableDragList.Count; i++)
        {
            tableDragList[indexes[posIndex]].gameObject.SetActive(true);
            iTween.MoveTo(tableDragList[indexes[posIndex]].armMove.gameObject, iTween.Hash("islocal", true, "position", Vector3.zero, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutCirc));
            yield return new WaitForSeconds(0.2f);

            posIndex++;
        }

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

        Debug.Log("Hide Game Element start");

        // Kimennek a macska kezei a képernyőből és töröljük őket
        int[] indexes = Common.GetRandomNumbers(tableDragList.Count);
        int posIndex = 0;
        for (int i = 0; i < tableDragList.Count; i++)
        {
            Debug.Log("Macska kéz : " + i);

            // Ha az aktuális még a képernyőn van, akkor kimegy
            if (tableDragList[indexes[i]].armMove.localPosition.x == 0)
            {
                iTween.MoveTo(tableDragList[indexes[posIndex]].armMove.gameObject, iTween.Hash("x", tableDragList[indexes[posIndex]].GetGlobalHideArmMovePos().x, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeInCirc));
                yield return new WaitForSeconds(0.2f);
                Destroy(tableDragList[indexes[posIndex]].gameObject, taskData.animSpeed1 - 0.2f);
            }
            else {
                Destroy(tableDragList[indexes[posIndex]].gameObject);
            }

            posIndex++;
        }

        yield return new WaitForSeconds(taskData.animSpeed1);

        Destroy(catArmsBase);

        Debug.Log("fa táblák lehullanak");


        // Lehullanak a fáról a táblák
        for (int i = tableTargetList.Count - 1; i >= 0; i--)
        {
            Debug.Log("Fa tábla : " + i);

            // Ha az a tábla létezik, akkor kimegy, majd töröljük
            if (tableTargetList[i] != null)
            {
                iTween.MoveTo(tableTargetList[i].gameObject, iTween.Hash("y", -2, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeInCirc));
                yield return new WaitForSeconds(0.2f);
                Destroy(tableTargetList[i].gameObject, taskData.animSpeed1 - 0.2f);
            }
        }

        yield return new WaitForSeconds(taskData.animSpeed1);
    }

    // A képernyő teljesen eltünt. Csinálhatunk valamit ilyenkor ha szükséges
    // De figyelni kell, mivel a következő pillanatban már inaktív lesz az egész képernyő. 
    // Tehát azonnal meg kell tennünk amit akarunk nem indíthatunk a képernyőn egy coroutine-t mivel úgy sem fog lefutni.
    // Kikapcsolt gameObject-eken nem fut a coroutine.
    // Ezt a ScreenController hívja meg képernyő váltásnál
    override public IEnumerator ScreenHideFinish()
    {
        StartCoroutine(base.ScreenHideFinish());

        // Eltüntetjük a macska képét, hogy a következő indításnál újra felbukkanhasson
        catMonsterMove.SetActive(false);

        // Töröljük a fán levő táblákat
        foreach (var item in tableTargetList)
            if (item != null)
                Destroy(item.gameObject);

        // Töröljük a macska kezeit
        Destroy(catArmsBase);

        yield return null;
    }

    // Update is called once per frame
    /*
    void Update()
    {
        menu.menuEnabled = (status == Status.Play);
        dragAndDropControl.dragAndDropEnabled = (status == Status.Play);

        if (status == Status.Play) // Ha megy a játék, akkor megy az óra
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
        taskData = (TaskAffixData)task;

        yield return StartCoroutine(PrepareTask());

        // Megjelenítjük a kérdés szövegét
        textScreenQuestion.transform.localScale = Vector3.one;

        // Megjelenítjük a fán levő táblákat
        foreach (var item in tableTargetList)
            if (item != null)
                item.tableMove.transform.localScale = Vector3.one;

        // Megjelenítjük a macskát
        if (!catMonsterMove.activeSelf)
        {
            catMonsterMove.SetActive(true);
            catMonsterMove.transform.localPosition = Vector3.zero;
        }

        // Megjelnítjük a macska kezeit
        for (int i = 0; i < tableDragList.Count; i++)
        {
            tableDragList[i].gameObject.SetActive(true);
            tableDragList[i].armMove.transform.localPosition = Vector3.zero;
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
                int questionIndex = jsonNodeMessage[C.JSONKeys.selectedQuestion].AsInt;
                int subQuestionIndex = jsonNodeMessage[C.JSONKeys.selectedSubQuestion].AsInt;

                foreach (TableDragTarget tableDragTarget in tableTargetList)
                    if (tableDragTarget != null && tableDragTarget.questionIndex == questionIndex)
                        dragTarget = tableDragTarget;

                //DragItem dragItem = null;
                int answerIndex = jsonNodeMessage[C.JSONKeys.selectedAnswer].AsInt;
                DragItem dragItem = tableDragList[answerIndex];

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

        // Beállítjuk az odú képeit
        lair.SetPictures(
            layoutManager.GetSprite("lairCover"),
            layoutManager.GetSprite("squirrel"),
            layoutManager.GetSprite("owl")
            );

        cat.sprite = layoutManager.GetSprite("cat");
        catShadow.sprite = layoutManager.GetSprite("catShadow");

        foreach (TableDrag tableDrag in tableDragList) {
            tableDrag.SetPictures(
                layoutManager.GetSprite("tableRight"), layoutManager.GetColor("tableRight"),
                layoutManager.GetSprite("tableRightShadow"), layoutManager.GetColor("tableRightShadow"),
                layoutManager.GetSprite("tableRightGlow"), layoutManager.GetColor("tableRightGlow"),
                layoutManager.GetSprite("arm"), layoutManager.GetColor("arm"),
                layoutManager.GetSprite("armShadow"), layoutManager.GetColor("armShadow")
                );

            tableDrag.SetTextColor(layoutManager.GetColor("textColor"));

        }

        foreach (TableDragTarget tableDragTarget in tableTargetList) {
            if (tableDragTarget != null)
            {
                tableDragTarget.SetPictures(
                    layoutManager.GetSprite("tableLeft"), layoutManager.GetColor("tableLeft"),
                    layoutManager.GetSprite("tableLeftShadow"), layoutManager.GetColor("tableLeftShadow"),
                    layoutManager.GetSprite("tableLeftGlow"), layoutManager.GetColor("tableLeftGlow")
                    );

                tableDragTarget.ChangeTextColor(layoutManager.GetColor("textColor"));
            }
        }

        // beállítjuk az óra layout-ját is
        clock.SetPictures();
    }


}