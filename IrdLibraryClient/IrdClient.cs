using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IrdLibraryClient.Compression;
using IrdLibraryClient.Utils;
using IrdLibraryClient.IrdFormat;
using IrdLibraryClient.POCOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IrdLibraryClient
{
    public class IrdClient
    {
        public static readonly string BaseUrl = "http://jonnysp.bplaced.net";

        private readonly HttpClient client;
        private readonly MediaTypeFormatterCollection underscoreFormatters;
        private static readonly Regex IrdFilename = new(@"ird/(?<filename>\w{4}\d{5}-[A-F0-9]+\.ird)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        public IrdClient()
        {
            client = HttpClientFactory.Create(new CompressionMessageHandler());
            var underscoreSettings = new JsonSerializerSettings
            {
                ContractResolver = new JsonContractResolver(NamingStyles.Underscore),
                NullValueHandling = NullValueHandling.Ignore
            };
            var mediaTypeFormatter = new JsonMediaTypeFormatter { SerializerSettings = underscoreSettings };
            mediaTypeFormatter.SupportedMediaTypes.Add(new("text/html"));
            underscoreFormatters = new(new[] { mediaTypeFormatter });
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
                    try
                    {
                        var responseBytes = await client.GetByteArrayAsync(requestUri).ConfigureAwait(false);
                        var result = Deserialize(responseBytes);
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
                        Log.Error(e, "Failed to make API call to IRD Library");
                        return null;
                    }
            }
            catch (Exception e)
            {
                Log.Error(e);
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
                    Log.Warn(e, "Error accessing local IRD cache: " + e.Message);
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
                        Log.Warn(ex, $"Failed to write {irdInfo.Filename} to local cache: {ex.Message}");
                    }
                }
                catch (Exception e)
                {
                    Log.Warn(e, $"Failed to download {irdInfo.Filename}: {e.Message}");
                }
                return result;
            }
            catch (Exception e)
            {
                Log.Error(e);
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
                Log.Warn("Couldn't parse IRD filename from " + html);
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

        private static SearchResult Deserialize(byte[] content)
        {
            var result = new SearchResult();
            using (var stream = new MemoryStream(content))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var json = JObject.Load(jsonReader);
                result.RecordsFiltered = (int?)json["recordsFiltered"] ?? 0;
                result.RecordsTotal = (int?)json["recordsTotal"] ?? 0;
                result.Data = new();
                foreach (JObject obj in json["data"])
                {
                    result.Data.Add(new()
                    {
                        Id = (string)obj["id"],
                        AppVersion = (string)obj["app_version"],
                        UpdateVersion = (string)obj["update_version"],
                        GameVersion = (string)obj["game_version"],
                        Title = (string)obj["title"],
                        Filename = (string)obj["filename"],
                    });
                }
            }
            return result;
        }
   }
}
