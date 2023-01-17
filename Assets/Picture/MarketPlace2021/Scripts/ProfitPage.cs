using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarketPlace2021
{
    public class ProfitPage : MonoBehaviour
    {
        Image imageUser;
        SetLanguageText textGold;

        Transform content;
        GridLayoutGroup gridLayoutGroup;
        GameObject prefabWebshopItem;
        GameObject textClose;

        JSONNode data;
        List<GameObject> madeWebshopItems = new List<GameObject>();

        int itemNumber;

        // Start is called before the first frame update
        void Awake()
        {
            imageUser = gameObject.SearchChild("ImageUser").GetComponent<Image>();
            textGold = gameObject.SearchChild("TextGold").GetComponent<SetLanguageText>();

            content = gameObject.SearchChild("Content").transform;
            gridLayoutGroup = content.GetComponent<GridLayoutGroup>();
            prefabWebshopItem = gameObject.SearchChild("WebshopItem").gameObject;
            prefabWebshopItem.SetActive(false);
            textClose = gameObject.SearchChild("TextClose").gameObject;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Initialize(JSONNode data)
        {
            EmailGroupPictureController.instance.GetPictureFromUploadsDir(data[C.JSONKeys.ownProfile][C.JSONKeys.profilePicture], (Sprite sprite) => { imageUser.sprite = sprite; });
            textGold.AddParams("value", Common.GroupingNumber((int)data[C.JSONKeys.ownProfile][C.JSONKeys.yield].AsFloat));

//            this.data = data[C.JSONValues.getWebshopItems];
            this.data = data[C.JSONKeys.webshop][C.JSONKeys.answer];
            JSONNode webshopItems = this.data;

            // Kitöröljük a korábban létrehozott GameObject-eket
            for (int i = 0; i < madeWebshopItems.Count; i++)
                Destroy(madeWebshopItems[i]);

            madeWebshopItems.Clear();

            // Létrehozzuk az új tartalmat
            for (int i = 0; i < webshopItems[C.JSONKeys.items].Count; i++)
            {
                // Cégek létrehozása
                GameObject newWebshopItem = Instantiate(prefabWebshopItem, content);
                newWebshopItem.SetActive(true);
                madeWebshopItems.Add(newWebshopItem);

                newWebshopItem.GetComponent<WebShopItem>().Initialize(webshopItems[C.JSONKeys.items][i], i.ToString(), ButtonClick);
            }

            // Ha nincsenek tartalmak, akkor egy szöveget írunk ki
            textClose.SetActive(webshopItems[C.JSONKeys.items].Count == 0);

            float xSize = Common.GetRectTransformPixelSize((RectTransform)content, yNeed: false).x;
            int count = (int)(xSize / gridLayoutGroup.cellSize.x);
            gridLayoutGroup.cellSize = new Vector2((int)(xSize / count), gridLayoutGroup.cellSize.y);
        }

        public void ButtonClick(string buttonName)
        {
            if (int.TryParse(buttonName, out itemNumber))
            {
                ErrorPanel.instance.Show(
                    Common.languageController.Translate(C.Texts.SureWebshopBuy),
                    Common.languageController.Translate(C.Texts.Ok),
                    button2Text: Common.languageController.Translate(C.Texts.Cancel),
                    button2Color: Color.red,
                    callBack: (string bName) =>
                    {
                        ErrorPanel.instance.Hide(() =>
                        {
                            if (bName == "button1") // Common.languageController.Translate(C.Texts.ImInvesting))
                                {
                                    ClassYServerCommunication.instance.purchaseWebshopItem(data[C.JSONKeys.items][itemNumber][C.JSONKeys.id], 1, true,  
                                        (bool success, JSONNode response) =>
                                        {
                                            ClassYServerCommunication.instance.getMarketModuleItems(true,
                                                (bool success2, JSONNode response2) =>
                                                {
                                                    // Válasz feldolgozása
                                                    if (success2)
                                                    {
                                                        OTPMain.instance.marketPlaceData = response2[C.JSONKeys.answer];
                                            
                                                        MarketPlace2021_Serious mp = GetComponentInParent<MarketPlace2021_Serious>();
                                                        mp.DrawDatas();
                                            
                                                        if (OTPMain.instance.marketPlaceData[C.JSONKeys.webshop][C.JSONKeys.error].AsBool)
                                                            mp.ChangePage(C.Texts.PersonalProfil);
                                                        else
                                                            mp.setPageImmediatelly(C.Texts.ProfitPage);
                                                    }
                                                }
                                            );
                                        });
                                }
                        });
                    });
            }
        }
    }
}
