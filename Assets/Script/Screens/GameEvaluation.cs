using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEvaluation : MonoBehaviour, IDataProvider {

    static public GameEvaluation instance;

    RectTransform rectTransformCanvas;
    RectTransform rectTransformMove;
    GameObject gameObjectSimpleTable;
    GameObject gameObjectfullTable;

    GameObject levelUp, levelStay, levelDown;

    GameObject coin;    // Zseton ki/be kapcsolására

    GameObject textPointBase; // A pontokat és a pontok keretét tartalmazó gameObject
    Text textPoints;
    Text textGratulation;
    GameObject textLevelInfo;

    Text textButtonNext;
    Text textButtonExit;

    GameObject nextButton;

    ProgressBar progressBar;

    GameObject Server2020;

    RectTransform[] stars = new RectTransform[3];

    Common.CallBack_In_String callBack;
    Common.CallBack hideCallBack;

    JSONNode data;

    void Awake()
    {
        instance = this;

        rectTransformCanvas = gameObject.SearchChild("Canvas").GetComponent<RectTransform>();
        rectTransformMove = gameObject.SearchChild("Move").GetComponent<RectTransform>();

        gameObjectfullTable = gameObject.SearchChild("ImageBackground").gameObject;
        gameObjectSimpleTable = gameObject.SearchChild("SimpleTable").gameObject;

        levelUp = gameObject.SearchChild("ImageLevelUp").gameObject;
        levelStay = gameObject.SearchChild("ImageLevelStay").gameObject;
        levelDown = gameObject.SearchChild("ImageLevelDown").gameObject;

        coin = gameObject.SearchChild("ImagePCoin").gameObject;

        textPointBase = gameObject.SearchChild("TextPoint").gameObject;
        textPoints = gameObject.SearchChild("TextPoints").GetComponent<Text>();
        textGratulation = gameObject.SearchChild("TextGratulation").GetComponent<Text>();
        textLevelInfo = gameObject.SearchChild("TextLevel").gameObject;

        textButtonNext = gameObject.SearchChild("TextNext").GetComponent<Text>();
        textButtonExit = gameObject.SearchChild("TextExit").GetComponent<Text>();

        nextButton = gameObject.SearchChild("ButtonNext").gameObject;

        progressBar = gameObject.SearchChild("ProgressBar").GetComponent<ProgressBar>();

        Server2020 = gameObject.SearchChild("Server2020").gameObject;

        transform.position = Vector3.zero;

        // Csillagok begyűjtése
        for (int i = 0; i < 3; i++)
            stars[i] = gameObject.SearchChild("ImageStar" + (i + 1)).GetComponent<RectTransform>();
    }

    void Start()
    {
        HideImmediatelly();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="starNumber"></param>
    /// <param name="points"></param>
    /// <param name="button2Enabled">Kell-e következő tananyag gomb.</param>
    /// <param name="callBack"></param>
    public void Show(int starNumber, int? points, bool button2Enabled, Common.CallBack_In_String callBack, JSONNode jsonStatusData = null)
    {
        string button2Text = null;
        string button1Text = null;
        string text = null;
        int? levelIndicator = null;
        int possibleMaxLevelNumber = 1;
        bool isComplete = false;
        bool simpleTable = false;

        data = jsonStatusData;

        Server2020.SetActive(jsonStatusData != null);

        if (jsonStatusData != null)
        {
            coin.SetActive(data[C.JSONKeys.coinReceived].AsBool);

            button2Text = C.Texts.Next; // nextGame; // Curriculum; // Game;
            text = data[C.JSONKeys.statusInfo].Value;
            levelIndicator = (data.ContainsKey(C.JSONKeys.levelIndicator) ? (int?)data[C.JSONKeys.levelIndicator].AsInt : null);

            possibleMaxLevelNumber = data[C.JSONKeys.possibleMaxLevelNumber].AsInt;
            isComplete = data[C.JSONKeys.isComplete].AsBool;
            simpleTable = data[C.JSONKeys.nextGamePartOfLastGame].AsBool;

            progressBar.SetValue(data[C.JSONKeys.learnRoutePathProgress].AsFloat / 100);

            textLevelInfo.SetActive(possibleMaxLevelNumber > 1);
        }

        //=  string button2Text = null, string button1Text = null, string text = null, int? levelIndicator = null

        //button2Text: C.Texts.Next,
        //text: statusData[C.JSONKeys.statusInfo].Value,
        //levelIndicator: (statusData.ContainsKey(C.JSONKeys.levelIndicator) ? (int?)statusData[C.JSONKeys.levelIndicator].AsInt : null)

        this.callBack = callBack;

        // Beállítjuk a következő tananyag gombot
        nextButton.SetActive(button2Enabled);

        iTween.Stop(gameObject); // Leállítjuk a korábbi animációkat, ha még mennének

        textPointBase.SetActive(levelIndicator == null);

        //levelUp.SetActive(levelIndicator != null && levelIndicator.Value == 1);
        //levelStay.SetActive(levelIndicator != null && levelIndicator.Value == 0);
        //levelDown.SetActive(levelIndicator != null && levelIndicator.Value == -1);
        levelUp.SetActive(possibleMaxLevelNumber > 1 && levelIndicator.Value == 1);
        levelStay.SetActive(possibleMaxLevelNumber > 1 && levelIndicator.Value == 0);
        levelDown.SetActive(possibleMaxLevelNumber > 1 && levelIndicator.Value == -1);

        // Kiírjuk a pontot és a csillagokat megmutatjuk
        textPoints.text = (points != null) ? points.Value.ToString() : "";
        for (int i = 0; i < stars.Length; i++)
            stars[i].gameObject.SetActive(i < starNumber);

        // Kiírjuk a szövegeket
        //textGratulation.text = Common.languageController.Translate(text != null ? text : C.Texts.gameGratulation); // Ezt máshol állítjuk be
        textButtonNext.text = Common.languageController.Translate(button2Text != null ? button2Text : C.Texts.Next);
        textButtonExit.text = Common.languageController.Translate(button1Text != null ? button1Text : C.Texts.exit);

        // Elindítjuk az animációt, megjelenik az értékelő tábla
        Tween.TweenAnimation animation = new Tween.TweenAnimation(
            startPos: -rectTransformCanvas.sizeDelta.y, //rectTransformMove.anchoredPosition.y,
            endPos: 0f,
            easeType: Tween.EaseType.easeOutExpo,
            time: 1,
            onUpdate: UpdateBackgroundPos
        );

        Tween.StartAnimation(animation);




        //iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformImageBackground.anchoredPosition.y, "to", 0, "easetype", iTween.EaseType.easeOutExpo, "onupdate", "UpdateBackgroundPos", "onupdatetarget", gameObject));

        Common.fade.Show(Color.white, 0.4f, 1, null);

        // Előugranak a csillagok


        // A pontok leszámolnak

        // Bekapcsoljuk a megfelelő táblát
        gameObjectSimpleTable.SetActive(simpleTable);
        gameObjectfullTable.SetActive(!simpleTable);

        // Ki / be kaapcsoljuk a gameObject-et így kikényszerítünk egy újra fordítást, mivel minden OnEnabled-kor megtörténik
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void HideImmediatelly() {
        // Eltüntetjük az értékelő képernyőt
        rectTransformMove.anchoredPosition = new Vector2(0, -rectTransformCanvas.sizeDelta.y);
        Common.fade.HideImmediatelly();
    }

    public void Hide(Common.CallBack callBack = null)
    {
        //this.hideCallBack = callBack;

        Tween.TweenAnimation animation = new Tween.TweenAnimation(
            startPos: 0f,
            endPos: -rectTransformCanvas.sizeDelta.y,
            easeType: Tween.EaseType.easeOutCubic,
            time: 0.5f,
            onUpdate: UpdateBackgroundPos,
            onComplete: () => {
                if (callBack != null)
                    callBack();
            }
        );

        Tween.StartAnimation(animation);

        Common.fade.Hide(0.5f);
    }

    void UpdateBackgroundPos(object o) // float pos)
    {
        float pos = (float)o;
        rectTransformMove.anchoredPosition = new Vector2(rectTransformMove.anchoredPosition.x, pos);
    }

    public void ButtonClick(string buttonName)
    {
        Debug.Log(buttonName);

        if (callBack != null)
            callBack(buttonName);
    }



    public string DataProvider(string token)
    {
        Debug.Log("DataProvider: " + token);
        string result = null;

        if (data != null)
        {
            switch (token)
            {
                case C.JSONKeys.gameScore: result = data.ContainsKey(C.JSONKeys.gameScore) ? data[C.JSONKeys.gameScore].Value + "/" + data[C.JSONKeys.maxGameScore] + " " + Common.languageController.Translate(C.Texts.Point) : ""; break;

                    /*
                //case C.JSONKeys.learnRoutePathScore: result = Common.languageController.Translate(C.Texts.OnTheRouteSoFar) + Common.GroupingNumber(data[C.JSONKeys.learnRoutePathScore].AsInt); break;
                    */

                case C.JSONKeys.levelIndicatorText:
                    result = "";

                    if (data != null &&
                        data[C.JSONKeys.possibleMaxLevelNumber].AsInt > 1 &&
                        data[C.JSONKeys.levelIndicatorText].Value != "levelStay")
                    {
                        result = Common.languageController.Translate(data[C.JSONKeys.levelIndicatorText]) + "\n";
                    }

                    //result = (data != null) ? data[C.JSONKeys.levelIndicatorText] != "levelStay" ? Common.languageController.Translate(data[C.JSONKeys.levelIndicatorText]) + "\n" : "" : ""; 
                    break;
                case C.JSONKeys.statusInfo: result = (data != null) ? Common.languageController.Translate(data[C.JSONKeys.statusInfo]) : C.Texts.gameGratulation; break;
                case C.Texts.Level: result = Common.languageController.Translate(C.Texts.Level) + data[C.JSONKeys.currentLevelNumber] + "/" + data[C.JSONKeys.possibleMaxLevelNumber]; break;
                case C.JSONKeys.learnRoutePathProgress: result = ((int)data[C.JSONKeys.learnRoutePathProgress].AsFloat).ToString(); break;
                case C.JSONKeys.nextGameScore: result = Common.GroupingNumber(data[C.JSONKeys.nextGameScore].AsInt, ",") + " " + Common.languageController.Translate(C.Texts.Point); break;
                case C.JSONKeys.learnRoutePathScore: result = Common.GroupingNumber(data[C.JSONKeys.learnRoutePathScore].AsInt, ","); break;
                case "pathScoreValue": result = Common.GroupingNumber(data[C.JSONKeys.learnRoutePathScore].AsInt, ","); break;

                case "actLevelValue": result = data[C.JSONKeys.currentLevelNumber]; break;
                case "maxLevelValue": result = data[C.JSONKeys.possibleMaxLevelNumber]; break;
                case "nextUnitValue": result = Common.GroupingNumber(data[C.JSONKeys.nextGameScore].AsInt, ","); break;
            }
        }

        /*
        paramValues.Add(C.JSONKeys.gameScore, data.ContainsKey(C.JSONKeys.gameScore) ? data[C.JSONKeys.gameScore].Value + "/" + data[C.JSONKeys.maxGameScore] + " " + Common.languageController.Translate(C.Texts.Point) : "");
        paramValues.Add(C.JSONKeys.learnRoutePathScore, Common.languageController.Translate(C.Texts.OnTheRouteSoFar) + Common.GroupingNumber(data[C.JSONKeys.learnRoutePathScore].AsInt));
        paramValues.Add(C.JSONKeys.levelIndicatorText, (data != null) ? Common.languageController.Translate(data[C.JSONKeys.levelIndicatorText]) + "\n" : "");
        paramValues.Add(C.JSONKeys.statusInfo, (data != null) ? Common.languageController.Translate(data[C.JSONKeys.statusInfo]) : C.Texts.gameGratulation);
        paramValues.Add(C.Texts.Level, Common.languageController.Translate(C.Texts.Level) + data[C.JSONKeys.currentLevelNumber] + "/" + data[C.JSONKeys.possibleMaxLevelNumber]);
        paramValues.Add(C.JSONKeys.learnRoutePathProgress, data[C.JSONKeys.learnRoutePathProgress]);
        paramValues.Add(C.JSONKeys.nextGameScore, Common.GroupingNumber(data[C.JSONKeys.nextGameScore].AsInt, ",") + " " + Common.languageController.Translate(C.Texts.Point));
        */


        return result;
    }
}
