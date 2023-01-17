using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InfoPanelAfterGrouping : InfoPanelBase {

    [Tooltip("Hány másodpercről kezdődjön a visszaszámlálás")]
    public int time;

    Text textGroupingHappened;  // Megtörtént a csoportosítás
    Text textRemark;            // "Kooperatív óra-mozaik indítása automatikus csoportkiosztással" szöveg megjelenítéséhez
    Text textCountdown;         // Szöveg a visszaszámláláshoz

    Text textOkButton;          // A Ok gomb szövege (A csoportok rendeződtek, óra-mozaik indítása)
    Text textCancelButton;      // A Cancel gomb szövege

    float remainTime;           // Mennyi idő maradt még a visszaszámlálásból

    override protected void SearchComponents()
    {
        Common.infoPanelAfterGrouping = this;

        textGroupingHappened = Common.SearchGameObject(gameObject, "TextGroupingHappened").GetComponent<Text>();
        textRemark = Common.SearchGameObject(gameObject, "TextRemark").GetComponent<Text>();
        textCountdown = Common.SearchGameObject(gameObject, "TextCountdown").GetComponent<Text>();
        textOkButton = Common.SearchGameObject(gameObject, "TextOk").GetComponent<Text>();
        textCancelButton = Common.SearchGameObject(gameObject, "TextCancel").GetComponent<Text>();
    }

    override protected void ShowComponents()
    {
        textGroupingHappened.text = Common.languageController.Translate("GroupingHappened");
        textRemark.text = Common.languageController.Translate("GroupsOrderingByScreenColor");
        textCountdown.text = "";
        textOkButton.text = Common.languageController.Translate("GroupsOrderedLessonMosaicStart");
        textCancelButton.text = Common.languageController.Translate("Cancel");

        remainTime = time + 0.999f;
    }

    void Update()
    {
        if (remainTime == 0)
            return;

        remainTime = Mathf.Clamp(remainTime - Time.deltaTime, 0, float.MaxValue);

        //string s = Common.languageController.Translate("TheLessonMosaicStartAfterXSeconds");

        string[] elements = System.Text.RegularExpressions.Regex.Split(Common.languageController.Translate("TheLessonMosaicStartAfter(time)Seconds"), @"\(time\)");

        textCountdown.text = elements[0] + ((int)remainTime).ToString() + elements[1];

        if (remainTime == 0)
            ButtonClick("Ok");
    }

}
