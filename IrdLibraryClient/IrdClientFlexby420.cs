using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using IrdLibraryClient.Compression;

namespace IrdLibraryClient;

public class IrdClientFlexby420: IrdClient
{
    public override Uri BaseUri { get; } = new("https://flexby420.github.io/playstation_3_ird_database/all.json");
    protected override Regex IrdFilename => throw new NotImplementedException();
    private Uri BaseDownloadUri { get; } = new("https://github.com/FlexBy420/playstation_3_ird_database/raw/main/");
    private readonly JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower, };
    private readonly HttpClient client = HttpClientFactory.Create(new CompressionMessageHandler());

    [DebuggerDisplay("{Link}", Name="{Title}")]
    public class IrdInfo
    {
        public string Title { get; set; } = null!;
        public string? FwVer { get; set; }
        public string? GameVer { get; set; }
        public string? AppVer { get; set; }
        public string Link { get; set; } = null!;
    }
    
    public override async Task<List<(string productCode, string irdFilename)>> GetFullListAsync(CancellationToken cancellationToken)
    {
        try
        {
            try
            {
                var data = await client.GetFromJsonAsync<Dictionary<string, IrdInfo[]>>(BaseUri, jsonOptions, cancellationToken).ConfigureAwait(false);
                if (data is null or { Count: 0 })
                    return [];
                
                var result = new List<(string productCode, string irdFilename)>(5000);
                foreach (var (productCode, irdInfoList) in data)
                foreach (var irdInfo in irdInfoList)
                    result.Add((productCode, irdInfo.Link));
                return result;
            }
            catch (Exception e)
            {
                Log.Warn(e, "Failed to make API call to IRD Library");
                return [];
            }
        }
        catch (Exception e)
        {
            Log.Error(e);
            return [];
        }
    }

    protected override string GetDownloadLink(string irdFilename)
    {
        var builder = new UriBuilder(BaseDownloadUri);
        builder.Path = Path.Combine(builder.Path, irdFilename);
        return builder.ToString();
    }
}