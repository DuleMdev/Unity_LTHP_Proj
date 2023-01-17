using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarketPlace2021
{
    public class StartUpperPage : MonoBehaviour
    {
        Text textCompanyName;

        Transform content;
        GameObject prefabCompanyPartner;

        SetLanguageText valueYourCapital;
        SetLanguageText valuePlace;
        SetLanguageText valueLearningSilver;
        SetLanguageText valueInvestedSilver;

        SetLanguageText valueHighestLowestCompanyValue;


        List<GameObject> makedCompanyMates = new List<GameObject>();

        // Start is called before the first frame update
        void Awake()
        {
            textCompanyName = gameObject.SearchChild("TextCompanyName").GetComponent<Text>();

            content = gameObject.SearchChild("Content").transform;
            prefabCompanyPartner = gameObject.SearchChild("PrefabPartner").gameObject;
            prefabCompanyPartner.SetActive(false);

            valueYourCapital = gameObject.SearchChild("TextValueYourCapital").GetComponent<SetLanguageText>();
            valuePlace = gameObject.SearchChild("TextValuePlace").GetComponent<SetLanguageText>();
            valueLearningSilver = gameObject.SearchChild("TextValueLearningSilver").GetComponent<SetLanguageText>();
            valueInvestedSilver = gameObject.SearchChild("TextValueInvestedSilver").GetComponent<SetLanguageText>();

            valueHighestLowestCompanyValue = gameObject.SearchChild("TextHighestLowestCompanyValue").GetComponent<SetLanguageText>();
        }

        public void Initialize(JSONNode data)
        {
            JSONNode companyProfile = data[C.JSONKeys.companyProfile];

            textCompanyName.text = companyProfile[C.JSONKeys.name];

            valueYourCapital.AddParams("value", Common.GroupingNumber((int)companyProfile[C.JSONKeys.marketSituation][C.JSONKeys.ownCompanyValue].AsFloat));
            valuePlace.AddParams("value", Common.GroupingNumber((int)companyProfile[C.JSONKeys.marketSituation][C.JSONKeys.ownCompanyPlace].AsFloat));
            valueLearningSilver.AddParams("value", Common.GroupingNumber((int)companyProfile[C.JSONKeys.coinsFromLearning].AsFloat));
            valueInvestedSilver.AddParams("value", Common.GroupingNumber((int)companyProfile[C.JSONKeys.coinsFromItems].AsFloat));

            valueHighestLowestCompanyValue.AddParams(
                "highvalue", ((int)companyProfile[C.JSONKeys.marketSituation][C.JSONKeys.maxCompanyValue].AsFloat).ToString(),
                "lowvalue", ((int)companyProfile[C.JSONKeys.marketSituation][C.JSONKeys.minCompanyValue].AsFloat).ToString()
                );

            // Kitöröljük a korábban létrehozott GameObject-eket
            for (int i = 0; i < makedCompanyMates.Count; i++)
                Destroy(makedCompanyMates[i]);

            makedCompanyMates.Clear();

            // Létrehozzuk az új tartalmat
            for (int i = 0; i < companyProfile[C.JSONKeys.partners].Count; i++)
            {
                // Email csoport gomb létrehozása
                GameObject newCompanyPartner = Instantiate(prefabCompanyPartner, content);
                newCompanyPartner.SetActive(true);
                makedCompanyMates.Add(newCompanyPartner);

                JSONNode partnerData = companyProfile[C.JSONKeys.partners][i];

                if (partnerData[C.JSONKeys.profilePicture] != null)
                    EmailGroupPictureController.instance.GetPictureFromUploadsDir(partnerData[C.JSONKeys.profilePicture], (Sprite sprite) => { newCompanyPartner.SearchChild("ImagePartner").GetComponent<Image>().sprite = sprite; });

                //int bronzeCoins = ((int)partnerData[C.JSONKeys.coinsFromLearning].AsFloat).ToString();

                newCompanyPartner.SearchChild("TextPartnerName").GetComponent<Text>().text = partnerData[C.JSONKeys.name];
                newCompanyPartner.GetComponentInChildren<SetLanguageText>().AddParams("value", Common.GroupingNumber((int)partnerData[C.JSONKeys.coinsFromLearning].AsFloat));
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
