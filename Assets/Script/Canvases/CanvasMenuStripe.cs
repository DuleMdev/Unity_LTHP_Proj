using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class CanvasMenuStripe : MonoBehaviour {

    Image imageBackground;
    GameObject buttonBack;
    GameObject buttonHome;
    Text textFlowName;
    GameObject buttonPlay;
    GameObject buttonPause;
    Text textExactTime;
    Text textElapsedTime;

    public Common.CallBack_In_String buttonClick;

    // Use this for initialization
    void Awake () {
        //Common.canvasMenuStripe = this;

        imageBackground = Common.SearchGameObject(gameObject, "ImageBackground").GetComponent<Image>();
        buttonBack = Common.SearchGameObject(gameObject, "ButtonBack").gameObject;
        buttonHome = Common.SearchGameObject(gameObject, "ButtonHome").gameObject;
        textFlowName = Common.SearchGameObject(gameObject, "TextFlowName").GetComponent<Text>();
        buttonPlay = Common.SearchGameObject(gameObject, "ButtonPlay").gameObject;
        buttonPause = Common.SearchGameObject(gameObject, "ButtonPause").gameObject;
        textExactTime = Common.SearchGameObject(gameObject, "TextExactTime").GetComponent<Text>();
        textElapsedTime = Common.SearchGameObject(gameObject, "TextElapsedTime").GetComponent<Text>();

        // A menüsávon található összes komponenst eltüntetjük
        SetItem();
    }

    void Update () {
        // A rendszer időt ki kell írni a pontos időt mutató textbe
        textElapsedTime.text = Common.languageController.Translate("Elapsed Time") + ": " +
            System.DateTime.UtcNow.ToString("HH:mm"); 
    }

    /// <summary>
    /// A fejléc sávon található elemeket inicializálja.
    /// Ha valamelyik érték null vagy false az ki lesz kapcsolva, ha más az értéke akkor látható lesz.
    /// </summary>
    /// <param name="backgroundColor">Milyen szinű legyen a háttér.</param>
    /// <param name="backButton">Látható legyen-e a vissza gomb.</param>
    /// <param name="homeButton">Látható legyen-e a haza gomb.</param>
    /// <param name="flowName">Mi legyen látható a flow név mezőben.</param>
    /// <param name="playButton">Látható legyen-e a indítás gomb.</param>
    /// <param name="pauseButton">Látható legyen-e a szünet gomb.</param>
    /// <param name="elapsedTime">Mi legyen látható az eltelt idő mezőben, csak az időt kell megadni.</param>
    public void SetItem(Color? backgroundColor = null, bool backButton = false, bool homeButton = false, string flowName = null, bool playButton = false, bool pauseButton = false, string elapsedTime = null) {
        imageBackground.gameObject.SetActive(backgroundColor != null);
        if (backgroundColor != null)
            imageBackground.color = backgroundColor.Value;

        buttonBack.SetActive(backButton);
        buttonHome.SetActive(homeButton);

        textFlowName.transform.parent.gameObject.SetActive(flowName != null);
        textFlowName.text = flowName;

        buttonPlay.SetActive(playButton);
        buttonPause.SetActive(pauseButton);

        textElapsedTime.transform.parent.gameObject.SetActive(elapsedTime != null);
        textElapsedTime.text = elapsedTime;
    }

    /// <summary>
    /// Ezzel a metódussal megváltoztathatjuk a fejléc sávon található elemek láthatóságát.
    /// Vagy akár frissíthetjük az elemek tartalmát.
    /// </summary>
    /// <param name="playButton">Változtassuk-e és mire az indítás gomb láthatóságát.</param>
    /// <param name="pauseButton">Változtassuk-e és mire a szünet gomb láthatóságát.</param>
    /// <param name="elapsedTime">Megváltoztathatjuk az eltelt időt.</param>
    public void ChangeItem(bool? playButton = null, bool? pauseButton = null, string elapsedTime = null) {
        if (playButton != null)
            buttonPlay.SetActive(playButton.Value);
        if (pauseButton != null)
            buttonPause.SetActive(pauseButton.Value);
        if (elapsedTime != null)
            textElapsedTime.text = elapsedTime;
    }

    /// <summary>
    /// A menü fejlécében levő gombok megnyomásakor hívódik meg ez a metódus.
    /// </summary>
    /// <param name="clickedButtonName">A menü fejlécében megnyomott gomb neve.</param>
    public void ButtonClick(string clickedButtonName) {
        // A gomb nyomás eseményt tovább passzoljuk a felíratkozott metódusnak
        if (buttonClick != null)
            buttonClick(clickedButtonName);
    }
}
