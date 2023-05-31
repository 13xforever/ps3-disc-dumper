using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace Ps3DiscDumper.Utils;

public static class PatternFormatter
{
    private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars()
        .Concat(Path.GetInvalidPathChars())
        .Concat(new[] { ':', '/', '\\', '?', '*', '<', '>', '|' })
        .Distinct()
        .ToArray();
        
    public static string Format(string pattern, NameValueCollection items)
    {
        if (string.IsNullOrEmpty(pattern))
            return pattern;

        foreach (var k in items.AllKeys)
            pattern = pattern.Replace($"%{k}%", items[k]);
        pattern = pattern.Replace("®", "")
            .Replace("™", "")
            .Replace("(TM)", "")
            .Replace("(R)", "");
        pattern = ReplaceInvalidChars(pattern);
        if (string.IsNullOrEmpty(pattern))
            pattern = ReplaceInvalidChars($"{DateTime.Now:s}");
        return pattern;
    }

    private static string ReplaceInvalidChars(string dirName)
    {
        foreach (var invalidChar in InvalidChars)
            dirName = dirName.Replace(invalidChar, '_');
        return dirName;
    }
}