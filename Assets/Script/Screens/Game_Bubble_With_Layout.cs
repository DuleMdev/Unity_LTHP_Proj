using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;
using SimpleJSON;
using UnityEngine.Networking;

public class Game_Bubble_With_Layout : TrueGameAncestor
{
    public GameObject bubblePrefab;

    LayoutManager layoutManager;        // A különböző layoutokhoz tartozó képeket tartalmazza
    GameObject extraObjects;

    GameObject questionMove;            // A kérdés előugrásához kell
    Text textQuestion;                  // A kérdés szövegének kiírásához
    Text textNotice;                    // Segítség a játékhoz (pukkaszd ki a rossz választ tartalmazó buborékokat)

    GameObject questionPictureMove;     // A kérdés képét tartalmazó gameObject, az (előugráshoz kell)
    Image imageQuestionPicture;         // A kérdéshez tartozó kép
    Image imageBorder;                  // A kép körül levő keret
    GameObject bubbleBirthPos;          // A buborékok születésének pozícióját tárolja
    ParticleSystem particleSystemPopOut;// A buborékok kipukkanásakor történő effect

    TaskBubbleData taskData;

    List<Bubble2> listOfBubble = new List<Bubble2>();   // A létrehozott buborékok listája

    // Use this for initialization
    override public void Awake()
    {
        base.Awake();

        layoutManager = GetComponentInChildren<LayoutManager>();

        extraObjects = Common.SearchGameObject(gameObject, "ExtraObjects").gameObject;

        questionMove = Common.SearchGameObject(gameObject, "move_question").gameObject;
        textQuestion = Common.SearchGameObject(gameObject, "textQuestion").GetComponent<Text>();
        textNotice = Common.SearchGameObject(gameObject, "textNotice").GetComponent<Text>();
        particleSystemPopOut = Common.SearchGameObject(gameObject, "bubbleBursts").GetComponent<ParticleSystem>();

        questionPictureMove = Common.SearchGameObject(gameObject, "QuestionPictureMove").gameObject;
        imageQuestionPicture = Common.SearchGameObject(gameObject, "ImageQuestion").GetComponent<Image>();
        imageBorder = Common.SearchGameObject(gameObject, "ImageBorder").GetComponent<Image>();

        bubbleBirthPos = Common.SearchGameObject(gameObject, "bubbleBirthPos").gameObject;

        // Gombok szkriptjének beállítása
        foreach (Button button in GetComponentsInChildren<Button>())
        {
            // Ha a gomb a menün található, akkor nem állítjuk be a buttonClick-jét
            if (Common.IsDescendant(menu.transform, button.transform)) continue;
            button.buttonClick = ButtonClick;
        }

        listOfBubble = new List<Bubble2>(GetComponentsInChildren<Bubble2>(true));
    }

    void DeleteAllBubble()
    {
        // Ha léteznek már buborékok, akkor azokat megsemmisítjük
        // Végig megyünk a buborékok listáján és megsemmísítjük őket
        foreach (Bubble2 bubble in listOfBubble)
        {
            bubble.transform.localScale = Vector3.zero;
            GameObject.Destroy(bubble.gameObject);
        }
        listOfBubble.Clear();
    }

    /// <summary>
    /// Felkészülünk a feladat megmutatására.
    /// </summary>
    /// <returns></returns>
    override public IEnumerator PrepareTask()
    {
        DeleteAllBubble();

        menu.Reset();
        menu.gameObject.SetActive(false);

        exitButton.SetActive(false); // Common.configurationController.deviceIsServer);

        clock.timeInterval = taskData.time;
        clock.Reset(0);

        imageQuestionPicture.enabled = false;
        imageBorder.enabled = false;
        // A kérdés képét elhelyezzük és összezsugorítjuk
        if (!string.IsNullOrEmpty(taskData.questionPicture))
        {
            string pictureName = taskData.questionPicture;
            if (pictureName[0] == '#')
                pictureName = pictureName.Substring(1);


            imageQuestionPicture.sprite = taskData.gameData.GetSprite(pictureName);

            yield return null;


            //yield return Common.pictureController.LoadSpriteFromFileSystemCoroutine(pictureName);
            //imageQuestionPicture.sprite = Common.pictureController.resultSprite;



            imageQuestionPicture.enabled = true;

            // Beállítjuk a kép körül a keret láthatóságát, attól függően, hogy kell-e vagy sem
            imageBorder.enabled = taskData.questionPictureBorder;
        }

        textQuestion.text = taskData.question;
        //textNotice.text = "(" + Common.languageController.Translate("PopOutWrongAnswers") + ")";
        //textNotice.text = "(" + taskData.screenQuestion + ")";
        // Ha van utasítás szöveg, akkor azt írjuk ki egyébként a default szöveget (Pukkaszd ki a rossz választ tartalmazó buborékokat)
        textNotice.text = "(" + ((string.IsNullOrEmpty(taskData.taskText)) ? Common.languageController.Translate("PopOutWrongAnswers") : taskData.taskText) + ")";

        // Létrehozunk annyi buborékot, amennyi szükséges
        //List<string> listOfAnswers = taskData.GetAnswers();

        foreach (int answerID in taskData.GetAnswers())
        {
            // Létrehozunk egy buborékot
            GameObject newBubbleGameObject = GameObject.Instantiate(bubblePrefab);
            newBubbleGameObject.SetActive(false);

            //newBuble.transform.position = new Vector2(Common.random.NextDouble())
            Bubble2 bubbleScript = newBubbleGameObject.GetComponent<Bubble2>(); // Megkeressük a Bubble szkriptet

            // Kitatálunk egy színt a buboréknak
            Color newColor = new Color(
                (float)(Common.random.NextDouble() * 0.5 + 0.5),
                (float)(Common.random.NextDouble() * 0.5 + 0.5),
                (float)(Common.random.NextDouble() * 0.5 + 0.5),
                1f
                );

            // A buborék inicializálása
            if (Common.configurationController.isServer2020)
                yield return StartCoroutine(bubbleScript.InitializeServer2020(taskData, answerID, new Vector2((float)(Common.random.NextDouble() * 2 * Camera.main.aspect - Camera.main.aspect + transform.position.x), (float)(Common.random.NextDouble() * 2 - 1 + transform.position.y)), newColor));
            else
                yield return StartCoroutine(bubbleScript.InitializeServerOld(taskData, answerID, new Vector2((float)(Common.random.NextDouble() * 2 * Camera.main.aspect - Camera.main.aspect + transform.position.x), (float)(Common.random.NextDouble() * 2 - 1 + transform.position.y)), newColor));
            //yield return StartCoroutine(bubbleScript.Initialize(taskData, answerID, new Vector2((float)(Common.random.NextDouble() * 2 * Camera.main.aspect - Camera.main.aspect + transform.position.x), (float)(Common.random.NextDouble() * 2 - 1 + transform.position.y)), newColor));  // A buborékot pozícionáljuk, megadjuk a színét és a szövegét

            newBubbleGameObject.transform.SetParent(background.transform);          // Beállítjuk a buborék szülő GameObject-jét a backround GameObject-re
            newBubbleGameObject.transform.localScale = Vector3.one * 0.8f;          // A buborék mérete 0.8

            newBubbleGameObject.GetComponent<Button>().buttonClick = BubbleClick; // Beállítjuk a boborékra kattintást feldolgozó metódust

            listOfBubble.Add(bubbleScript);
        }

        SetPictures();

        yield return null;
    }

    override public IEnumerator InitCoroutine()
    {
        status = Status.Init;

        // Lekérdezzük a feladat adatait
        taskData = (TaskBubbleData)Common.taskController.task;

        questionPictureMove.transform.localScale = Vector3.one * 0.001f;
        questionMove.transform.localScale = Vector3.one * 0.001f;

        StartCoroutine(PrepareTask());

        yield return null;
    }

    // Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        // Megjelenítjük a kérdés képét ha van
        if (!string.IsNullOrEmpty(taskData.questionPicture))
        {
            iTween.ScaleTo(questionPictureMove, iTween.Hash("islocal", true, "scale", Vector3.one, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutElastic));
            Common.audioController.SFXPlay("boing");
            yield return new WaitForSeconds(taskData.animSpeed1);
        }

        // Megjelenítjük a kérdés szövegét
        iTween.ScaleTo(questionMove, iTween.Hash("islocal", true, "scale", Vector3.one, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutElastic));
        Common.audioController.SFXPlay("boing");
        yield return new WaitForSeconds(taskData.animSpeed1);

        // Megjelenítjük a buborékokat
        yield return StartCoroutine(ShowBubble(taskData.animSpeed1, 0.1f, true));

        Common.audioController.SetBackgroundMusic(1, 0.05f, 4); // Elindítjuk a háttérzenét

        status = Status.Play;
        Common.HHHnetwork.messageProcessingEnabled = true;
    }

    // A kérdéshez tartozó játék elemeket kell eltávolítani, ha meghívják ezt a metódust. A TaskController fogja új feladatnál ha az új feladatot ugyan annak a képernyőnek kell megjelenítenie
    // Meglehet adni neki egy callBack függvényt, amit akkor hív meg ha végzet a játék elemek elrejtésével, mivel ez sokáig is eltarthat és addig nem kéne tovább menni az új feladatra.
    override public IEnumerator HideGameElement()
    {
        clock.Reset(1); // Az órát alaphelyzetbe állítja

        // Lezsugorítjuk a kérdést és a képet
        iTween.ScaleTo(questionPictureMove, iTween.Hash("islocal", true, "scale", Vector3.one * 0.0001f, "time", taskData.animSpeed1, "easeType", iTween.EaseType.linear));
        iTween.ScaleTo(questionMove, iTween.Hash("islocal", true, "scale", Vector3.one * 0.0001f, "time", taskData.animSpeed1, "easeType", iTween.EaseType.linear));

        // Végig megyünk a buborékok listáján és amelyik látható azt kipukkasztjuk
        foreach (Bubble2 bubble in listOfBubble)
        {
            if (bubble.gameObject.activeSelf) // Ha nem látható megjelenítjük
            {
                BubblePopOut(bubble.gameObject);
                yield return new WaitForSeconds(0.5f); // Várunk egy keveset
            }
        }

        yield return new WaitForSeconds(taskData.animSpeed1); // Várunk egy keveset
    }

    // A képernyő teljesen eltünk. Csinálhatunk valamit ilyenkor ha szükséges
    // De figyelni kell, mivel a következő pillanatban már inaktív lesz az egész képernyő. 
    // Tehát azonnal meg kell tennünk amit akarunk nem indíthatunk a képernyőn egy coroutine-t mivel úgy sem fog lefutni.
    // Kikapcsolt gameObject-eken nem fut a coroutine.
    // Ezt a ScreenController hívja meg képernyő váltásnál
    override public IEnumerator ScreenHideFinish()
    {
        StartCoroutine(base.ScreenHideFinish());

        DeleteAllBubble();

        yield return null;
    }

    // Játéknak vége letelt az idő, vagy a játék befejeződött
    override public IEnumerator GameEnd()
    {
        status = Status.Result;

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
        taskData = (TaskBubbleData)task;

        questionPictureMove.transform.localScale = Vector3.one;
        questionMove.transform.localScale = Vector3.one;

        yield return StartCoroutine(PrepareTask());
        yield return StartCoroutine(ShowBubble(0, 0, false));
    }

    // Update is called once per frame
    /*
    void Update()
    {
        menu.menuEnabled = (status == Status.Play);

        if (status == Status.Play)
            clock.Go();
        else
            clock.Stop();
    }
    */

    // 
    /// <summary>
    /// A nem látható buborékokat megjelenítjük egymás után 
    /// </summary>
    /// <param name="animSpeed">Mennyi idő alatt születik meg a buborék</param>
    /// <param name="waitBetweenBubbleBorn">A buborékok születései közötti idő</param>
    /// <param name="sfx">legyen-e hangja a buborékok születésének (preview módban nem kell).</param>
    /// <returns></returns>
    IEnumerator ShowBubble(float animSpeed, float waitBetweenBubbleBorn, bool sfx)
    {
        // Végig megyünk a buborékok listáján és amelyik nem látható azt láthatóvá tesszük
        foreach (Bubble2 bubble in listOfBubble)
        {
            if (!bubble.gameObject.activeSelf) // Ha nem látható megjelenítjük
            {
                yield return new WaitForSeconds(waitBetweenBubbleBorn); // Várunk egy keveset

                bubble.gameObject.SetActive(true);
                yield return StartCoroutine(bubble.InitializeServer2020(position: bubbleBirthPos.transform.position));   // Beállítjuk a buborék kezdeti pozícióját
                bubble.BubbleBorn(animSpeed);                           // Majd megszületik, azaz felfújódik :)
                if (sfx)
                    Common.audioController.SFXPlay("bubbleStart");
            }
        }

        // Várunk amíg az utolsó animáció is befejeződik
        yield return new WaitForSeconds(animSpeed);
    }

    // Villogtatja az élő buborékokat
    void FlashBubbles(Color color)
    {
        // Végig megyünk a buborékok listáján és amelyik nem látható azt láthatóvá tesszük
        foreach (Bubble2 bubble in listOfBubble)
            if (bubble.gameObject.activeSelf) // Ha nem látható megjelenítjük
                bubble.FlashGlow(color);
    }

    /// <summary>
    /// A megadott GameObject-et kikapcsoljuk és a gameObject helyén egy kipukkanás effektet játszunk le
    /// </summary>
    /// <param name="booble">Az eltüntetendő gameObject</param>
    void BubblePopOut(GameObject booble)
    {
        booble.SetActive(false); // Kipukkasztjuk a buborékot
        Common.audioController.SFXPlay("bubbleKilled");
        particleSystemPopOut.transform.position = booble.transform.position;
        particleSystemPopOut.Simulate(0.1f);
        particleSystemPopOut.Play();
    }

    // Ha rákattintottak a buborékra, akkor meghívódik ez az eljárás a buborékon levő Button szkript által
    void BubbleClick(Button button)
    {
        if (status == Status.Play)
        { // Ha játékmódban vagyunk, akkor elküldjük a kattintás eseményt a szervernek
            JSONClass jsonClass = new JSONClass();
            jsonClass[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
            jsonClass[C.JSONKeys.gameEventType] = C.JSONValues.answer;
            jsonClass[C.JSONKeys.selectedAnswer].AsInt = button.GetComponent<Bubble2>().answerID;

            Common.taskController.SendMessageToServer(jsonClass);
        }
    }

    IEnumerator ExitCoroutine()
    {
        status = Status.Exit;
        Common.taskController.GameExit();

        yield return null;
    }

    /*
    void EvaluateCoroutine(JSONNode jsonData)
    {
        switch (jsonData[C.JSONKeys.gameEventType])
        {
            case C.JSONValues.goodSolution:

                break;

            case C.JSONValues.wrongSolution:

                break;
        }
    }
    */

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

                    // Kipukasztjuk az elküldött felíratú buborékot az aktívak közül
                    foreach (Bubble2 bubble in listOfBubble)
                        if (bubble.gameObject.activeSelf && bubble.answerID == jsonNodeMessage[C.JSONKeys.selectedAnswer].AsInt)
                        {
                            BubblePopOut(bubble.gameObject);
                            break;
                        }

                    break;
                case C.JSONValues.goodSolution:
                    // Helyes a válasz
                    status = Status.Result;

                    FlashBubbles(Color.green);
                    Common.audioController.SFXPlay("positive");

                    break;

                case C.JSONValues.wrongSolution:
                    // A válasz helytelen
                    status = Status.Result;

                    FlashBubbles(Color.red);
                    Common.audioController.SFXPlay("negative");

                    Common.configurationController.WaitTime(1.2f, () => {
                        StartCoroutine(ShowBubble(taskData.animSpeed1, taskData.animSpeed2, true));
                    });

                    break;



                // Ez nem jó helyen van - A gameEventType-nak nincs nextPlayer értéke, a dataContent-nek van olyan értéke
                case C.JSONValues.nextPlayer:
                    status = Status.Play;

                    break;
            }

            if (jsonNodeMessage[C.JSONKeys.dataContent].Value == C.JSONValues.nextPlayer)
                status = Status.Play;


        }
    }

    // Ha rákattintottak egy colliderre amin található egy Button szkript, akkor meghívódik ez az eljárás a gameObject-en levő Button szkript által
    override protected void ButtonClick(Button button)
    {
        switch (button.buttonType)
        {
            case Button.ButtonType.Exit:
                if (status != Status.Exit)
                    StartCoroutine(ExitCoroutine());
                break;

            case Button.ButtonType.SwitchLayout: // Megnyomták a layout váltó gombot
                //layoutManager.ChangeLayout();
                //SetPictures();
                break;
        }
    }

    /* Ezt már a szerveren (tanári tablet) ellenőrízzük
    // Ellenőrízzük, hogy a maradék buborék jó válasz-e
    IEnumerator VerifyCoroutine()
    {
        // Megszámoljuk, hány buborék van még életben
        int liveBubble = 0;
        foreach (Bubble2 bubble in listOfBubble)
            if (bubble.gameObject.activeSelf)
                liveBubble++;

        // Ha egyenlő a kívánt jó válaszok számával
        if (liveBubble == taskData.goodAnswerPiece)
        {

            // Akkor megnézzük, hogy a még létező buborékok a jó válaszok közül valóak-e
            bool goodAnswer = true;
            foreach (Bubble2 bubble in listOfBubble)
                if (bubble.gameObject.activeSelf)
                    if (!taskData.goodAnswers.Contains(bubble.text.text))
                    {
                        goodAnswer = false;
                        break;  // Megszakítjuk a ciklust, felesleges tovább futnia, mert találtunk egy rossz választ
                    }

            if (goodAnswer)
            {   // Ha igen, akkor vége
                status = Status.Result;

                FlashBubbles(Color.green);
                Common.audioController.SFXPlay("positive");
                yield return new WaitForSeconds(1.2f);

                Common.taskControllerOld.TaskEnd(null);
                //StartCoroutine(ExitCoroutine());
            }
            else
            {   // Ha nem újra termeljük a buborékokat
                status = Status.Init;

                FlashBubbles(Color.red);
                Common.audioController.SFXPlay("negative");
                yield return new WaitForSeconds(1.2f);

                yield return StartCoroutine(ShowBubble(taskData.animSpeed1, taskData.animSpeed2, true));
                status = Status.Play;
            }
        }
    }
    */

    /// <summary>
    /// A megadott layout-nak megfelelően beállítja a képeket.
    /// Meghívása előtt a LayoutManager-ben ki kell választani a megfelelő képi világot.
    /// </summary>
    void SetPictures()
    {
        background.GetComponent<SpriteRenderer>().sprite = layoutManager.GetSprite("background");

        foreach (Bubble2 bubble in listOfBubble)
        {
            bubble.SetPictures(
                layoutManager.GetSprite("bubble"), layoutManager.GetColor("bubble"),
                layoutManager.GetSprite("gleam"), layoutManager.GetColor("gleam"),
                layoutManager.GetSprite("bubblePictureBorder"), layoutManager.GetColor("bubblePictureBorder")
                );
        }

        // Ki vagy bekapcsoljuk az extra tartalmat a beállított design-nak megfelelően
        extraObjects.SetActive(LayoutManager.actLayoutSetName == "1");

        // beállítjuk az óra layout-ját is
        clock.SetPictures();
    }

}

