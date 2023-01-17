using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InfoPanelAutoStartNextLessonMosaic : InfoPanelBase {

    [Tooltip("Hány másodpercről kezdődjön a visszaszámlálás")]
    public int time;

    Text textLessonMosaicStart;     // "Óra-mozaik indítása" szöveg megjelenítéséhez
    Text textCountdown;             // Szöveg a visszaszámláláshoz

    Text textCancelButton;          // A Cancel gomb szövege

    float remainTime;               // Mennyi idő maradt még a visszaszámlálásból

    override protected void SearchComponents()
    {
        Common.infoPanelAutoStartNextLessonMosaic = this;

        textLessonMosaicStart = Common.SearchGameObject(gameObject, "TextLessonMosaicStart").GetComponent<Text>();
        textCountdown = Common.SearchGameObject(gameObject, "TextCountdown").GetComponent<Text>();
        textCancelButton = Common.SearchGameObject(gameObject, "TextCancel").GetComponent<Text>();
    }

    override protected void ShowComponents()
    {
        textLessonMosaicStart.text = Common.languageController.Translate("LessonMosaicStart");
        textCountdown.text = "";
        textCancelButton.text = Common.languageController.Translate("Cancel");

        remainTime = time + 0.999f;
    }

    void Update()
    {
        if (remainTime == 0)
            return;

        remainTime = Mathf.Clamp(remainTime - Time.deltaTime, 0, float.MaxValue);

        textCountdown.text = Common.languageController.Translate("AutoStrart") + ": "
            + ((int)remainTime).ToString() + " " + Common.languageController.Translate("second");

        if (remainTime == 0)
            ButtonClick("AutoStartOk");
    }
}
