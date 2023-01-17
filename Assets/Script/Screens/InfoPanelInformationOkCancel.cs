using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InfoPanelInformationOkCancel : InfoPanelBase
{
    public string information;  // Megjelenítendő szöveg

    Text textInformation;       // information szöveg megjelenítéséhez

    Text textOkButton;          // A Ok gomb szövege
    Text textCancelButton;      // A Cancel gomb szövege

    override protected void SearchComponents()
    {
        Common.infoPanelInformationOkCancel = this;

        textInformation = Common.SearchGameObject(gameObject, "TextInformation").GetComponent<Text>();
        textOkButton = Common.SearchGameObject(gameObject, "TextOk").GetComponent<Text>();
        textCancelButton = Common.SearchGameObject(gameObject, "TextCancel").GetComponent<Text>();
    }

    public void Show(string information, Common.CallBack_In_String callBack)
    {
        this.information = information;

        Show(callBack);
    }

    override protected void ShowComponents()
    {
        textInformation.text = Common.languageController.Translate(information);

        textOkButton.text = Common.languageController.Translate("Ok");
        textCancelButton.text = Common.languageController.Translate("Cancel");
    }
}
