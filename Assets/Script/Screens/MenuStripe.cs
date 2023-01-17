using UnityEngine;
using System.Collections;
using System;

using UnityEngine.UI;

public class MenuStripe : MonoBehaviour { 
    GameObject buttonBack;
    GameObject buttonHome;
    Text textFlowName;
    GameObject buttonPlay;
    GameObject buttonPause;
    Text textExactTime;
    Text textElapsedTime;

    public Common.CallBack_In_String buttonClick;

    DateTime elapsedTimeStart;

    // Use this for initialization
    void Awake()
    {
        Common.menuStripe = this;

        // Bekapcsoljuk a Canvas-t mert az Editorban való átláthatóbb munka miatt ki vannak kapcsolva
        Common.SearchGameObject(gameObject, "Canvas").gameObject.SetActive(true);

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

    void Update()
    {
        // A rendszer időt ki kell írni a pontos időt mutató textbe
        textExactTime.text = Common.languageController.Translate("ExactTime") + ": " +
            DateTime.Now.ToString("HH:mm");

        TimeSpan timeSpan = DateTime.Now.Subtract(elapsedTimeStart);
        textElapsedTime.text = Common.languageController.Translate("ElapsedTime") + ": " +
            String.Format("{0:00}:{1:00}", timeSpan.TotalMinutes, timeSpan.Seconds);
    }

    /// <summary>
    /// Nullázza az eltelt időt.
    /// </summary>
    public void ResetElapsedTime() {
        elapsedTimeStart = DateTime.Now;
    }

    /// <summary>
    /// A fejléc sávon található elemeket inicializálja.
    /// Ha valamelyik érték null vagy false az ki lesz kapcsolva, ha más az értéke akkor látható lesz.
    /// </summary>
    /// <param name="backButton">Látható legyen-e a vissza gomb.</param>
    /// <param name="badgeButton">Látható legyen-e a haza gomb.</param>
    /// <param name="flowName">Mi legyen látható a flow név mezőben.</param>
    /// <param name="playButton">Látható legyen-e az indítás gomb.</param>
    /// <param name="pauseButton">Látható legyen-e a szünet gomb.</param>
    /// <param name="elapsedTime">Mi legyen látható az eltelt idő mezőben, csak az időt kell megadni.</param>
    public void SetItem(bool backButton = false, bool badgeButton = false, string flowName = null, bool playButton = false, bool pauseButton = false, string elapsedTime = null)
    {
        buttonBack.SetActive(backButton);
        buttonHome.SetActive(badgeButton);

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
    public void ChangeItem(bool? playButton = null, bool? pauseButton = null, string elapsedTime = null)
    {
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
    public void ButtonClick(string clickedButtonName)
    {
        // A gomb nyomás eseményt tovább passzoljuk a felíratkozott metódusnak
        if (buttonClick != null)
            buttonClick(clickedButtonName);
    }
}
