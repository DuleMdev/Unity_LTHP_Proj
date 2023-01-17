using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;

public class CanvasScreenClientWaitStart : HHHScreen
{
    [Tooltip("Feladatokat tartalmazó JSON file.")]
    // public TextAsset tasksInJSON;        // A feladatokat tartalmazó json file

    GameObject canvas;

    Text textConnectSuccess;
    Text textUserName;

    Image imageBackground;          // A háttérkép, ami egyszínű

    Color defaultBackgroundColor;   // Mi a háttér alapszíne

    float lastGoMenu;               // Mikor kérte az Update utoljára, hogy mennjünk a menübe

    // Use this for initialization
    new void Awake()
    {
        // Összeszedjük az állítandó componensekre a referenciákat
        canvas = Common.SearchGameObject(gameObject, "Canvas").gameObject;

        textConnectSuccess = Common.SearchGameObject(gameObject, "TextConnectSuccess").GetComponent<Text>();
        textUserName = Common.SearchGameObject(gameObject, "TextUserName").GetComponent<Text>();

        imageBackground = Common.SearchGameObject(gameObject, "ImageBackground").GetComponent<Image>();

        defaultBackgroundColor = imageBackground.color;
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        // Beállítjuk a szövegek szövegét a nyelvnek megfelelően
        textConnectSuccess.text = Common.languageController.Translate("Connect success");
        textUserName.text = Common.configurationController.userName;

        // Beállítjuk, hogy a szervertől kapjuk meg az adatokat
        Common.HHHnetwork.callBackNetworkEvent = ReceivedNetworkEvent;

        //Common.HHHnetwork.SendClientIdentity(); // Elküldjük a kliens adatokat a szervernek

        Common.HHHnetwork.messageProcessingEnabled = true;

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

    // Network objektum hívja meg ha érkezett valamilyen esemény
    void ReceivedNetworkEvent(NetworkEventType networkEventType, int connectionID, JSONNode receivedData)
    {
        switch (networkEventType)
        {
            case NetworkEventType.DataEvent:
                string s = receivedData["dataContent"];

                switch (s)
                {
                    case "setGroup" :
                        Common.taskControllerOld.groupID = receivedData["groupID"].AsInt;
                        Common.taskControllerOld.indexInGroup = receivedData["indexInGroup"].AsInt;
                        Common.taskControllerOld.groupHeadCount = receivedData["groupHeadCount"].AsInt;

                        SetColor();
                        break;

                    case "startFlow" :
                        int flowIndex = receivedData["flowIndex"].AsInt;

                        JSONNode node = JSONNode.Parse(Common.configurationController.tasksInJSON.text);
                        // Common.taskController.PlayQuestionList(node["flows"][flowIndex].ToString(), false, () => { Common.screenController.ChangeScreen(gameObject.name); });

                        break;

                    case "startFreePlay" :
                        Common.screenController.ChangeScreen("MainMenu");
                        break;

                    case "playStop" :
                        // Ezt nem itt kell feldolgozni, hanem a TaskController-ben
                        break;
                }

                break;

            case NetworkEventType.DisconnectEvent:
                // Common.screenController.ChangeScreen("CanvasScreenClientMenu");
                break;
        }
    }

    // Beállítja a háttérszínt a klinens csoport számának megfelelően
    void SetColor() {

        imageBackground.color = (Common.taskControllerOld.groupID == -1) ? defaultBackgroundColor : Common.configurationController.groupColors[Common.taskControllerOld.groupID];
        //imageBackground.color = Common.configurationController.groupColors[Common.taskController.groupID];
    }


    // Az Update figyeli, hogy ha megszünt a kapcsolat a szerverrel, akkor menjünk a kliens menübe, hogy újra tudjunk csatlakozni
    void Update() {
        // Ha a kapcsolat megszünt a szerverrel és ez a képernyő aktív és az u, akkor kilépünk a kliens menübe
        if (!Common.HHHnetwork.clientIsConnected && // Ha a kapcsolat megszünt a szerverrel 
            Common.screenController.actScreen == this && // és ez a képernyő az aktív
            Mathf.Abs(lastGoMenu - Time.time) > 5) // és 5 másodperc már eltelt az előző menjünk a menübe kérés óta
        {
            lastGoMenu = Time.time;
            Common.screenController.ChangeScreen("CanvasScreenClientMenu");
        }
    }

}
