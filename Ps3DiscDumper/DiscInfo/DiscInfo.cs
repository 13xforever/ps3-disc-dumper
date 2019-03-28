using System.Collections.Generic;

namespace Ps3DiscDumper.DiscInfo
{
    using Hashes = Dictionary<string, byte[]>; // hash type - hash value

    public class DiscInfo
    {
        public string ProductCode;    // BLUS12345
        public string DiscVersion;    // VERSION field from PARAM.SFO
        public byte[] DiscKeyRawData; // IRD
        public byte[] DiscKey;        // Redump, and what is actually used for decryption
        public FileInfo DiscImage;
        public Dictionary<string, FileInfo> Files;
    }

    public class FileInfo
    {
        public long Offset;
        public long Size;
        public Hashes Hashes;
    }
}
