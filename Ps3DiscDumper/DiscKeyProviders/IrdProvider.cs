using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IrdLibraryClient;
using IrdLibraryClient.IrdFormat;

namespace Ps3DiscDumper.DiscKeyProviders;

public class IrdProvider : IDiscKeyProvider
{
    private static readonly IIrdClient[] Clients = [
        new IrdClientFlexby420(),
        new IrdClientFreeFr(),
        new IrdClientAldos(),
    ];

    public async Task<HashSet<DiscKeyInfo>> EnumerateAsync(string discKeyCachePath, string productCode, CancellationToken cancellationToken)
    {
        productCode = productCode.ToUpperInvariant();
        var result = new HashSet<DiscKeyInfo>();
        var knownFilenames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        Log.Trace("Searching local cache for a match...");
        if (Directory.Exists(discKeyCachePath))
        {
            var matchingIrdFiles = Directory.GetFiles(discKeyCachePath, "*.ird", new EnumerationOptions{RecurseSubdirectories = true, MaxRecursionDepth = 2, IgnoreInaccessible = true,});
            foreach (var irdFile in matchingIrdFiles)
            {
                try
                {
                    try
                    {
                        var ird = IrdParser.Parse(await File.ReadAllBytesAsync(irdFile, cancellationToken).ConfigureAwait(false));
                        result.Add(new(ird.Data1, null, irdFile, KeyType.Ird, ird.Crc32.ToString("x8")));
                        knownFilenames.Add(Path.GetRelativePath(discKeyCachePath, irdFile).Replace('\\', '/'));
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

        Log.Trace($"Searching for IRDs using {Clients.Length} source{(Clients.Length == 1 ? "" : "s")} for a match...");
        var searchTasks = new Task<List<(string productCode, string irdFilename)>>[Clients.Length];
        for (var i = 0; i < Clients.Length; i++)
            searchTasks[i] = Clients[i].GetFullListAsync(cancellationToken);
        var irdList = new List<(IIrdClient client, string irdFilename)>();
        for (var i = 0; i < Clients.Length; i++)
        {
            var client = Clients[i];
            try
            {
                var irdNameList = await searchTasks[i].ConfigureAwait(false);
                var filteredInfo = irdNameList.Where(v =>
                    !knownFilenames.Contains(v.irdFilename) &&
                    v.productCode.Equals(productCode, StringComparison.OrdinalIgnoreCase)
                ).Select(v => (client, v.irdFilename));
                irdList.AddRange(filteredInfo);
            }
            catch (Exception e)
            {
                Log.Warn(e, $"Failed to get IRD list from {client.BaseUri.Host}");
            }
        }
        if (irdList.Count == 0)
            Log.Debug("No new matching IRD file was found at any source");
        else
        {
            Log.Info($"Found {irdList.Count} new IRD match{(irdList.Count == 1 ? "" : "es")}");
            foreach (var (client, irdName) in irdList)
            {
                if (knownFilenames.Contains(irdName))
                {
                    Log.Debug($"Skipping {irdName} download from {client.BaseUri.Host}...");
                    continue;
                }
                
                try
                {
                    Log.Debug($"Trying to download {irdName} from {client.BaseUri.Host}...");
                    var ird = await client.DownloadAsync(irdName, discKeyCachePath, cancellationToken).ConfigureAwait(false);
                    result.Add(new(ird.Data1, null, Path.Combine(discKeyCachePath, irdName), KeyType.Ird, ird.Crc32.ToString("x8")));
                    knownFilenames.Add(irdName);
                }
                catch (Exception e)
                {
                    Log.Warn(e, $"Failed to download {irdName} from {client.BaseUri.Host}");
                }
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