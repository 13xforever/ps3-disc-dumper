using System.Text.RegularExpressions;

namespace IrdLibraryClient;

public partial class IrdClientAldos: IrdClient
{
    public override Uri BaseUri { get; } = new Uri("https://ps3.aldostools.org/ird/").SetQueryParameters(new Dictionary<string, string>
    {
        ["F"] = "0",
        ["_"] = DateTime.UtcNow.Ticks.ToString(),
    });
    protected override Regex IrdFilename { get; } = AldosLink();

    [GeneratedRegex(@"href=""(?<filename>(?<product_code>\w{4}\d{5})-[A-F0-9]+\.ird)""", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Singleline)]
    private static partial Regex AldosLink();
}