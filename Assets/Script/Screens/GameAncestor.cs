using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameAncestor : HHHScreen
{
    virtual protected bool userInputIsEnabled { get { return !Common.screenController.changeScreenInProgress; } }

    public float elapsedGameTime { get; protected set; }  // Mennyi időt töltött a felhasználó a játékban
    protected float inactiveTimeLimit = 1200;    // 20 perc után inaktívnak tekinti a felhasználót (1200 másodperc)
    protected float inactiveTime; // Mióta inaktív a felhasználó, az utolsó interakció óta mennyi idő telt el

    protected JSONNode finalMessageJson;  // A végső üzenetet gyűjtjük itt

    protected PulseGameObject infoButton;

    // Use this for initialization
    virtual public void Awake()
    {
        /*
        GameObject go = gameObject.SearchChild("InfoButton");
        if (go)
            infoButton = go.GetComponent<PulseGameObject>();

        */
        infoButton = GetComponentInChildren<PulseGameObject>(true);

        //infoButton = gameObject.SearchChild("InfoButton").GetComponent<PulseGameObject>();
    }

    // Amikor az új képernyő megjelenítése elkezdődik akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowStartCoroutine()
    {
        Common.audioController.SFXPlay("gameStart");

        StartCoroutine(base.ScreenShowStartCoroutine());

        ExtendedBackgroundDatas extendedBackgroundDatas = GetComponent<ExtendedBackgroundDatas>();
        Color owlColor = Color.black;
        if (extendedBackgroundDatas != null &&
            extendedBackgroundDatas.extendedBackgroundDataList.Count > 1)
            owlColor = Color.Lerp(extendedBackgroundDatas.extendedBackgroundDataList[0].color, extendedBackgroundDatas.extendedBackgroundDataList[1].color, 0.5f);
        GameMenu.instance.Initialize(ButtonClick, owlColor);
        GameMenu.instance.Enabled(Common.configurationController.deviceIsServer);
        elapsedGameTime = 0;
        yield return null;

        // Bekapcsoljuk a QuestionPlusInfo gombot, ha van plusz info
        if (Common.taskController.task.info != null && infoButton != null)
        {
            infoButton.gameObject.SetActive(true);

            // A plus info-t beírjuk a megjelenítőbe
            GamePlusQuestionInfo.instance.Initialize(
                Common.taskController.task.info,
                Common.taskController.task.gameData.GetSprite,
                Zoomer.instance);
        }
    }

    public override IEnumerator ScreenHideFinish()
    {
        StartCoroutine(base.ScreenHideFinish());

        GameMenu.instance.Enabled(false);
        Zoomer.instance.Reset();
        GamePlusQuestionInfo.instance.Reset();

        yield return null;
    }

    /// <summary>
    /// Üzenet érkezett a hálózaton.
    /// </summary>
    /// <param name="networkEventType"></param>
    /// <param name="connectionID"></param>
    /// <param name="jsonNodeMessage"></param>
    virtual public void MessageArrived(NetworkEventType networkEventType, int connectionId, JSONNode jsonNodeMessage)
    {
        if (networkEventType == NetworkEventType.DataEvent)
        {
            switch (jsonNodeMessage[C.JSONKeys.gameEventType])
            {
                case C.JSONValues.gameEnd:
                    // Elküldjük a végső üzenetet
                    SendFinalMessage();

                    break;
            }
        }
    }

    virtual protected void ButtonClick(string buttonName)
    {
        Button button = new Button();
        switch (buttonName)
        {
            case C.Program.GameMenuPrevious:
            case C.Program.GameMenuExit:
            case C.Program.GameMenuNext:
                SendFinalMessage();
                button.buttonType = Button.ButtonType.Exit;
                ButtonClick(button);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="button"></param>
    virtual protected void ButtonClick(Button button)
    {
        switch (button.buttonType)
        {
            case Button.ButtonType.Exit:
                Common.taskController.GameExit();
                break;
        }
    }

    // Update is called once per frame
    virtual public void Update()
    {
        MeasureElapsedTime();
    }

    virtual protected void MeasureElapsedTime()
    {
        /*
        // Inactivitás mérő Version : 1 
        // Ha user aktivitás történt és nem telt el az inactiveTimeLimit, akkor jóváírjuk az utolsó aktivitástól
        // eltelt időt. Ha túl vagyunk már az inactiveTimeLimit-en, akkor nem.
        // Tehát a felhasználónak aktivitást kell mutatnia az inactiveTimeLimit időn belül, hogy jóvá legyen írva
        // az eltelt idő.
        if (Input.GetMouseButtonDown(0))
        {
            if (inactiveTime < inactiveTimeLimit) 
                elapsedGameTime += inactiveTime;

            inactiveTime = 0;
        }

        inactiveTime += Time.deltaTime;
        */


        // Inactivitás mérő Version : 2
        // A user-nek folyamatosan jóváírjuk az idejét amíg az utolsó activitásától számolva el nem telik az
        // inactiveTimeLimit. Ha a user inactivitása meghaladja az inactiveTimeLimit-et, akkor már nem írjuk
        // jóvá, csak ha úgra aktívvá válik
        if (inactiveTime < inactiveTimeLimit)
            elapsedGameTime += Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            inactiveTime = 0;
        }

        inactiveTime += Time.deltaTime;

        // A végső üzenetben elküldjük
        //if (Common.taskController.gameControl != null)
        //    Common.taskController.gameControl.task.elapsedGameTime = elapsedGameTime;
        //Common.taskController.task.elapsedGameTime = elapsedGameTime;
    }

    public void SendFinalMessage()
    {
        CollectFinalMessage();
        Common.taskController.SendMessageToServer(finalMessageJson);
    }

    virtual public void CollectFinalMessage()
    {
        finalMessageJson = new JSONClass();

        finalMessageJson[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
        finalMessageJson[C.JSONKeys.gameEventType] = C.JSONValues.finalMessage;
        finalMessageJson[C.JSONKeys.elapsedGameTime].AsFloat = elapsedGameTime;
    }
}
