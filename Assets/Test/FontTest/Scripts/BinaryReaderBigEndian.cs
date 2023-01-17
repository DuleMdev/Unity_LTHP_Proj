
using System;
using System.IO;

public class BinaryReaderBigEndian
{
    public FileStream fileStreeam;

    byte[] bytes8 = new byte[8];
    byte[] bytes4 = new byte[4];
    byte[] bytes2 = new byte[2];

    public long Position {
        get { return fileStreeam.Position; }
        set { fileStreeam.Position = value; }
    }

    public BinaryReaderBigEndian(FileStream fileStream)
    {
        this.fileStreeam = fileStream;
    }

    public int GetInt()
    {
        fileStreeam.Read(bytes4, 0, 4);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes4);

        return BitConverter.ToInt32(bytes4, 0);
    }

    public uint GetUInt()
    {
        return (uint)GetInt();
    }

    public short GetShort()
    {
        fileStreeam.Read(bytes2, 0, 2);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes2);

        return BitConverter.ToInt16(bytes2, 0);
    }

    public ushort GetUShort()
    {
        return (ushort)GetShort();
    }

    public string GetString4()
    {
        fileStreeam.Read(bytes4, 0, 4);

        return System.Text.Encoding.UTF8.GetString(bytes4);
    }


    /*
    byte[] bytes = { 0, 0, 0, 25 };

// If the system architecture is little-endian (that is, little end first),
// reverse the byte array.
if (BitConverter.IsLittleEndian)
    Array.Reverse(bytes);

int i = BitConverter.ToInt32(bytes, 0);
    Console.WriteLine("int: {0}", i);
    */
}
