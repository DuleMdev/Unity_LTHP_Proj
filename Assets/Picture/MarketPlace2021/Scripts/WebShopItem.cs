using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarketPlace2021
{
    public class WebShopItem : MonoBehaviour
    {
        Image imageItem;
        Text textItemName;
        Text textItemPrice;
        Text textPurchased;

        Common.CallBack_In_String callBack;
        string buttonName;

        // Start is called before the first frame update
        void Awake()
        {
            imageItem = gameObject.SearchChild("ImageItem").GetComponent<Image>();
            textItemName = gameObject.SearchChild("TextItemName").GetComponent<Text>();
            textItemPrice = gameObject.SearchChild("TextItemPrice").GetComponent<Text>();
            textPurchased = gameObject.SearchChild("TextPurchased").GetComponent<Text>();
        }

        public void Initialize(JSONNode json, string buttonName = "", Common.CallBack_In_String callBack = null)
        {
            this.callBack = callBack;
            this.buttonName = buttonName;

            EmailGroupPictureController.instance.GetPictureFromMarketWebshopItemsDir(json[C.JSONKeys.itemImage], (Sprite sprite) => { imageItem.sprite = sprite; });
            textItemName.text = json[C.JSONKeys.itemName];
            textItemPrice.text = json[C.JSONKeys.itemPrice];
            textPurchased.text = json[C.JSONKeys.purchasedQuantity];

            // Eltüntetjük a vásárlások számát, ha még egyszer sem lett megvásárolva
            textPurchased.transform.parent.gameObject.SetActive(textPurchased.text != "0");
        }

        public void ButtonClick()
        {
            if (callBack != null)
                callBack(buttonName);
        }
    }
}