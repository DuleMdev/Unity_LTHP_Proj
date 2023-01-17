using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Networking;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using SimpleJSON;

/*



Itt zajlik az alapvető kommunikáció.

- Itt indítjuk el a szervert

- A szerver figyeli és kezeli a kapcsolódási kéréseket,


- A szerver figyeli a szétkapcsolódásokat.
A lekapcsolódott klienshez tartozó ClientData-ban beállítja a státuszát disconnect-re.

- 


*/

public class HHHNetwork : MonoBehaviour {

    public const int PACK_DATA_LENGTH = 1000;   // A hálózaton csomagonként mennyi adatot küldünk

    const int ARRAY_LENGTH = 1024;              // Egy csomag tényleges maximális mérete



    public string ipAddress;

    int reliableCannelID;   // Akkor van jelentősége ha több csatornán kommunikálunk és tudni szeretnénk, hogy az adat melyik csatornán jött be
    int hostID;             // Akkor van jelentősége ha több portot nyitottunk és tudni szeretnénk, hogy az adat melyik porton jöbb be
    public int clientConnectionID; // Milyen kapcsolati azonosítót kapott a kliens szever kapcsolat a kliens oldalon

    bool hostStart;      // El van indítva a hálózat, akkor figyeljük a hálózati forgalmat

    //[HideInInspector]
    // A hálózaton érkező üzenetek feldolgozása engedélyezve van?
    bool _messageProcessingEnabled;
    public bool messageProcessingEnabled {
        get { return _messageProcessingEnabled; }
        set { Common.configurationController.Log("messageProcessingEnabled " + _messageProcessingEnabled + " => " + value);
            _messageProcessingEnabled = value;
        }
    }  

    public Common.CallBack_In_NetworkEventType_Int_JSONNode callBackNetworkEvent;

    ClientCommunicationManager clientCommunicationManager;

    public bool clientIsConnected { get; private set; } // True ha a kliens kapcsolódva van a szerverhez



    // Use this for initialization
    void Awake()
    {
        Common.HHHnetwork = this;

        ipAddress = LocalIPAddress();
    }

    // Konfiguráljuk a hálózatot és elindítjuk.
    // A Server változóval megadhatjuk, hogy szervert indítunk vagy klienst.
    // Annyi a különbség a kliens és a szerver között, hogy a szervernek egy meghatározott portot kell megnyitnia,
    // míg a kliensnek egy bármilyen portot.
    void StartNetwork(bool Server)
    {
        clientCommunicationManager = new ClientCommunicationManager(MessageArrived);

#if !UNITY_WEBGL        
        // Alapértelmezett értékekkel inicializáljuk a szállító réteget
        GlobalConfig gc = new GlobalConfig();
        gc.MaxPacketSize = 20000;                        // Egy csomag maximális mérete     
        gc.ReactorMaximumReceivedMessages = 1024;       // Maximum hány csomagot tud tárolni a csomagokat fogadó sor
        gc.ReactorMaximumSentMessages = 1024;           // Maximum hány csomagot tud tárolni a csomogokat küldő sor
        gc.ReactorModel = ReactorModel. FixRateReactor;  // Milyen módot használjon a csomag kezelésre
        gc.ThreadAwakeTimeout = 10;

        NetworkTransport.Init(gc);

        // Létrehozzuk a szükséges csatornát
        ConnectionConfig config = new ConnectionConfig();
        reliableCannelID = config.AddChannel(QosType. Reliable);

        // Létrehozzuk a hálózati topológiát
        HostTopology topology = new HostTopology(config, (Server) ? 50 /*Common.configurationController.maxConnectNumber*/ : 5);

        // Létrehozzuk a szükséges portot
        if (Server)
            hostID = NetworkTransport.AddHost(topology, Common.configurationController.portNumber);
        else
            // Kliens esetén nem adunk meg port számot, mivel mindegy. A port szám választását az operációs rendszerre bízzuk
            hostID = NetworkTransport.AddHost(topology);
        // Ha minden jól ment létrejött a port

        hostStart = true;

        clientConnectionID = 0;
#endif        
    }

    // Elindítjuk a szervert
    public void StartServerHost()
    {
        StartNetwork(true);
    }

    // Elindítjuk a klienst
    public void StartClientHost()
    {
        StartNetwork(false);
    }

    /// <summary>
    /// Leállítjuk a hálózatot. 
    /// </summary>
    public void StopHost()
    {
        NetworkTransport.RemoveHost(hostID);
        hostStart = false;
    }

    /// <summary>
    /// Megpróbálunk kapcsolódni a szerverhez 
    /// </summary>
    /// <returns>A vissza adott érték tartalmazza, hogy sikeres volt-e a kapcsolódás, vagy hiba történt.</returns>
    public NetworkError ConnectToServer()
    {
        byte error;

        Debug.Log("Try connect : " + Common.configurationController.serverAddress + ":" + Common.configurationController.portNumber);

        clientConnectionID = NetworkTransport.Connect(hostID,
            Common.configurationController.serverAddress,
            Common.configurationController.portNumber,
            0, out error);

        GetNetworkError(error, "ConnectToServer request: ");

        return (NetworkError)error;
    }

    // Update is called once per frame
    void Update()
    {
        // Ha nincs elindítva a hálózat vagy az üzenetek feldolgozása nem engedélyezett
        if (!hostStart || !messageProcessingEnabled) return;


        bool infiniteLoop = true;

        do
        {
            int recHostId;
            int recConnectionId;
            int recChannelId;
            byte[] recBuffer = new byte[ARRAY_LENGTH];
            //int bufferSize = ARRAY_LENGTH;
            int dataSize;
            byte error;
            NetworkEventType networkEventType = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, recBuffer.Length, out dataSize, out error);

            // A központi feldolgozó, ha olyan adatot kaptunk, amit központilag kell kezelni
            switch (networkEventType)
            {
                case NetworkEventType.Nothing:  // Nem történt semmi a hálózaton
                    infiniteLoop = false; // Leállítjuk a ciklust mivel nincs több esemény
                    continue;
                    break;

                case NetworkEventType.ConnectEvent:     // Egy kliens kapcsolódott
                    Common.canvasNetworkHUD.Connect();

                    if (Common.configurationController.deviceIsServer) {
                        // Ha szerveren vagyunk, akkor minden ConnectEvent-hez létrehozunk egy ClientCommunication objektumot
                        clientCommunicationManager.NewClient(recConnectionId);

                        Common.configurationController.Log("Egy kliens kapcsolódott a szerverhez.");
                    }
                    else {
                        // Ha kliensek vagyunk akkor a kapcsolódási kérelmét elfogadta a szerver
                        if (recConnectionId == clientConnectionID)
                        {
                            clientIsConnected = true;

                            Debug.Log("Client Is Connected");
                            Common.configurationController.Log("A kliens kapcsolódása sikerült.");
                            // Létrehozunk egy objektumot minek segítségével majd kommunikálhat a szerverrel
                            clientCommunicationManager.NewClient(recConnectionId);
                        }
                    }

                    // Kiírjuk a létrejött kapcsolat adatait
                    int port;
                    string address;
                    UnityEngine.Networking.Types.NetworkID netId;
                    UnityEngine.Networking.Types.NodeID nodeId;

                    NetworkTransport.GetConnectionInfo(recHostId, recConnectionId, out address, out port, out netId, out nodeId, out error);

                    Debug.Log("Connect :" + address + ":" + port + " - connection ID : " + recConnectionId);

                    break;

                case NetworkEventType.DataEvent:        // Adatot küldtek
                    Common.canvasNetworkHUD.Data();

                    // Ha adat jött, akkor azt továbbítjuk a clientCommunicationManager-nek
                    clientCommunicationManager.ReceivedData(recConnectionId, new DataPacket(recBuffer));
                    break;

                case NetworkEventType.DisconnectEvent:  // Egy kapcsolat megszünt
                    Debug.Log("Connect Is Disconnect");

                    Common.canvasNetworkHUD.Disconnect();

                    // A kliens kapcsolata megszünt a szerverrel
                    if (recConnectionId == clientConnectionID) {
                        clientIsConnected = false;

                        // Ha nem a start képernyő aktív, akkor átváltunk rá
                        if (Common.screenController.actScreen.name != "ClientStartScreen")

                            Common.menuInformation.Hide(() => {
                                Common.screenController.ChangeScreen("ClientStartScreen");
                            });
                    }

                    // Eltávolítjuk a kapcsolatot a clientCommunicationManger-ből
                    clientCommunicationManager.RemoveClient(recConnectionId);

                    Common.configurationController.Log("Megszakadt a kapcsolat. ConnectionID = " + recConnectionId);

                    break;
            }

            // Ha be van állítva feldolgozó, akkor átadjuk neki is a hálózati eseményt kivéve az adatérkezést mivel azt csak akkor adjuk tovább ha már össze van rakva az adathalmaz
            if (networkEventType != NetworkEventType.DataEvent) {
                if (callBackNetworkEvent != null)
                    callBackNetworkEvent(networkEventType, recConnectionId, null);
            }

        } while (infiniteLoop && messageProcessingEnabled);

        /*
        // Ha el van indítva a hálózat
        if (networkStart) {
            // Ha szerverek vagyunk, akkor a szerver eseményeket figyeljük
            if (Common.configurationController.tabletType == ConfigurationController.TabletType.Client)
                GetServerEvent();
            // Ha kliensek vagyunk, akkor a kliens eseményeket
            if (Common.configurationController.tabletType == ConfigurationController.TabletType.Client)
                GetClientEvent();
        }
        */

        clientCommunicationManager.Update();
    }

    /// <summary>
    /// A megadott azonosítóju kapcsolatot lezárjuk.
    /// </summary>
    /// <param name="connectionID">A lezárandó kapcsolat kapcsolat azonosítója.</param>
    public void DisconnectClient(int connectionID) {
        byte error;
        NetworkTransport.Disconnect(hostID, connectionID, out error);
        GetNetworkError(error);
    }

    /// <summary>
    /// A szerveren lekérdezhetjük ezzel, hogy melyik kapcsolat azonosítójú klienstől hány százalékban vettük
    /// az éppen összarakási fázisban levő adathalmazt.
    /// </summary>
    /// <param name="connectionID">A kliens kapcsolat azonosítója amelyikre kíváncsiak vagyunk.</param>
    /// <returns>A bejövő adathalmaz százalékos feldolgozottsága.</returns>
    public float GetReceivedPercent(int connectionID) {
        return clientCommunicationManager.GetReceiverPercent(connectionID);
    }

    /// <summary>
    /// A kliensen lekérdezhetjük ezzel, hogy a szervertől hány százalékban vettük a következő adathalmazt.
    /// </summary>
    /// <returns>A bejövő adathalmaz százalékos feldogozottsága.</returns>
    public float GetClientReceivedPercent() {
        return clientCommunicationManager.GetReceiverPercent(clientConnectionID);
    }

    /// <summary>
    /// Valamelyik ClientCommunication objektum összerakott egy üzenetet.
    /// </summary>
    /// <param name="networkEventType"></param>
    /// <param name="connectionID"></param>
    /// <param name="jsonNodeMessage"></param>
    public void MessageArrived(NetworkEventType networkEventType, int connectionId, JSONNode jsonNodeMessage) {

        // Logoljuk a fogadott üzenetet
        if (Common.configurationController.deviceIsServer)
        {
            // ConnectionID alapján megkeressük a kliensID-t
            ClientData clientData = Common.gameMaster.GetClientDataByConnectionID(connectionId);

            if (clientData != null)
            {
                Common.configurationController.Log("<= Recive clientID = " + clientData.tabletID, json: jsonNodeMessage);
            }
            else {
                Common.configurationController.Log("<= Recive : A connectionID nem tartozik klienshez, valószínű éppen most kér egy azonosítót", json: jsonNodeMessage);
            }
        }
        else {
            // Kliensen vagyunk
            Common.configurationController.Log("<= Recive : clientID = " + Common.configurationController.clientID, json: jsonNodeMessage);
        }

        Debug.Log(Common.GetStringPart(Common.Now() + " : Message Arrived : " + jsonNodeMessage.ToString(" "), 1000));

        if (!Common.configurationController.deviceIsServer) {
            // Ha a kliensen vagyunk
            switch (networkEventType)
            {
                // Adathalmaz érkezett
                case NetworkEventType.DataEvent:
                    switch (jsonNodeMessage[C.JSONKeys.dataContent])
                    {
                        case C.JSONValues.clientID:
                            // Kliens azonosítót küldtek
                            Common.configurationController.clientID = jsonNodeMessage[C.JSONKeys.clientID].AsInt;

                            // Ha nincs kliens név, akkor a kliens azonosítót írjuk ki
                            Common.canvasNetworkHUD.SetText(
                                (jsonNodeMessage[C.JSONKeys.clientName].Value == "") ?
                                    jsonNodeMessage[C.JSONKeys.clientID].Value + "." :
                                    jsonNodeMessage[C.JSONKeys.clientName].Value
                                );

                            Common.configurationController.studentName = jsonNodeMessage[C.JSONKeys.clientName].Value;
                            Common.configurationController.Save();

                            Common.configurationController.Log("Set Client - ID = " + Common.configurationController.clientID + " : name = " + Common.configurationController.studentName);

                            break;

                        case C.JSONValues.groupID:

                            // Beállítjuk a kliens csoport számát
                            Common.configurationController.userGroup = jsonNodeMessage[C.JSONKeys.clientGroupID].AsInt;

                            // Megmutatjuk a csoport szín mutató képernyőt ha szükséges
                            if (jsonNodeMessage[C.JSONKeys.clientGroupScreenShow].AsBool)
                            {
                                Common.menuClientGrouping.Refresh();
                                Common.screenController.ChangeScreen(C.Screens.MenuClientGrouping);
                            }

                            break;

                        case C.JSONValues.playStart:
                            Common.taskController.PlayTask(jsonNodeMessage);
                            break;

                        case C.JSONValues.EvaluationScreen:

                            if (Common.screenController.actScreen.name != C.JSONValues.EvaluationScreenSingle) {
                                // Elindítjuk a single értékelő képernyőt
                                Common.evaluationScreenSingle.jsonData = jsonNodeMessage;
                                Common.screenController.ChangeScreen(C.JSONValues.EvaluationScreenSingle);
                            }
                            else
                            {
                                // Frissítjük az értékelő képernyőt
                                Common.evaluationScreenSingle.UpdateData(jsonNodeMessage);
                            }

                            break;

                        case C.JSONValues.pauseOn:

                            // Feldobjuk a pause ablakot
                            Common.infoPanelInformation.Show(C.Texts.LessonPlanIsPaused, false, null);

                            break;

                        case C.JSONValues.pauseOff:

                            // Eltüntetjük a pause ablakot, feltehetőleg az látszik
                            Common.menuInformation.Hide();

                            break;
                    }

                    break;
                case NetworkEventType.ConnectEvent:
                    Common.configurationController.Log("Kliens kapcsolódott a szerverhez : kliensID = " + Common.configurationController.DeviceUID);
                    break;
                case NetworkEventType.DisconnectEvent:
                    Common.configurationController.Log("Kliens és a szerver kapcsolat megszünt");
                    break;
                case NetworkEventType.Nothing:
                    break;
                case NetworkEventType.BroadcastEvent:
                    break;
            }
        }

        // Ha be van állítva feldolgozó, akkor átadjuk neki a bejövő adatokat
        if (callBackNetworkEvent != null)
            callBackNetworkEvent(networkEventType, connectionId, jsonNodeMessage);
    }

    /*
    // Elküldi a szervernek a kliens azonosítására szánt adatokat
    public void SendClientIdentity()
    {
        // Elkészítjük az adatokat egy JSON fájlban

        JSONClass node = new JSONClass();
        node["dataContent"] = "connect";
        node["tabletID"] = Common.configurationController.tabletID;
        node["clientID"] = Common.configurationController.clientID.ToString();
        node["userName"] = Common.configurationController.userName;

        // Elküldjük a szervernek a bejelentkező adatokat
        SendJSONClientToServer(node); //.ToString());
    }*/
    
    /// <summary>
    /// A szervertől küldünk egy json adat csomagot a megadott kapcsolat azonosítójú kliensnek.
    /// </summary>
    /// <param name="connectionID">Melyik kapcsolat azonosítójú kliensnek szánjuk az információt.</param>
    /// <param name="message">A küldendő információ json formában.</param>
    public void SendMessage(int connectionID, JSONNode message) {
        //Debug.Log(message.ToString(" ").Substring(0, 1000));
        Debug.Log(Common.GetStringPart(Common.Now() + " : Send Message : " + message.ToString(" "), 1000));

        // Logoljuk a küldendő üzenetet
        if (Common.configurationController.deviceIsServer)
        {
            // ConnectionID alapján megkeressük a kliensID-t
            ClientData clientData = Common.gameMaster.GetClientDataByConnectionID(connectionID);

            if (clientData != null)
            {
                Common.configurationController.Log("=> Send : clientID = " + clientData.tabletID, json: message);
            }
            else {
                Common.configurationController.Log("=> Send : A connectionID nem tartozik klienshez ERROR WTF?");
            }
        }
        else {
            // Kliensen vagyunk
            Common.configurationController.Log("=> Send : clientID = " + Common.configurationController.clientID, json: message);
        }

        clientCommunicationManager.SendMessage(connectionID, message);
    }

    /// <summary>
    /// A klienstől küldünk egy json adat csomagot a szervernek.
    /// </summary>
    /// <param name="message">A küldendő információ json formában.</param>
    public void SendMessageClientToServer(JSONNode message) {
        SendMessage(clientConnectionID, message);
    }

    public byte SendData(int clientConnectionID, byte[] message) {
        if (!hostStart) return (byte)NetworkError.NoResources;

        //Debug.Log(System.DateTime.Now.ToString("HH:mm:ss.fff") + " Adatcsomag küldés.");

        byte error;
        NetworkTransport.Send(hostID, clientConnectionID, reliableCannelID, message, message.Length, out error);

        Debug.Log("SendData - hostID : " + hostID + " | connectionID : " + clientConnectionID + " | message.Length : " + message.Length);

        GetNetworkError(error);

        return error;
    }

    /*
    /// <summary>
    /// Elküldi a megadott string tartalmát a szervernek a megadott socket, kapcsolat és csatornán keresztűl.
    /// A kliens csak a szerverrel tartja a kapcsolatot, ezért a socket, connection és a csatorna egyértelmű, mivel ezekből amúgy is csak egy van.
    /// </summary>
    /// <param name="message">Az elküldendő üzenet szövege.</param>
    public byte SendDataClientToServer(byte[] message)
    {
        if (!hostStart) return (byte)NetworkError.NoResources;

        byte error;
        NetworkTransport.Send(hostID, clientConnectionID, reliableCannelID, message, message.Length, out error);

        GetNetworkError(error);

        return error;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="clientConnectionID"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public NetworkError SendDataServerToClient(int clientConnectionID, byte[] message) {
        if (!hostStart) return NetworkError.NoResources;

        byte error;
        NetworkTransport.Send(hostID, clientConnectionID, reliableCannelID, message, message.Length, out error);

        return (NetworkError)error;
    }
    */

    /*
    // Egy json node tartalmát küldjük el a szervernek
    public byte SendJSONClientToServer(JSONNode jsonNode)
    {
        string message = jsonNode.ToString();

        return SendDataClientToServer(message);
    }
    */

        /*
    public byte SendDataServerToClient(int connectionID, string message)
    {
        // Send the server a message
        byte error;
        byte[] buffer = new byte[ARRAY_LENGTH];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter f = new BinaryFormatter();
        f.Serialize(stream, message);

        NetworkTransport.Send(hostID, connectionID, reliableCannelID, buffer, (int)stream.Position, out error);

        GetNetworkError(error);

        return error;
    }*/


        /*
    // Egy json node tartalmát küldjük el a kliensnek
    public byte SendJSONServerToClient(int connectionID, JSONNode jsonNode)
    {
        string message = jsonNode.ToString();

        return SendDataServerToClient(connectionID, message);
    }*/


    /// <summary>
    /// Átkonvertálja a megadott bájt tömben található adatokat string formára és azt visszaadja.
    /// </summary>
    /// <param name="data">A konvertálandó byte tömb.</param>
    string GetStringFromSerializedByteArray(byte[] data)
    {
        Stream stream = new MemoryStream(data);
        BinaryFormatter f = new BinaryFormatter();

        return f.Deserialize(stream).ToString();
    }




    /// <summary>
    /// Vissza adja a hálózati hibát szövegesen.
    /// </summary>
    /// <param name="error">A hiba kódja.</param>
    /// <returns>A hiba szövegesen.</returns>
    string GetNetworkError(byte error, string message = "")
    {
        return GetNetworkError((NetworkError)error, message);
    }

    /// <summary>
    /// Vissza adja a hálózati hibát szövegesen.
    /// </summary>
    /// <param name="error">A hiba kódja.</param>
    /// <returns>A hiba szövegesen.</returns>
    string GetNetworkError(NetworkError networkError, string message = "")
    {
        //if (nerror != NetworkError.Ok)
        Debug.Log(message + networkError.ToString());

        return networkError.ToString();
    }




    /*
    void OnGUI()
    {
        GUI.Label(new Rect(10, 220, 100, 50), "Szöveg");
        GUI.Button(new Rect(10, 100, 100, 100), ipAddress);
    }
    */

    public string LocalIPAddress()
    {
#if UNITY_WEBGL
        return ""; // Bekapcsoljuk a WebGL képernyőt
#else
        IPHostEntry host;
        string localIP = "";

        try
        {
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                Debug.Log("IP address : " + ip.AddressFamily + " - [" + ip.ToString() + "]");

                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (string.IsNullOrEmpty(localIP)) localIP = ip.ToString();
                    break;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Valószínűleg nincs NET! \n" + e.Message);
        }

        return localIP;
        //return NetworkManager.singleton.networkAddress; // NullReferenceException (singleton)
        //return Network.player.ipAddress; // Obsolote
#endif




    }
}
