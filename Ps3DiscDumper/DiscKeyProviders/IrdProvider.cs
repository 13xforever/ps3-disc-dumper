using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IrdLibraryClient;
using IrdLibraryClient.IrdFormat;
using IrdLibraryClient.POCOs;

namespace Ps3DiscDumper.DiscKeyProviders
{
    public class IrdProvider : IDiscKeyProvider
    {
        private static readonly IrdClient Client = new();

        public async Task<HashSet<DiscKeyInfo>> EnumerateAsync(string discKeyCachePath, string ProductCode, CancellationToken cancellationToken)
        {
            ProductCode = ProductCode?.ToUpperInvariant();
            var result = new HashSet<DiscKeyInfo>();
            var knownFilenames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            Log.Trace("Searching local cache for a match...");
            if (Directory.Exists(discKeyCachePath))
            {
                var matchingIrdFiles = Directory.GetFiles(discKeyCachePath, "*.ird", SearchOption.TopDirectoryOnly);
                foreach (var irdFile in matchingIrdFiles)
                {
                    try
                    {
                        try
                        {
                            var ird = IrdParser.Parse(File.ReadAllBytes(irdFile));
                            result.Add(new(ird.Data1, null, irdFile, KeyType.Ird, ird.Crc32.ToString("x8")));
                            knownFilenames.Add(Path.GetFileName(irdFile));
                        }
                        catch (InvalidDataException)
                        {
                            File.Delete(irdFile);
                            continue;
                        }
                        catch (Exception e)
                        {
                            Log.Warn(e);
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e, e.Message);
                    }
                }
            }

            Log.Trace("Searching IRD Library for match...");
            var irdInfoList = await Client.SearchAsync(ProductCode, cancellationToken).ConfigureAwait(false);
            var irdList = irdInfoList?.Data?.Where(
                              i => !knownFilenames.Contains(i.Filename) && i.Filename.Substring(0, 9).ToUpperInvariant() == ProductCode
                          ).ToList() ?? new List<SearchResultItem>(0);
            if (irdList.Count == 0)
                Log.Debug("No matching IRD file was found in the Library");
            else
            {
                Log.Info($"Found {irdList.Count} new match{(irdList.Count == 1 ? "" : "es")} in the IRD Library");
                foreach (var irdInfo in irdList)
                {
                    var ird = await Client.DownloadAsync(irdInfo, discKeyCachePath, cancellationToken).ConfigureAwait(false);
                    result.Add(new(ird.Data1, null, Path.Combine(discKeyCachePath, irdInfo.Filename), KeyType.Ird, ird.Crc32.ToString("x8")));
                    knownFilenames.Add(irdInfo.Filename);
                }
            }
            if (knownFilenames.Count == 0)
            {
                Log.Warn("No valid matching IRD file could be found");
                Log.Info($"If you have matching IRD file, you can put it in '{discKeyCachePath}' and try dumping the disc again");
            }

            Log.Info($"Found {result.Count} IRD files");
            return result;
        }
    }
}
