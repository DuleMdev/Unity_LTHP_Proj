using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.IO.Compression;
using System.Collections;

public class DataPackerSender {

    int connectionID;   // A kliens hálózati kapcsolat azonosítója
    int packID;         // A csomagok azonosítója
    bool compressed;    // Tömörített az adathalmaz
    byte[] dataSet;        // A küldendő adat

    int index;          // Hányadik csomag a következő
    public bool ready { get; private set; }  // Kész van az adat küldés

    // Vissza adja, hogy az adathalmaz hány százaléka lett már elküldve
    public float percent { get { return (index * HHHNetwork.PACK_DATA_LENGTH) / dataSet.Length; } }


    public DataPackerSender(int connectionID, int packID, byte[] dataSet) {

        Common.configurationController.Log("Adathalmaz küldés. ConnectioID = " + connectionID + " - CsomgID = " + packID + " - Adat hossz = " + dataSet.Length);

        this.connectionID = connectionID;
        this.dataSet = dataSet;
        this.packID = packID;
        compressed = false;

        index = 0;

        /*
        // Ha az adathalmaz nagyobb mint egy csomag maximális mérete, akkor tömörítjük
        if (dataSet.Length > HHHNetwork.PACK_DATA_LENGTH) {
            MemoryStream input = new MemoryStream(dataSet);
            MemoryStream output = new MemoryStream();

            GZipStream zipStream = new GZipStream(input, CompressionMode.Compress);

            Common.CopyStream(zipStream, output);

            byte[] b = output.ToArray();
            // Ha a tömörítás után kisebb méretet kaptunk, akkor azt küldjük át, egyébként az eredetit
            if (b.Length < dataSet.Length) {
                dataSet = b;
                compressed = true;
            }
        }
        */
    }

    public void SendNextPacks()
    {
        // Kiszámoljuk, hogy mennyi adat maradt még.
        int byteLeft = dataSet.Length - index * HHHNetwork.PACK_DATA_LENGTH;

        // Ha nulla, akkor készen vagyunk
        if (byteLeft <= 0)
        {
            ready = true;
            Common.configurationController.Log("Adathalmaz küldés befejezve. ConnectionID = " + connectionID + " - CsomgID = " + packID);
        }
        else {
            int count = HHHNetwork.PACK_DATA_LENGTH;

            // Ha kisebb mint a csomag maximális mérete, akkor a maradékot fogjuk átküldeni
            if (byteLeft <= HHHNetwork.PACK_DATA_LENGTH)
                count = byteLeft;

            // Kimásoljuk az elküldendő adatokat
            byte[] pack = new byte[count];
            System.Buffer.BlockCopy(dataSet, index * HHHNetwork.PACK_DATA_LENGTH, pack, 0, count);

            byte error = Common.HHHnetwork.SendData(connectionID, (new DataPacket(dataSet.Length, packID, index, pack)).Serialize());

            if ((NetworkError)error == NetworkError.Ok)
                index++;
        }
    }
}
