using System.Collections.Generic;

namespace Ps3DiscDumper.DiscInfo;

public class FileInfo
{
    public required long Offset;
    public required long Size;
    public required Dictionary<string, List<string>> Hashes;
}