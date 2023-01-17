
using System.Collections.Generic;

namespace TTF_Font
{


    public class Table_cmap
    {
        public ushort version;
        public ushort numberSubtables;

        public List<cmap_Subtable> listOfSubtable;

        public Table_cmap(BinaryReaderBigEndian reader)
        {
            long baseFilePosition = reader.Position;

            version = reader.GetUShort();
            numberSubtables = reader.GetUShort();

            listOfSubtable = new List<cmap_Subtable>();

            // Beolvassuk a táblák fejlécét
            for (int i = 0; i < numberSubtables; i++)
            {
                listOfSubtable.Add(new cmap_Subtable(reader));
            }

            // Beolvassuk a tábla többi adatát
            foreach (var item in listOfSubtable)
            {
                reader.Position = baseFilePosition + item.offset;
                item.Read_cmap_Data(reader);
            }
        }
    }

    public class cmap_Subtable
    {
        public ushort platformID;
        public ushort platformSpecificID;
        public uint offset;

        public cmap_Platforms platformIDenum;
        public UnicodePlatformSpecifics unicodePlatformSpecificsIDenum;
        public WindowsPlatformSpeecifics windowsPlatformSpeecificsIDenum;

        // additional data
        public ushort format;

        public object format_;


        /*
        public cmap_Platforms getPlatform {
            get {
                Common.
            }
        }
        */

        public cmap_Subtable(BinaryReaderBigEndian reader)
        {
            platformID = reader.GetUShort();
            platformSpecificID = reader.GetUShort();
            offset = reader.GetUInt();

            platformIDenum = (cmap_Platforms)platformID;
            unicodePlatformSpecificsIDenum = (UnicodePlatformSpecifics)platformSpecificID;
            windowsPlatformSpeecificsIDenum = (WindowsPlatformSpeecifics)platformSpecificID;
        }

        public void Read_cmap_Data(BinaryReaderBigEndian reader)
        {
            format = reader.GetUShort();

            if (format != 4)
                return;

            format_ = new format4(reader);
        }
    }

    public class format4
    {
        public ushort length;
        public ushort language;
        public ushort segCountX2;
        public ushort searchRange;
        public ushort entrySelector;
        public ushort rangeShift;
        public ushort[] endCode;
        public ushort reservedPad;
        public ushort[] startCode;
        public ushort[] idDelta;
        public ushort[] idRangeOffset;
        public ushort[] glyphIndexArray;

        public format4(BinaryReaderBigEndian reader)
        {
            length = reader.GetUShort();
            language = reader.GetUShort();
            segCountX2 = reader.GetUShort();
            searchRange = reader.GetUShort();
            entrySelector = reader.GetUShort();
            rangeShift = reader.GetUShort();

            int count = segCountX2 / 2;

            endCode = new ushort[count];
            for (int i = 0; i < count; i++)
                endCode[i] = reader.GetUShort();

            reservedPad = reader.GetUShort();

            startCode = new ushort[count];
            for (int i = 0; i < count; i++)
                startCode[i] = reader.GetUShort();

            idDelta = new ushort[count];
            for (int i = 0; i < count; i++)
                idDelta[i] = reader.GetUShort();

            idRangeOffset = new ushort[count];
            for (int i = 0; i < count; i++)
                idRangeOffset[i] = reader.GetUShort();
        }

        // Megvizsgálja a megadott karakterkből, hogy melyek találhatóak meg a betűtípusban.
        // Azokat adja vissza amelyek nem találhatóak meg benne.
        public string ContainStringTest(string s)
        {
            string result = "";

            foreach (var item in s)
            {
                if (!ContainCharTest(item))
                    if (result.IndexOf(item) == -1)
                        result += item;
            }

            return result;
        }

        public bool ContainCharTest(char c)
        {
            for (int i = 0; i < segCountX2 / 2; i++)
            {
                if (startCode[i] <= c && endCode[i] >= c)
                    return true;
            }

            return false;
        }
    }

    public enum cmap_Platforms
    {
        Unicode,
        Macintosh,
        Reserved,
        Microsoft,

        Ignored, // Ha nem az előző négyből kerül ki a Platform ID-ja, akkor a cmap figyelmen kívűl lesz hagyva
    }

    public enum UnicodePlatformSpecifics
    {
        Version_1_0,
        Version_1_1,
        ISO_10646_1993,
        Unicode_2_0_or_later_BMP_only,
        Unicode_2_0_or_later_non_BMP_characters_allowed,
        Unicode_Variation_Sequences,
        Last_Report, // Végső megoldás

        Unknown,
    }

    public enum WindowsPlatformSpeecifics
    {
        Symbol,
        Unicode_BMP_only_UCS_2,
        Shift_JS,
        PRC,
        BigFive,
        Johab,
        Unicode_UCS_4 = 10,

        Unknown,
    }



}


