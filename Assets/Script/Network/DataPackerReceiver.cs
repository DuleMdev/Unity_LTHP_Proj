using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Collections;



public class DataPackerReceiver {


    public int packID { get; private set; } // A csomagok azonosítója
    public byte[] dataSet;     // A fogadott adat

    public bool ready;  // Kész van az adat küldés

    bool compressed;    // Tömörített az adathalmaz
    
    int second;         // Mennyit várjon az utolsó fogadott adatcsomag után, hogy outOfTime jelzést adjon

    float lastReceiveTime;  // Mikor fogadott utoljára adatot az objektum

    public bool outOfTime {
        get { return !ready && lastReceivedDataPacketTime >= second; } // Time.time; }
    }

    public float lastReceivedDataPacketTime {
        get { return Time.time - lastReceiveTime; }
    }

    BitArray bitArray;  // Melyik csomagot kaptuk már meg, ha minden bit igaz, akkor az összeset
    int index;          // Melyik a legkisebb indexű adat csomag amit még nem kaptunk meg

    // Vissza adja, hogy az adathalmaz hány százaléka lett már fogadva
    public float percent { get { return (ready) ? 1 : ((float)index) / bitArray.Count; } }

    public DataPackerReceiver(DataPacket dataPacket, int second = 60)
    {
        this.second = second;
        packID = dataPacket.packID;
        compressed = dataPacket.compressed;
        index = 0;

        // Létrehozzuk az adatHalmaz összeállításához szükséges byte tömböt
        dataSet = new byte[dataPacket.dataSetLength];

        // Az összes szükséges csomag kiszámítása
        int allPack = (dataPacket.dataSetLength + HHHNetwork.PACK_DATA_LENGTH - 1) / HHHNetwork.PACK_DATA_LENGTH;

        // Létrehozzuk hozzá a szükséges bit tömböt
        bitArray = new BitArray(allPack);

        Common.configurationController.Log("Adathalmaz összeállítás elkezdése. CsomgID = " + packID + " - Csomag darabok = " + allPack + " - Adat hossz = " + dataPacket.dataSetLength);

        ReceiveData(dataPacket);
    }

    public void ReceiveData(DataPacket dataPacket)
    {
        // Megvizsgáljuk, hogy a csomag azonosítója egyezik-e 
        if (dataPacket.packID != packID) {
            Debug.LogError("Rossz csomag azonosító!");
        } else {
            // Ha már minden adat megérkezett, akkor kilépünk
            if (ready)
                return;

            lastReceiveTime = Time.time;

            // Ha a megadott csomagot még nem vettük, akkor tároljuk
            if (!bitArray[dataPacket.packOrder]) {
                // Belemásoljuk az adathalmazba a csomag tartalmát
                System.Buffer.BlockCopy(dataPacket.pack, 0, dataSet, dataPacket.packOrder * HHHNetwork.PACK_DATA_LENGTH, dataPacket.pack.Length);

                bitArray[dataPacket.packOrder] = true;

                // Megnézzük, hogy melyik a legkisebb csomag ami még nem érkezett meg.
                while (bitArray[index]) {
                    index++;
                    if (index >= bitArray.Length) {
                        // Az összes csomag megérkezett
                        ready = true;
                        Common.configurationController.Log("Adathalmaz összeállítás befejezve. CsomgID = " + packID);

                        // Ha tömörítve van az adathalmaz, akkor kitömörítjük
                        if (compressed) {
                            MemoryStream input = new MemoryStream(dataSet);
                            MemoryStream output = new MemoryStream();

                            GZipStream zipStream = new GZipStream(input, CompressionMode.Decompress);

                            Common.CopyStream(zipStream, output);

                            dataSet = output.ToArray();
                        }

                        break;
                    }
                }
            }
        }
    }
}
