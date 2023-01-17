using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketPlaceCompanyMember : MonoBehaviour
{
    Text textCollectByLearnText;    // "Tanulással gyűjtött Pcoin" szöveg kiírásához
    Text textCollectByLearnValue;   // Tanulással gyűjtött Pcoin értékének kiírásához
    Text textOwnershipText;         // "Tulajdonrész: " szöveg kiírásához
    Text textOwnershipValue;        // Tulajdonrész értékének kiírásához

    Text textMemberName;            // Cégtárs nevének kiírásához

	// Use this for initialization
	void Awake ()
    {
        textCollectByLearnText = gameObject.SearchChild("CollectByLearnText").GetComponent<Text>();
        textCollectByLearnValue = gameObject.SearchChild("CollectByLearnValue").GetComponent<Text>();
        textOwnershipText = gameObject.SearchChild("OwnershipText").GetComponent<Text>();
        textOwnershipValue = gameObject.SearchChild("OwnershipValue").GetComponent<Text>();

        textMemberName = gameObject.SearchChild("MemberName").GetComponent<Text>();
    }

    public void Initialize()
    {
        JSONClass json = new JSONClass();
        json["coinsFromLearning"] = "1";
        json["ownership"] = "2";
        json["name"] = "Gipsz Jakab";
        Initialize(json);
    }

    public void Initialize(JSONNode json) {
        textCollectByLearnText.text = Common.languageController.Translate(C.Texts.CollectByLearn);
        textCollectByLearnValue.text = json["coinsFromLearning"];
        textOwnershipText.text = Common.languageController.Translate(C.Texts.OwnerShip);
        textOwnershipValue.text = json["ownership"];

        textMemberName.text = json["name"];
        textMemberName.enabled = true;
    }
}
