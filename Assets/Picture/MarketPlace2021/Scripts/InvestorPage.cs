using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarketPlace2021
{
    public class InvestorPage : MonoBehaviour
    {
        Image imageUser;
        SetLanguageText setLanguageTextsilver;

        Transform content;
        GameObject prefabCompanyItem;
        GameObject goNoInvestmentOpportunities;

        GameObject companyInfoPanel;
        Text textCompanyNameLogo;
        Text textCompanyName;
        Text textProductName;
        GameObject goInvestmentButton;

        JSONNode data;
        List<GameObject> madeCompanyItems = new List<GameObject>();

        int companyNumber;

        // Start is called before the first frame update
        void Awake()
        {
            imageUser = gameObject.SearchChild("ImageUser").GetComponent<Image>();
            setLanguageTextsilver = gameObject.SearchChild("TextSilver").GetComponent<SetLanguageText>();

            content = gameObject.SearchChild("Content").transform;
            prefabCompanyItem = gameObject.SearchChild("CompanyItem").gameObject;
            prefabCompanyItem.SetActive(false);
            goNoInvestmentOpportunities = gameObject.SearchChild("TextNoInvestmentOpportunities").gameObject;

            companyInfoPanel = gameObject.SearchChild("CompanyInfo").gameObject;
            textCompanyNameLogo = companyInfoPanel.SearchChild("TextCompanyNameLogo").GetComponent<Text>();
            textCompanyName = companyInfoPanel.SearchChild("TextCompanyName").GetComponent<Text>();
            textProductName = companyInfoPanel.SearchChild("TextProductName").GetComponent<Text>();
            goInvestmentButton = companyInfoPanel.SearchChild("ImageInvest").gameObject;
        }

        public void Initialize(JSONNode data)
        {
            this.data = data;

            EmailGroupPictureController.instance.GetPictureFromUploadsDir(data[C.JSONKeys.ownProfile][C.JSONKeys.profilePicture], (Sprite sprite) => { imageUser.sprite = sprite; });

            setLanguageTextsilver.AddParams(
                "value", Common.GroupingNumber((int)data[C.JSONKeys.ownProfile][C.JSONKeys.investable].AsFloat),
                "silver", "#"
            );

            // Kitöröljük a korábban létrehozott GameObject-eket
            for (int i = 0; i < madeCompanyItems.Count; i++)
                Destroy(madeCompanyItems[i]);

            madeCompanyItems.Clear();

            // Létrehozzuk az új tartalmat
            for (int i = 0; i < data[C.JSONKeys.items].Count; i++)
            {
                // Cégek létrehozása
                GameObject newCompanyItem = Instantiate(prefabCompanyItem, content);
                newCompanyItem.SetActive(true);
                madeCompanyItems.Add(newCompanyItem);

                newCompanyItem.GetComponent<CompanyItem>().Initialize(data[C.JSONKeys.items][i], i.ToString(), ButtonClick);
            }

            // Ha nincsenek tartalmak, akkor egy szöveget írunk ki
            goNoInvestmentOpportunities.SetActive(data[C.JSONKeys.items].Count == 0);
        }

        public void ButtonClick(string buttonName)
        {
            switch (buttonName)
            {
                case "hideCompanyInfo":
                    companyInfoPanel.SetActive(false);
                    break;

                case "play":
                    // Lekérdezzük a szervertől a játék adatait
                    ClassYServerCommunication.instance.GetGameForPlay(data[C.JSONKeys.items][companyNumber][C.JSONKeys.gameID],
                            (bool success, JSONNode response) =>
                            {
                                if (success)
                                {
                                    // Szólunk a ScreenController-nek, hogy képernyő váltáskor ne törölje ezt a képernyőt, hogy ugyan ebben az állapotban jöjjön vissza
                                    Common.screenController.holdingActScreen = true;

                                    // Elindítjuk a játékot
                                    GameMenu.instance.SetPreviousButton(false);
                                    GameMenu.instance.SetNextButton(false);
                                    Common.configurationController.WaitCoroutine(ServerPlay.instance.PlayGameProjectProductdCoroutine(response),
                                        () => {
                                                // Ha végzet vissza váltunk az OTPMain képernyőre
                                                Common.screenController.ChangeScreen(C.Screens.MarketPlace2021);
                                        }
                                    );
                                }
                            }
                        );
                    break;

                case "investment":
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
                                    ClassYServerCommunication.instance.SetUserInvestment(data[C.JSONKeys.items][companyNumber][C.JSONKeys.id],
                                        (bool success, JSONNode response) =>
                                        {
                                            if (success)
                                            {
                                                ClassYServerCommunication.instance.getMarketModuleItems(true,
                                                    (bool success2, JSONNode response2) =>
                                                    {
                                                        // Válasz feldolgozása
                                                        if (success)
                                                        {
                                                            OTPMain.instance.marketPlaceData = response2[C.JSONKeys.answer];

                                                            MarketPlace2021_Serious mp = GetComponentInParent<MarketPlace2021_Serious>();
                                                            mp.DrawDatas();
                                                            mp.setPageImmediatelly(C.Texts.InvestorPage);
                                                        }
                                                    }
                                                );
                                            }
                                        });
                                }
                            });
                        });

                    break;

                default:
                    if (int.TryParse(buttonName, out companyNumber))
                    {
                        textCompanyNameLogo.text = data[C.JSONKeys.items][companyNumber][C.JSONKeys.observedGroupName];
                        textCompanyName.text = data[C.JSONKeys.items][companyNumber][C.JSONKeys.observedGroupName];
                        textProductName.text = data[C.JSONKeys.items][companyNumber][C.JSONKeys.gameName];
                        goInvestmentButton.SetActive(data[C.JSONKeys.items][companyNumber][C.JSONKeys.investable].AsBool);
                        companyInfoPanel.SetActive(true);
                    }
                    break;
            }
        }



        /*
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
        */
    }
}
