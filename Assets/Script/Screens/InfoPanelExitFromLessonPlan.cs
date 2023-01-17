using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InfoPanelExitFromLessonPlan : InfoPanelBase {

    Text textSureQuestion;  // "Kooperatív óra-mozaik indítása automatikus csoportkiosztással" szöveg megjelenítéséhez
    Text textAttention;     // Szöveg a visszaszámláláshoz
    Text textRemark;        // "Az óraterv elhagyásakor az oktatói és diák tabletek közötti kapcsolat megszakad" szöveg megjelenítéséhez

    Text textOkButton;      // A Ok gomb szövege
    Text textCancelButton;  // A Cancel gomb szövege

    override protected void SearchComponents()
    {
        Common.infoPanelExitFromLessonPlan = this;

        textSureQuestion = Common.SearchGameObject(gameObject, "TextSureQuestion").GetComponent<Text>();
        textAttention = Common.SearchGameObject(gameObject, "TextAttention").GetComponent<Text>();
        textRemark = Common.SearchGameObject(gameObject, "TextRemark").GetComponent<Text>();
        textOkButton = Common.SearchGameObject(gameObject, "TextOk").GetComponent<Text>();
        textCancelButton = Common.SearchGameObject(gameObject, "TextCancel").GetComponent<Text>();
    }

    override protected void ShowComponents()
    {
        textSureQuestion.text = Common.languageController.Translate("SureLeaveLessonPlan");
        textAttention.text = Common.languageController.Translate("Attention");
        textRemark.text = Common.languageController.Translate("ConnectBreakBetweenTeacherAndStudent");
        textOkButton.text = Common.languageController.Translate("Ok");
        textCancelButton.text = Common.languageController.Translate("Cancel");
    }
}
