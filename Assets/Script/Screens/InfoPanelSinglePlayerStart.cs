using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InfoPanelSinglePlayerStart : InfoPanelBase
{

    [Tooltip("Hány másodpercről kezdődjön a visszaszámlálás")]
    public int time;

    Text textSinglePlayerStart; // Megtörtént a csoportosítás
    Text textCountdown;         // Szöveg a visszaszámláláshoz

    Text textOkButton;          // A Ok gomb szövege (A csoportok rendeződtek, óra-mozaik indítása)
    Text textCancelButton;      // A Cancel gomb szövege

    float remainTime;           // Mennyi idő maradt még a visszaszámlálásból

    override protected void SearchComponents()
    {
        Common.infoPanelSinglePlayerStart = this;

        textSinglePlayerStart = Common.SearchGameObject(gameObject, "TextSinglePlayerStart").GetComponent<Text>();
        textCountdown = Common.SearchGameObject(gameObject, "TextCountdown").GetComponent<Text>();
        textOkButton = Common.SearchGameObject(gameObject, "TextOk").GetComponent<Text>();
        textCancelButton = Common.SearchGameObject(gameObject, "TextCancel").GetComponent<Text>();
    }

    override protected void ShowComponents()
    {
        textSinglePlayerStart.text = Common.languageController.Translate("StartSinglePlayer");
        textCountdown.text = "";
        textOkButton.text = Common.languageController.Translate("Ok");
        textCancelButton.text = Common.languageController.Translate("Cancel");

        remainTime = time + 0.999f;
    }

    void Update()
    {
        if (remainTime == 0)
            return;

        remainTime = Mathf.Clamp(remainTime - Time.deltaTime, 0, float.MaxValue);

        //string s = Common.languageController.Translate("TheLessonMosaicStartAfterXSeconds");

        string[] elements = System.Text.RegularExpressions.Regex.Split(Common.languageController.Translate("AutomaticStart(time)Sec"), @"\(time\)");

        textCountdown.text = elements[0] + ((int)remainTime).ToString() + elements[1];

        if (remainTime == 0)
            ButtonClick("Ok");
    }

}
