using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class CanvasScreenSetupStart : HHHScreen {

    GameObject canvas;

    Text ServerText;
    Text ClientText;

    // Use this for initialization
    new void Awake()
    {
        // Összeszedjük az állítandó componensekre a referenciákat
        canvas = Common.SearchGameObject(gameObject, "Canvas").gameObject;

        ServerText = Common.SearchGameObject(gameObject, "TextServer").GetComponent<Text>();
        ClientText = Common.SearchGameObject(gameObject, "TextClient").GetComponent<Text>();
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        ServerText.text =
            Common.languageController.Translate("Server") + "\n\n" +
            Common.languageController.Translate("Teacher tablet");
        ClientText.text =
            Common.languageController.Translate("Client") + "\n\n" +
            Common.languageController.Translate("Student tablet");

        yield return null;
    }

    // Amikor az új képernyő megjelenítése elkezdődik akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowStartCoroutine()
    {
        canvas.SetActive(true);

        yield return null;
    }

    void OnDisable()
    {
        canvas.SetActive(false);
    }

    // A szerver módot választották
    public void ButtonClickSelectServer()
    {
        Debug.Log("Select szerver!");
        Common.screenController.ChangeScreen("CanvasScreenServerConfig");
    }

    // A kliens módot választották
    public void ButtonClickSelectClient()
    {
        Debug.Log("Select kliens!");
        Common.screenController.ChangeScreen("CanvasScreenClientConfig");
    }
}
