using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJSON;
using UnityEngine.Networking;

public class MenuClientGrouping : HHHScreen {

    GameObject canvas;

    Text textWaitToStart;           // Várakozás az indításra szöveg kiírásához
    Text textSearchGroupmates;      // Keresd meg a csoport társaidat szöveg kiírásához

    Image imageBackground;          // A háttérkép, ami egyszínű

    Color defaultBackgroundColor;   // Mi a háttér alapszíne

    JSONNode jsonData;

    // Use this for initialization
    void Awake()
    {
        Common.menuClientGrouping = this;

        // Összeszedjük az állítandó componensekre a referenciákat
        canvas = Common.SearchGameObject(gameObject, "Canvas").gameObject;

        textWaitToStart = Common.SearchGameObject(gameObject, "TextWaitToStart").GetComponent<Text>();
        textSearchGroupmates = Common.SearchGameObject(gameObject, "TextSearchGroupmates").GetComponent<Text>();

        imageBackground = Common.SearchGameObject(gameObject, "ImageBackground").GetComponent<Image>();

        defaultBackgroundColor = imageBackground.color;
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        Refresh();

        yield return null;
    }

    // Amikor az új képernyő megjelenítése elkezdődik akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowStartCoroutine()
    {
        canvas.SetActive(true);

        yield return null;
    }

    /// <summary>
    /// Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController 
    /// </summary>
    /// <returns></returns>
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        Common.HHHnetwork.messageProcessingEnabled = true;
        yield break;
    }

    // Ha letiltják a gameObject-et, akkor letiltjuk a Canvast, hogy ha engedélyezik, akkor a Canvas ne legyen látható azonnal
    void OnDisable()
    {
        canvas.SetActive(false);
    }

    public void Refresh() {
        // Beállítjuk a szövegek szövegét a nyelvnek megfelelően
        textWaitToStart.text = Common.languageController.Translate("Connect success");
        textSearchGroupmates.text = Common.languageController.Translate("SearchGroupmates");

        SetColor();
    }

    // Beállítja a háttérszínt a klinens csoport számának megfelelően
    void SetColor()
    {
        imageBackground.color = (Common.configurationController.userGroup == -1) ? defaultBackgroundColor : Common.configurationController.groupColors[Common.configurationController.userGroup];
        //imageBackground.color = Common.configurationController.groupColors[Common.taskController.groupID];
    }
}
