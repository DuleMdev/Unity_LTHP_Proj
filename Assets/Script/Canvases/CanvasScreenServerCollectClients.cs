using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;

using SimpleJSON;


public class CanvasScreenServerCollectClients : HHHScreen {

    public GameObject clientPrefab;

    [Tooltip("Távolság a klienstábla és a panel széle között.")]
    public float margo;

    GameObject canvas;

    RectTransform content;
    Text textAllClient;     // Az összes csatlakozott kliens számának kiírásához
    Text textGroupNumber;   // A csoport szám kiírásához
    Text textButtonGrouping;    // A csoportosítás gombon a szöveg kiírásához

    InputField inputFieldGroupNumber;   // Hány csoportot hozzunk létre

    float clientHeight;

    List<UIConnectedClient> listOfClientPanel;

	// Use this for initialization
	new void Awake () {
        // Összeszedjük az állítandó componensekre a referenciákat
        canvas = Common.SearchGameObject(gameObject, "Canvas").gameObject;

        content = (RectTransform)Common.SearchGameObject(gameObject, "Content").GetComponent<Transform>();
        textAllClient = Common.SearchGameObject(gameObject, "TextAllClient").GetComponent<Text>();
        textGroupNumber = Common.SearchGameObject(gameObject, "TextGroupNumber").GetComponent<Text>();
        textButtonGrouping = Common.SearchGameObject(gameObject, "TextButtonGrouping").GetComponent<Text>();

        inputFieldGroupNumber = Common.SearchGameObject(gameObject, "InputFieldGroupNumber").GetComponent<InputField>();

        listOfClientPanel = new List<UIConnectedClient>();

        /*
        AddClient("#15", "Iksz ipszilon");
        AddClient("#08", "Valami");
        AddClient("#03", "Gőz Aranka");
        AddClient("#15", "Iksz ipszilon");
        AddClient("#08", "Valami");
        AddClient("#03", "Gőz Aranka");
        AddClient("#15", "Iksz ipszilon");
        AddClient("#08", "Valami");
        AddClient("#03", "Gőz Aranka");
        */
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        // Beállítjuk a szövegek szövegét a nyelvnek megfelelően
        textAllClient.text = Common.languageController.Translate("All client");
        textGroupNumber.text = Common.languageController.Translate("Group number") + " : ";
        textButtonGrouping.text = Common.languageController.Translate("Grouping");

        // Elindítjuk a szervert
        Common.HHHnetwork.StartServerHost();

        // Beállítjuk, hogy a szervertől kapjuk meg az adatokat
        Server_Logic.callBackNetworkEvent = ReceivedNetworkEvent;
        //Common.networkTest.callBackNetworkEvent = ReceivedNetworkEvent;

        Common.HHHnetwork.messageProcessingEnabled = true;

        SetClientPosition();

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

    // Egy új klienst ad a listához
    public void AddClient(string clientID, string userName) {
        UIConnectedClient connectedClient = Instantiate(clientPrefab).GetComponent<UIConnectedClient>();
        connectedClient.transform.SetParent(content, false);
        connectedClient.transform.localScale = Vector3.one;
        connectedClient.SetTexts(clientID: clientID, userName: userName);

        listOfClientPanel.Add(connectedClient);

        SetClientPosition();
    }
    

    // Végig megyünk a kliensek listáján és pozíciónáljuk őket
    public void SetClientPosition() {
        textAllClient.text = Common.languageController.Translate("All client") + " : " + Server_Logic.connectedClient.Count;
                                                                 
        for (int i = 0; i < Server_Logic.connectedClient.Count; i++)
        {
            // Ha kevés a kliensek megjelenítésére szánt tábla, akkor létrehoz egy új sort
            while (i >= listOfClientPanel.Count)
                AddClient("", "");

            // Beállítjuk a panelen megjelenő szövegeket
            //listOfClientPanel[i].SetTexts((i + 1).ToString(), Server_Logic.connectedClient[i].clientID, Server_Logic.connectedClient[i].userName);
            //((RectTransform)listOfConnectedClient[i].transform).position.y = 

            // Beállítjuk a panel elhelyezkedését
            listOfClientPanel[i].transform.localPosition = Common.ChangeVector3(
                listOfClientPanel[i].transform.localPosition, y: -margo - i * 36);

            // Beállítjuk a panel színét
            if (Server_Logic.connectedClient[i].groupID == -1)
                listOfClientPanel[i].SetPanelColor(null);
            else
                listOfClientPanel[i].SetPanelColor(Common.configurationController.groupColors[Server_Logic.connectedClient[i].groupID]);

            /*
            Color? panelColor = (Server_Logic.connectedClient[i].groupID == -1) ? null : Common.configurationController.groupColors[Server_Logic.connectedClient[i].groupID];
            listOfClientPanel[i].SetPanelColor(null);
            listOfClientPanel[i].SetPanelColor((Server_Logic.connectedClient[i].groupID == -1) ? null : Common.configurationController.groupColors[Server_Logic.connectedClient[i].groupID]);
            */
        }

        // Eltávolítjuk a felesleges paneleket
        while (listOfClientPanel.Count > Server_Logic.connectedClient.Count) {
            Destroy(listOfClientPanel[listOfClientPanel.Count - 1].gameObject);
            listOfClientPanel.RemoveAt(listOfClientPanel.Count - 1);
        }

        // Beállítjuk a klienseket tartalmazó panel méretét
        content.sizeDelta = new Vector2(content.sizeDelta.x, listOfClientPanel.Count * 36 + 2 * margo);
    }

    // Beállítja a kliensek panel színét a csoportszámuknak megfelelően
    public void SetClientPanelColor() {
        for (int i = 0; i < Server_Logic.connectedClient.Count; i++)
            listOfClientPanel[i].SetPanelColor(Common.configurationController.groupColors[Server_Logic.connectedClient[i].groupID]);
    }

    void ReceivedNetworkEvent(NetworkEventType networkEventType, int connectionID, JSONNode receivedData) {

        switch (networkEventType)
        {
            case NetworkEventType.DataEvent:
                Debug.Log("CollectClient - received data" + "\n" + receivedData.ToString(" "));

                string s = receivedData["dataContent"];

                if (s == "connect")
                {
                    Server_Logic.NewClient(
                        connectionID,
                        receivedData["tabletID"],
                        receivedData["clientID"],
                        receivedData["userName"]
                        );

                    SetClientPosition();
                }
                break;

            case NetworkEventType.ConnectEvent:

                break;

            case NetworkEventType.DisconnectEvent:
                // El kell távolítani a lecsatlakozott játékost a listából
                Server_Logic.RemoveClient(connectionID);

                SetClientPosition();
                break;
        }
    }

    // A becsatlakozott klienseket csoportokba rendezzük
    public void ButtonClickGroupingClient() {

        int groupNumber = 0;
        if (!string.IsNullOrEmpty(inputFieldGroupNumber.text))
            groupNumber = System.Convert.ToInt32(Common.StringToIntToString(inputFieldGroupNumber.text));

        if (groupNumber < 1)
        {
            Common.canvasInformation.ShowError("GroupNumberError", () => {
                inputFieldGroupNumber.Select();
                inputFieldGroupNumber.ActivateInputField();
            });

            return;
        }

        // Csoportokba szervezzük a klienseket
        //Server_Logic.GroupingClient(groupNumber);

        SetClientPosition();
        //SetClientPanelColor();
    }

    /// <summary>
    /// Törli a beállított csoport adatokat.
    /// </summary>
    public void ButtonClickDeleteGroups() {
        //Server_Logic.DeleteGroupDataAllClients();
        SetClientPosition();
    }

    /*
    // Tesztelési céllal létrehozz egy új klienst
    public void ButtonTestNewClient() {
        Server_Logic.NewClient(0, "adfas", "3", "izéke");

        SetClientPosition();
    }*/

    // A TextField-ben megváltoztatták a csoport számot
    // Ellenőrízzük, hogy a beírt karakter szám-e
    public void ChangeGroupNumber(string text)
    {
        string number = Common.StringToIntToString(inputFieldGroupNumber.text).ToString();

        Debug.Log("Port : " + inputFieldGroupNumber.text);

        if (inputFieldGroupNumber.text != number)
            inputFieldGroupNumber.text = number;
    }

    // Elindítja a megadott Flow-t
    void StartFlow(int flowIndex) {
        // Az összes kliensnek el kell küldeni a flow indítási eseményt
        JSONClass jsonClass = new JSONClass();
        jsonClass["dataContent"] = "startFlow";
        jsonClass["flowIndex"] = flowIndex.ToString();

        Server_Logic.SendJSONToEveryClient(jsonClass);

        // Átváltunk az eredményeket megjelenítő képre
        Common.configurationController.selectedFlow = flowIndex;

        //Common.screenController.ChangeScreen("CanvasScreenServerResult");
    }

    public void ButtonClickFlow1() {
        StartFlow(0);
    }

    public void ButtonClickFlow2()
    {
        StartFlow(1);
    }

    public void ButtonClickFlow3()
    {
        StartFlow(2);
    }

    /// <summary>
    /// Küldünk a klienseknek egy FreePlay eseményt, ami azt jelenti, hogy bejön a fő menü
    /// és mindenki azt a játékot választja amelyiket akarja.
    /// </summary>
    public void ButtonClickFreePlay() {

        ButtonClickDeleteGroups();

        JSONClass jsonClass = new JSONClass();
        jsonClass["dataContent"] = "startFreePlay";

        Server_Logic.SendJSONToEveryClient(jsonClass);
    }

    /// <summary>
    /// Leállítja az elindított flow-t vagy a freePlay üzemmódot.
    /// Kliensek ilyenkor vissza kerülnek a várakozó képernyőre.
    /// </summary>
    public void ButtonClickPlayStop()
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass["dataContent"] = "playStop";

        Server_Logic.SendJSONToEveryClient(jsonClass);
    }

    // Update is called once per frame
    void Update () {
	    
	}
}
