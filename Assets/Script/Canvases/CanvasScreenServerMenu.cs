using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanvasScreenServerMenu : HHHScreen
{

    GameObject canvas;

    Text textServerIP;              // A szerver IP címének kiírásához
    Text textServerConfig;          // A szerver konfigurálás szöveg kiírásához
    Text textServerStart;           // A szerver start szöveg kiírásához

    // Use this for initialization
    new void Awake()
    {
        // Összeszedjük az állítandó componensekre a referenciákat
        canvas = Common.SearchGameObject(gameObject, "Canvas").gameObject;

        textServerIP = Common.SearchGameObject(gameObject, "TextServerIP").GetComponent<Text>();
        textServerConfig = Common.SearchGameObject(gameObject, "TextServerConfig").GetComponent<Text>();
        textServerStart = Common.SearchGameObject(gameObject, "TextServerStart").GetComponent<Text>();
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        // Beállítjuk a szövegek szövegét a nyelvnek megfelelően
        textServerIP.text = Common.languageController.Translate("Server address") + " : " + Common.HHHnetwork.LocalIPAddress();
        textServerConfig.text = Common.languageController.Translate("Server config");
        textServerStart.text = Common.languageController.Translate("Server start");

        // Más szükséges értékeket is beállítunk alapértékekre

        yield return null;
    }

    // Amikor az új képernyő megjelenítése elkezdődik akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowStartCoroutine()
    {
        canvas.SetActive(true);

        yield return null;
    }

    // Ha letiltják a gameObject-et, akkor letiltjuk a Canvast, hogy ha engedélyezik, akkor a Canvas ne legyen látható azonnal
    void OnDisable()
    {
        canvas.SetActive(false);
    }


    // A szerver beállítások gombra kattintottak
    public void ButtonClickServerSetup()
    {
        // Hiba ellenőrzés  ************************************************************

        Debug.Log("Megnyomták a szerver beállítása gombot!");
        Common.screenController.ChangeScreen("CanvasScreenServerConfig");
    }

    // A szerver indítás gombra kattintottak
    public void ButtonClickServerStrart()
    {
        // Hiba ellenőrzés  ************************************************************

        Debug.Log("Megnyomták a szerver indítása gombot!");
        //Common.screenController.ChangeScreen("CanvasScreenServerStart");
        Common.screenController.ChangeScreen("CanvasScreenServerCollectClients");
    }

}

