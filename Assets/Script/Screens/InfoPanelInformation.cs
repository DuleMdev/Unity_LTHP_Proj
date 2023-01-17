using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InfoPanelInformation : InfoPanelBase
{
    public string information;  // Megjelenítendő szöveg
    public bool okButton;       // Kell-e az okButton

    Text textInformation;       // information szöveg megjelenítéséhez

    Text textOkButton;          // A Ok gomb szövege

    override protected void SearchComponents()
    {
        Common.infoPanelInformation = this;

        textInformation = Common.SearchGameObject(gameObject, "TextInformation").GetComponent<Text>();
        textOkButton = Common.SearchGameObject(gameObject, "TextOk").GetComponent<Text>();
    }

    public void Show(string information, bool okButton, Common.CallBack_In_String callBack)
    {
        this.information = information;
        this.okButton = okButton;

        Show(callBack);
    }

    override protected void ShowComponents()
    {
        textInformation.text = Common.languageController.Translate(information);

        textOkButton.text = Common.languageController.Translate("Ok");
        textOkButton.transform.parent.gameObject.SetActive(okButton); // Be / Ki kapcsoljuk az Ok gombot attól függően, hogy kell vagy nem kell
    }
}