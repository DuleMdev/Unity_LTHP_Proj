using UnityEngine;
using System.Collections;

using System.Collections.Generic;

using SimpleJSON;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Game_Hangman : TrueGameAncestor
{
//    [Tooltip("A kérdés betüinek prefabja.")]
//    public GameObject LetterPrefab;
    [Tooltip("A kérdés betüi közötti távolság a betű méretéhez visszonyítva.")]
    public float distanceBetweenChars;

    Text textScreenQuestion;

    GameObject topRight;        // A betük megjelenés területének bal felső sarka
    GameObject bottomLeft;      // A betük megjelenés területének jobb alsó sarka

    Letter letterPrefab;        // Ezt klónozzuk a betűk létrehozásakor
    HangmanMonster monster;     // A szörny vezérlő szkriptje
    ABC_Table abcTable;         // A választható betűket tartalmazó tábla

    // ------------------------------------------------------------
    TaskHangmanData taskData;  // A feladatot tartalmazó objektum

    List<string> lines;         // A sorokra bontott szöveg

    List<Letter> listOfLetters; // A kérdés szövegének betűi

    override public void Awake()
    {
        base.Awake();

        textScreenQuestion = gameObject.SearchChild("textQuestion").GetComponent<Text>();

        topRight = Common.SearchGameObject(background.gameObject, "TopRight").gameObject;
        bottomLeft = Common.SearchGameObject(background.gameObject, "BottomLeft").gameObject;
        letterPrefab = Common.SearchGameObject(background.gameObject, "Letter").GetComponent<Letter>();
        monster = Common.SearchGameObject(background.gameObject, "Monster").GetComponent<HangmanMonster>();
        abcTable = Common.SearchGameObject(background.gameObject, "abcTable").GetComponent<ABC_Table>();

        // Gombok szkriptjének beállítása
        //foreach (Button button in GetComponentsInChildren<Button>())
        //    button.buttonClick = ButtonClick;

        letterPrefab.Awake(); // Mivel inaktív a letter prefab, ezért nem fut le az Awake metódusa, de ez kell ahhoz, hogy meg tudjuk hívni a GetHeight metódusát, ezért az Awake metódusát direktbe meghívjuk
    }

    /// <summary>
    /// Felkészülünk a feladat megmutatására.
    /// </summary>
    /// <returns></returns>
    override public IEnumerator PrepareTask()
    {
        clock.timeInterval = taskData.time;
        clock.Reset(0);

        exitButton.SetActive(false); // Common.configurationController.deviceIsServer);

        // Beállítjuk a képernyő kérdést
        textScreenQuestion.text = taskData.taskDescription;
        textScreenQuestion.transform.localScale = Vector3.one * 0.001f;

        monster.Init(taskData.tries); // Alaphelyzetbe állítjuk a szörnyet

        //abcTable.Init(KeyButtonClick, taskData.keyset);
        abcTable.Init(KeyButtonClick, taskData.gameData.keyset); // Common.languageController.actLanguageID); // A készülék nyelve határozza meg a billentyűzetet

        // Ha a betűk már létre vannak hozva, akkor megsemmísítjük őket
        if (listOfLetters != null)
            foreach (Letter letter in listOfLetters)
                Destroy(letter.gameObject);

        // Kiszámoljuk, hogy mekkorák lehetnek a betűk, hogy a legjobban kiférjenek a képernyő adott területére
        float size = GetLetterSize(taskData.puzzle);
        //size = Mathf.Clamp(size, 0, 2); // Beállítjuk, hogy a méret ne legyen nagyobb mint ...

        // Mekkora lesz egy betű
        Vector2 letterSize = new Vector2(letterPrefab.GetWidth() * size, letterPrefab.GetHeight() * size);
        // Mekkora távolságra van két betű középpontja a következőtől vízszintesen és függőlegesen
        Vector2 letterDistance = letterSize + Vector2.one * letterPrefab.GetWidth() * size * distanceBetweenChars;

        int maxLinesLength = 0;
        foreach (string line in lines)
            if (line.Length > maxLinesLength)
                maxLinesLength = line.Length;

        // Mekkora helyre fér ki a kiírandó szöveg
        float textWidth = letterSize.x + (maxLinesLength - 1) * letterDistance.x;
        float textHeight = letterSize.y + (lines.Count - 1) * letterDistance.y;
        Vector3 availableArea = topRight.transform.position - bottomLeft.transform.position;

        // Kiszámoljuk a bázis pozícióját vagyis az első sor első betűjének a helyét
        Vector3 basePos = new Vector3(
            ((availableArea.x - textWidth) + letterSize.x) / 2 + bottomLeft.transform.position.x,
            ((availableArea.y - textHeight) + letterSize.y) / -2 + topRight.transform.position.y);

        // Létrehozzuk a betűket
        listOfLetters = new List<Letter>();
        Vector3 lineBasePos = basePos; // A sor első betűjének a helye
        foreach (string lineText in lines)
        {
            Vector3 letterPos = lineBasePos;    // A betű pozíciója
            foreach (char letter in lineText)
            {
                if (letter != ' ')
                {
                    Letter newLetter = Instantiate(letterPrefab.gameObject, background.transform).GetComponent<Letter>();
                    newLetter.Init(letter.ToString());
                    //newLetter.transform.SetParent(background.transform);
                    newLetter.transform.localScale = Vector3.one * size;
                    newLetter.transform.position = letterPos;

                    // Ha a betű nem választható az abc tábláról, akkor alapból láthatóvá tesszük
                    if (!abcTable.allLetters.Contains(letter.ToString()))
                    {
                        newLetter.SetBackground(false);
                        newLetter.ShowText();
                    }

                    //newLetter.SetPictures(layoutManager.GetSprite("answer"), layoutManager.GetColor("answer"));
                    listOfLetters.Add(newLetter);
                }

                letterPos += new Vector3(letterDistance.x, 0, 0);
            }

            lineBasePos -= new Vector3(0, letterDistance.y, 0);
        }

        yield break;
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát
    override public IEnumerator InitCoroutine()
    {
        status = Status.Init;

        // Lekérdezzük a feladat adatait
        taskData = (TaskHangmanData)Common.taskController.task;

        yield return StartCoroutine(PrepareTask());
    }

    /// <summary>
    /// Vissza adja, hogy mekkorának kell lennie a betűknek, hogy kiférjen a teljes szöveg a rendelkezésre álló helyre
    /// </summary>
    /// <returns></returns>
    float GetLetterSize(string text) {

        Vector3 availableArea = topRight.transform.position - bottomLeft.transform.position;

        float maxSize = availableArea.y / letterPrefab.GetHeight();
        float actSize = maxSize;
        float goodSize = 0;

        // 20 iterációban közelítjük a megfelelő méretet
        for (int i = 0; i < 20; i++)
        {
            // Kiszámoljuk, hogy az adott méret mellett hány sor és oszlop fér ki a rendelkezésre álló területen
            int lineLength = (int)((availableArea.x - letterPrefab.GetWidth() * (goodSize + actSize)) / 
                ((letterPrefab.GetWidth() + letterPrefab.GetWidth() * distanceBetweenChars) * (goodSize + actSize))) + 1;
            int rowCount = (int)((availableArea.y - letterPrefab.GetHeight() * (goodSize + actSize)) / 
                ((letterPrefab.GetHeight() + letterPrefab.GetHeight() * distanceBetweenChars) * (goodSize + actSize))) + 1;

            // Megpróbáljuk a szöveget feldarabolni, hogy beférjen a sor és oszlop számba
            List<string> lines = WrapText(text, lineLength, rowCount);
            if (lines != null)
            { // Ha sikerült, akkor a méret megfelelő
                goodSize += actSize;
                this.lines = lines;
                Debug.Log("new size : " + goodSize + " | column : " + lineLength + " | row : " + rowCount);
            }
            else {
                Debug.Log("wrong size : " + (goodSize + actSize) + " | column : " + lineLength + " | row : " + rowCount);
            }

            actSize /= 2; // Tovább próbálkozunk egy kisebb méret hozzáadásával
        }

        return Mathf.Min(goodSize, maxSize);
    }

    /// <summary>
    /// Felbontja a megadott szöveget sorokra. Egy sorba maximum a lineLength által meghatározott karakter állhat.
    /// A vissza adott érték null ha van olyan szó a szövegben ami egymaga hosszabb mint a lineLength vagy több
    /// sorra van szükség a megadottnál.
    /// </summary>
    /// <param name="text">A felbontandó szöveg</param>
    /// <param name="lineLength">Egy sorba levő karakterek száma</param>
    /// <param name="maxRowNumber">Egy sorba levő karakterek száma</param>
    /// <returns></returns>
    List<string> WrapText(string text, int lineLength, int maxRowNumber) {
        string[] substrings = text.Split(' ');

        // Sorokba szervezzük a szavakat
        List<string> result = new List<string>();
        result.Add("");
        foreach (string word in substrings)
        {
            // Ha valamelyik szó önmagában hosszabb mint a megengedett sorhossz, akkor hiba
            if (word.Length > lineLength)
                return null;

            // Hozzáadjuk az utolsó sorhoz az aktuális szót ha még odafér
            string lastRow = result[result.Count - 1];
            if (lastRow.Length + word.Length + ((string.IsNullOrEmpty(lastRow)) ? 0 : 1) <= lineLength)
            {
                if (!string.IsNullOrEmpty(lastRow))
                    lastRow += " ";

                lastRow += word;

                result[result.Count - 1] = lastRow;
            }
            else { // Ha nem fér oda, akkor egy új sort kezdünk a szóval
                result.Add(word);
            }
        }

        // Ha több sor született a megengedetnél, akkor null-t adunk vissza egyébként az eredményt
        return (result.Count > maxRowNumber) ? null : result;
    }

    // Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        Debug.Log(Common.Now() + " - ScreenShowFinishCoroutine - Start");

        Debug.Log(Common.Now() + " - Show question text");
        // Megjelenítjük a kérdés szövegét ha van
        if (!string.IsNullOrEmpty(taskData.taskDescription))
        {
            iTween.ScaleTo(textScreenQuestion.gameObject, iTween.Hash("islocal", true, "scale", Vector3.one, "time", taskData.animSpeed1, "easeType", iTween.EaseType.easeOutElastic));
            Common.audioController.SFXPlay("boing");
            yield return new WaitForSeconds(taskData.animSpeed1);
        }

        Debug.Log(Common.Now() + " - Show letters");
        // Megjelennek a betűk
        float waitTime = 0;
        foreach (Letter letter in listOfLetters)
        {
            Debug.Log(Common.Now() + " - Show - " + letter.letter);

            do
            {
                // Ha még nem telt el a szükséges idő, akkor újra fog indulni a ciklus, de előtt ezt a kört befejezzük, hogy a következőbe frissüljön a deltatime
                if (waitTime < taskData.animSpeed2)
                {
                    yield return null;
                    waitTime += Time.deltaTime;
                }

            } while (waitTime < taskData.animSpeed2);

            letter.Show();
            waitTime -= taskData.animSpeed2;
        }

        /*
        foreach (Letter letter in listOfLetters)
        {
            Debug.Log(Common.Now() + " - Show - " + letter.letter);
            letter.Show();
            yield return new WaitForSeconds(taskData.animSpeed2); // Várunk egy keveset a következő betű megjelenítéséig
        }
        */

        //yield return new WaitForSeconds(taskData.animSpeed1); // Várunk amíg az animáció befejeződik

        Debug.Log(Common.Now() + " - Show monster");
        monster.MonsterIn(taskData.animSpeed1 * 4.5f);
        //yield return new WaitForSeconds(monster.MonsterIn()); // Várunk amíg az animáció befejeződik

        Debug.Log(Common.Now() + " - Show table");
        abcTable.Show(taskData.animSpeed1);

        yield return new WaitForSeconds(taskData.animSpeed1); // Várunk amíg az animáció befejeződik

        Common.audioController.SetBackgroundMusic(1, 0.05f, 4); // Elindítjuk a háttérzenét

        status = Status.Play;
        Common.HHHnetwork.messageProcessingEnabled = true;

        Debug.Log(Common.Now() + " - ScreenShowFinishCoroutine - Start");

        yield return null;
    }

    // A kérdéshez tartozó játék elemeket kell eltávolítani, ha meghívják ezt a metódust. A TaskController fogja új feladatnál ha az új feladatot ugyan annak a képernyőnek kell megjelenítenie
    // Meglehet adni neki egy callBack függvényt, amit akkor hív meg ha végzet a játék elemek elrejtésével, mivel ez sokáig is eltarthat és addig nem kéne tovább menni az új feladatra.
    override public IEnumerator HideGameElement()
    {
        clock.Reset(1); // Az órát alaphelyzetbe állítja

        // Összezsugorítjuk a betüket, majd a végén eltüntetjük őket
        foreach (Letter letter in listOfLetters)
            letter.Hide();
        listOfLetters = null;

        yield return new WaitForSeconds(1); // Várunk amíg az animáció befejeződik

        yield return new WaitForSeconds(monster.MonsterOut());

        yield return new WaitForSeconds(abcTable.Hide());
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

    // Játéknak vége letelt az idő, vagy a játékot sikeresen megoldottuk
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
        taskData = (TaskHangmanData)task;

        yield return StartCoroutine(PrepareTask());

        // Megjelenítjük a kérdés szövegét
        textScreenQuestion.transform.localScale = Vector3.one;

        // Megjelennek a betűk
        foreach (Letter letter in listOfLetters)
        {
            letter.Show(0);
            letter.ShowText();
        }

        monster.MonsterIn(0); // Megjelenik a szörny

        abcTable.Show(0); // Megjelenik az ABC tábla
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

    /// <summary>
    /// Ha rákattintottak egy betű gombra, akkor meghívódik ez az eljárás.
    /// </summary>
    /// <param name="keyButton">Melyik betű gombra kattintottak rá.</param>
    void KeyButtonClick(KeyButton keyButton) {
        if (status == Status.Play) {
            // Elküldjük a szervernek, hogy melyik gombot nyomták meg

            // Ha játékmódban vagyunk, akkor elküldjük a játékos választását
            JSONClass jsonClass = new JSONClass();
            jsonClass[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
            jsonClass[C.JSONKeys.gameEventType] = C.JSONValues.answer;
            jsonClass[C.JSONKeys.selectedAnswer] = keyButton.key;

            Common.taskController.SendMessageToServer(jsonClass);
        }
    }

    IEnumerator EvaluateCoroutine(JSONNode jsonData)
    {
        KeyButton keyButton = abcTable.GetKeyButton(jsonData[C.JSONKeys.selectedAnswer].Value);

        // Ha nincs meg a kereett billentyű, akkor kilépünk
        if (keyButton == null) yield break;

        switch (jsonData[C.JSONKeys.evaluateAnswer].Value)
        {
            case C.JSONValues.evaluateIsTrue:
                // Helyes a válasz

                // Ha igen, akkor bekapcsoljuk a szükséges betűket és zöldel villogtatjuk őket
                foreach (Letter letter in listOfLetters)
                    if (letter.letter == keyButton.key)
                    {
                        letter.ShowText();
                        letter.Flashing();
                    }

                // A megnyomott betűt elszűrkítjük
                keyButton.Deactivate();
                Common.audioController.SFXPlay("positive");

                break;

            case C.JSONValues.evaluateIsFalse:
                // A válasz helytelen

                // Az elemet inaktívvá tesszük és villogtatjuk pirossal
                keyButton.Deactivate();
                keyButton.Flashing();
                Common.audioController.SFXPlay("negative");

                monster.BallonFlyAway();

                break;
        }

        yield return new WaitForSeconds(1.2f);

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

    /*
    // Task controller hívja meg ha történt valamilyen esemény
    // networkEvent változóban található a történt esemény
    // jsonNode-ban esetleg lehetnek további paraméterek az esemény kiegészítésére
    override public void EventHappened(JSONNode jsonNode)
    {
        switch (jsonNode[C.JSONKeys.gameEvent])
        {
            case C.NetworkGameEvent.KeyPressed:
                status = Status.Result;
                StartCoroutine(EvaluateCoroutine(jsonNode[C.JSONKeys.gameEventData]));
                break;
            case C.NetworkGameEvent.OutOfTime:
                Common.audioController.SFXPlay("negative");

                Common.taskControllerOld.GameEventHappend(TaskControllerOld.GameEvent.OutOfTime);
                StartCoroutine(GameEnd());

                //StartCoroutine(EvaluationCoroutine(null));
                break;
        }
    }
    */

    // Ha rákattintottak egy gombra, akkor meghívódik ez az eljárás a gombon levő Button szkript által
    /*
    override protected void ButtonClick(Button button)
    {
        if (userInputIsEnabled)
        {
            switch (button.buttonType)
            {
                case Button.ButtonType.Exit: // Ha megnyomták a kilépés gombot
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