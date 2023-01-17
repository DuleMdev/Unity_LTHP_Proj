using UnityEngine;
using System.IO;
using System.Collections;

/*
A hálózaton átküldött adat csomag.
Egy csomagban maximum 1000 bájtot lehet küldeni.
Az objektumban töltött adatokat byte tömbbé alakítja.
Illetve képes egy bájt tömbböl kiolvasni az adatokat és vissza állítani magát.

*/

public class DataPacket {

    public int dataSetLength;   // Az átküldendő adathalmaz teljes hossza (nem a csomag hossza) Ebből számítható, hogy hány csomag tartozik ehhez az adathalmazhoz
    public int packID;          // Csomag azonosító (Ez az érték csak az egy adathalmazba tartozó csomagoknál azonos)
    public int packOrder;       // Hányadik csomaz az adathalmazban
    public bool compressed;     // Tömörített az adathalmaz
    public byte[] pack;         // Maga a csomag (mérete 1000 bájt, csak az utolsó csomag mérete lehet ennél kisebb)

    public DataPacket(int dataLength, int packID, int packOrder, byte[] pack) {
        this.dataSetLength = dataLength;
        this.packID = packID;
        this.packOrder = packOrder;
        this.pack = pack;
    }

    /// <summary>
    /// A megadott bájt tömbböl kiolvassa és felépíti az objektumot. (Deserialize)
    /// </summary>
    /// <param name="pack">A hálózaton bejött adat.</param>
    public DataPacket(byte[] pack) {
        using (MemoryStream ms = new MemoryStream(pack))
        {
            using (BinaryReader reader = new BinaryReader(ms))
            {
                dataSetLength = reader.ReadInt32();
                packID = reader.ReadInt32();
                packOrder = reader.ReadInt32();
                compressed = reader.ReadBoolean();

                int Length = reader.ReadInt32();
                this.pack = reader.ReadBytes(Length);
            }
        }
    }

    /// <summary>
    /// Az objektum adatmezőit egy bájt tömbe konvertálja.
    /// </summary>
    /// <returns>Az objektum bájt tömb formában.</returns>
    public byte[] Serialize() {
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(dataSetLength);       
                writer.Write(packID);
                writer.Write(packOrder);
                writer.Write(compressed);

                writer.Write(pack.Length);
                writer.Write(pack);
            }
            return ms.ToArray();
        }
    }
}
