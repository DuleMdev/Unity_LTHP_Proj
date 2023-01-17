using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarketPlace2021
{
    public class Badge : MonoBehaviour
    {
        Image image;
        Text textNumber;

        Common.CallBack_In_String callBack;
        string buttonName;

        // Start is called before the first frame update
        void Awake()
        {
            image = gameObject.GetComponent<Image>();
            textNumber = gameObject.SearchChild("TextNumber").GetComponent<Text>();
        }

        public void Initialize(string pictureName, string number, string buttonName = "", Common.CallBack_In_String callBack = null)
        {
            this.callBack = callBack;
            this.buttonName = buttonName;

            EmailGroupPictureController.instance.GetPictureFromMarketBadgesDir(pictureName, (Sprite sprite) => { image.sprite = sprite; });
            textNumber.text = number;
        }

        public void ButtonClick()
        {
            if (callBack != null)
                callBack(buttonName);
        }
    }

}

