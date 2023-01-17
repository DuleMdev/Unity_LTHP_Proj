using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ball_Game : HHHScreen
{
    public AudioClip positive;
    public AudioClip negative;

    [Tooltip("A labda maximális pattanási magassága")]
    public float ballMaxHeight;
    [Tooltip("A labda minimális pattanási magassága")]
    public float ballMinHeight;
    [Tooltip("Hány másodperc alat forduljon meg a labda")]
    public float ballTurnSpeed;

    [Space]
    [Tooltip("Hány oszlop legyen a játékban")]
    public int columnNumber;

    [Tooltip("Emelkedési szög")]
    public int raisedAngle;

    [Tooltip("Kezdetben hány másodpercig tart egy pattanás")]
    public float startSpeed;

    [Tooltip("Mennyire gyorsuljon fel a játék (2 a kétszerese) ")]
    public float QuickFactor = 2f;


    static ConfigurationController.BallGameDatas ballGameDatas;
    static public Common.CallBack callBack; // Ha befejeztük a játékot mit hívjon meg
    static int gamePrice;   // Hány coinba kerül egy játék
    static bool purchasedGameSuccess;          // Sikerült a játék vásárlás
    static string purchasedGameID = "";        // A megvásárolt játék azonosítója, amit majd vissza kell küldeni az eredménnyel együtt



    Text textActScore;
    Text textBestScore;
    Text textLevel;
    Text textInfo;

    ShowCoinsPiece showCoinsPiece;
    GameObject againGameObject;

    Ball ball;

    AudioSource audioSource;

    // 15 25 50 75 100 150 200 250 400 600 1000


    Transform MovePath;
    GameObject columnPrefab;
    GameObject columnLast;

    GameObject menu;

    bool start = false;
    float time; // Hol tartunk az időben
    float actQuick; // Aktuális gyorsaság

    int lastWholeValue;
    int maxScore;

    List<GameObject> listOfColumns = new List<GameObject>();
    List<bool> listOfColumsColor;

    bool serverCommunicationSuccess;    // Volt kommunikáció a szerverrel

    // Use this for initialization
    public void Awake()
    {
        //ballGameDatas = Common.configurationController.ballGameDatas;

        textActScore = gameObject.SearchChild("TextActScore").GetComponent<Text>();
        textBestScore = gameObject.SearchChild("TextBestScore").GetComponent<Text>();
        textLevel = gameObject.SearchChild("TextLevel").GetComponent<Text>();
        textInfo = gameObject.SearchChild("TextInfo").GetComponent<Text>();

        showCoinsPiece = GetComponentInChildren<ShowCoinsPiece>(true);
        againGameObject = gameObject.SearchChild("Again").gameObject;

        ball = GetComponentInChildren<Ball>();
        audioSource = GetComponent<AudioSource>();
        MovePath = gameObject.SearchChild("MovePath").transform;
        columnPrefab = gameObject.SearchChild("Column").gameObject;
        columnLast = gameObject.SearchChild("ColumnLast").gameObject;

        menu = gameObject.SearchChild("Menu").gameObject;
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        yield return null;
    }

    public override IEnumerator ScreenShowStartCoroutine()
    {
        Initialize();

        yield return null;
    }

    public override IEnumerator ScreenShowFinishCoroutine()
    {
        yield return null;
    }

    void Initialize()
    {
        showCoinsPiece.SetCount(gamePrice);

        columnNumber = ballGameDatas.getMaxCloumnInLevel;

        textLevel.text = Common.languageController.Translate(C.Texts.Level) + ballGameDatas.getLevel;

        // Alapállapotba helyezzük a játékot
        ShowMenu(false);
        time = -4;
        start = true;
        MovePath.localPosition = Vector3.zero;
        ball.transform.localPosition = Vector3.zero;
        textInfo.text = "";
        lastWholeValue = 0;
        ball.stop = false;

        // Kitöröljük a korábban létrehozott GameObject-eket
        for (int i = 0; i <  listOfColumns.Count; i++)
            Destroy(listOfColumns[i]);

        listOfColumns.Clear();

        listOfColumsColor = new List<bool>();
        listOfColumsColor.Add(true); // Az első oszlop színét létrehozzuk (igazából mindegy milyen)
        // Létrehozzuk az új oszlopokat
        for (int i = 1; i <= columnNumber; i++)
        {
            bool color = Random.Range(0, 2) == 0;
            listOfColumsColor.Add(color);

            // Egy oszlop létrehozása
            GameObject newColumn = Instantiate(columnPrefab, MovePath);
            listOfColumns.Add(newColumn);

            newColumn.GetComponent<Image>().color = color ? Color.red : Color.blue;
            newColumn.transform.localPosition = new Vector3(i * 400, 0);
            newColumn.transform.Rotate(0, 0, -raisedAngle);
        }

        columnLast.transform.localPosition = new Vector3((columnNumber + 1) * 400, 0);
    }

    // Eljátszunk egy coin-t
    void GetCoin(Common.CallBack_In_Bool_JSONNode callBack)
    {
        //Elküldjük a választott karaktereket a szervernek is, hogy tudjon róla
        ClassYServerCommunication.instance.PurchaseBonusGame(
            "1", 
            (bool success, JSONNode response) =>
            {
                purchasedGameSuccess = success;

                if (success)
                {
                    //purchasedGameID =
                    start = true;
                }
            }
        );
    }

    public void Update()
    {
        if (!start)
            return;

        float t = time / columnNumber;

        actQuick = Mathf.Lerp(startSpeed, startSpeed / QuickFactor, t);
        time += Time.deltaTime / actQuick;

        ball.Refresh(time, Mathf.Lerp(ballMaxHeight, ballMinHeight, t));


        // Az utolsó oszlop az már a vége
        //int actWholeValue = Mathf.Clamp((int)Mathf.Floor(time), 0, columnNumber - 1);
        int actWholeValue = (int)Mathf.Floor(time);

        if (actWholeValue != lastWholeValue)
        {
            // Megvizsgáljuk, hogy a labdának a megfelelő oldala érinti az oszlopot
            if (actWholeValue >= 1 && actWholeValue < columnNumber)
                if (ball.RedIsBottom() != listOfColumsColor[actWholeValue])
                {
                    // Ha nem, akkor hiba
                    start = false;

                    ball.stop = true;

                    textInfo.text = "Game Over";

                    audioSource.clip = negative;
                    audioSource.Play();

                    ShowMenu(true);
                    return;
                }

            ballGameDatas.bestScore = Mathf.Max(Mathf.Min(actWholeValue, columnNumber), ballGameDatas.bestScore);

            textBestScore.text = Common.languageController.Translate(C.Texts.BestScore) + ballGameDatas.bestScore;
            textActScore.text = Mathf.Min(Mathf.Max(actWholeValue, 0), columnNumber).ToString() + "/" + columnNumber;

            // Szövegek kiírása
            if (actWholeValue == -3) textInfo.text = "3";
            if (actWholeValue == -2) textInfo.text = "2";
            if (actWholeValue == -1) textInfo.text = "1";
            if (actWholeValue == 0) textInfo.text = "Go";
            if (actWholeValue == 1) textInfo.text = "";

            // Ha felértünk a tetejére
            if (actWholeValue == columnNumber + 1)
            {
                audioSource.clip = positive;
                audioSource.Play();

                textInfo.text = "Nyertél";

                ballGameDatas.LevelIncrease();
            }

            lastWholeValue = actWholeValue;
        }

        //if (time >= columnNumber)
        //{
        //}

        if (time > columnNumber + 10)
        {
            start = false;
            ShowMenu(true);

            // Elküldjük az eredményt a szervernek

        }

        // Mozgatjuk az oszlopokat
        if (time > 0 && time < columnNumber + 1)
            MovePath.localPosition = new Vector3(time * -400, 0);

        // Mozgatjuk a labdát
        if (time > columnNumber + 1)
            ball.transform.localPosition = new Vector3((time - (columnNumber + 1)) * 400, 0);
    }

    void ShowMenu(bool show)
    {
        againGameObject.SetActive(gamePrice < CastleGameInventoryScreen.inventoryData.coinsNumber);
        menu.SetActive(show);

        if (show)
        {
            // A játék eredményét elküldjük a szervernek
            ClassYServerCommunication.instance.updateBonusGameLog(
                purchasedGameID,
                ballGameDatas.bestScore,
                ballGameDatas.getLevel,
                null
            /*
            (bool success, JSONNode response) =>
            {

            }
            */
            );
        }
    }

    public void ButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "TurnBall":
                ball.Turn();
                break;

            case "Again":
                BuyAGame( (bool success) => { if (success) Initialize(); } );
                break;

            case "Exit":
                callBack();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Vásárolunk egy játékot és ha sikerült, akkor a callBack függvény true értékkel lesz meghívva egyébként false
    /// </summary>
    /// <returns></returns>
    static public void BuyAGame(Common.CallBack_In_Bool callBack)
    {
        // Vásárolunk egy játékot
        ClassYServerCommunication.instance.PurchaseBonusGame(
            "1",
            (bool success, JSONNode response) =>
            {
                purchasedGameSuccess = success;

                if (success)
                {
                    JSONNode answer = response[C.JSONKeys.answer];

                    // Ha sikeres volt a letöltés, akkor inicializálunk néhány adatot, majd betöltjük a labdás játékot
                    purchasedGameID = answer[C.JSONKeys.bonusGamePurchaseID];

                    ballGameDatas = new ConfigurationController.BallGameDatas(
                        answer[C.JSONKeys.maxGameLevel].AsInt,
                        answer[C.JSONKeys.maxGameScore].AsInt
                        );

                    CastleGameInventoryScreen.inventoryData.coinsNumber = answer[C.JSONKeys.currentBonusCoins].AsInt;
                }

                callBack(success); // Vissza üzenünk, hogy sikerült-e a játék vásárlás
            }
        );
    }

    /// <summary>
    /// Elindítjuk a labdás játékot
    /// </summary>
    /// <param name="gamePrice">Hány Coinba kerül a játék</param>
    /// <param name="callBack"></param>
    static public void Load(int gamePrice, Common.CallBack callBack)
    {
        Ball_Game.callBack = callBack;
        Ball_Game.gamePrice = gamePrice;

        Common.screenController.ChangeScreen(C.Screens.Ball_Game);

    }
}

