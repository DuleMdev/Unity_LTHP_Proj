using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class MenuLessonPlanRow : MonoBehaviour {

    Text textLessonPlanName;
    Text textLessonPLanLabels;

    int lessonPlanID;

    Common.CallBack_In_Int_String buttonCallBack;

	// Use this for initialization
	void Awake () {
        textLessonPlanName = Common.SearchGameObject(gameObject, "TextLessonPlanName").GetComponent<Text>();
        textLessonPLanLabels = Common.SearchGameObject(gameObject, "TextLessonPlanLabels").GetComponent<Text>();
    }

    public void Initialize(int lessonPlanID, string lessonPlanName, string lessonPlanLabels, Common.CallBack_In_Int_String buttonCallBack) {
        textLessonPlanName.text = Common.languageController.Translate(C.Texts.LessonPlanID) + ": " + lessonPlanName;
        textLessonPLanLabels.text = Common.languageController.Translate(C.Texts.LessonPlanLabels) + ": " + lessonPlanLabels;
        this.buttonCallBack = buttonCallBack;

        this.lessonPlanID = lessonPlanID;
    }

    // Update is called once per frame
    void Update () {
        // Ha a szövegek nem férnek ki a rendelkezésre álló helyre, akkor 
        // fényújság szerűen kellene megjelenniük
	}

    public void ButtonClick(string buttonName) {
        if (buttonCallBack != null)
            buttonCallBack(lessonPlanID, buttonName);
    }
}
