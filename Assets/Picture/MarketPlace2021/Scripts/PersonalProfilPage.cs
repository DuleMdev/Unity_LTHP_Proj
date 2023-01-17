using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarketPlace2021
{
    public class PersonalProfilPage : MonoBehaviour
    {

        Image imageUser;
        Text nameUser;

        Text bronzeCoinValue;
        Text silverCoinValue;
        Text goldCoinValue;

        Transform content;
        GameObject prefabBadge;

        GameObject badgeInfoPanel;
        SetLanguageText badgeInfoName;
        SetLanguageText badgeInfoDescription;

        JSONNode data;
        List<GameObject> madeExperianceBadges = new List<GameObject>();

        // Start is called before the first frame update
        void Awake()
        {
            imageUser = gameObject.SearchChild("ImageUser").GetComponent<Image>();
            nameUser = gameObject.SearchChild("TextName").GetComponent<Text>();

            bronzeCoinValue = gameObject.SearchChild("BronzeCoinValue").GetComponent<Text>();
            silverCoinValue = gameObject.SearchChild("SilverCoinValue").GetComponent<Text>();
            goldCoinValue = gameObject.SearchChild("GoldCoinValue").GetComponent<Text>();

            content = gameObject.SearchChild("Content").transform;
            prefabBadge = gameObject.SearchChild("Badge").gameObject;
            prefabBadge.SetActive(false);

            badgeInfoPanel = gameObject.SearchChild("BadgeInfo").gameObject;
            badgeInfoName = gameObject.SearchChild("TextBadgeName").GetComponent<SetLanguageText>();
            badgeInfoDescription = gameObject.SearchChild("TextBadgeDescription").GetComponent<SetLanguageText>();
        }

        public void Initialize(JSONNode data) //, Common.CallBack_In_String callBack)
        {
            this.data = data[C.JSONKeys.ownProfile];
            JSONNode ownProfile = this.data;

            EmailGroupPictureController.instance.GetPictureFromUploadsDir(data[C.JSONKeys.ownProfile][C.JSONKeys.profilePicture], (Sprite sprite) => { imageUser.sprite = sprite; });
            nameUser.text = data[C.JSONKeys.name];

            bronzeCoinValue.text = Common.GroupingNumber((int)ownProfile[C.JSONKeys.coinsFromLearning].AsFloat);
            silverCoinValue.text = Common.GroupingNumber((int)ownProfile[C.JSONKeys.investable].AsFloat);
            goldCoinValue.text = Common.GroupingNumber((int)ownProfile[C.JSONKeys.yield].AsFloat);

            // Kitöröljük a korábban létrehozott GameObject-eket
            for (int i = 0; i < madeExperianceBadges.Count; i++)
                Destroy(madeExperianceBadges[i]);

            madeExperianceBadges.Clear();

            // Létrehozzuk az új tartalmat
            for (int i = 0; i < ownProfile[C.JSONKeys.badges].Count; i++)
            {
                // Email csoport gomb létrehozása
                GameObject newBadge = Instantiate(prefabBadge, content);
                newBadge.SetActive(true);
                madeExperianceBadges.Add(newBadge);

                newBadge.GetComponentInChildren<Badge>().Initialize(
                    ownProfile[C.JSONKeys.badges][i][C.JSONKeys.image],
                    ownProfile[C.JSONKeys.badges][i][C.JSONKeys.badgeCount],
                    i.ToString(), ButtonClick);
            }
        }

        public void ButtonClick(string buttonName)
        {
            switch (buttonName)
            {
                case "hideBadgeInfo":
                    badgeInfoPanel.SetActive(false);
                    break;
                default:
                    int badgeNumber;
                    if (int.TryParse(buttonName, out badgeNumber))
                    {
                        badgeInfoName.SetTextID(data[C.JSONKeys.badges][badgeNumber][C.JSONKeys.badgeName] + "Title");
                        badgeInfoDescription.SetTextID(data[C.JSONKeys.badges][badgeNumber][C.JSONKeys.badgeName] + "Description");
                        badgeInfoPanel.SetActive(true);
                    }
                    break;
            }
        }
    }
}
