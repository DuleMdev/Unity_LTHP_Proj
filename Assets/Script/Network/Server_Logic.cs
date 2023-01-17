using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using SimpleJSON;

static public class Server_Logic {

    /*
    Szerver tablet hálózati logikáját tartalmazza
    */

    static public List<ClientData> connectedClient = new List<ClientData>();

    static public List<ClientGroup> groups;

    static public Common.CallBack_In_NetworkEventType_Int_JSONNode callBackNetworkEvent;

    static Server_Logic() {
        Common.HHHnetwork.callBackNetworkEvent = ReceivedNetworkEvent;
    }


    /// <summary>
    /// Egy új klienst ad a csatlakozott kliensek listájához
    /// </summary>
    /// <param name="connectionID">A kliens hálózati kapcsolat azonosítója.</param>
    /// <param name="tabletID">A tablet azonosítója.</param>
    /// <param name="clientID">A kliens azonosítója.</param>
    /// <param name="userName">A felhasználó neve.</param>
    /// <returns></returns>
    static public ClientData NewClient(int connectionID, string tabletID, string clientID, string userName) {

        ClientData client = SearchClientWithClientID(clientID);

        /*
        if (client == null) {
            client = new Server_ClientData(connectionID, tabletID, clientID, userName);

            connectedClient.Add(client);
        }*/

        return client;
    }

    /// <summary>
    /// Megpróbálja megkereseni a megadott kliens azonosítójú klienst.
    /// Ha nem találta null-t ad vissza.
    /// </summary>
    /// <param name="clientID">A keresendő kliens azonosító</param>
    /// <returns>Vissza adja a megtalált klienst. Ha nem találta meg akkor null-t ad vissza.</returns>
    static ClientData SearchClientWithClientID(string clientID) {

        /*
        foreach (Server_ClientData client in connectedClient)
            if (client.clientID == clientID)
                return client;
                */
        return null;
    }

    /// <summary>
    /// Megpróbálja megkereseni a megadott kapcsolat azonosítójú klienst.
    /// Ha nem találta null-t ad vissza.
    /// </summary>
    /// <param name="connectionID">A keresendő kapcsolat azonosító</param>
    /// <returns>Vissza adja a megtalált klinest. Ha nem találta meg akkor null-t ad vissza.</returns>
    static ClientData SearchClientWithConnectionID(int connectionID)
    {
        foreach (ClientData client in connectedClient)
            if (client.connectionID == connectionID)
                return client;

        return null;
    }

    /*
    /// <summary>
    /// Kliensek csoportokba sorsolása.
    /// </summary>
    /// <param name="groupNumber">A létrehozandó csoportok száma</param>
    static public void GroupingClient(int groupNumber) {
        // Készítünk egy másolatot a kliensekről
        List<ClientData> groupingClient = new List<ClientData>(connectedClient);
        Common.Shuffle(groupingClient); // Véletlenszerű sorrendbe rendezzük a klienseket

        // Maximum annyi csoport lehet, mint amennyi kliens van
        if (groupNumber > connectedClient.Count)
            groupNumber = connectedClient.Count;

        // Létrehozzuk a megadott mennyiségű csoportot
        groups = new List<ClientGroup>();
        for (int i = 0; i < groupNumber; i++)
            groups.Add(new ClientGroup());

        // A klienseket a csoportokba soroljuk
        int groupIndex = 0;
        foreach (ClientData client in groupingClient)
        {
            groups[groupIndex].AddClient(client);

            groupIndex++;
            if (groupIndex >= groupNumber)
                groupIndex = 0;
        }

        // Beállítjuk a klienseknek a csoport adatait
        for (int i = 0; i < groupNumber; i++)
        {
            for (int j = 0; j < groups[i].listOfClientData.Count; j++)
            {
                // Beállítjuk a kliens adatokat a csoportba sorolás szerint a szerveren
                groups[i].listOfClientData[j].SetGroup(i, groups[i].listOfClientData.Count, j);

                /*
                // A csoportba sorolási adatokat elküldjük a hálózaton a klienseknek is
                JSONClass node = new JSONClass();
                node["dataContent"] = "setGroup";
                node["groupID"] = i.ToString();
                node["indexInGroup"] = j.ToString();
                node["groupHeadCount"] = groups[i].listOfClientData.Count.ToString();

                Common.networkTest.SendJSONServerToClient(groups[i].listOfClientData[j].connectionID, node);
                */

                /*
            }
        }

        // Elküldjük a csoport adatokat a klienseknek
        SendGroupDataToClients();
    }
    */

    /// <summary>
    /// Elküldi a csoport adatait minden kliensnek
    /// </summary>
    static public void SendGroupDataToClients() {
        foreach (ClientData client in connectedClient) {
            // A csoportba sorolási adatokat elküldjük a hálózaton a klienseknek is
            JSONClass node = new JSONClass();
            node["dataContent"] = "setGroup";
            node["groupID"] = client.groupID.ToString();
            node["indexInGroup"] = client.indexInGroup.ToString();
            node["groupHeadCount"] = client.groupHeadCount.ToString();

            //client.SendJSONToClient(node);
            //Common.networkTest.SendJSONServerToClient(groups[i].listOfClientData[j].connectionID, node);
        }
    }

    /*
    /// <summary>
    /// A kliensek csoport adatait törli
    /// </summary>
    static public void DeleteGroupDataAllClients() {
        // Az összes kliens GroupID-ját -1 re állítja
        foreach (ClientData client in connectedClient)
            client.SetGroup(-1, 0, 0);

        // 
        SendGroupDataToClients();
    }
    */

    /// <summary>
    /// Eltávolítja a megadott kapcsolat azonosítójú klienst a kliensek közül és a csoportjából is.
    /// </summary>
    /// <param name="clientConnectionID">Az eltávolítandó kliens kapcsolat azonosítója.</param>
    static public void RemoveClient(int clientConnectionID) {
        ClientData clientData = SearchClientWithConnectionID(clientConnectionID);
        if (clientData != null) {

            // Eltávolítjuk a klienst a kliensek listájából.
            connectedClient.Remove(clientData);

            // Eltávolítjuk a klienst a csoportjából.
            if (clientData.groupID >= 0)
                groups[clientData.groupID].RemoveClient(clientConnectionID);
        }
    }

    /*
    /// <summary>
    /// Üzenetet küldünk minden kliensnek szöveges formában.
    /// </summary>
    /// <param name="stringData">A küldendő szöveges üzenet</param>
    static public void SendDataToEveryClient(string stringData)
    {
        foreach (Server_ClientData client in connectedClient)
            client.SendDataToClient(stringData);
    }*/

    /// <summary>
    /// Üzenetet küldünk minden kliensnek json formában
    /// </summary>
    /// <param name="jsonData">A küldendő JSON adat.</param>
    static public void SendJSONToEveryClient(JSONNode jsonData)
    {
        /*
        foreach (Server_ClientData client in connectedClient)
            client.SendJSONToClient(jsonData);
            */
    }

    /*
    /// <summary>
    /// Üzenetet küldünk egy csoportnak.
    /// Az üzenetet küldheti egy kliens amit elküldünk a kliens csoportjában minden kliensnek kivéve önmagát
    /// és küldheti a szerver is.
    /// Ha egy kliens küldi, akkor a clientID ki van töltve a groupID nincs.
    /// Ha egy szerver küldi, akkor a clientID nincs kitöltve a groupID pedig ki van.
    /// </summary>
    /// <param name="stringData">A küldendő üzenet string formában.</param>
    /// <param name="clientID">Annak a kliensnek a kapcsolat azonosítója ami az üzenetet küldte a csoportjának, ha -1 akkor nem egy kliens akar küldeni üzenetet hanem a szerver.</param>
    /// <param name="groupID">Melyik csoportnak küldjük az üzenetet. Ha nem adjuk meg vagy -1 -et adunk, akkor a clientID által meghatározott csoportnak küldjük.</param>
    static public void SendDataAGroup(string stringData, string clientID = "", int groupID = -1)
    {
        if (groupID == -1 && clientID == "") return; // Hiba - Nem tudjuk meghatározni, hogy melyik csoportnak kell küldeni az üzenetet

        // Ha nincs megadva a cél csoport, akkor a kliens csoportja lesz
        if (groupID == -1)
            groupID = SearchClientWithClientID(clientID).groupID;

        groups[groupID].SendDataToAllGroupClient(stringData, clientID);
    }*/

    /// <summary>
    /// Üzenetet küldünk egy csoportnak.
    /// Az üzenetet küldheti egy kliens amit elküldünk a kliens csoportjában minden kliensnek kivéve önmagát
    /// és küldheti a szerver is.
    /// Ha egy kliens küldi, akkor a clientID ki van töltve a groupID nincs.
    /// Ha egy szerver küldi, akkor a clientID nincs kitöltve a groupID pedig ki van.
    /// </summary>
    /// <param name="jsonData">A küldendő üzenet json formában.</param>
    /// <param name="clientID">Annak a kliensnek a kapcsolat azonosítója ami az üzenetet küldte a csoportjának, ha -1 akkor nem egy kliens akar küldeni üzenetet hanem a szerver.</param>
    /// <param name="groupID">Melyik csoportnak küldjük az üzenetet. Ha nem adjuk meg vagy -1 -et adunk, akkor a clientID által meghatározott csoportnak küldjük.</param>
    static public void SendJSONAGroup(JSONNode jsonData, int connectionID = -1, int groupID = -1)
    {
        if (groupID == -1 && connectionID == -1) return; // Hiba - Nem tudjuk meghatározni, hogy melyik csoportnak kell küldeni az üzenetet

        // Ha nincs megadva a cél csoport, akkor a kliens csoportja lesz
        if (groupID == -1)
        {
            ClientData clientData = SearchClientWithConnectionID(connectionID);
            if (clientData == null) return;

            groupID = SearchClientWithConnectionID(connectionID).groupID;
            if (groupID == -1) return;
        }

        groups[groupID].SendMessageForAllClient(jsonData, connectionID);
    }

    // Fogadjuk a hálózaton érkező adatokat, amit a NetworkTest réteg továbbít
    static void ReceivedNetworkEvent(NetworkEventType networkEventType, int connectionID, JSONNode receivedData)
    {
        if (networkEventType == NetworkEventType.DataEvent)
        {

            Debug.Log("Server_Logic - received data" + "\n" + receivedData.ToString(" "));

            string s = receivedData["dataContent"];

            if (s == "groupData")
            {
                SendJSONAGroup(receivedData, connectionID);
            }
        }

        // Ha be van állítva feldolgozó, akkor átadjuk neki a bejövő adatokat
        if (callBackNetworkEvent != null)
            callBackNetworkEvent(networkEventType, connectionID, receivedData);
    }
}
