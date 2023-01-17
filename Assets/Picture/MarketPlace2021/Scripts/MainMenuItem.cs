using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarketPlace2021
{
    public class MainMenuItem : MonoBehaviour
    {
        public float selectedPlusHeight;

        Text text;
        RectTransform rectTransform;

        float defaultHeight;

        Common.CallBack_In_String callBackButton;
        string buttonName;

        // Start is called before the first frame update
        void Awake()
        {
            text = gameObject.SearchChild("Text").GetComponent<Text>();
            rectTransform = gameObject.GetComponent<RectTransform>();

            defaultHeight = rectTransform.sizeDelta.y;
        }

        public void Initialize(string text, Common.CallBack_In_String callBackButton)
        {
            this.text.text = Common.languageController.Translate(text);
            this.callBackButton = callBackButton;
            this.buttonName = text;
        }

        public void Selected(string buttonName)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, this.buttonName == buttonName ? defaultHeight + selectedPlusHeight : defaultHeight);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ButtonClick()
        {
            callBackButton(buttonName);
        }
    }
}