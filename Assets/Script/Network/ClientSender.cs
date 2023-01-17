using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientSender {

    int connectionID;
    int packID;     // Mi legyen a következő csomag azonosító

    List<DataPackerSender> listOfPacker;    // A küldő csomagolók listája

    public int Count { get { return listOfPacker.Count; } private set{ } }

    // Vissza adja, hogy az első adathalmaz hány százaléka lett már elküldve
    public float percent { get { return (listOfPacker.Count > 0) ? listOfPacker[0].percent : 1; } }

    public ClientSender(int connectID) {
        this.connectionID = connectID;

        listOfPacker = new List<DataPackerSender>();
    }

    public void SendData(byte[] dataSet) {
        listOfPacker.Add(new DataPackerSender(connectionID, packID++, dataSet));

        Update();
    }

	public void Update () {
        // A sorban az első csomagolóból küldünk egy csomagot
        if (listOfPacker.Count > 0) {
            listOfPacker[0].SendNextPacks();

            // Ha végzet kitöröljük
            if (listOfPacker[0].ready)
                listOfPacker.RemoveAt(0);
        }

        /*

        // Végig megyünk a csomagolókon és elküldjük a tartalmukat
        foreach (DataPackerSender packer in listOfPacker)
            packer.SendNextPacks();

        // Újra végig megyünk a csomogalókon, ha valamelyik befejezte a dolgát, akkor kitöröljük
        for (int i = listOfPacker.Count - 1; i >= 0; i--)
            if (listOfPacker[i].ready)
                listOfPacker.RemoveAt(i);
                */
    }
}
