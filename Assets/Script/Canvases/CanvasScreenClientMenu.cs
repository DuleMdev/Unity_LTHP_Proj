using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using UnityEngine.Networking;
using SimpleJSON;

public class CanvasScreenClientMenu : HHHScreen
{

    GameObject canvas;

    Text textButtonClientConfig;              // A szerver IP címének kiírásához
    Text textUserName;
    Text textButtonConnect;
    Text textWaitToServer;                  // Várakozása a szerverre üzenet kiírása

    InputField inputFieldUserName;

    GameObject buttonConnection;            // A connection gomb, amit láthatatlanná kell tenni ha megnyomták a kapcsolódás gombot

    // Use this for initialization
    new void Awake()
    {
        // Összeszedjük az állítandó componensekre a referenciákat
        canvas = Common.SearchGameObject(gameObject, "Canvas").gameObject;

        textButtonClientConfig = Common.SearchGameObject(gameObject, "TextClientConfig").GetComponent<Text>();
        textUserName = Common.SearchGameObject(gameObject, "TextUserName").GetComponent<Text>();
        textButtonConnect = Common.SearchGameObject(gameObject, "TextConnect").GetComponent<Text>();
        textWaitToServer = Common.SearchGameObject(gameObject, "TextWaitToServer").GetComponent<Text>();

        inputFieldUserName = Common.SearchGameObject(gameObject, "InputFieldUserName").GetComponent<InputField>();

        buttonConnection = Common.SearchGameObject(gameObject, "ButtonConnect").gameObject;
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        // Beállítjuk a szövegek szövegét a nyelvnek megfelelően
        textButtonClientConfig.text = Common.languageController.Translate("Client config");
        textUserName.text = Common.languageController.Translate("User name");
        textButtonConnect.text = Common.languageController.Translate("Connect");
        textWaitToServer.text = Common.languageController.Translate("Wait to server");

        // Más szükséges értékeket is beállítunk alapértékekre
        inputFieldUserName.text = Common.configurationController.userName;

        ButtonShow(true);

        yield return null;
    }

    void ButtonShow(bool show) {
        buttonConnection.SetActive(show);
        textWaitToServer.gameObject.SetActive(!show);
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

    // A kliens beállítások gombra kattintottak
    public void ButtonClickClientSetup()
    {
        // Hiba ellenőrzés  ************************************************************

        Debug.Log("Megnyomták a kliens beállítása gombot!");
        Common.screenController.ChangeScreen("CanvasScreenClientConfig");
    }

    // A kliens kapcsolódás gombra kattintottak
    public void ButtonClickConnect()
    {
        // Hiba ellenőrzés  ************************************************************
        if (string.IsNullOrEmpty(inputFieldUserName.text))
        {
            Common.canvasInformation.ShowError("EmptyUserNameError");
        }
        else {
            Common.configurationController.userName = inputFieldUserName.text;
            Common.configurationController.Save();

            Common.HHHnetwork.callBackNetworkEvent = ReceivedNetworkEvent;

            Common.HHHnetwork.StartClientHost(); // Létrehozzuk a portot

            Common.HHHnetwork.ConnectToServer(); // Megkisérlünk kapcsolódni a szerverhez

            Common.HHHnetwork.messageProcessingEnabled = true;

            ButtonShow(false);

            Debug.Log("Megnyomták a kliens indítása gombot!");

            //Common.screenController.ChangeScreen("CanvasScreenServerStart");
            //Common.screenController.ChangeScreen("MainMenu");
            
        }
    }

    void ReceivedNetworkEvent(NetworkEventType networkEventType, int connectionID, JSONNode receivedData)
    {
        switch (networkEventType)
        {
            case NetworkEventType.ConnectEvent:
                //Common.screenController.ChangeScreen("MainMenu");
                Common.screenController.ChangeScreen("CanvasScreenClientWaitStart");
                break;
            case NetworkEventType.DisconnectEvent:
                Common.canvasInformation.ShowError(Common.languageController.Translate("Server not found"));
                ButtonShow(true);
                break;
        }
    }
}
