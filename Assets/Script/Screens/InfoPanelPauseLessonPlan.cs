using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InfoPanelPauseLessonPlan : InfoPanelBase {

    Text textMessage;       // "Az óraterv szünetel." szöveg megjelenítéséhez
    Text textRemark;        // "A folytatáshoz nyomd meg a fejlécen a 'play' gombot!" szöveg megjelenítéséhez

    Text textOkButton;      // A Ok gomb szövege

    override protected void SearchComponents()
    {
        Common.infoPanelPauseLessonPlan = this;

        textMessage = Common.SearchGameObject(gameObject, "TextMessage").GetComponent<Text>();
        textRemark = Common.SearchGameObject(gameObject, "TextRemark").GetComponent<Text>();
        textOkButton = Common.SearchGameObject(gameObject, "TextOk").GetComponent<Text>();
    }

    override protected void ShowComponents()
    {
        textMessage.text = Common.languageController.Translate(C.Texts.LessonPlanIsBreaking);
        textRemark.text = Common.languageController.Translate(C.Texts.PressPlayButtonToContinue);
        textOkButton.text = Common.languageController.Translate("Ok");
    }
}
