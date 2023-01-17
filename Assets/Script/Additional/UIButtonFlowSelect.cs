using UnityEngine;
using System.Collections;

public class UIButtonFlowSelect : MonoBehaviour {

    /*
    Ez az objektum a szerveren a kliensek gyűjtésénél használatos.
    Ezzel tudunk egy gombot létrehozni ami indít egy Flow-t.

    Az objektumnak meg kell adni egy eljárást, amit a gomb megnyomásakor kell meghívnia.
    SetButtonPressed

    */

    public delegate void CallBack_In_String(string buttonName);

    CallBack_In_String callBackButtonPressed;   // Mit hívjon meg ha megnyomták a gombot

    Button button;  // A szkript gombja

    // Use this for initialization
    void Awake () {

        button = GetComponent<Button>();

        //button.  onClick.AddListener(ButtonPressed);
    }

    // Beállíthatjuk vele a gomb nevét
    void SetButtonName(string buttonName) {
        gameObject.name = buttonName;
    }

    // Beállítjuk, hogy milyen eseményt hívjon meg gombnyomáskor
    void SetButtonPressed(CallBack_In_String callBackButtonPressed) {
        this.callBackButtonPressed = callBackButtonPressed;
    }

	// Update is called once per frame
	void Update () {
	
	}

    // Meghívódik, ha megnyomták a gombot
    void ButtonPressed() {
        if (callBackButtonPressed != null)
            callBackButtonPressed(gameObject.name);
    }
}
