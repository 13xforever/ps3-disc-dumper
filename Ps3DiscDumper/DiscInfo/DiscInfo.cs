using System.Collections.Generic;

namespace Ps3DiscDumper.DiscInfo;

public class DiscInfo
{
    public string ProductCode;    // BLUS12345
    public string DiscVersion;    // VERSION field from PARAM.SFO
    public string DiscKeyRawData; // IRD
    public string DiscKey;        // Redump, and what is actually used for decryption
    public FileInfo DiscImage;
    public Dictionary<string, FileInfo> Files;
}