using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using SimpleJSON;

public class ClientCommunicationManager {

    List<ClientCommunication> listOfClientCommunication = new List<ClientCommunication>();

    Common.CallBack_In_NetworkEventType_Int_JSONNode callBack;

    public ClientCommunicationManager(Common.CallBack_In_NetworkEventType_Int_JSONNode callBack) {
        this.callBack = callBack;
    }

    public void NewClient(int connectionID) {
        RemoveClient(connectionID);

        listOfClientCommunication.Add(new ClientCommunication(connectionID, MessageArrived));
    }

    public void RemoveClient(int connectionID) {
        // Megkeressük az eltávolítandó klienst és kitöröljük
        foreach (ClientCommunication clientCommunication in listOfClientCommunication)
        {
            if (clientCommunication.connectionID == connectionID)
            {
                listOfClientCommunication.Remove(clientCommunication);
                break;
            }
        }
    }

    public float GetReceiverPercent(int connectionID) {
        // Megkeressük a klienst
        foreach (ClientCommunication clientCommunication in listOfClientCommunication)
        {
            if (clientCommunication.connectionID == connectionID)
            {
                return clientCommunication.receiverPercent;
            }
        }

        return 0;
    }

    // Update is called once per frame
    public void Update()
    {
        foreach (ClientCommunication clientCommunication in listOfClientCommunication)
        {
            clientCommunication.Update();
        }
    }

    public void ReceivedData(int connectionID, DataPacket dataPacket) {
        // Megkeressük a connectionID-val foglalkozó objektumot és átadjuk neki az érkezett adatcsomagot
        foreach (ClientCommunication clientCommunication in listOfClientCommunication)
        {
            if (clientCommunication.connectionID == connectionID)
            {
                clientCommunication.ReceivedData(dataPacket);
                break;
            }
        }
    }

    public void SendMessage(int connectionID, JSONNode message) {
        // Megkeressük a connectionID-val foglalkozó objektumot és átadjuk neki az elküldendő üzenetet
        foreach (ClientCommunication clientCommunication in listOfClientCommunication)
        {
            if (clientCommunication.connectionID == connectionID)
            {
                clientCommunication.SendMessage(message);
                break;
            }
        }
    }

    /// <summary>
    /// Valamelyik ClientCommunication objektum összerakott egy üzenetet, akkor meghívja ezt a metódust.
    /// </summary>
    /// <param name="networkEventType"></param>
    /// <param name="connectionID"></param>
    /// <param name="jsonNodeMessage"></param>
    void MessageArrived(NetworkEventType networkEventType, int connectionId, JSONNode jsonNodeMessage)
    {
        // Ha be van állítva feldolgozó, akkor átadjuk neki a bejövő adatokat
        if (callBack != null)
            callBack(networkEventType, connectionId, jsonNodeMessage);
    }
}
