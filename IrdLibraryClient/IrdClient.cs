using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IrdLibraryClient.Compression;
using IrdLibraryClient.Utils;
using IrdLibraryClient.IrdFormat;
using IrdLibraryClient.POCOs;
using Newtonsoft.Json;

namespace IrdLibraryClient
{
    public class IrdClient
    {
        public static readonly string BaseUrl = "http://jonnysp.bplaced.net";

        private readonly HttpClient client;
        private readonly MediaTypeFormatterCollection underscoreFormatters;
        private static readonly Regex IrdFilename = new Regex(@"ird/(?<filename>\w{4}\d{5}-[A-F0-9]+\.ird)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        public IrdClient()
        {
            client = HttpClientFactory.Create(new CompressionMessageHandler());
            var underscoreSettings = new JsonSerializerSettings
            {
                ContractResolver = new JsonContractResolver(NamingStyles.Underscore),
                NullValueHandling = NullValueHandling.Ignore
            };
            var mediaTypeFormatter = new JsonMediaTypeFormatter { SerializerSettings = underscoreSettings };
            mediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            underscoreFormatters = new MediaTypeFormatterCollection(new[] { mediaTypeFormatter });
        }

        public static string GetDownloadLink(string irdFilename) => $"{BaseUrl}/ird/{irdFilename}";
        public static string GetInfoLink(string irdFilename) => $"{BaseUrl}/info.php?file=ird/{irdFilename}";

        public async Task<SearchResult> SearchAsync(string query, CancellationToken cancellationToken)
        {
            try
            {
                var requestUri = new Uri(BaseUrl + "/data.php").SetQueryParameters(new Dictionary<string, string>
                    {
                        ["draw"] = query.Length.ToString(),

                        ["columns[0][data]"] = "id",
                        ["columns[0][name]"] = "",
                        ["columns[0][searchable]"] = "true",
                        ["columns[0][orderable]"] = "true",
                        ["columns[0][search][value]"] = "",
                        ["columns[0][search][regex]"] = "false",

                        ["columns[1][data]"] = "title",
                        ["columns[1][name]"] = "",
                        ["columns[1][searchable]"] = "true",
                        ["columns[1][orderable]"] = "true",
                        ["columns[1][search][value]"] = "",
                        ["columns[1][search][regex]"] = "false",

                        ["order[0][column]"] = "0",
                        ["order[0][dir]"] = "asc",

                        ["start"] = "0",
                        ["length"] = "10",

                        ["search[value]"] = query.Trim(100),

                        ["_"] = DateTime.UtcNow.Ticks.ToString(),
                    });
                using (var getMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
                using (var response = await client.SendAsync(getMessage, cancellationToken).ConfigureAwait(false))
                    try
                    {
                        await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
                        var result = await response.Content.ReadAsAsync<SearchResult>(underscoreFormatters, cancellationToken).ConfigureAwait(false);
                        result.Data = result.Data ?? new List<SearchResultItem>(0);
                        foreach (var item in result.Data)
                        {
                            item.Filename = GetIrdFilename(item.Filename);
                            item.Title = GetTitle(item.Title);
                        }
                        return result;
                    }
                    catch (Exception e)
                    {
                        ConsoleLogger.PrintError(e, response);
                        return null;
                    }
            }
            catch (Exception e)
            {
                ApiConfig.Log.Error(e);
                return null;
            }
        }

        public async Task<Ird> DownloadAsync(SearchResultItem irdInfo, string localCachePath, CancellationToken cancellationToken)
        {
            Ird result = null;
            try
            {
                var localCacheFilename = Path.Combine(localCachePath, irdInfo.Filename);
                // first we search local cache and try to load whatever data we can
                try
                {
                    if (File.Exists(localCacheFilename))
                        return IrdParser.Parse(File.ReadAllBytes(localCacheFilename));
                }
                catch (Exception e)
                {
                    ApiConfig.Log.Warn(e, "Error accessing local IRD cache: " + e.Message);
                }
                try
                {
                    var resultBytes = await client.GetByteArrayAsync(GetDownloadLink(irdInfo.Filename)).ConfigureAwait(false);
                    result = IrdParser.Parse(resultBytes);
                    try
                    {
                        if (!Directory.Exists(localCachePath))
                            Directory.CreateDirectory(localCachePath);
                        File.WriteAllBytes(localCacheFilename, resultBytes);
                    }
                    catch (Exception ex)
                    {
                        ApiConfig.Log.Warn(ex, $"Failed to write {irdInfo.Filename} to local cache: {ex.Message}");
                    }
                }
                catch (Exception e)
                {
                    ApiConfig.Log.Warn(e, $"Failed to download {irdInfo.Filename}: {e.Message}");
                }
                return result;
            }
            catch (Exception e)
            {
                ApiConfig.Log.Error(e);
                return result;
            }
        }

        private static string GetIrdFilename(string html)
        {
            if (string.IsNullOrEmpty(html))
                return null;

            var matches = IrdFilename.Matches(html);
            if (matches.Count == 0)
            {
                ApiConfig.Log.Warn("Couldn't parse IRD filename from " + html);
                return null;
            }

            return matches[0].Groups["filename"]?.Value;
        }

        private static string GetTitle(string html)
        {
            if (string.IsNullOrEmpty(html))
                return null;

            var idx = html.LastIndexOf("</span>");
            var result = html.Substring(idx + 7).Trim();
            if (string.IsNullOrEmpty(result))
                return null;

            return result;
        }
   }
}
