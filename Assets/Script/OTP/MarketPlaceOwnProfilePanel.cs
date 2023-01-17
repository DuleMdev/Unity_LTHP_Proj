using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketPlaceOwnProfilePanel : MonoBehaviour
{
    // Saját adatok megjelenítésére
    Text textOwnName;
    TextHelper textUpToNowAllYieldValue;
    Text textUpToNowAllYieldText;

    Text textThisPeriodInvestableText;
    TextHelper textInvestableValue;
    TextHelper textInvestableText;

    TextHelper textInvestedValue;
    TextHelper textInvestedText;

    // TopList megjelenítésére
    Text textMarketState;   // Piaci helyzet szöveg kiírásához

    Text textMostSuccessfulInvestorText;
    TextHelper textMostSuccessfulInvestorValue;

    Text textTopListName;
    TextHelper textTopListOwnPcoinValue;
    Text textTopListOwnPlace;

    Text textLeastSuccessfulInvestorText;
    TextHelper textLeastSuccessfulInvestorValue;

	// Use this for initialization
	void Awake ()
    {
        // Saját adatok 
        textOwnName = gameObject.SearchChild("OwnDatasName").GetComponent<Text>();
        textUpToNowAllYieldValue = gameObject.SearchChild("OwnDatasUpToNowAllYieldValue").GetComponent<TextHelper>();
        textUpToNowAllYieldText = gameObject.SearchChild("OwnDatasUpToNowAllYieldText").GetComponent<Text>();

        textThisPeriodInvestableText = gameObject.SearchChild("OwnDatasThisPeriodInvestableText").GetComponent<Text>();
        textInvestableValue = gameObject.SearchChild("OwnDatasInvestableValue").GetComponent<TextHelper>();
        textInvestableText = gameObject.SearchChild("OwnDatasInvestableText").GetComponent<TextHelper>();

        textInvestedValue = gameObject.SearchChild("OwnDatasInvestedValue").GetComponent<TextHelper>();
        textInvestedText = gameObject.SearchChild("OwnDatasInvestedText").GetComponent<TextHelper>();

        // TopList 
        textMarketState = gameObject.SearchChild("TopListMarketState").GetComponent<Text>();

        textMostSuccessfulInvestorText = gameObject.SearchChild("TopListMostSuccessfulInvestor").GetComponent<Text>();
        textMostSuccessfulInvestorValue = gameObject.SearchChild("TopListMostSuccessfulInvestorValue").GetComponent<TextHelper>();

        textTopListName = gameObject.SearchChild("TopListName").GetComponent<Text>();
        textTopListOwnPcoinValue = gameObject.SearchChild("TopListOwnPcoinValue").GetComponent<TextHelper>();
        textTopListOwnPlace = gameObject.SearchChild("TopListOwnPlace").GetComponent<Text>();

        textLeastSuccessfulInvestorText = gameObject.SearchChild("TopListLeastSuccessfulInvestor").GetComponent<Text>();
        textLeastSuccessfulInvestorValue = gameObject.SearchChild("TopListLeastSuccessfulInvestorValue").GetComponent<TextHelper>();
    }

    //public void Initialize(JSONNode node)
    public void Initialize(string ownName, string ownPcoin, string investablePcoin, string investedPcoin, string yieldPcoin, string mostSuccessfulInvestorPcoin, string ownPlace, string leastSuccesfulInvestorPcoin)
    {
        // Saját adatok
        textOwnName.text = ownName;
        textUpToNowAllYieldValue.SetText(ownPcoin);
        textUpToNowAllYieldText.text = Common.languageController.Translate(C.Texts.AllYield);

        textThisPeriodInvestableText.text = Common.languageController.Translate(C.Texts.ThisPeriodInvestable);
        textInvestableValue.SetText(investablePcoin);
        textInvestableText.SetText(Common.languageController.Translate(C.Texts.EvenXHours));

        textInvestedValue.SetText(investedPcoin);
        textInvestedText.SetText(Common.languageController.Translate(C.Texts.InvestedPcoin));

        // TopList
        textMarketState.text = Common.languageController.Translate(C.Texts.MarketState);

        textMostSuccessfulInvestorText.text = Common.languageController.Translate(C.Texts.MostSuccessfulInvestor);
        textMostSuccessfulInvestorValue.SetText(mostSuccessfulInvestorPcoin);

        textTopListName.text = textOwnName.text; // "Kiss Pista";
        textTopListOwnPcoinValue.SetText(ownPcoin);
        textTopListOwnPlace.text = ownPlace + Common.languageController.Translate(C.Texts.OwnPlace);

        textLeastSuccessfulInvestorText.text = Common.languageController.Translate(C.Texts.LeastSuccessfulInvestor);
        textLeastSuccessfulInvestorValue.SetText(leastSuccesfulInvestorPcoin);
    }

    public void Initialize(JSONNode json)
    {
        // Saját adatok
        textOwnName.text = json["name"];
        textUpToNowAllYieldValue.SetText(json["yield"]);
        textUpToNowAllYieldText.text = Common.languageController.Translate(C.Texts.AllYield);

        textThisPeriodInvestableText.text = Common.languageController.Translate(C.Texts.ThisPeriodInvestable);
        textInvestableValue.SetText(json["investable"]);
        //textInvestableText.SetText(Common.languageController.Translate(C.Texts.EvenXHours).Replace("[x]", "124"));
        textInvestableText.SetText(Common.languageController.Translate(C.Texts.EvenXHours));

        textInvestedValue.SetText(json["invested"]);
        textInvestedText.SetText(Common.languageController.Translate(C.Texts.InvestedPcoin));

        // TopList
        JSONNode marketSituationJson = json["marketSituation"];
        textMarketState.text = Common.languageController.Translate(C.Texts.MarketState);

        textMostSuccessfulInvestorText.text = Common.languageController.Translate(C.Texts.MostSuccessfulInvestor);
        textMostSuccessfulInvestorValue.SetText(marketSituationJson["maxInvested"]);

        textTopListName.text = textOwnName.text; // "Kiss Pista";
        textTopListOwnPcoinValue.SetText(marketSituationJson["ownInvested"]);
        textTopListOwnPlace.text = marketSituationJson["ownPlace"] + Common.languageController.Translate(C.Texts.OwnPlace);

        textLeastSuccessfulInvestorText.text = Common.languageController.Translate(C.Texts.LeastSuccessfulInvestor);
        textLeastSuccessfulInvestorValue.SetText(marketSituationJson["minInvested"]);
    }

    // Frissítjük az órák számát, ha valaki esetleg órákig nézné a saját profil panelt
    //void Update()
    //{
    //    if (MarketPlacePanel.timeCountDown)
    //        textInvestableText.SetText(Common.languageController.Translate(C.Texts.EvenXHours).Replace("[x]", MarketPlacePanel.hours));
    //}

}
