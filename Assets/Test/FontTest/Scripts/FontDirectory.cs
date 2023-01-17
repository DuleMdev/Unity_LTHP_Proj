
using System.Collections.Generic;
using System.IO;

namespace TTF_Font
{


    public class FontDirectory
    {
        // offset subtable data
        public uint scalerType;
        public ushort numTables;
        public ushort searchRange;
        public ushort entrySelector;
        public ushort rangeShift;

        public List<FontDirectoryEntry> listOfFontDirectoryEntries;

        public FontDirectory(BinaryReaderBigEndian reader)
        {
            scalerType = reader.GetUInt();
            numTables = reader.GetUShort();
            searchRange = reader.GetUShort();
            entrySelector = reader.GetUShort();
            rangeShift = reader.GetUShort();

            /*
            scalerType = reader.ReadUInt32();
            numTables = reader.ReadUInt16();
            searchRange = reader.ReadUInt16();
            entrySelector = reader.ReadUInt16();
            rangeShift = reader.ReadUInt16();
            */

            listOfFontDirectoryEntries = new List<FontDirectoryEntry>();

            for (int i = 0; i < numTables; i++)
            {
                listOfFontDirectoryEntries.Add(new FontDirectoryEntry(reader));
            }
        }

        // Megkeresi a megadott tag-et a FontDirectory bejegyzések között és a fájl mutatót rá állítja, hamis értéket ad vissza ha nem találtaa meg
        public bool SetFilePositionToTag(FileStream stream, string tag)
        {
            foreach (FontDirectoryEntry item in listOfFontDirectoryEntries)
            {
                if (item.tag == tag)
                {
                    stream.Position = item.offset;
                    return true;
                }
            }

            return false;
        }

    }

    public class FontDirectoryEntry
    {
        public string tag;
        public uint checkSum;
        public uint offset;
        public uint length;

        public FontDirectoryEntry(BinaryReaderBigEndian reader)
        {
            tag = reader.GetString4();
            checkSum = reader.GetUInt();
            offset = reader.GetUInt();
            length = reader.GetUInt();
        }
    }

    public enum TableNames
    {
        acnt,
        ankr,
        avar,
        bdat,
        bhed,
        bloc,
        bsln,
        cmap,
        cvar,
        cvt,
        EBSC,
        fdsc,
        feat,
        fmtx,
        fond,
        fpgm,
        fvar,
        gasp,
        gcid,
        glyf,
        gvar,
        hdmx,
        head,
        hhea,
        hmtx,
        just,
        kern,
        kerx,
        lcar,
        loca,
        ltag,
        maxp,
        meta,
        mort,
        morx,
        name,
        opbd,
        OS_2,
        post,
        prep,
        prop,
        sbix,
        trak,
        vhea,
        vmtx,
        xref,
        Zapf,
    }
}
