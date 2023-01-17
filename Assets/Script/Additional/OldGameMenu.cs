using UnityEngine;
using System.Collections;

/*



*/

public class OldGameMenu : MonoBehaviour {

    [HideInInspector]
    public bool menuEnabled;        // Engedélyezett-e a menüt nyomkodni a játékokban
    public Button.ButtonClick buttonClick; // Melyik metódust hívja meg ha rákattintottak a menüre

    Transform menuItems;            // Az előugró menüt tartalmazza
    float menuItemsHide;            // A menü magától eltűnik ha letelt az idő. Ez a változó tartalmazza a hátralevő időt

    // Use this for initialization
    void Awake() {

        menuItems = Common.SearchGameObject(gameObject, "menuItems").transform;
        Reset();

        // Gombok szkriptjének beállítása
        foreach (Button button in GetComponentsInChildren<Button>())
            button.buttonClick = ButtonClick;

        menuEnabled = false;
    }

    public void Reset() {
        menuItems.localScale = Vector3.zero;
        menuItemsHide = 0;
        iTween.Stop(menuItems.gameObject);
    }
	
	// Update is called once per frame
	void Update () {
        // Menü kezelés
        menuItemsHide -= Time.deltaTime;

        if (menuItemsHide <= 0 && menuItems.localScale.x == 1)
        { // Ha letelt az idő elrejtjük a menüItems-et
            iTween.ScaleTo(menuItems.gameObject, iTween.Hash("islocal", true, "scale", Vector3.zero, "time", 0.5f, "easeType", iTween.EaseType.easeInQuad));
        }
    }

    // Ha rákattintottak a buborékra, akkor meghívódik ez az eljárás a buborékon levő Button szkript által
    void ButtonClick(Button button)
    {
        if (!menuEnabled) return;

        if (!Common.screenController.changeScreenInProgress)
        { // Ha játékmódban vagyunk, akkor 
            switch (button.buttonType)
            {
                case Button.ButtonType.Menu:
                    iTween.ScaleTo(menuItems.gameObject, iTween.Hash("islocal", true, "scale", Vector3.one, "time", 1, "easeType", iTween.EaseType.easeOutElastic));
                    menuItemsHide = 3; // 3 másodperc múlva eltűnik a menü
                    break;
                case Button.ButtonType.GoMenu:
                    if (menuItems.localScale.x > 0.2f)
                    { // Ha a menu teljesen látszik, akkor vissza megyünk a főmenübe
                        iTween.ScaleTo(menuItems.gameObject, iTween.Hash("islocal", true, "scale", Vector3.one, "time", 1, "easeType", iTween.EaseType.easeOutElastic));
                        iTween.ShakePosition(menuItems.gameObject, iTween.Hash("islocal", true, "amount", new Vector3(0.1f, 0.1f, 0), "time", 2));
                        menuItemsHide = 10; // 10 másodprec múlva eltűnik a menü

                        if (buttonClick != null)
                            buttonClick(button);
                    }
                    break;
            }
        }
    }
}
