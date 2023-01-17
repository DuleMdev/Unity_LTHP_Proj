using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/*

Egy csoportba szervezendő tanulók számának beállításához.

*/

public class InfoPanelSelectGroupNumber : MonoBehaviour {

    [Tooltip("Milyik legyen a legnagyobb szám a gombokon.")]
    public int maxButtonNumber;

    [Tooltip("Menyi legyen a gomb bázisa közötti távolság.")]
    public float distanceBetweenButtons;

    Text textIdealNumber;   // "Add meg a csoportok ideális létszámát!" szöveg kiírására
    GameObject groupButton; // Egy csoport választó gomb (prefab) - Ezt fogom sokszorosítani a kellő mennyiségben
    Text textOkButton;      // A Ok gomb szövege
    Text textCancelButton;  // A Cancel gomb szövege

    List<InfoPanelGroupButton> listOfButton;    // A létrehozott gombok listája

    public int selectedNumber { get; private set; }

    Common.CallBack_In_String callBack;

	// Use this for initialization
	void Awake () {
        Common.infoPanelSelectGroupNumber = this;

        // Bekapcsoljuk a panelen levő elemek gyökér elemét
        Common.SearchGameObject(gameObject, "Show").SetActive(true);

        textIdealNumber = Common.SearchGameObject(gameObject, "TextLabelIdealNumber").GetComponent<Text>();
        groupButton = Common.SearchGameObject(gameObject, "GroupButton").gameObject;
        textOkButton = Common.SearchGameObject(gameObject, "TextOk").GetComponent<Text>();
        textCancelButton = Common.SearchGameObject(gameObject, "TextCancel").GetComponent<Text>();

        // Létrehozzuk a szükséges gomb mennyiséget
        listOfButton = new List<InfoPanelGroupButton>();
        for (int i = 0; i < maxButtonNumber-1; i++)
        {
            InfoPanelGroupButton newButtonScript = Instantiate(groupButton).GetComponent<InfoPanelGroupButton>();
            newButtonScript.transform.SetParent(groupButton.transform.parent, false);
            newButtonScript.transform.localPosition = new Vector3(i * distanceBetweenButtons, 0, 0);
            newButtonScript.Initialize((i + 2).ToString(), ButtonClick);

            groupButton.transform.parent.localPosition = groupButton.transform.parent.localPosition.SetX(i * distanceBetweenButtons / -2);

            listOfButton.Add(newButtonScript);
        }

        SelectNumber(0);

        // Kikapcsoljuk a prefabnak használt gombot
        groupButton.SetActive(false);
        // Kikapcsoljuk a panelt is
        gameObject.SetActive(false);
    }

    /// <summary>
    /// A panelt megjelenítjük.
    /// </summary>
    public void Show(Common.CallBack_In_String callBack) {
        this.callBack = callBack;

        textIdealNumber.text = Common.languageController.Translate("IdealPeopleNumberInAGroup");
        textOkButton.text = Common.languageController.Translate("Ok");
        textCancelButton.text = Common.languageController.Translate("Cancel");

        // Megkérjük a MenuInformation szkriptet, hogy jelenítse meg a felúgró ablakot.
        Common.menuInformation.Show(gameObject);
    }

    /// <summary>
    /// Kiválasztottá teszi a megadott számú gombot.
    /// </summary>
    /// <param name="number">Melyik gomb legyen kiválasztva.</param>
    void SelectNumber(int number) {
        Debug.Log("Selected button : " + number);

        selectedNumber = number;
        string stringNumber = number.ToString();

        // Végig megyünk a gombok listáján és beállítjuk mindegyik gombnak a kiválasztottságát
        foreach (InfoPanelGroupButton button in listOfButton)
        {
            button.Selected(button.buttonName == stringNumber);
        }
    }

    /// <summary>
    /// Melyik gombot nyomták meg. A gombok ezt a metódust hívják ha rájuk kattintottak.
    /// </summary>
    /// <param name="buttonName">A megnyomott gomb neve.</param>
    public void ButtonClick(string buttonName) {
        // Csak akkor reagálunk a gombnyomásra, ha látható a panel teljesen
        if (Common.menuInformation.show) {
            switch (buttonName)
            {
                case "Ok":
                case "Cancel":
                    callBack(buttonName);
                    break;
                default:
                    // Feltehetőleg egy számot tartalmazó gombra kattintottak
                    // A szöveges számot átalakítjuk valódi számmá
                    SelectNumber(Convert.ToInt32(buttonName));
                    break;
            }
        }
    }
}
