using System.Collections.Generic;

namespace Ps3DiscDumper;

public sealed class KeyFinderResult
{
    public required string SelectedPhysicalDevice { get; init; }
    public required string? SelectedPhysicalDeviceName { get; init; }
    public required long SectorSize { get; init; }
    public required KeyType DiscKeyType { get; init; }
    public required string? DiscKeyFilename { get; init; }
    public required HashSet<DiscKeyInfo> AllMatchingKeys { get; init; }
}