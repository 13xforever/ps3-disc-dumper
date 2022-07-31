using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IrdLibraryClient.Compression;
using IrdLibraryClient.IrdFormat;

namespace IrdLibraryClient
{
    public class IrdClient
    {
        public static readonly string BaseUrl = "https://ps3.aldostools.org/ird/";

        private readonly HttpClient client;
        private static readonly Regex IrdFilename = new(@"href=""(?<filename>\w{4}\d{5}-[A-F0-9]+\.ird)""", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        public IrdClient() => client = HttpClientFactory.Create(new CompressionMessageHandler());

        public static string GetDownloadLink(string irdFilename) => $"{BaseUrl}{irdFilename}";

        public async Task<List<string>> GetFullListAsync(CancellationToken cancellationToken)
        {
            try
            {
                var requestUri = new Uri(BaseUrl).SetQueryParameters(new Dictionary<string, string>
                    {
                        ["F"] = "0",
                        //["P"] = query.Trim(100),

                        ["_"] = DateTime.UtcNow.Ticks.ToString(),
                    });
                    try
                    {
                        var responseData = await client.GetStringAsync(requestUri, cancellationToken).ConfigureAwait(false);
                        return Deserialize(responseData);
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e, "Failed to make API call to IRD Library");
                        return new();
                    }
            }
            catch (Exception e)
            {
                Log.Error(e);
                return new();
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
                        return IrdParser.Parse(await File.ReadAllBytesAsync(localCacheFilename, cancellationToken).ConfigureAwait(false));
                }
                catch (Exception e)
                {
                    Log.Warn(e, "Error accessing local IRD cache: " + e.Message);
                }
                try
                {
                    var resultBytes = await client.GetByteArrayAsync(GetDownloadLink(irdName), cancellationToken).ConfigureAwait(false);
                    result = IrdParser.Parse(resultBytes);
                    try
                    {
                        if (!Directory.Exists(localCachePath))
                            Directory.CreateDirectory(localCachePath);
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

        private static List<string> Deserialize(string? content)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(content))
                return result;

            var matches = IrdFilename.Matches(content);
            foreach (Match match in matches)
            {
                if (match.Success)
                    result.Add(match.Groups["filename"].Value);
            }
            return result;
        }
   }
}
