using System;
using System.Collections.Generic;
using System.Text;

namespace IrdLibraryClient.IrdFormat
{
    public class Ird
    {
        public static int Magic = BitConverter.ToInt32(Encoding.ASCII.GetBytes("3IRD"), 0);
        public byte Version;
        public string ProductCode; // 9
        public byte TitleLength;
        public string Title;
        public string UpdateVersion; // 4
        public string GameVersion; // 5
        public string AppVersion; // 5
        public int Id; // v7 only?
        public int HeaderLength;
        public byte[] Header; // gz
        public int FooterLength;
        public byte[] Footer; // gz
        public byte RegionCount;
        public List<byte[]> RegionMd5Checksums; // 16 each
        public int FileCount;
        public List<IrdFile> Files;
        public int Unknown; // always 0?
        public byte[] Pic; // 115, v9 only?
        public byte[] Data1; // 16
        public byte[] Data2; // 16
        // Pic for <v9
        public int Uid;
        public uint Crc32;
    }

    public class IrdFile
    {
        public long Offset;
        public byte[] Md5Checksum;
    }
}