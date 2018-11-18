using System.Collections.Generic;
using System.Text;
using static BitConverter.EndianBitConverter;

namespace Ps3DiscDumper
{
    public class Sfb
    {
        public static int Magic = BigEndian.ToInt32(Encoding.ASCII.GetBytes(".SFB"), 0);
        public short VersionMajor;
        public short VersionMinor;
        public byte[] Unknown1; // 0x18
        public List<SfbKeyEntry> KeyEntries = new List<SfbKeyEntry>();
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