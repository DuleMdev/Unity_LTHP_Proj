using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarketPlace2021
{
    public class CertificatePage : MonoBehaviour
    {
        Image imageUser;
        GameObject prefabPartnerData;
        Transform silverPanel;

        JSONNode data;
        JSONNode bronzeData;
        JSONNode silverData;
        JSONNode goldData;

        List<GameObject> madePartners = new List<GameObject>();

        void Awake()
        {
            imageUser = gameObject.SearchChild("ImageUser").GetComponent<Image>();
            prefabPartnerData = gameObject.SearchChild("partnerData").gameObject;
            prefabPartnerData.SetActive(false);
            silverPanel = gameObject.SearchChild("PanelSilver").transform;

            // Megkeressük a SetLanguageText componenseket és beállítjuk az Adatforrásukat 
            SetLanguageText[] setLanguagesArray = transform.GetComponentsInChildren<SetLanguageText>(true);

            foreach (var item in setLanguagesArray)
                item.dataProvider = DataProvider;
        }

        public void Initialize(JSONNode data)
        {
            this.data = data[C.JSONKeys.achievements][C.JSONKeys.answer];
            bronzeData = this.data["bronze"];
            silverData = this.data["silver"];
            goldData = this.data["gold"];

            EmailGroupPictureController.instance.GetPictureFromUploadsDir(this.data[C.JSONKeys.profilePicture], (Sprite sprite) => { imageUser.sprite = sprite; });

            DrawBadges("bronze");
            DrawBadges("silver");
            DrawBadges("gold");

            // Kiírjuk a partnerek neveit és eredményeit

            // Kitöröljük a korábban létrehozott GameObject-eket
            for (int i = 0; i < madePartners.Count; i++)
                Destroy(madePartners[i]);

            madePartners.Clear();

            // Létrehozzuk az új tartalmat
            float sizeY = 0;
            for (int i = 0; i < silverData[C.JSONKeys.partners].Count; i++)
            {
                // Partner adatok megjelenítéséhez komponensek létrehozása
                GameObject newPartners = Instantiate(prefabPartnerData, silverPanel);
                newPartners.SetActive(true);
                madePartners.Add(newPartners);

                RectTransform rectTransform = newPartners.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y - i * rectTransform.sizeDelta.y);

                // Beállítjuk a nevet és a szerzett ezüstök számát
                SetLanguageText setLanguageText = newPartners.SearchChild("TextName").GetComponent<SetLanguageText>();
                setLanguageText.SetTextID("*" + C.JSONKeys.name + ":" + i);
                setLanguageText.dataProvider = DataProvider;

                setLanguageText = newPartners.SearchChild("TextpartnerCoinsFromLearning").GetComponent<SetLanguageText>();
                setLanguageText.SetTextID("partnerCoinsFromLearning|*" + C.JSONKeys.coinsFromLearning + ":" + i);
                setLanguageText.dataProvider = DataProvider;

                sizeY = -rectTransform.anchoredPosition.y + rectTransform.sizeDelta.y;
            }

            ((RectTransform)silverPanel.parent).sizeDelta = new Vector2(((RectTransform)silverPanel.parent).sizeDelta.x, Mathf.Max(sizeY + 80, 280));
        }

        void DrawBadges(string s) //" JSONNode badges, GameObject badgesGameObject)
        {
            JSONNode badges = data[s]["badges"];
            GameObject badgesGameObject = gameObject.SearchChild("Panel" + char.ToUpper(s[0]) + s.Substring(1));

            // megkeressük a Image komponenseket
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (badges.Count > i)
                        badgesGameObject.SearchChild("Badge" + (i + 1)).GetComponent<Badge>().Initialize(badges[i][C.JSONKeys.image].Value, badges[i][C.JSONKeys.badgeCount]);
                    else
                        badgesGameObject.SearchChild("Badge" + (i + 1)).SetActive(false);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        public string DataProvider(string token)
        {
            string result = "";

            string[] tokenParts = token.Split(':');
            int i = 0;

            switch (tokenParts[0])
            {
                case C.JSONKeys.userName:                 result = data[C.JSONKeys.userName]; break;
                case C.JSONKeys.userEmail:                result = data[C.JSONKeys.userEmail]; break;

                case C.JSONKeys.playTime:                 result = bronzeData[C.JSONKeys.playTime]; break;
                case C.JSONKeys.playCount:                result = bronzeData[C.JSONKeys.playCount]; break;
                case C.JSONKeys.bronzeCoinsFromLearning:  result = bronzeData[C.JSONKeys.bronzeCoinsFromLearning]; break;
                case C.JSONKeys.maxCoinsFromLearning:     result = bronzeData[C.JSONKeys.maxCoinsFromLearning]; break;
                case C.JSONKeys.coinsFromLearningPercent: result = bronzeData[C.JSONKeys.coinsFromLearningPercent]; break;

                case C.JSONKeys.copanyName:               result = silverData[C.JSONKeys.copanyName]; break;
                case C.JSONKeys.silverCoinsFromLearning:  result = silverData[C.JSONKeys.silverCoinsFromLearning]; break;
                case C.JSONKeys.capitalGains:             result = silverData[C.JSONKeys.capitalGains]; break;
                case C.JSONKeys.companyValue:             result = silverData[C.JSONKeys.companyValue]; break;
                case C.JSONKeys.companyPlace:             result = silverData[C.JSONKeys.companyPlace]; break;
                case C.JSONKeys.investable:               result = silverData[C.JSONKeys.investable]; break;
                case C.JSONKeys.invested:                 result = silverData[C.JSONKeys.invested]; break;
                case C.JSONKeys.mostValuable:             result = silverData[C.JSONKeys.mostValuable]; break;
                case C.JSONKeys.leastValuable:            result = silverData[C.JSONKeys.leastValuable]; break;
                case C.JSONKeys.lastPlace:                result = silverData[C.JSONKeys.lastPlace]; break;
                case C.JSONKeys.name:
                    i = int.Parse(tokenParts[1]);
                    result = silverData[C.JSONKeys.partners][i][C.JSONKeys.name];
                    break;
                case C.JSONKeys.coinsFromLearning:
                    i = int.Parse(tokenParts[1]);
                    result = silverData[C.JSONKeys.partners][i][C.JSONKeys.coinsFromLearning];
                    break;

                case C.JSONKeys.investmentsYield:         result = goldData[C.JSONKeys.investmentsYield]; break;
                case C.JSONKeys.exitYield:                result = goldData[C.JSONKeys.exitYield]; break;
                case C.JSONKeys.yield:                    result = goldData[C.JSONKeys.yield]; break;
                case C.JSONKeys.allGoldCoins:             result = goldData[C.JSONKeys.allGoldCoins]; break;
                case C.JSONKeys.mostGoldCoins:            result = goldData[C.JSONKeys.mostGoldCoins]; break;
                case C.JSONKeys.leastGoldCoins:           result = goldData[C.JSONKeys.leastGoldCoins]; break;
                case C.JSONKeys.place:                    result = goldData[C.JSONKeys.place]; break;
                case "goldLastPlace":                     result = goldData[C.JSONKeys.lastPlace]; break;
            }

            return result;
        }
    }
}