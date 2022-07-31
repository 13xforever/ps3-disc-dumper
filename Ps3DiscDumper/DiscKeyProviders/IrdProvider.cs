using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IrdLibraryClient;
using IrdLibraryClient.IrdFormat;

namespace Ps3DiscDumper.DiscKeyProviders
{
    public class IrdProvider : IDiscKeyProvider
    {
        private static readonly IrdClient Client = new();

        public async Task<HashSet<DiscKeyInfo>> EnumerateAsync(string discKeyCachePath, string productCode, CancellationToken cancellationToken)
        {
            productCode = productCode.ToUpperInvariant();
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
                            var ird = IrdParser.Parse(await File.ReadAllBytesAsync(irdFile, cancellationToken).ConfigureAwait(false));
                            result.Add(new(ird.Data1, null, irdFile, KeyType.Ird, ird.Crc32.ToString("x8")));
                            knownFilenames.Add(Path.GetFileName(irdFile));
                        }
                        catch (InvalidDataException)
                        {
                            File.Delete(irdFile);
                        }
                        catch (Exception e)
                        {
                            Log.Warn(e);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e, e.Message);
                    }
                }
            }

            Log.Trace("Searching IRD Library for match...");
            var irdNameList = await Client.GetFullListAsync(cancellationToken).ConfigureAwait(false);
            var irdList = irdNameList.Where(i => !knownFilenames.Contains(i) && i.StartsWith(productCode, StringComparison.OrdinalIgnoreCase)).ToList();
            if (irdList.Count == 0)
                Log.Debug("No matching IRD file was found in the Library");
            else
            {
                Log.Info($"Found {irdList.Count} new match{(irdList.Count == 1 ? "" : "es")} in the IRD Library");
                foreach (var irdName in irdList)
                {
                    var ird = await Client.DownloadAsync(irdName, discKeyCachePath, cancellationToken).ConfigureAwait(false);
                    result.Add(new(ird.Data1, null, Path.Combine(discKeyCachePath, irdName), KeyType.Ird, ird.Crc32.ToString("x8")));
                    knownFilenames.Add(irdName);
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
