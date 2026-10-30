using System.Collections.Generic;

namespace Ps3DiscDumper.DiscInfo;

public class DiscInfo
{
    public required string ProductCode;    // BLUS12345
    public string? DiscVersion;            // VERSION field from PARAM.SFO
    public required string DiscKeyRawData; // IRD
    public required string DiscKey;        // Redump, and what is actually used for decryption
    public FileInfo? DiscImage;
    public required Dictionary<string, FileInfo> Files;
}