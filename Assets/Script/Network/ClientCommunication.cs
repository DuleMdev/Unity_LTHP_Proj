using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;

using System.Runtime.Serialization.Formatters.Binary;
using SimpleJSON;

/*

Minden kliensen létrejön egy ilyen objektum, ami majd kommunikál a szerverrel.

A szerveren pedig minden klienshez létrejön egy ilyen objektum.










*/

public class ClientCommunication
{
    public int connectionID { get; private set; }

    ClientSender clientSender;
    ClientReceiver clientReceiver;

    Common.CallBack_In_NetworkEventType_Int_JSONNode callBack;

    // Vissza adja, hogy a fogadó, hol tart az adat összeépítésben
    public float receiverPercent { get { return clientReceiver.percent; } }

    public ClientCommunication(int connectionID, Common.CallBack_In_NetworkEventType_Int_JSONNode callBack) {
        this.connectionID = connectionID;
        this.callBack = callBack;

        clientSender = new ClientSender(connectionID);
        clientReceiver = new ClientReceiver(DataSetArrived);
    }

    // Update is called once per frame
    public void Update() {
        clientSender.Update();
        clientReceiver.Update();
    }

    /// <summary>
    /// Egy json üzenetet küldünk át a hálózaton.
    /// </summary>
    /// <param name="message">A json üzenet.</param>
    public void SendMessage(JSONNode jsonNode) {
        MemoryStream ms = new MemoryStream();
        BinaryFormatter f = new BinaryFormatter();
        f.Serialize(ms, jsonNode.ToString());

        clientSender.SendData(ms.ToArray());
    }

    // Ez egyenlőre nem fontos, mivel úgy sem tudok túl sok adatot átküldeni, mivel lassú az adatátvitel,
    // kevés adatnál viszont nem probléma, hogy kliensenként létre jön egy verzió az óratervből.
    /*
    /// <summary>
    /// Egy byte tömböt küldünk át a hálózaton.
    /// </summary>
    /// <remarks>
    /// Ezt csak a óratervek átküldésekor használjuk.
    /// Tehát az óraterv json-ját átkonvertáljuk előre bájt tömbbe, hogy ne kelljen a küldőnek ezzel foglalkozni
    /// hiszen több kliensnek is el kell küldeni ugyan azt az üzenetet és nem hatákony, ha a szerver minden
    /// kliensnek átkonvertálja a json bájt tömbbe.
    /// </remarks>
    /// <param name="message">A json üzenet.</param>
    public void SendMessage(JSONNode jsonNode)
    {
        MemoryStream ms = new MemoryStream();
        BinaryFormatter f = new BinaryFormatter();
        f.Serialize(ms, jsonNode.ToString());

        clientSender.SendData(ms.ToArray());
    }
    */

    /// <summary>
    /// Adatcsomag érkezett a hálózaton.
    /// </summary>
    /// <param name="dataPacket">Az érkezett adatcsomag.</param>
    public void ReceivedData(DataPacket dataPacket) {
        clientReceiver.ReceiveData(dataPacket);
    }

    /// <summary>
    /// Egy teljes adathalmaz érkezett a hálózaton.
    /// </summary>
    /// <param name="dataSet">Az érkezett adathalmaz.</param>
    void DataSetArrived(byte[] dataSet) {
        MemoryStream ms = new MemoryStream(dataSet);
        BinaryFormatter f = new BinaryFormatter();

        JSONNode node = null;

        try {
            node = JSON.Parse(f.Deserialize(ms).ToString());
        } catch (System.Exception ex) {
            Debug.Log("DataSet arrived : " + ex.Message);
            return;
        }

        callBack(NetworkEventType.DataEvent, connectionID, node);
    }
}
