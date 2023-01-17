using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SimpleJSON;
using UnityEngine.Networking;

public class Game_Boom_With_Layout : TrueGameAncestor
{
    [Tooltip("Hány százalék távolság legyen az elemek között az elemek szélességéhez képest")]
    public float padding;

    LayoutManager layoutManager;    // A különböző layoutokhoz tartozó képeket tartalmazza

    SpriteRenderer foreground;

    Transform questionMove; // A kérdés előugrásához a transform komponens
    Text questionText;      // A kérdés kiírásához a Text komponens
    SpriteRenderer goodLight;   // A jó lámpa fénye (zöld)
    SpriteRenderer badLight;    // A rossz lámpa fénye (piros)

    GameObject topRight;    // A válaszok megjelenés területének bal felső sarka
    GameObject bottomLeft;  // A válaszok megjelenés területének jobb alsó sarka

    SpriteRenderer TV;      // A TV készülék képe
    TVScreen tvScreen;      // A TV képernyőjének kezelő szkriptje, ez lesz sokszorosítva

    TaskBoomData taskData;  // A feladatot tartalmazó objektum

    List<TVScreen> tvScreenList; // A létrehozott válasz objektumok
    Dictionary<string, Sprite> pictures;

    //Vector2 answerPosBase;  // Az első válasz pozícója
    //Vector2 answerDistance; // A kérdések közötti távolság x és y irányban

    int columns;            // A válasz terület hány oszlopból áll
    int rows;               // A válasz terület hány sorból áll

    bool firstTime;         // Az első kérdés következik? (A tv-nek be kell "melegednie")

    new protected void Awake() {
        base.Awake();

        layoutManager = GetComponentInChildren<LayoutManager>();

        foreground = transform.Find("background/foreground").GetComponent<SpriteRenderer>();

        questionMove = Common.SearchGameObject(gameObject, "questionMove").transform;
        questionText = Common.SearchGameObject(gameObject, "questionUIText").GetComponent<Text>();
        //switchOn = Common.SearchGameObject(gameObject, "switchOn").gameObject;
        goodLight = Common.SearchGameObject(gameObject, "goodLight").GetComponent<SpriteRenderer>();
        badLight = Common.SearchGameObject(gameObject, "badLight").GetComponent<SpriteRenderer>();

        topRight = Common.SearchGameObject(gameObject, "TopRight").gameObject;
        bottomLeft = Common.SearchGameObject(gameObject, "BottomLeft").gameObject;

        TV = Common.SearchGameObject(gameObject, "TV").GetComponent<SpriteRenderer>();
        tvScreen = Common.SearchGameObject(gameObject, "TVscreen").GetComponent<TVScreen>();

        // Gombok szkriptjének beállítása
        foreach (Button button in GetComponentsInChildren<Button>())
        {
            // Ha a gomb a menün található, akkor nem állítjuk be a buttonClick-jét
            if (Common.IsDescendant(menu.transform, button.transform)) continue;
            button.buttonClick = ButtonClick;
        }

        firstTime = true;
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

        // A feladatban használt képeket betötljük
        pictures = new Dictionary<string, Sprite>();
        foreach (TaskBoomData.AnswerData item in taskData.GetAnswers())
        {
            if (item.answer[0] == '#')
            {
                //Sprite sprite = Common.menuLessonPlan.GetPicture(item.Substring(1));

                Sprite sprite = taskData.gameData.GetSprite(item.answer.Substring(1));

                yield return null;

                if (sprite != null)
                    pictures.Add(item.answer, sprite);

                /*
                yield return Common.pictureController.LoadSpriteFromFileSystemCoroutine(item.Substring(1));
                if (Common.pictureController.resultSprite != null) // Ha sikeres volt a kép beolvasás akkor a képet a szótárba tesszük
                    pictures.Add(item, Common.pictureController.resultSprite);

                */
            }
        }

        // Beállítjuk a kérdést
        questionText.text = taskData.question;
        // Ha nincs a kérdésnek szövege, akkor a kérdés 1 méretű egyébként 0.001f
        if (!string.IsNullOrEmpty(taskData.question))
            questionMove.localScale = Vector3.one * 0.001f; // összenyomjuk a kérdést, hogy elő tudjon ugrani

        // Lekérdezzük a megadandó válaszok számát és létrehozzuk a szükséges mennyiségű tárolót
        int holdingNeed = (taskData.needGoodAnswers) ? taskData.goodAnswers.Count : taskData.wrongAnswers.Count;

        // Kiszámoljuk az elemek elhelyezésének bázis pozícióját
        float sizeX = GetMaxSize(tvScreen.GetWidth(), tvScreen.GetHeight(), holdingNeed);
        if (sizeX > tvScreen.GetWidth()) sizeX = tvScreen.GetWidth(); // A válaszok ne lehessenek nagyobbak mint az eredeti tévé képernyője
        float sizeY = sizeX / tvScreen.GetWidth() * tvScreen.GetHeight();

        float fullSizeX = (columns + (columns - 1) * padding) * sizeX;
        float fullSizeY = (rows * sizeY + (rows - 1) * padding * sizeX);

        Vector3 availableArea = topRight.transform.position - bottomLeft.transform.position;
        Vector3 basePos = new Vector3(
            ((availableArea.x - fullSizeX) + sizeX) / 2 + bottomLeft.transform.position.x,
            ((availableArea.y - fullSizeY) + sizeY) / -2 + topRight.transform.position.y);

        // Elhelyezzük a szükséges elemeket
        tvScreenList = new List<TVScreen>();
        for (int i = 0; i < holdingNeed; i++)
        {
            TVScreen newTVScreen = Instantiate(tvScreen.gameObject).GetComponent<TVScreen>();
            newTVScreen.Reset();
            newTVScreen.transform.SetParent(background.transform);
            newTVScreen.transform.localScale = Vector3.one * sizeX / tvScreen.GetWidth();

            int aktRow = i / columns;
            int aktColumn = i % columns;

            newTVScreen.transform.position = basePos + new Vector3(
                aktColumn * sizeX + aktColumn * padding * sizeX,
                -(aktRow * sizeY + aktRow * padding * sizeX));

            newTVScreen.ShowQuestionMark();
            newTVScreen.tvScreenMove.transform.localScale = Vector3.one * 0.001f; // Összenyomjuk a válaszokat, hogy elő tudjanak ugrani

            tvScreenList.Add(newTVScreen);
        }

        SetPictures();
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő képernyő. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        status = Status.Init;

        // Lekérdezzük a feladat adatait
        taskData = (TaskBoomData)Common.taskController.task;

        yield return StartCoroutine(PrepareTask());
    }

    // Kiszámoljuk, hogy mekkorának kell lennie egy elemnek, hogy kiférjen a megadott számú
    // needItems    - Hány elemet szeretnénk elhelyezni
    // padding      - Az elemek között hány százalék a térköz az elem méretéhez képest
    //                pl. 0.1 esetén a térköz az elem méretének 10%-a
    // xSize        - Az elhelyezendő elemek x mérete
    // ySize        - Az elhelyezendő elemek y mérete
    float GetMaxSize(float xSize, float ySize, int needItems) {

        float ratio = xSize / ySize;
        float paddingX = padding;
        float paddingY = paddingX / ySize;

        Vector3 availableArea = topRight.transform.position - bottomLeft.transform.position;

        columns = 1;
        rows = 1;
        float resultSize;

        //float tvScreenWidth = tvScreen.GetWidth();
        //float tvScreenHeight = tvScreen.GetHeight();

        do
        {
            // Kiszámoljuk, hogy a megadott darabszám esetén, mekkorának kell lennie az elemnek
            float sizeX = availableArea.x / (columns + (columns - 1) * paddingX);
            float sizeY = availableArea.y / (rows + (rows - 1) * paddingY);
            sizeY *= ratio; 

            bool greaterIsX = (sizeX > sizeY);

            resultSize = (greaterIsX) ? sizeX : sizeY;

            int calculatedX = (int)(availableArea.x / (resultSize * (1 + padding)));
            if (resultSize * (1 + padding) * calculatedX + resultSize < availableArea.x)
                calculatedX++;

            int calculatedY = (int)(availableArea.y / (resultSize * (1 + padding)));
            if (resultSize + (1 + padding) * calculatedY + resultSize < availableArea.y)
                calculatedY++;

            if (((greaterIsX) ? columns * calculatedY : rows * calculatedX) >= needItems)
            {
                if (greaterIsX)
                    rows--;
                else
                    columns--;

                break;
            }

            if (greaterIsX)
                columns++;
            else
                rows++;

        } while (true);

        rows = needItems / columns;
        if (needItems % columns != 0)
            rows++;

        return resultSize;
    }

    // Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        // Felugrik a kérdés szövege
        iTween.ScaleTo(questionMove.gameObject, iTween.Hash("islocal", true, "scale", Vector3.one, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutElastic));
        Common.audioController.SFXPlay("boing");
        yield return new WaitForSeconds(taskData.animSpeed1); // Várunk amíg az animáció befejeződik

        // Megjelennek a kérdések, egymás után felugranak
        foreach (TVScreen tvScreen in tvScreenList)
        {
            iTween.ScaleTo(tvScreen.tvScreenMove, iTween.Hash("islocal", true, "scale", Vector3.one, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutElastic));
            Common.audioController.SFXPlay("boing");
            yield return new WaitForSeconds(0.2f); // Várunk 2 tizedmásodpercet
        }

        yield return new WaitForSeconds(taskData.animSpeed1);

        // Ha ez az első kérdés, akkor a TV bemelegszik (láthatóvá válik folyamatosan a csatorna hiba)
        if (firstTime) {
            tvScreen.TVOn(2);
            yield return new WaitForSeconds(2);
        }

        Common.audioController.SetBackgroundMusic(1, 0.05f, 4); // Elindítjuk a háttérzenét

        status = Status.Play;
        Common.HHHnetwork.messageProcessingEnabled = true;
    }

    /*
    // Meghatározzuk a kérdések sorrendjét
    void SetQuestionOrder() {
        actQuestion = 0;
        questionOrder = new List<string>(questionList);

        if (taskData.random)
            Common.Shuffle(questionOrder);
    }
    */

    // A kérdéshez tartozó játék elemeket kell eltávolítani, ha meghívják ezt a metódust. A TaskController fogja új feladatnál ha az új feladatot ugyan annak a képernyőnek kell megjelenítenie
    // Meglehet adni neki egy callBack függvényt, amit akkor hív meg ha végzet a játék elemek elrejtésével, mivel ez sokáig is eltarthat és addig nem kéne tovább menni az új feladatra.
    override public IEnumerator HideGameElement()
    {
        clock.Reset(1); // Az órát alaphelyzetbe állítja

        //switchOn.SetActive(false);
        goodLight.gameObject.SetActive(false);
        badLight.gameObject.SetActive(false);
        tvScreen.ShowQuestion(false);

        // A kérdés összezsugorodik
        iTween.ScaleTo(questionMove.gameObject, iTween.Hash("islocal", true, "scale", Vector3.one * 0.001f, "time", taskData.animSpeed1, "easeType", iTween.EaseType.linear));
        yield return new WaitForSeconds(1);

        // Lehullanak a válaszok
        for (int i = tvScreenList.Count - 1; i >= 0; i--)
        {
            iTween.MoveTo(tvScreenList[i].gameObject, iTween.Hash("y", -2, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeInCirc));
            Destroy(tvScreenList[i].gameObject, taskData.animSpeed1);
            yield return new WaitForSeconds(0.2f);
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

        tvScreen.Reset();
        //switchOn.SetActive(false);
        goodLight.gameObject.SetActive(false);
        badLight.gameObject.SetActive(false);

        // Eltüntetjük a korábban létrehozott válaszok tárolására szánt objektumokat
        foreach (TVScreen tvScreen in tvScreenList)
            Destroy(tvScreen.gameObject);

        firstTime = true; // A következő játék indításnál újra legyen tv "bemelegedés" effect

        yield return null;
    }

    // Update is called once per frame
    /*
    void Update()
    {
        menu.menuEnabled = (status == Status.Play);

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
        taskData = (TaskBoomData)task;

        yield return StartCoroutine(PrepareTask());

        // Megjelenítjük a kérdés szövegét
        questionMove.transform.localScale = Vector3.one;

        // Megjelenítjük a válaszok helyét és bennük a helyes választ
        for (int i = 0; i < tvScreenList.Count; i++)
        {
            tvScreenList[i].tvScreenMove.transform.localScale = Vector3.one;

            // Lekérdezzük a mutatandó választ
            string answer = (taskData.needGoodAnswers) ? taskData.goodAnswers[i].answer : taskData.wrongAnswers[i].answer;

            // Megjelenik a jó válasz a válaszok között
            tvScreenList[i].BackgroundGreen();
            tvScreenList[i].ShowAnswer(answer, (pictures.ContainsKey(answer)) ? pictures[answer] : null, 0, 0);
        }

        tvScreen.TVOn(0);
    }

    // A menüből kiválasztották a kilépést a játékból
    IEnumerator ExitCoroutine()
    {
        status = Status.Exit;
        clock.Stop();

        Common.taskController.GameExit();
        yield return null;
    }

    void EvaluateCoroutine(JSONNode jsonData)
    {
        switch (jsonData[C.JSONKeys.evaluateAnswer].Value)
        {
            case C.JSONValues.evaluateIsTrue:
                // Helyes a válasz
                goodLight.gameObject.SetActive(true);

                int answerIndex = jsonData[C.JSONKeys.answerCount].AsInt;
                string answer = jsonData[C.JSONKeys.selectedAnswer].Value;

                // Megjelenik a jó válasz a válaszok között
                tvScreenList[answerIndex].BackgroundGreen();
                tvScreenList[answerIndex].ShowAnswer(tvScreen.GetText(), (pictures.ContainsKey(answer)) ? pictures[answer] : null);

                Common.audioController.SFXPlay("positive");

                break;

            case C.JSONValues.evaluateIsFalse:
                // A válasz helytelen
                badLight.gameObject.SetActive(true);

                Common.audioController.SFXPlay("negative");

                break;
        }
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
                    EvaluateCoroutine(jsonNodeMessage);

                    break;

                case C.JSONValues.show:
                    status = Status.Play;

                    string question = jsonNodeMessage[C.JSONKeys.showItem].Value;

                    if (!string.IsNullOrEmpty(question))
                    {
                        // Bekapcsoljuk a TV-n a soron következő elemet
                        if (question[0] == '#' && pictures.ContainsKey(question))
                            // Képet mutatunk, ha a soronkövetkező elem kép és sikeres volt a betöltése
                            tvScreen.SetPicture(pictures[question]);
                        else
                            // Egyébként a megadott szöveget írjuk ki, vagy a kép nevét ha nem sikerült a kép betöltése
                            tvScreen.SetText(question);

                        tvScreen.ShowQuestion(true);
                    }
                    else {
                        tvScreen.ShowQuestion(false);

                        goodLight.gameObject.SetActive(false);
                        badLight.gameObject.SetActive(false);
                    }

                    break;

                case C.JSONValues.nextPlayer:
                    status = Status.Play;

                    break;
            }
        }
    }

    // Ha rákattintottak egy gombra, akkor meghívódik ez az eljárás a gombon levő Button szkript által
    override protected void ButtonClick(Button button)
    {
        if (userInputIsEnabled)
        {
            switch (button.buttonType)
            {
                case Button.ButtonType.Exit:
                    StartCoroutine(ExitCoroutine());
                    break;

                case Button.ButtonType.Switch:
                    // Elküldjük a szervernek, hogy megérintették a tv-t

                    // Ha játékmódban vagyunk, akkor elküldjük a játékos választását
                    JSONClass jsonClass = new JSONClass();
                    jsonClass[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
                    jsonClass[C.JSONKeys.gameEventType] = C.JSONValues.answer;
                    //jsonClass[C.JSONKeys.selectedAnswer] = panel.GetText();

                    Common.taskController.SendMessageToServer(jsonClass);
                    break;

                case Button.ButtonType.SwitchLayout: // Megnyomták a layout váltó gombot
                    //layoutManager.ChangeLayout();
                    //SetPictures();
                    break;
            }
        }
    }

    void SetPictures()
    {
        background.GetComponent<SpriteRenderer>().sprite = layoutManager.GetSprite("background");
        foreground.sprite = layoutManager.GetSprite("foreground");

        questionText.color = layoutManager.GetColor("questionTextColor");

        TV.sprite = layoutManager.GetSprite("tv");
        tvScreen.Set(
            layoutManager.GetSprite("screen"),
            layoutManager.GetSprite("questionMark"),
            layoutManager.GetSprite("channelError"),
            layoutManager.GetColor("answerTextColor")
            );

        goodLight.sprite = layoutManager.GetSprite("goodAnswer");
        badLight.sprite = layoutManager.GetSprite("badAnswer");

        // Lecseréljük az összes válasz képernyőn a képeket
        foreach (TVScreen tvScreenItem in tvScreenList)
        {
            tvScreenItem.Set(
                layoutManager.GetSprite("screen"),
                layoutManager.GetSprite("questionMark"),
                layoutManager.GetSprite("channelError"),
                layoutManager.GetColor("answerTextColor")
                );
        }

        // beállítjuk az óra layout-ját is
        clock.SetPictures();
    }
}


