using UnityEngine;
using System.Collections;
using SimpleJSON;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TrueGameAncestor : GameAncestor {

    enum TextComponentType
    {
        Text,
        TEXDraw
    }

    //TextComponentType textComponentType = TextComponentType.TEXDraw;
    TextComponentType textComponentType = TextComponentType.Text;

    protected enum Status
    {
        Init,           // A játék inicializálása majd a megjelenítése zajlik, ha a megjelenítés befejeződött, akkor átvált Play módba
        Play,           // Fut a játék, kezeljük a felhasználói inputokat feltéve ha a ScreenController.userInputEnabled nem false értékű
        Pause,          // A játék futása fel van függesztve
        Result,         // Értékeljük a játékot, majd jöhet a következő kérdés vagy ha nincs, akkor kilépünk
        Exit,           // Befejeztük a játékot kilépés zajlik
    }

    protected Background background;

    protected Clock_Ancestor clock;
    protected OldGameMenu menu;
    protected GameObject exitButton;              // Az exit button gameObject-je, amit csak szerveren kell láthatóvá tenni
    protected Zoomer zoomer;

    GameEndingButtons gameEndingButtons;    // Tovább és az újra gombok lenyomásának érzékelője

    protected Status status;

    protected bool paused;      // A játék le van állítva, ekkor nem megy az óra

    protected override bool userInputIsEnabled { get { return !Common.screenController.changeScreenInProgress && status == Status.Play && !paused; } }

    // Use this for initialization
    override public void Awake()
    {
        base.Awake();

        try
        {
            background = GetComponentInChildren<Background>();

            clock = GetComponentInChildren<Clock_Ancestor>();
            menu = GetComponentInChildren<OldGameMenu>();
            exitButton = gameObject.SearchChild("ExitButton").gameObject;
            zoomer = GetComponentInChildren<Zoomer>();

            gameEndingButtons = GetComponentInChildren<GameEndingButtons>();
        }
        catch (System.Exception e)
        {
            Debug.LogError(Common.GetGameObjectHierarchy(gameObject) + "\n" + e.Message + "\n" + e.StackTrace);
        }
    }

    public void Start()
    {
        if (menu)
        {
            menu.buttonClick = ButtonClick;
            menu.Reset();
            menu.gameObject.SetActive(false);
        }

        if (gameEndingButtons)
            gameEndingButtons.Initialize(UIButtonClick);
    }

    /// <summary>
    /// Üzenet érkezett a hálózaton.
    /// </summary>
    /// <param name="networkEventType"></param>
    /// <param name="connectionID"></param>
    /// <param name="jsonNodeMessage"></param>
    override public void MessageArrived(NetworkEventType networkEventType, int connectionId, JSONNode jsonNodeMessage)
    {
        base.MessageArrived(networkEventType, connectionId, jsonNodeMessage);

        if (networkEventType == NetworkEventType.DataEvent) {
            switch (jsonNodeMessage[C.JSONKeys.dataContent])
            {
                case C.JSONValues.pauseOn:
                    paused = true;
                    //status = Status.Pause;

                    break;
                case C.JSONValues.pauseOff:
                    paused = false;
                    //status = Status.Play;

                    break;
                case C.JSONValues.nextPlayer:
                    status = Status.Play;

                    break;
            }

            switch (jsonNodeMessage[C.JSONKeys.gameEventType])
            {
                case C.JSONValues.outOfTime:
                    Common.audioController.SFXPlay("negative");
                    clock.SetTime(0);

                    break;
                case C.JSONValues.gameEnd:
                    status = Status.Exit;

                    break;
            }

            if (jsonNodeMessage[C.JSONKeys.dataContent].Value == C.JSONValues.nextPlayer)
                status = Status.Play;

            // Ha idő beállítás érkezett a hálózaton, akkor beállítjuk az időt
            if (jsonNodeMessage.ContainsKey(C.JSONKeys.timeEvent)) {
                clock.SetTime(jsonNodeMessage[C.JSONKeys.timeEvent].AsFloat);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="button"></param>
    override protected void ButtonClick(Button button)
    {
        switch (button.buttonType)
        {
            case Button.ButtonType.Exit:
                status = Status.Exit;
                clock.Stop();
                Common.taskController.GameExit();

                StopAllCoroutines();
                break;
        }
    }

    public void UIButtonClick(string button)
    {
        //if (userInputIsEnabled)
        {
            string gameEventType = "";

            switch (button)
            {
                case "NextOrSelfVerify": // következő képernyő vagy ön ellenőrzés vagy következő tanegység
                //case "SelfVerify": 
                    if (Common.configurationController.answerFeedback == ConfigurationController.AnswerFeedback.NoFeedback)
                    {
                        // Következő képernyő
                        //GameMenu.instance.ButtonClick(C.Program.GameMenuNext);
                        gameEventType = C.JSONValues.exitScreen;
                    }
                    else
                    {
                        // Önellenőrzés
                        // ??? Hogyan?
                    }

                    // Ha replay módban vagyunk ...
                    if (Common.taskController.task.replayMode)
                    {
                        gameEventType = C.JSONValues.nextReplay;
                    }

                    break;
                case "screenAgain":
                    gameEventType = C.JSONValues.screenAgain;
                    break;

                case "info":
                    // Megmutatjuk a plus információs panelt
                    GamePlusQuestionInfo.instance.Show();
                    infoButton.Stop();

                    break;
            }

            // Elküldjük a feldolgozott gomb nyomást
            if (!string.IsNullOrEmpty(gameEventType))
            {
                JSONClass jsonClass = new JSONClass();
                jsonClass[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
                jsonClass[C.JSONKeys.gameEventType] = gameEventType;
                Common.taskController.SendMessageToServer(jsonClass);
            }
        }
    }

    // Update is called once per frame
    override public void Update()
    {
        base.Update();

        if (menu)
            menu.menuEnabled = (status == Status.Play);

        if (status == Status.Play && !paused) // Ha megy a játék, akkor megy az óra
            clock.Go();
        else
            clock.Stop();
    }
}
