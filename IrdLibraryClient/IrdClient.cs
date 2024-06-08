using System.Text.RegularExpressions;
using IrdLibraryClient.Compression;
using IrdLibraryClient.IrdFormat;

namespace IrdLibraryClient;

public abstract class IrdClient: IIrdClient
{
    public abstract Uri BaseUri { get; }
    protected abstract Regex IrdFilename { get; }
    protected readonly HttpClient Client = HttpClientFactory.Create(new CompressionMessageHandler());

    protected IrdClient()
    {
        Client.Timeout = TimeSpan.FromSeconds(30);
    }

    protected virtual string GetDownloadLink(string irdFilename)
    {
        var builder = new UriBuilder(BaseUri) { Query = "" };
        builder.Path = Path.Combine(builder.Path, irdFilename);
        return builder.ToString();
    }

    public virtual async Task<List<(string productCode, string irdFilename)>> GetFullListAsync(CancellationToken cancellationToken)
    {
        try
        {
            var requestUri = BaseUri;
            try
            {
                var responseData = await Client.GetStringAsync(requestUri, cancellationToken).ConfigureAwait(false);
                return GetIrdInfo(responseData);
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

    public async Task<Ird?> DownloadAsync(string irdName, string localCachePath, CancellationToken cancellationToken)
    {
        Ird? result = null;
        try
        {
            var localCacheFilename = Path.Combine(localCachePath, irdName);
            // first we search local cache and try to load whatever data we can
            try
            {
                if (File.Exists(localCacheFilename))
                {
                    var fi = new FileInfo(localCacheFilename);
                    if (fi.Length > 0)
                        return IrdParser.Parse(await File.ReadAllBytesAsync(localCacheFilename, cancellationToken).ConfigureAwait(false));
                        
                    Log.Warn($"Removing empty IRD file {localCacheFilename}...");
                    try
                    {
                        File.Delete(localCacheFilename);
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e, $"Failed to remove invalid IRD file {localCacheFilename}");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn(e, "Error accessing local IRD cache: " + e.Message);
            }
            try
            {
                var resultBytes = await Client.GetByteArrayAsync(GetDownloadLink(irdName), cancellationToken).ConfigureAwait(false);
                result = IrdParser.Parse(resultBytes);
                try
                {
                    var savePath = Path.GetDirectoryName(localCacheFilename)!;
                    if (!Directory.Exists(savePath))
                        Directory.CreateDirectory(savePath);
                    await File.WriteAllBytesAsync(localCacheFilename, resultBytes, cancellationToken);
                }
                catch (Exception ex)
                {
                    Log.Warn(ex, $"Failed to write {irdName} to local cache: {ex.Message}");
                }
            }
            catch (Exception e)
            {
                Log.Warn(e, $"Failed to download {irdName}: {e.Message}");
            }
            return result;
        }
        catch (Exception e)
        {
            Log.Error(e);
            return result;
        }
    }

    private List<(string productCode, string irdFilename)> GetIrdInfo(string? content)
    {
        var result = new List<(string, string)>();
        if (string.IsNullOrEmpty(content))
            return result;

        var matches = IrdFilename.Matches(content);
        foreach (Match match in matches)
        {
            if (match.Success)
                result.Add((match.Groups["product_code"].Value, match.Groups["filename"].Value));
        }
        return result;
    }
}