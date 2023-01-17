using System.IO;

namespace TTF_Font
{


    public class TTF_FontProcessor
    {
        //// offset subtable data
        //uint scalerType;
        //ushort numTables;
        //ushort searchRange;
        //ushort entrySelector;
        //ushort rangeShift;

        public FontDirectory fontDirectory;
        public Table_cmap table_cmap;

        public TTF_FontProcessor(string fileName)
        {
            // Ha a file létezik, akkor betöltjük a tartalmát
            if (File.Exists(fileName))
            {
                using (FileStream stream = new FileStream(fileName, FileMode.Open))
                {
                    BinaryReaderBigEndian reader = new BinaryReaderBigEndian(stream);

                    //using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.BigEndianUnicode))
                    {
                        fontDirectory = new FontDirectory(reader);
                    }

                    if (fontDirectory.SetFilePositionToTag(stream, "cmap"))
                    {
                        table_cmap = new Table_cmap(reader);
                    }
                }
            }
        }
    }
}

