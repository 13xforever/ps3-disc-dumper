using System.Collections.Generic;
using Ps3DiscDumper.Sfo;

namespace Ps3DiscDumper;

public class DiscDetectionResult
{
    public required byte[] DiscSfbData { get; init; } 
    public required List<string> DiscFilenames { get; init; }
    public required ParamSfo ParamSfo { get; init; }
    public required string ProductCode { get; init; }
    public required string Title { get; init; }
    public required string InputDevicePath { get; init; }
    public required int TotalFileCount { get; init; }
    public required long TotalFileSize { get; init; }

    public string? DiscVersion { get; init; }
    public long? TotalDiscSize { get; init; } // can be 0
    public char Drive { get; init; } // only on windows
}