using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Networking;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


using SimpleJSON;

public class NetworkTest : MonoBehaviour {

    public delegate int CallBackReceiveData(string receivedData);

    const int ARRAY_LENGTH = 1024;

    public string ipAddress;

    int reliableCannelID;   // Akkor van jelentősége ha több csatornán kommunikálunk és tudni szeretnénk, hogy az adat melyik csatornán jött be
    int hostID;             // Akkor van jelentősége ha több portot nyitottunk és tudni szeretnénk, hogy az adat melyik porton jöbb be
    int clientConnectionID; // Milyen kapcsolati azonosítót kapott a kliens szever kapcsolat a kliens oldalon

    bool networkStart;      // El van indítva a hálózat, akkor figyeljük a hálózati forgalmat
    [HideInInspector]
    public bool messageProcessingEnabled;  // A hálózaton érkező üzenetek feldolgozása engedélyezve van?

    public Common.CallBack_In_NetworkEventType_Int_JSONNode callBackNetworkEvent;

    public bool clientIsConnected { get; private set; } // True ha a kliens kapcsolódva van a szerverhez



    // Use this for initialization
    void Awake() {
        //Common.HHHnetwork = this;

        ipAddress = LocalIPAddress();

        //SendClientIdentity();
    }


    void Start () {
        //ipAddress = LocalIPAddress();

    }

    // Configuráljuk a hálózatot és elindítjuk
    // A Server változóval megadhatjuk, hogy szervert indítunk vagy klienst
    // Annyi a különbség a kliens és a szerver között, hogy a szervernek egy meghatározott portot kell megnyitnia
    void StartNetwork(bool Server) {
#if !UNITY_WEBGL        
        // Alapértelmezett értékekkel inicializáljuk a szállító réteget
        NetworkTransport.Init();

        // Létrehozzuk a szükséges csatornát
        ConnectionConfig config = new ConnectionConfig();
        reliableCannelID = config.AddChannel(QosType.Reliable);

        // Létrehozzuk a hálózati topológiát
        HostTopology topology = new HostTopology(config, (Server)? Common.configurationController.maxConnectNumber : 5);

        // Létrehozzuk a szükséges portot
        if (Server)
            hostID = NetworkTransport.AddHost(topology, Common.configurationController.portNumber);
        else
            // Kliens esetén nem adunk meg port számot, mivel mindegy. A port szám választását az operációs rendszerre bízzuk
            hostID = NetworkTransport.AddHost(topology);
        // Ha minden jól ment létrejött a port

        networkStart = true;
#endif        
    }

    // Elindítjuk a szervert
    public void StartServerHost() {
        StartNetwork(true);
    }

    // Elindítjuk a klienst
    public void StartClientHost() {
        StartNetwork(false);
    }

    // Leállítjuk a hálózatot
    public void StopNetwork() {

    }

    // Megpróbálunk kapcsolódni a szerverhez
    public void ConnectToServer() {
        byte error;

        clientConnectionID = NetworkTransport.Connect(hostID,
            Common.configurationController.serverAddress,
            Common.configurationController.portNumber,
            0, out error);
    }

    // Update is called once per frame
    void Update () {
        // Ha nincs elindítva a hálózat vagy az üzenetek feldolgozása nem engedélyezett
        if (!networkStart || !messageProcessingEnabled) return;


        bool infiniteLoop = true;

        do
        {
            int recHostId;
            int connectionId;
            int channelId;
            byte[] recBuffer = new byte[ARRAY_LENGTH];
            //int bufferSize = ARRAY_LENGTH;
            int dataSize;
            byte error;
            NetworkEventType networkEventType = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, recBuffer.Length, out dataSize, out error);

            JSONNode jsonNode = null;

            // Ha adatokat kaptunk, akkor átkonvertáljuk stringé
            if (networkEventType == NetworkEventType.DataEvent)
            {
                Debug.Log("Network - received data");
                jsonNode = JSONNode.Parse(GetStringFromSerializedByteArray(recBuffer));
            }

            //string receivedData = (networkEventType == NetworkEventType.DataEvent) ? GetStringFromSerializedByteArray(recBuffer) : "";




            // A központi feldolgozó, ha olyan adatot kaptunk, amit központilag kell kezelni
            switch (networkEventType)
            {
                case NetworkEventType.Nothing:  // Nem történt semmi a hálózaton
                    infiniteLoop = false; // Leállítjuk a ciklust mivel nincs több esemény
                    continue;
                    break;

                case NetworkEventType.ConnectEvent:     // Egy kliens kapcsolódott
                    Common.canvasNetworkHUD.Connect();

                    // A kliens kapcsolódási kérelmét elfogadta a szerver
                    if (connectionId == clientConnectionID) 
                        clientIsConnected = true;

                    // Kiírjuk a létrejött kapcsolat adatait
                    int port;
                    string address;
                    UnityEngine.Networking.Types.NetworkID netId;
                    UnityEngine.Networking.Types.NodeID nodeId;
                    NetworkTransport.GetConnectionInfo(recHostId, connectionId, out address, out port, out netId, out nodeId, out error);

                    Debug.Log("Connect :" + address + ":" + port + " - connection ID : " + connectionId);

                    break;

                case NetworkEventType.DataEvent:        // Adatot küldtek
                    Common.canvasNetworkHUD.Data();
                    break;

                case NetworkEventType.DisconnectEvent:  // Egy kapcsolat megszünt
                    Common.canvasNetworkHUD.Disconnect();

                    // A kliens kapcsolata megszünt a szerverrel
                    if (connectionId == clientConnectionID)
                        clientIsConnected = false;

                    break;
            }

            // Ha be van állítva feldolgozó, akkor átadjuk neki a bejövő adatokat
            if (callBackNetworkEvent != null)
                callBackNetworkEvent(networkEventType, connectionId, jsonNode);




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
    }

    void GetServerEvent() {

        bool infiniteLoop = true;

        do
        {
            int recHostId;
            int connectionId;
            int channelId;
            byte[] recBuffer = new byte[ARRAY_LENGTH];
            //int bufferSize = ARRAY_LENGTH;
            int dataSize;
            byte error;
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, recBuffer.Length, out dataSize, out error);
            switch (recData)
            {
                case NetworkEventType.Nothing:  // Nem történt semmi a hálózaton
                    infiniteLoop = false; // Leállítjuk a ciklust mivel nincs több adat
                    break;
                case NetworkEventType.ConnectEvent:     // Egy kliens kapcsolódott

                    break;
                case NetworkEventType.DataEvent:        // Adatot küldtek

                    break;
                case NetworkEventType.DisconnectEvent:  // Egy kapcsolat megszünt

                    break;
            }
        } while (infiniteLoop);
    }

    void GetClientEvent() {
        bool infiniteLoop = true;

        do
        {
            int recHostId;
            int connectionId;
            int channelId;
            byte[] recBuffer = new byte[ARRAY_LENGTH];
            //int bufferSize = ARRAY_LENGTH;
            int dataSize;
            byte error;
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, recBuffer.Length, out dataSize, out error);
            switch (recData)
            {
                case NetworkEventType.Nothing:         //1
                    infiniteLoop = false; // Leállítjuk a ciklust mivel nincs több adat
                    break;
                case NetworkEventType.ConnectEvent:    //2
                    // Sikerült a kapcsolódás elküldjük a kliens azonosítására szánt adatokat a szervernek
                    SendClientIdentity();
                    break;
                case NetworkEventType.DataEvent:       //3
                    // Adatot küldtek
                    SimpleJSON.JSONArray array = new SimpleJSON.JSONArray();
                    array[0] = "15";

                    break;
                case NetworkEventType.DisconnectEvent: //4
                    // Egy kapcsolat megszünt

                    break;
            }
        } while (infiniteLoop);
    }

    // Elküldi a szervernek a kliens azonosítására szánt adatokat
    public void SendClientIdentity() {
        // Elkészítjük az adatokat egy JSON fájlban

        JSONClass node = new JSONClass();
        node["dataContent"] = "connect";
        node["tabletID"] = Common.configurationController.tabletID;
        node["clientID"] = Common.configurationController.clientID.ToString();
        node["userName"] = Common.configurationController.userName;

        // Elküldjük a szervernek a bejelentkező adatokat
        SendJSONClientToServer(node); //.ToString());
    }

    /// <summary>
    /// Elküldi a megadott string tartalmát a szervernek a megadott socket, kapcsolat és csatornán keresztűl.
    /// A kliens csak a szerverrel tartja a kapcsolatot, ezért a socket, connection és a csatorna egyértelmű, mivel ezekből amúgy is csak egy van.
    /// </summary>
    /// <param name="message">Az elküldendő üzenet szövege.</param>
    public byte SendDataClientToServer(string message)
    {
        if (!networkStart) return (byte)NetworkError.NoResources;

        // Send the server a message
        byte error;
        byte[] buffer = new byte[ARRAY_LENGTH];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter f = new BinaryFormatter();
        f.Serialize(stream, message);

        NetworkTransport.Send(hostID, clientConnectionID, reliableCannelID, buffer, (int)stream.Position, out error);

        GetNetworkError(error);

        return error;
    }

    
    // Egy json node tartalmát küldjük el a szervernek
    public byte SendJSONClientToServer(JSONNode jsonNode) {
        string message = jsonNode.ToString();

        return SendDataClientToServer(message);
    }
    

    public byte SendDataServerToClient(int connectionID, string message) {
        // Send the server a message
        byte error;
        byte[] buffer = new byte[ARRAY_LENGTH];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter f = new BinaryFormatter();
        f.Serialize(stream, message);

        NetworkTransport.Send(hostID, connectionID, reliableCannelID, buffer, (int)stream.Position, out error);

        GetNetworkError(error);

        return error;
    }

    
    // Egy json node tartalmát küldjük el a kliensnek
    public byte SendJSONServerToClient(int connectionID, JSONNode jsonNode)
    {
        string message = jsonNode.ToString();

        return SendDataServerToClient(connectionID, message);
    }
    

    /// <summary>
    /// Átkonvertálja a megadott bájt tömben található adatokat string formára és azt visszaadja.
    /// </summary>
    /// <param name="data">A konvertálandó byte tömb.</param>
    string GetStringFromSerializedByteArray(byte[] data) {
        Stream stream = new MemoryStream(data);
        BinaryFormatter f = new BinaryFormatter();

        return f.Deserialize(stream).ToString();
    }

    /// <summary>
    /// Vissza adja a hálózati hibát szövegesen.
    /// </summary>
    /// <param name="error">A hiba kódja.</param>
    /// <returns>A hiba szövegesen.</returns>
    string GetNetworkError(byte error)
    {
        NetworkError nerror = (NetworkError)error;

        if (nerror != NetworkError.Ok)
            Debug.Log("Network error: " + nerror.ToString());

        return nerror.ToString();
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
        IPHostEntry host;
        string localIP = "";
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

        return localIP;
    }
}
