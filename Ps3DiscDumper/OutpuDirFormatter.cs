using System;
using System.IO;
using System.Linq;
using IrdLibraryClient;
using Ps3DiscDumper.Utils;

namespace Ps3DiscDumper;

public class OutputDirFormatter
{
    public static string Format(DiscDetectionResult discInfo, Func<DiscDetectionResult, string>? outputDirFormatter = null)
    {
        outputDirFormatter ??= d => PatternFormatter.Format(SettingsProvider.Settings.DumpNameTemplate, new()
        {
            [Patterns.ProductCode] = d.ProductCode,
            [Patterns.ProductCodeLetters] = d.ProductCode[..4],
            [Patterns.ProductCodeNumbers] = d.ProductCode[4..],
            [Patterns.Title] = d.Title,
            [Patterns.Region] = Dumper.RegionMapping[d.ProductCode[2..3]] ?? "",
        });
        var path = new string([.. outputDirFormatter(discInfo).Where(c => !Dumper.InvalidChars.Contains(c))]);
        var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        var pathParts = path.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(p => p.TrimEnd('.'));
        var outputDir = string.Join(Path.DirectorySeparatorChar, pathParts);
        Log.Debug($"Dump folder name: {outputDir}");
        return outputDir;
    }
}