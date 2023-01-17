using UnityEngine;
using System.Collections.Generic;

/*

Fogadja a kliensnek küldött adatot a hálózaton.
    

    
*/

public class ClientReceiver {

    List<DataPackerReceiver> listOfPacker;    // A küldő csomagolók listája

    int second;         // Ha az utolsó fogadott adat óta ennyi másodperc telt el és még nincs kész a letöltés, akkor hiba
    int index;          // Minek kellene lennie a következő adathalmaz azonosítójának

    public int Count { get { return listOfPacker.Count; } private set { } }

    Common.CallBack_In_ByteArray callBack;

    // Vissza adja, hogy az első adathalmaz hány százaléka lett már fogadva
    public float percent { get { return (listOfPacker.Count > 0) ? listOfPacker[0].percent : 1; } }

    float nextSendTime;     // Mikor volt az utolsó üzenet átadás

    public ClientReceiver(Common.CallBack_In_ByteArray callBack, int second = 60) {
        this.callBack = callBack;
        this.second = second;

        listOfPacker = new List<DataPackerReceiver>();
    }

    public void ReceiveData(DataPacket dataPacket) {

        bool processed = false; // Feldolgoztuk az adathalmazt? azaz találtunk hozzá tartozó feldolgozót?

        // Megnézzük, hogy melyik feldolgozó foglalkozik ezzel az adathalmazzal
        foreach (DataPackerReceiver packer in listOfPacker)
        {
            if (packer.packID == dataPacket.packID) {
                packer.ReceiveData(dataPacket);
                processed = true;
                break;
            }
        }

        // Ha nincs feldolgozó, akkor készítünk egy újat az adatcsomagnak
        if (!processed) {
            listOfPacker.Add(new DataPackerReceiver(dataPacket));
        }

        Update();
    }

    // Update is called once per frame
    /// <summary>
    /// A metódus megnézi, hogy össze lett-e már állítva egy adathalmaz.
    /// Az adathalmazokat packID sorrendjében adja vissza, tehát mindig a legkisebb azonosítójút.
    /// Ha a legkissebb azonosítójú elakadt, azt egy idő múlva kifogja dobni a listából, ha outOfTime-os lesz.
    /// Ha a legkissebb azonosítójú az aminek következnie kellene (index), akkor azonnal visszaadja.
    /// Ha viszont nem az, akkor várnia kell 5 másodpercet és ha nem jön kisebb azonosítójú adathalmaz 
    /// ezalatt az öt másodperc alatt, akkor visszaadja.
    /// </summary>
	public void Update () {
        // Csak akkor nézzük meg, hogy van-e összeállított adathalmaz, ha nincs letiltva az üzenet feldolgozás
        if (Common.HHHnetwork.messageProcessingEnabled)
        {
            // Végig megyünk a listán és kidobunk mindent ami már outOfTime-os
            for (int i = listOfPacker.Count - 1; i >= 0; i--)
                if (listOfPacker[i].outOfTime)
                {
                    Common.configurationController.Log("Adathalmaz fogadás sikertelen - OutOfTime. CsomgID = " + listOfPacker[i].packID + " - % = " + listOfPacker[i].percent);

                    listOfPacker.RemoveAt(i);
                }

            // Ha a lista nem üres és az utolsó adat átadás óta eltelt egy tized másodperc
            if (listOfPacker.Count != 0 && nextSendTime <= Time.time)
            {
                // Megkeressük a legkisebb csomag azonosítójú adathalmaz-t
                DataPackerReceiver minPacker = null;
                int minDataPacketID = int.MaxValue;
                foreach (DataPackerReceiver packer in listOfPacker)
                {
                    if (packer.packID < minDataPacketID)
                    {
                        minPacker = packer;
                        minDataPacketID = packer.packID;
                    }
                }

                // Megnézzük, hogy kész van-e
                if (minPacker.ready)
                {
                    if (minPacker.packID <= index || minPacker.lastReceivedDataPacketTime > 5)
                    {
                        // Meghatározzuk, hogy melyik csomag azonosítót kell legközelebb vissza adni
                        index = minPacker.packID + 1;
                        // Ha kész van töröljük a listából és továbbítjuk
                        listOfPacker.Remove(minPacker);
                        callBack(minPacker.dataSet);
                        nextSendTime = Time.time + 0.1f;
                    }
                    else {
                        Common.configurationController.Log(
                            "Adathalmaz nem átadható, mivel az azonosítója nagyobb mint a várt és még nem telt el öt másodperc a fogadása óta.\n Csomag ID = " +
                            minPacker.packID + " - Várt ID = " + index + " - Eltelt idő = " + minPacker.lastReceivedDataPacketTime);
                    }
                }
            }
        }
    }
}
