using UnityEngine;
using System.Collections;

using UnityEngine.UI;

/*

Az InfoPanelSelectedGroupNumber panelen levő gombok szkriptje, amivel az ideális csoport létszámot tudjuk beállítani.



*/

public class InfoPanelGroupButton : MonoBehaviour {

    Image imageSelected;    // Ez az image jelzi, hogy a gomb kiválasztott állapotban van-e
    Text textButton;        // A gombon megjelenő szöveg (ami egy szám lesz egyébként 2-10)

    Common.CallBack_In_String callBack;

    public string buttonName { get; private set; }

	// Use this for initialization
	void Awake () {
        imageSelected = GetComponent<Image>();
        textButton = Common.SearchGameObject(gameObject, "Text").GetComponent<Text>();
	}

    /// <summary>
    /// Inicializáljuk a gomb komponenst. Megadhatjuk a nevét és hogy melyik metódust hívja meg gombnyomáskor.
    /// </summary>
    /// <param name="buttonText">A gombon megjelenítendő szöveg.</param>
    /// <param name="callBack">A gombnyomáskor meghívandó metódus.</param>
    public void Initialize(string buttonText, Common.CallBack_In_String callBack) {
        textButton.text = buttonText;
        buttonName = buttonText;
        this.callBack = callBack;
    }

    /// <summary>
    /// Ezzel a metódussal kapcsolhatjuk ki/be a gomb kiválasztását
    /// </summary>
    /// <param name="selected"></param>
    public void Selected(bool selected) {
        imageSelected.enabled = selected;
    }

    /// <summary>
    /// Button componens fogja meghívna ha megnyomták
    /// </summary>
    public void ButtonClick() {
        callBack(buttonName);
    }
}
