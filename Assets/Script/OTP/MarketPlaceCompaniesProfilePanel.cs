using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketPlaceCompaniesProfilePanel : MonoBehaviour
{
    // Saját adatok összegzéséhez szövegek
    Text textCompanyName;
    TextHelper textAllLearnText;
    TextHelper textAllLearnValue;
    TextHelper textAllProjectProductText;
    TextHelper textAllProjectProductValue;

    // Cégtársak
    Text textCompanyMembers;

    // Toplistán szereplő szövegek
    Text textMarketState;
    Text textHighestCompanyValueText;
    TextHelper textHighestCompanyValue;
    Text textOwnCompanyValueText;
    TextHelper textOwnCompanyValue;
    Text textOwnCompanyPlace;
    Text textLowestCompanyValueText;
    TextHelper textLowestCompanyValue;

    // 
    RectTransform prefabCompanyMember;
    RectTransform content;

    /// <summary>
    /// 
    /// </summary>
    List<GameObject> listOfCompanyMembers = new List<GameObject>();

	// Use this for initialization
	void Awake ()
    {
        // Saját adatok összegzéséhez szövegek
        textCompanyName = gameObject.SearchChild("CompanyName").GetComponent<Text>();
        textAllLearnText = gameObject.SearchChild("AllLearnText").GetComponent<TextHelper>();
        textAllLearnValue = gameObject.SearchChild("AllLearnValue").GetComponent<TextHelper>();
        textAllProjectProductText = gameObject.SearchChild("AllProjectProductText").GetComponent<TextHelper>();
        textAllProjectProductValue = gameObject.SearchChild("AllProjectProductValue").GetComponent<TextHelper>();

        // Cégtársak
        textCompanyMembers = gameObject.SearchChild("CompanyMembersText").GetComponent<Text>();

        // Toplistán szereplő szövegek
        textMarketState = gameObject.SearchChild("MarketState").GetComponent<Text>();
        textHighestCompanyValueText = gameObject.SearchChild("HighestCompanyValueText").GetComponent<Text>();
        textHighestCompanyValue = gameObject.SearchChild("HighestCompanyValue").GetComponent<TextHelper>();
        textOwnCompanyValueText = gameObject.SearchChild("OwnCompanyValueText").GetComponent<Text>();
        textOwnCompanyValue = gameObject.SearchChild("OwnCompanyValue").GetComponent<TextHelper>();
        textOwnCompanyPlace = gameObject.SearchChild("OwnCompanyPlace").GetComponent<Text>();
        textLowestCompanyValueText = gameObject.SearchChild("LowestCompanyValueText").GetComponent<Text>();
        textLowestCompanyValue = gameObject.SearchChild("LowestCompanyValue").GetComponent<TextHelper>();

        // 
        prefabCompanyMember = gameObject.SearchChild("CompanyMember").GetComponent<RectTransform>();
        content = gameObject.SearchChild("Content").GetComponent<RectTransform>();

        //prefabCompanyMember.GetComponent<RectTransform>().localPosition = new Vector3(-1000, -1000);
        prefabCompanyMember.gameObject.SetActive(false);

    }

    public void Initialize(string companyName, string allLearnCoin, string allProjectProductCoin, string highestCompanyValueCoin, string ownCompanyValueCoin, string ownCompanyPlace, string lowestCompanyValueCoin)
    {
        textCompanyName.text = companyName;
        textAllLearnText.SetText(Common.languageController.Translate(C.Texts.AllLearn));
        textAllLearnValue.SetText(allLearnCoin);
        textAllProjectProductText.SetText(Common.languageController.Translate(C.Texts.AllProjectProduct));
        textAllProjectProductValue.SetText(allProjectProductCoin);

        // Cégtársak
        textCompanyMembers.text = Common.languageController.Translate(C.Texts.CompanyMembers);

        // Toplistán szereplő szövegek
        textMarketState.text = Common.languageController.Translate(C.Texts.CompanyMarketState);
        textHighestCompanyValueText.text = Common.languageController.Translate(C.Texts.HighestCompanyValue);
        textHighestCompanyValue.SetText(highestCompanyValueCoin);
        textOwnCompanyValueText.text = Common.languageController.Translate(C.Texts.OwnCompanyValue);
        textOwnCompanyValue.SetText(ownCompanyValueCoin);
        textOwnCompanyPlace.text = ownCompanyPlace;
        textLowestCompanyValueText.text = Common.languageController.Translate(C.Texts.LowestCompanyValue);
        textLowestCompanyValue.SetText(lowestCompanyValueCoin);

        // Töröljük az esetlegesen már létező menü elemeket
        for (int i = 0; i < listOfCompanyMembers.Count; i++)
            Destroy(listOfCompanyMembers[i]);
        listOfCompanyMembers.Clear();
        //MarketPlaceCompanyMember[] existsItem = GetComponentsInChildren<MarketPlaceCompanyMember>(true);
        //for (int i = 0; i < existsItem.Length; i++)
        //    Destroy(existsItem[i].gameObject);

        // Létrehozzuk az új tartalmat
        int count = 0;
        for (int i = 0; i < count; i++)
        {
            MarketPlaceCompanyMember newItem = Instantiate(prefabCompanyMember, content).GetComponent<MarketPlaceCompanyMember>();
            newItem.gameObject.SetActive(true);
            newItem.Initialize();
            newItem.GetComponent<RectTransform>().localPosition = new Vector3(prefabCompanyMember.sizeDelta.x * i, 0);

            listOfCompanyMembers.Add(newItem.gameObject);
        }

        // Beállítjuk a content méretét a létrehozott elemeknek megfelelően
        content.sizeDelta = new Vector2(prefabCompanyMember.GetComponent<RectTransform>().sizeDelta.x * count, content.sizeDelta.y);
    }

    public void Initialize(JSONNode json)
    {
        textCompanyName.text = json["name"];
        textAllLearnText.SetText(Common.languageController.Translate(C.Texts.AllLearn));
        textAllLearnValue.SetText(json["coinsFromLearning"]);
        textAllProjectProductText.SetText(Common.languageController.Translate(C.Texts.AllProjectProduct));
        textAllProjectProductValue.SetText(json["coinsFromItems"]);

        // Cégtársak
        textCompanyMembers.text = Common.languageController.Translate(C.Texts.CompanyMembers);

        // Toplistán szereplő szövegek
        JSONNode marketSituationJson = json["marketSituation"];
        textMarketState.text = Common.languageController.Translate(C.Texts.CompanyMarketState);
        textHighestCompanyValueText.text = Common.languageController.Translate(C.Texts.HighestCompanyValue);
        textHighestCompanyValue.SetText(marketSituationJson["maxCompanyValue"]);
        textOwnCompanyValueText.text = Common.languageController.Translate(C.Texts.OwnCompanyValue);
        textOwnCompanyValue.SetText(marketSituationJson["ownCompanyValue"]);
        textOwnCompanyPlace.text = marketSituationJson["ownCompanyPlace"];
        textLowestCompanyValueText.text = Common.languageController.Translate(C.Texts.LowestCompanyValue);
        textLowestCompanyValue.SetText(marketSituationJson["minCompanyValue"]);

        // Töröljük az esetlegesen már létező menü elemeket
        for (int i = 0; i < listOfCompanyMembers.Count; i++)
            Destroy(listOfCompanyMembers[i]);
        listOfCompanyMembers.Clear();
        //MarketPlaceCompanyMember[] existsItem = GetComponentsInChildren<MarketPlaceCompanyMember>(true);
        //for (int i = 0; i < existsItem.Length; i++)
        //    Destroy(existsItem[i].gameObject);

        // Létrehozzuk az új tartalmat
        int count = json["partners"].Count;
        for (int i = 0; i < count; i++)
        {
            MarketPlaceCompanyMember newItem = Instantiate(prefabCompanyMember, content).GetComponent<MarketPlaceCompanyMember>();
            newItem.gameObject.SetActive(true);
            newItem.Initialize(json["partners"][i]);
            newItem.GetComponent<RectTransform>().localPosition = new Vector3(prefabCompanyMember.sizeDelta.x * i, 0);

            listOfCompanyMembers.Add(newItem.gameObject);
        }

        // Beállítjuk a content méretét a létrehozott elemeknek megfelelően
        content.sizeDelta = new Vector2(prefabCompanyMember.GetComponent<RectTransform>().sizeDelta.x * count, content.sizeDelta.y);
    }

}
