using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InfoPanelAutoGroup : InfoPanelBase {

    [Tooltip("Hány másodpercről kezdődjön a visszaszámlálás")]
    public int time;

    Text textAutoGroup;     // "Kooperatív óra-mozaik indítása automatikus csoportkiosztással" szöveg megjelenítéséhez
    Text textCountdown;     // Szöveg a visszaszámláláshoz

    Text textOkButton;      // A Ok gomb szövege
    Text textCustomGroupButton;   // Egyéni csoportosítás gomb szövege
    Text textCancelButton;  // A Cancel gomb szövege

    float remainTime;       // Mennyi idő maradt még a visszaszámlálásból

    override protected void SearchComponents()
    {
        Common.infoPanelAutoGroup = this;

        textAutoGroup = Common.SearchGameObject(gameObject, "TextAutoGroup").GetComponent<Text>();
        textCountdown = Common.SearchGameObject(gameObject, "TextCountdown").GetComponent<Text>();
        textOkButton = Common.SearchGameObject(gameObject, "TextOk").GetComponent<Text>();
        textCustomGroupButton = Common.SearchGameObject(gameObject, "TextCustom").GetComponent<Text>();
        textCancelButton = Common.SearchGameObject(gameObject, "TextCancel").GetComponent<Text>();
    }

    override protected void ShowComponents()
    {
        textAutoGroup.text = Common.languageController.Translate("CooperativLessonMosaicStartAutoGroup");
        textCountdown.text = "";
        textOkButton.text = Common.languageController.Translate("Ok");
        textCustomGroupButton.text = Common.languageController.Translate("CustomGroup");
        textCancelButton.text = Common.languageController.Translate("Cancel");

        remainTime = time + 0.999f;
    }

    void Update() {
        if (remainTime == 0)
            return;

        remainTime = Mathf.Clamp(remainTime - Time.deltaTime, 0, float.MaxValue);

        textCountdown.text = Common.languageController.Translate("AutoGroup") + ": "
            + ((int)remainTime).ToString() + " " + Common.languageController.Translate("second");

        if (remainTime == 0) 
            ButtonClick("Ok");
    }
}
