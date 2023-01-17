using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OTPCurriculumPath : MonoBehaviour
{
    GameObject content; // Egy üres tanulási útvonal létrehozásához

    GameObject gameObjectFrameGame;
    GameObject gameObjectChest;
    GameObject gameObjectCoin;

    RectTransform rectTransformCurriculumPlay;

    Text textStartLearning;
    Text textResultSum;
    Text textPercent;
    Text textPlayTimeText;  // Tanulással eltöltött idő: szöveg megjelenítésére
    Text textPlayTimeValue;

    ProgressBar progressBar;

    string buttonName;
    Common.CallBack_In_String buttonClick;

    // Use this for initialization
    void Awake()
    {
        content = gameObject.SearchChild("Content").gameObject;

        gameObjectFrameGame = gameObject.SearchChild("ImageFrameGame");
        gameObjectChest = gameObject.SearchChild("ImageChest");
        gameObjectCoin = gameObject.SearchChild("ImageCoin");

        rectTransformCurriculumPlay = GetComponent<RectTransform>();

        progressBar = gameObject.SearchChild("ProgressBar").GetComponent<ProgressBar>();

        textStartLearning = gameObject.SearchChild("TextStartLearning").GetComponent<Text>();
        textResultSum = gameObject.SearchChild("TextResultSum").GetComponent<Text>();
        textPercent = gameObject.SearchChild("TextPercent").GetComponent<Text>();
        textPlayTimeText = gameObject.SearchChild("TextPlayTimeText").GetComponent<Text>();
        textPlayTimeValue = gameObject.SearchChild("TextPlayTimeValue").GetComponent<Text>();
    }

    /*
    public void Initialize(string curriculumPathName, float resultSum, float progressValue, string  string buttonName, Common.CallBack_In_String buttonClick)
    {
        this.buttonName = buttonName;
        this.buttonClick = buttonClick;

        textStartLearning.text = curriculumPathName; // Common.languageController.Translate(C.Texts.StartLearning);
        textResultSum.text = Common.languageController.Translate(C.Texts.ResultSum);
        textPercent.text = ((int)resultSum).ToString() + " %";

        progressBar.SetValue(progressValue);
    }
    */

    public void Initialize(CurriculumPathData curriculumPathData, string buttonName, Common.CallBack_In_String buttonClick)
    {
        this.buttonName = buttonName;
        this.buttonClick = buttonClick;

        // A régi CurriculumPlay panelen nincsenek keretjátékhoz tartozó ikonok
        if (gameObjectFrameGame)
            gameObjectFrameGame.SetActive(curriculumPathData.frameGameExists);
        if (gameObjectChest)
            gameObjectChest.SetActive(curriculumPathData.chestExists);
        if (gameObjectCoin)
            gameObjectCoin.SetActive(curriculumPathData.coinGameExists);

        textStartLearning.text = curriculumPathData.name; // Common.languageController.Translate(C.Texts.StartLearning);
        textResultSum.text = Common.languageController.Translate(C.Texts.ResultSum);
        textPercent.text = ((int)curriculumPathData.scorePercent).ToString() + " %";
        textPlayTimeText.text = Common.languageController.Translate(C.Texts.PlayTime);
        textPlayTimeValue.text = curriculumPathData.playTimeString;

        progressBar.SetValue(curriculumPathData.progress / 100);
    }

    public void Empty() {
        content.SetActive(false);
    }

    public void ButtonClick()
    {
        if (buttonClick != null)
            buttonClick(buttonName);
    }
}
