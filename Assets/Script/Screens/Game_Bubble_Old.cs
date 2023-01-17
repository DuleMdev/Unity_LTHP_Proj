using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game_Bubble_Old : HHHScreen {

    enum Status
    {
        Start,          // Az Update-ban figyeljük, hogy indulhat-e a játék, ha elindult, akkor átvált a ShowTask állapotba (Ez csak azért van, hogy ne indítse el a showTask-ot sokszor)
        ShowTask,       // Megjelenítjük a kérdést és a válaszokat tartalmazó buborékokat, ha a megjelenítés befejeződött, akkor átvált Play módba
        Play,           // Fut a játék a játékos kipukkaszthatja a buborékokat
        Result,         // Értékeljük a játékot
    }

    public GameObject bubblePrefab;

    public TextAsset defaultTask;
    public string taskText;

    Background background;

    TextMesh textMeshQuestion;
    TextMesh textMeshNotice;

    Status status;

    GameEngine_Question questionEngine;

    List<Bubble2> listOfBubble = new List<Bubble2>();

	// Use this for initialization
	new void Awake () {
        background = GetComponentInChildren<Background>();

        Common.game_Bubble = this;


	}

    override public IEnumerator InitCoroutine()
    {
        // Ha léteznek már buborékok, akkor azokat megsemmisítjük
        // Végig megyünk a buborékok listáján és megsemmísítjük őket
        foreach (Bubble2 bubble in listOfBubble)
            GameObject.Destroy(bubble.gameObject);

        Debug.Log("InitCoroutine - Start");

        if (string.IsNullOrEmpty(taskText)) {
            taskText = defaultTask.text;
        }

        questionEngine = new GameEngine_Question(new TaskSimpleQuestionData(taskText));

        // Létrehozunk annyi buborékot, amennyi szükséges
        List<string> listOfAnswers = questionEngine.GetAnswers();

        foreach (string answerText in listOfAnswers)
        {
            // Létrehozunk egy buborékot
            GameObject newBubbleGameObject = GameObject.Instantiate(bubblePrefab);



            //newBuble.transform.position = new Vector2(Common.random.NextDouble())
            Bubble2 bubbleScript = newBubbleGameObject.GetComponent<Bubble2>(); // Megkeressük a Bubble szkriptet

            Color newColor = new Color(
                (float)(Common.random.NextDouble() * 0.5 + 0.5),
                (float)(Common.random.NextDouble() * 0.5 + 0.5),
                (float)(Common.random.NextDouble() * 0.5 + 0.5),
                1f
                );

            //bubbleScript.Initialize(transform.position, newColor, answerText);  // A buborékot pozícionáljuk, megadjuk a színét és a szövegét
            //bubbleScript.Initialize(new Vector2((float)(Common.random.NextDouble() * 2 * Camera.main.aspect - Camera.main.aspect + transform.position.x), (float)(Common.random.NextDouble() * 2 - 1 + transform.position.y)), newColor, answerText);  // A buborékot pozícionáljuk, megadjuk a színét és a szövegét
            newBubbleGameObject.transform.SetParent(background.transform);      // Beállítjuk a buborék szülő GameObject-jét a backround GameObject-re
            newBubbleGameObject.transform.localScale = Vector3.one;             // A buborék mérete 1

            newBubbleGameObject.GetComponent<Button>().buttonClick = BubbleClick; // Beállítjuk a boborékra kattintást feldolgozó metódust
            newBubbleGameObject.SetActive(false);

            listOfBubble.Add(bubbleScript);
        }

        status = Status.Start;

        yield return null;
    }

    void ShowTask() {
        status = Status.ShowTask;
        StartCoroutine(ShowTaskCoroutine());
    }

    // Láthatóvá tesszük az összes buborékot
    IEnumerator ShowTaskCoroutine() {

        float animSpeed = 1;

        // Végig megyünk a buborékok listáján és amelyik nem látható azt láthatóvá tesszük
        foreach (Bubble2 bubble in listOfBubble)
        {
            if (!bubble.gameObject.activeSelf) {
                // Várunk egy keveset
                yield return new WaitForSeconds(0.1f);

                // GameObject méretét nullára állítjuk
                bubble.transform.localScale = Vector3.zero;
                bubble.gameObject.SetActive(true);
                // iTween komponensel előugratjuk
                iTween.ScaleTo(bubble.gameObject, iTween.Hash("scale", Vector3.one, "time", animSpeed, "easeType", iTween.EaseType.easeOutElastic));

                //bubble.Initialize(new Vector2((float)(Common.random.NextDouble() * 2 * Camera.main.aspect - Camera.main.aspect + transform.position.x), (float)(Common.random.NextDouble() * 2 - 1 + transform.position.y)));  // A buborékot pozícionáljuk, megadjuk a színét és a szövegét

            }
        }

        // Várunk amíg az utolsó animáció is befejeződik
        yield return new WaitForSeconds(animSpeed);

        status = Status.Play;
    }

    // Update is called once per frame
    void Update () {
        if (!Common.screenController.changeScreenInProgress) {
            if (status == Status.Start) {
                ShowTask();
            }
        }
	}

    // Ha rákattintottak a buborékra, akkor meghívódik ez az eljárás a buborékon levő Button szkript által
    void BubbleClick(Button button)
    {
        if (status == Status.Play) { // Ha játékmódban vagyunk, akkor 
            button.gameObject.SetActive(false); // Kipukkasztjuk a buborékot

            Verify();   // Majd meghívjuk az ellenőrzést
        }
    }

    // Ellenőrízzük, hogy a maradék buborék jó válasz-e
    void Verify() {
        // Megszámoljuk, hány buborék van még életben
        int liveBubble = 0;
        foreach (Bubble2 bubble in listOfBubble)
            if (bubble.gameObject.activeSelf)
                liveBubble++;

        // Ha egyenlő a kívánt jó válaszok számával
        if (liveBubble == questionEngine.taskSimpleQuestionData.goodAnswerPiece) {

            // Akkor megnézzük, hogy a még létező buborékok a jó válaszok közül valóak-e
            bool goodAnswer = true;


            // Ezt a scriptet egyébként sem használom már

            //foreach (Bubble2 bubble in listOfBubble)
            //    if (bubble.gameObject.activeSelf)
            //        if (!questionEngine.taskSimpleQuestionData.goodAnswers.Contains(bubble.text.text)) {
            //            goodAnswer = false;
            //            break;  // Megszakítjuk a ciklust, felesleges tovább futnia, mert találtunk egy rossz választ
            //        }



            if (goodAnswer)
            { // Ha igen, akkor vége
                status = Status.Result;
            }
            else
            { // Ha nem újra termeljük a buborékokat
                ShowTask();
            }
        }
    }
}
