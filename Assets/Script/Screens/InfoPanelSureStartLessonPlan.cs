using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*

Óraterv indításának megerősítéséhez

*/

public class InfoPanelSureStartLessonPlan : InfoPanelBase {

    Text textSureQuestion;
    Text textOkButton;      // A Ok gomb szövege
    Text textCancelButton;  // A Cancel gomb szövege


    override protected void SearchComponents()
    {
        Common.infoPanelSureStartLessonPlan = this;

        textSureQuestion = Common.SearchGameObject(gameObject, "TextSureQuestion").GetComponent<Text>();
        textOkButton = Common.SearchGameObject(gameObject, "TextOk").GetComponent<Text>();
        textCancelButton = Common.SearchGameObject(gameObject, "TextCancel").GetComponent<Text>();

    }

    override protected void ShowComponents()
    {
        textSureQuestion.text = Common.languageController.Translate("SureStartLessonPlan");
        textOkButton.text = Common.languageController.Translate("Ok");
        textCancelButton.text = Common.languageController.Translate("Cancel");
    }
}
