using System;
using System.Text.RegularExpressions;

namespace IrdLibraryClient;

public class IrdClientFreeFr : IrdClient
{
    public override Uri BaseUri { get; } = new("http://ps3ird.free.fr/");
    protected override Regex IrdFilename { get; } = new(
        @"href=""(?<filename>[A-F0-9_]+\.ird)""(.+?<td>(?<product_code>\w{4}\d{5})</td>)",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
    );
}