using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IrdLibraryClient;
using Ps3DiscDumper.Utils;

namespace Ps3DiscDumper.DiscKeyProviders
{
    public class RedumpProvider : IDiscKeyProvider
    {
        public async Task<HashSet<DiscKeyInfo>> EnumerateAsync(string discKeyCachePath, string ProductCode, CancellationToken cancellationToken)
        {
            var result = new HashSet<DiscKeyInfo>();

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var embeddedResources = assembly.GetManifestResourceNames().Where(n => n.Contains("Disc_Keys")).ToList();
                if (embeddedResources.Any())
                    Log.Trace("Loading embedded redump keys");
                foreach (var res in embeddedResources)
                {
                    using (var resStream = assembly.GetManifestResourceStream(res))
                    using (var zip = new ZipArchive(resStream, ZipArchiveMode.Read))
                        foreach (var zipEntry in zip.Entries.Where(e => e.Name.EndsWith(".dkey", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            if (zipEntry.Length > 256/8)
                            {
                                Log.Warn($"Disc key size is too big: {zipEntry.Length * 8} ({res}/{zipEntry.FullName})");
                                continue;
                            }

                            using (var keyStream = zipEntry.Open())
                            using (var memStream = new MemoryStream())
                            {
                                await keyStream.CopyToAsync(memStream).ConfigureAwait(false);
                                var discKey = memStream.ToArray();
                                result.Add(new DiscKeyInfo(null, discKey, zipEntry.FullName, KeyType.Redump, discKey.ToHexString()));
                            }
                        }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to load embedded redump keys");
            }

            Log.Trace("Loading cached redump keys");
            if (Directory.Exists(discKeyCachePath))
            {
                var matchingDiskKeys = Directory.GetFiles(discKeyCachePath, "*.dkey", SearchOption.TopDirectoryOnly);
                foreach (var dkeyFile in matchingDiskKeys)
                {
                    try
                    {
                        try
                        {
                            var discKey = File.ReadAllBytes(dkeyFile);
                            result.Add(new DiscKeyInfo(null, discKey, dkeyFile, KeyType.Redump, discKey.ToString()));
                        }
                        catch (InvalidDataException)
                        {
                            File.Delete(dkeyFile);
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
            return result;
        }
    }
}