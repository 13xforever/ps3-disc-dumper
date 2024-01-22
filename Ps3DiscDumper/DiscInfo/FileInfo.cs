using System.Collections.Generic;

namespace Ps3DiscDumper.DiscInfo;

public class FileInfo
{
    public long Offset;
    public long Size;
    public Dictionary<string, List<string>> Hashes;
}