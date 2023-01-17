using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimpleJSON;
using UnityEngine.Networking;

public class EvaluationScreenSingle : HHHScreen {

    EvaluationMulti evaluationMulti;
    EvaluationSingle evaluationSingle;
    Badges badges;

    public JSONNode jsonData; // Az adatok amiket meg kell jeleníteni az értékelő képernyőn

    bool multi;                 // Csoport értéklelő képernyő kell vagy nem
    bool onlyBadges;            // Csak a jelvény képernyőt kell megmutatni
    bool changeScreenHappen;    // Éppen képernyő csere történik

    // Use this for initialization
    void Awake () {
        Common.evaluationScreenSingle = this;

        evaluationMulti = GetComponentInChildren<EvaluationMulti>(true);
        evaluationSingle = GetComponentInChildren<EvaluationSingle>(true);
        badges = GetComponentInChildren<Badges>(true);
	}

    /// <summary>
    /// Mielőtt a képernyő láthatóvá válna a ScreenController meghívja ezt a metódust, hogy inicializája magát. 
    /// </summary>
    /// <returns></returns>
    override public IEnumerator InitCoroutine()
    {
        //jsonData[C.JSONKeys.levelBorder].AsInt = 10;

        if (jsonData[C.JSONKeys.evaluate].AsBool)
        {
        }

        evaluationMulti.InitInit(jsonData, ButtonClick);

        InitScreens();

        /*
        badges.Init(jsonData);
        evaluationSingle.Init(jsonData, ButtonClick);
        evaluationMulti.Init(jsonData, ButtonClick);

        multi = jsonData[C.JSONKeys.multi].AsBool;

        // Bekapcsoljuk az első képernyőt
        // Ha a jsonData-ban az onlyBadge false, akkor értékelő képernyővel kezdünk
        // egyébként az érem képernyővel
        onlyBadges = jsonData[C.JSONKeys.onlyBadge].AsBool;
        ChageScreen(onlyBadges);
        */
        yield break;
    }

    void InitScreens() {
        badges.Init(jsonData);
        evaluationSingle.Init(jsonData, ButtonClick);
        evaluationMulti.Init(jsonData, ButtonClick);

        multi = jsonData[C.JSONKeys.multi].AsBool;

        // Bekapcsoljuk az első képernyőt
        // Ha a jsonData-ban az onlyBadge false, akkor értékelő képernyővel kezdünk
        // egyébként az érem képernyővel
        onlyBadges = jsonData[C.JSONKeys.onlyBadge].AsBool;
        ChageScreen(onlyBadges);
    }

    // Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        Show();

        Common.HHHnetwork.callBackNetworkEvent = ReceivedNetworkEvent;
        Common.HHHnetwork.messageProcessingEnabled = true;

        yield break;
    }

    void Show() {

        if (jsonData[C.JSONKeys.lessonPlanEnd].AsBool)
        {
            Common.infoPanelInformationWithOkButton.Show(C.Texts.YouAreFinishedTheLessonPlan, true, (string bName) => {
                Common.menuInformation.Hide(() => {

                    jsonData[C.JSONKeys.onlyBadge].AsBool = true;
                    InitScreens();
                    //ButtonClick("Badge");
                });
            });
        }
        else {
            if (!onlyBadges)
                if (multi)
                    evaluationMulti.Show();
                //StartCoroutine(evaluationMulti.Show());
                else
                    StartCoroutine(evaluationSingle.Show());
        }
    }

    void ChageScreen(bool badge) {
        badges.gameObject.SetActive(badge);

        evaluationSingle.SetActive(!badge && !multi);
        evaluationMulti.SetActive(!badge && multi);
    }

    void ChangeScreenAnim(bool badge) {
        changeScreenHappen = true;
        Common.fadeEffect.FadeInColor(effectSpeed: 0.25f, callBack: () =>
        {
            ChageScreen(badge);
            Common.fadeEffect.FadeOut(effectSpeed: 0.25f, callBack: () => {
                changeScreenHappen = false;
            });
        });
    }

    public void UpdateData(JSONNode jsonData) {
        this.jsonData = jsonData;

        InitScreens();
        Show();

        /*
        badges.Init(jsonData);
        evaluationMulti.Init2(jsonData, ButtonClick);
        evaluationMulti.Show();
        */
    }

    // Ha rákattintottak egy válaszra, akkor meghívódik ez az eljárás a válaszpanelen levő Button szkript által
    public void ButtonClick(string buttonName)
    {
        if (changeScreenHappen) return;

        switch (buttonName)
        {
            case "Play":
                // Eltüntetjük a play gombot
                if (multi)
                    evaluationMulti.HidePlayButton();
                else
                    evaluationSingle.HidePlayButton();

                // Leállítjuk az esetlegesen járó számlálót
                evaluationMulti.StopCountDown();
                evaluationSingle.StopCountDown();

                // Ha nem az utolsó óra-mozaik volt
                if (!this.jsonData[C.JSONKeys.lessonPlanEnd].AsBool)
                {
                    // Üzenetet küldünk a szervernek, hogy kész vagyunk
                    JSONClass jsonData = new JSONClass();

                    jsonData[C.JSONKeys.dataContent] = C.JSONValues.nextOk;

                    Common.HHHnetwork.SendMessageClientToServer(jsonData);
                }
                else { // Ha az utolsó óra-mozaik volt
                    Common.infoPanelInformationWithOkButton.Show(C.Texts.YouAreFinishedTheLessonPlan, true, (string bName) => {
                        Common.menuInformation.Hide( () => {
                            ButtonClick("Badge");
                        });
                    });
                }

                break;

            case "Badge":
                ChangeScreenAnim(true);

                break;

            case "Back":
                ChangeScreenAnim(false);

                break;
        }
    }

    /// <summary>
    /// Esemény érkezett a hálózaton.
    /// </summary>
    /// <param name="networkEventType">A hálózati esemény típusa.(connect, data, disconnect, stb.)</param>
    /// <param name="connectionID">Melyik kapcsolat azonosítón jött be.</param>
    /// <param name="receivedData">A fogadott adat JSONNode formájában.</param>
    void ReceivedNetworkEvent(NetworkEventType networkEventType, int connectionID, JSONNode receivedData)
    {
        switch (networkEventType)
        {
            case NetworkEventType.DataEvent:
                string dataContent = receivedData[C.JSONKeys.dataContent];

                switch (dataContent)
                {
                    case C.JSONValues.pauseOn:
                        evaluationSingle.paused = true;
                        evaluationMulti.paused = true;
                        break;

                    case C.JSONValues.pauseOff:
                        evaluationSingle.paused = false;
                        evaluationMulti.paused = false;
                        break;
                }

                break;
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
