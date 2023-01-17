using UnityEngine;

using System.Collections;
using UnityEngine.Networking;

using SimpleJSON;
using UnityEngine.UI;
using System.IO;
/*

A kliens kezdő képernyője.
Ha először járunk itt, akkor simán be jön a képernyő.

Ha másodjára vagy sokadjára, akkor feldob egy információs ablakot, hogy megszakadt a kapcsolat a szerverrel,
mivel csak akkor kerülünk vissza erre a képernyőre ha ez történik.





*/
public class ClientStartScreen : HHHScreen {

    //GameObject playButton;

    Text textVersionNumber;

    bool clientStart;       // Elindítottuk már a klienst

    bool timeMeasure;
    float timeLimit;

	// Use this for initialization
	new void Awake () {
        //playButton = Common.SearchGameObject(gameObject, "PlayButton").gameObject;

        textVersionNumber = gameObject.SearchChild("TextVersionNumber").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update () {
        if (timeMeasure) {
            if (Time.time > timeLimit) {
                Common.infoPanelInformation.Show(C.Texts.ConnectToServerUnsuccessful, true, (string buttonName) => {
                    Common.menuInformation.Hide();
                });

                Common.HHHnetwork.StopHost();

                timeMeasure = false;
            }
        }
	}

    /// <summary>
    /// Amikor az új képernyő megjelenítése elkezdődik akkor ezt meghívja a ScreenController 
    /// </summary>
    /// <returns></returns>
    override public IEnumerator ScreenShowStartCoroutine()
    {
        // Kiírjuk az alkalmazás verzió számát
        textVersionNumber.text = Path.GetFileNameWithoutExtension(Common.configurationController.versionNumber);

        // Kiírjuk a tanuló nevét
        //Common.canvasNetworkHUD.SetText((Common.configurationController.deviceIsServer) ? "" : Common.configurationController.studentName);
        Common.canvasNetworkHUD.SetText(Common.configurationController.studentName);

        // Töröljük a csoport szinét
        Common.configurationController.userGroup = -1;

        yield break;
    }

    /// <summary>
    /// Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController 
    /// </summary>
    /// <returns></returns>
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        // Ha már el volt indítva a cliens, akkor valószínű, hogy azért vagyunk megint ezen a képernyőn mert megszakadt a kapcsolat
        if (clientStart) {
            Common.infoPanelInformation.Show(C.Texts.ConnectIsBroken, true, (string buttonName) => {
                Common.menuInformation.Hide();
            });
        }

        clientStart = false;

        yield break;
    }

    public void ButtonClick(string buttonName) {
        switch (buttonName)
        {
            case "Setup":
                Common.screenController.ChangeScreen("MenuSetup");
                break;
            case "Play":

                //playButton.SetActive(false);
                Common.configurationController.StartLog();
                Common.configurationController.Log("A kliens kapcsolódni próbál!");

                // Csatlakozunk a szerverhez
                Common.HHHnetwork.callBackNetworkEvent = MessageArrived;
                Common.HHHnetwork.messageProcessingEnabled = true;

                Common.HHHnetwork.StartClientHost();
                Common.HHHnetwork.ConnectToServer();

                Common.infoPanelInformation.Show(C.Texts.ConnectToServer, false, null);

                timeLimit = Time.time + 5; // 5 másodperc van a csatlakozásra
                timeMeasure = true;
                break;
        }
    }

    public void MessageArrived(NetworkEventType networkEvent, int connectionID, JSONNode jsonMessage) {
        switch (networkEvent)
        {
            case NetworkEventType.ConnectEvent:
                // Ha sikerült kapcsolatot létesíteni a szerverrel
                if (Common.HHHnetwork.clientConnectionID == connectionID) {

                    // elindítjuk a kliens várakozási képernyőt miután a kapcsolódási infoPanel eltünt
                    Common.menuInformation.Hide(() => {
                        Common.screenController.ChangeScreen(C.Screens.MenuClientWaitStart);
                    });

                    clientStart = true;
                    timeMeasure = false;
                }
                break;

            case NetworkEventType.DisconnectEvent:
                if (!Common.HHHnetwork.clientIsConnected && Common.HHHnetwork.clientConnectionID == connectionID) {

                    Common.infoPanelInformation.Show(C.Texts.ConnectToServerUnsuccessful, true, (string buttonName) => {
                        Common.menuInformation.Hide();
                    } );

                    //playButton.SetActive(true);
                }
                break;
            case NetworkEventType.DataEvent:

                break;
        }
    }
}
