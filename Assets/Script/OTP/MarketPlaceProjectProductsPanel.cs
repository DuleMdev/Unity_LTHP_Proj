using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketPlaceProjectProductsPanel : MonoBehaviour
{
    Text textInvestmentPeriod;
    Text textRemainHours;

    GameObject goNoInvestmentOpportunities;

    RectTransform content;              // Ebbe tesszük a létrehozott elemeket
    RectTransform prefabProjectProduct; // A projekt termékekhez ezt kell klónozni

    List<ProjectProduct> listOfProjectProduct = new List<ProjectProduct>(); // A létrehozott projekt termékek törléséhez szükséges

    List<ProjectProduct.ProjectProductData> listOfProjectProductData = new List<ProjectProduct.ProjectProductData>();

    string Pcoin;   // A felhasználó Pcoin-ja

    // Use this for initialization
    void Awake ()
    {
        textInvestmentPeriod = gameObject.SearchChild("InvestmentPeriod").GetComponent<Text>();
        textRemainHours = gameObject.SearchChild("RemainHours").GetComponent<Text>();

        goNoInvestmentOpportunities = gameObject.SearchChild("TextNoInvestmentOpportunities").gameObject;

        content = gameObject.SearchChild("Content").GetComponent<RectTransform>();
        prefabProjectProduct = gameObject.SearchChild("ProjectProduct").GetComponent<RectTransform>();

        prefabProjectProduct.gameObject.SetActive(false);
    }

    public void Initialize()
    {
        //textInvestmentPeriod.text = Common.languageController.Translate(C.Texts.StartInvestmentPeriod);
        //textRemainHours.text = "123";
        textInvestmentPeriod.text = "";
        textRemainHours.text = "";

        // Töröljük az esetlegesen már létező menü elemeket
        for (int i = 0; i < listOfProjectProduct.Count; i++)
            Destroy(listOfProjectProduct[i].gameObject);

        listOfProjectProduct.Clear();

        // Létrehozzuk az új tartalmat
        int count = 0;
        for (int i = 0; i < count; i++)
        {
            ProjectProduct.ProjectProductData data = new ProjectProduct.ProjectProductData("0", "0", "MicroSoft", "Windows", true, false);

            ProjectProduct newItem = Instantiate(prefabProjectProduct, content).GetComponent<ProjectProduct>();
            newItem.gameObject.SetActive(true);
            newItem.Initialize(
                i == 1 || i == 2,
                data,
                i.ToString(),
                ButtonClick,
                false);

            newItem.GetComponent<RectTransform>().localPosition = new Vector3((i % 2) * prefabProjectProduct.sizeDelta.x, (i / 2) * -prefabProjectProduct.sizeDelta.y);

            //RectTransform recTranform = curriculum.GetComponent<RectTransform>();
            //recTranform.localPosition = rectTransformCurriculumItem.localPosition.AddX((i % 2) * rectTransformCurriculumItem.sizeDelta.x).AddY((i / 2) * -rectTransformCurriculumItem.sizeDelta.y);

            listOfProjectProduct.Add(newItem);
        }
    }

    public void Initialize(JSONNode json, string Pcoin)
    {
        this.Pcoin = Pcoin;

        // Kiírjuk az időszakból hátralevő időt
        textInvestmentPeriod.text = "";
        textRemainHours.text = "";
        MarketPlacePanel.timeCountDown = false;
        if (!(json[C.JSONKeys.currentModuleRemainingTime].Value == "-1" || json[C.JSONKeys.currentModuleRemainingTime].Value == "null")) {
            textInvestmentPeriod.text = Common.languageController.Translate(C.Texts.EndInvestmentPeriod);
            MarketPlacePanel.actPeriodEnd = (DateTime.Now).AddSeconds(json[C.JSONKeys.currentModuleRemainingTime].AsInt);
            MarketPlacePanel.timeCountDown = true;
        } 
        else if (!(json[C.JSONKeys.nextModuleRemainingTime].Value == "-1" || json[C.JSONKeys.nextModuleRemainingTime].Value == "null")) {
            textInvestmentPeriod.text = Common.languageController.Translate(C.Texts.StartInvestmentPeriod);
            MarketPlacePanel.actPeriodEnd = (DateTime.Now).AddSeconds(json[C.JSONKeys.nextModuleRemainingTime].AsInt);
            MarketPlacePanel.timeCountDown = true;
        }

        // Teszt idő megadása
        //MarketPlacePanel.actPeriodEnd = (DateTime.Now).AddSeconds(10);
        //MarketPlacePanel.timeCountDown = true;

        // Töröljük az esetlegesen már létező menü elemeket
        for (int i = 0; i < listOfProjectProduct.Count; i++)
            Destroy(listOfProjectProduct[i].gameObject);

        listOfProjectProduct.Clear();

        // Feldolgozzuk a json-ban kapott adatokat
        listOfProjectProductData.Clear();
        for (int i = 0; i < json["items"].Count; i++)
            listOfProjectProductData.Add(new ProjectProduct.ProjectProductData(json["items"][i]));

        // Létrehozzuk az új tartalmat
        int count = listOfProjectProductData.Count;
        for (int i = 0; i < count; i++)
        {
            ProjectProduct.ProjectProductData data = listOfProjectProductData[i];

            ProjectProduct newItem = Instantiate(prefabProjectProduct, content).GetComponent<ProjectProduct>();
            newItem.gameObject.SetActive(true);
            newItem.Initialize(
                i % 4 == 1 || i % 4 == 2,
                data,
                i.ToString(),
                ButtonClick,
                false);

            newItem.GetComponent<RectTransform>().localPosition = new Vector3((i % 2) * prefabProjectProduct.sizeDelta.x, (i / 2) * -prefabProjectProduct.sizeDelta.y);

            listOfProjectProduct.Add(newItem);
        }

        // Beállítjuk a content méretét a létrehozott elemeknek megfelelően
        content.sizeDelta = new Vector2(content.sizeDelta.x, prefabProjectProduct.GetComponent<RectTransform>().sizeDelta.y * ((count + 1) / 2));

        goNoInvestmentOpportunities.SetActive(listOfProjectProduct.Count == 0);
    }

    void Update()
    {
        if (MarketPlacePanel.timeCountDown)
        {
            textRemainHours.text = MarketPlacePanel.remainTime + 
                ((!MarketPlacePanel.lastHour) ? " " + Common.languageController.Translate(C.Texts.Hours) : "");
        }

        /*
        if (timeCountDown)
        {
            TimeSpan timeSpan = actPeriodEnd - DateTime.Now;

            //TimeSpan timeSpan = new TimeSpan((long)(remainTime * 10000000));

            if ((long)(timeSpan.TotalHours) > 0)
                textRemainHours.text = (long)(timeSpan.TotalHours) + " " + Common.languageController.Translate(C.Texts.Hours);
            else
                textRemainHours.text = (timeSpan.Minutes.ToString()).PadLeft(2, '0') + ":" + (timeSpan.Seconds.ToString()).PadLeft(2, '0');

            if (timeSpan.Ticks < 0)
            {
                // Frissítjük a Piac tér adatait
                OTPMain.instance.Refresh();

                timeCountDown = false;
            }
        }
        */
    }

    public void ButtonClick(string buttonName)
    {
        Debug.Log(buttonName);
        int buttonNumber = int.Parse(buttonName);

        // Aktívvá tesszük a kiválasztott terméket, a korábban kiválasztottat pedig deaktíváljuk
        for (int i = 0; i < listOfProjectProduct.Count; i++)
            listOfProjectProduct[i].SetActive(buttonNumber == i);

        // Megmutatjuk a panelt
        MarketPlaceProjectProductsInvestmentPlayPanel.instance.Show(
            listOfProjectProductData[buttonNumber].productName,
            listOfProjectProductData[buttonNumber].companyName,
            Pcoin,
            listOfProjectProductData[buttonNumber].investable,
            (string pressedButton) => {
                Debug.Log(pressedButton);
                switch (pressedButton)
                {
                    case "Play":
                        // Lekérdezzük a szervertől a játék adatait
                        ClassYServerCommunication.instance.GetGameForPlay(listOfProjectProductData[buttonNumber].gameID,
                            (bool success, JSONNode response) =>
                            {
                                if (success)
                                {
                                    // Elindítjuk a játékot
                                    GameMenu.instance.SetPreviousButton(false);
                                    GameMenu.instance.SetNextButton(false);
                                    Common.configurationController.WaitCoroutine(ServerPlay.instance.PlayGameProjectProductdCoroutine(response),
                                        () => {
                                            // Ha végzet vissza váltunk az OTPMain képernyőre
                                            Common.screenController.ChangeScreen(C.Screens.OTPMain);
                                        }
                                    );
                                }
                            }
                        );

                        break;

                    case "Investment":
                        ErrorPanel.instance.Show(
                            Common.languageController.Translate(C.Texts.SureInvestment),
                            Common.languageController.Translate(C.Texts.ImInvesting),
                            button2Text: Common.languageController.Translate(C.Texts.Cancel),
                            button2Color: Color.red,
                            callBack: (string bName) =>
                            {
                                ErrorPanel.instance.Hide(() =>
                                {
                                    if (bName == "button1") // Common.languageController.Translate(C.Texts.ImInvesting))
                                    {
                                        ClassYServerCommunication.instance.SetUserInvestment(listOfProjectProductData[buttonNumber].projectProductID,
                                            (bool success, JSONNode response) =>
                                            {
                                                if (success)
                                                {
                                                    // Ha sikerült a befektetés, akkor eltüntetjük a befektető panelt
                                                    MarketPlaceProjectProductsInvestmentPlayPanel.instance.Hide(
                                                                () =>
                                                                {
                                                                // Frissítjük a Piac tér adatait
                                                                OTPMain.instance.Refresh();
                                                                });
                                                }
                                            });
                                    }
                                });
                            });
                        break;

                    case "FadeClick":
                        MarketPlaceProjectProductsInvestmentPlayPanel.instance.Hide();
                        break;
                }
            }
            );
        // Kiválasztottak egy terméket
        // Egy felbukkanó ablakban megmutatjuk
    }
}
