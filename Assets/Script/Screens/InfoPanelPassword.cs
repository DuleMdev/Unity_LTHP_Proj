using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InfoPanelPassword : InfoPanelBase {

    [HideInInspector]
    public string password { get { return inputFieldPassword.text; } }  // A begépelt jelszó

    Text textLabelPassword;
    InputField inputFieldPassword;

    Text textOkButton;              // Az Ok gomb szövege
    Text textCancelButton;          // A Cancel gomb szövege

    override protected void SearchComponents()
    {
        Common.infoPanelPassword = this;

        textLabelPassword = Common.SearchGameObject(gameObject, "TextLabelPassword").GetComponent<Text>();
        inputFieldPassword = Common.SearchGameObject(gameObject, "InputFieldPassword").GetComponent<InputField>();
        textOkButton = Common.SearchGameObject(gameObject, "TextOk").GetComponent<Text>();
        textCancelButton = Common.SearchGameObject(gameObject, "TextCancel").GetComponent<Text>();
    }

    override protected void ShowComponents()
    {
        textLabelPassword.text = Common.languageController.Translate("Password");
        inputFieldPassword.text = "";
        textOkButton.text = Common.languageController.Translate("Ok");
        textCancelButton.text = Common.languageController.Translate("Cancel");
    }
}
