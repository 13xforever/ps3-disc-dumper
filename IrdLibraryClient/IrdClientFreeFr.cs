﻿using System.Text.RegularExpressions;

namespace IrdLibraryClient;

public partial class IrdClientFreeFr : IrdClient
{
    public override Uri BaseUri { get; } = new("http://ps3ird.free.fr/");
    protected override Regex IrdFilename { get; } = FreeFrLink();

    [GeneratedRegex(@"href=""(?<filename>[A-F0-9_]+\.ird)""(.+?<td>(?<product_code>\w{4}\d{5})</td>)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Singleline)]
    private static partial Regex FreeFrLink();
}