using System.Collections.Generic;
using System.Text;
using BitConverter;

namespace Ps3DiscDumper.Sfb
{
    public class Sfb
    {
        public static int Magic = EndianBitConverter.BigEndian.ToInt32(Encoding.ASCII.GetBytes(".SFB"), 0);
        public short VersionMajor;
        public short VersionMinor;
        public byte[] Unknown1; // 0x18
        public List<SfbKeyEntry> KeyEntries = new();
    }

    public class SfbKeyEntry
    {
        public string Key; // 0x10
        public int ValueOffset;
        public int ValueLength;
        public long Unknown;
        public string Value;
    }
}