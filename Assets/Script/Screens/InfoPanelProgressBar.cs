using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InfoPanelProgressBar : InfoPanelBase
{
    public string information;  // Megjelenítendő szöveg


    Text textInformation;       // information szöveg megjelenítéséhez

    Transform imageProgressBarValue;    // A progressBar értékének beállításához


    override protected void SearchComponents()
    {
        Common.infoPanelProgressBar = this;

        textInformation = Common.SearchGameObject(gameObject, "TextInformation").GetComponent<Text>();
        imageProgressBarValue = Common.SearchGameObject(gameObject, "ImageProgressBarValue").transform;
    }

    public void Show(string information)
    {
        this.information = information;

        base.Show(null);
    }

    override protected void ShowComponents()
    {
        textInformation.text = Common.languageController.Translate(information);

        SetProgressBarValue(0);
    }

    /// <summary>
    /// Segítségével beállíthatjuk a progressBar értékét, ami 0 tól 1 -ig terjedő érték lehet.
    /// </summary>
    /// <param name="progressBarValue">A progressBar új értéke.</param>
    public void SetProgressBarValue(float progressBarValue) {
        imageProgressBarValue.localScale = imageProgressBarValue.localScale.SetX(Mathf.Clamp(progressBarValue, 0, 1));
    }
}
