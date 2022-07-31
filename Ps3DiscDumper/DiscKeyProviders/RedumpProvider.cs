using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IrdLibraryClient;
using Ps3DiscDumper.Utils;

namespace Ps3DiscDumper.DiscKeyProviders
{
    public class RedumpProvider : IDiscKeyProvider
    {
        public async Task<HashSet<DiscKeyInfo>> EnumerateAsync(string discKeyCachePath, string productCode, CancellationToken cancellationToken)
        {
            var result = new HashSet<DiscKeyInfo>();
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var embeddedResources = assembly.GetManifestResourceNames().Where(n => n.Contains("Disc_Keys") || n.Contains("Disc Keys")).ToList();
                if (embeddedResources.Any())
                    Log.Trace("Loading embedded redump keys");
                else
                    Log.Warn("No embedded redump keys found");
                foreach (var res in embeddedResources)
                {
                    await using var resStream = assembly.GetManifestResourceStream(res);
                    using var zip = new ZipArchive(resStream, ZipArchiveMode.Read);
                    foreach (var zipEntry in zip.Entries.Where(e => e.Name.EndsWith(".dkey", StringComparison.InvariantCultureIgnoreCase)
                                                                    || e.Name.EndsWith(".key", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        await using var keyStream = zipEntry.Open();
                        await using var memStream = new MemoryStream();
                        await keyStream.CopyToAsync(memStream, cancellationToken).ConfigureAwait(false);
                        var discKey = memStream.ToArray();
                        if (zipEntry.Length > 256/8*2)
                        {
                            Log.Warn($"Disc key size is too big: {discKey} ({res}/{zipEntry.FullName})");
                            continue;
                        }
                        if (discKey.Length > 16)
                        {
                            discKey = Encoding.UTF8.GetString(discKey).TrimEnd().ToByteArray();
                        }

                        try
                        {
                            result.Add(new(null, discKey, zipEntry.FullName, KeyType.Redump, discKey.ToHexString()));
                        }
                        catch (Exception e)
                        {
                            Log.Warn(e, $"Invalid disc key format: {discKey}");
                        }
                    }
                }
                if (result.Any())
                    Log.Info($"Found {result.Count} embedded redump keys");
                else
                    Log.Warn($"Failed to load any embedded redump keys");
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to load embedded redump keys");
            }

            Log.Trace("Loading cached redump keys");
            var diff = result.Count;
            try
            {
                if (Directory.Exists(discKeyCachePath))
                {
                    var matchingDiskKeys = Directory.GetFiles(discKeyCachePath, "*.dkey", SearchOption.TopDirectoryOnly)
                        .Concat(Directory.GetFiles(discKeyCachePath, "*.key", SearchOption.TopDirectoryOnly));
                    foreach (var dkeyFile in matchingDiskKeys)
                    {
                        try
                        {
                            try
                            {
                                var discKey = File.ReadAllBytes(dkeyFile);
                                if (discKey.Length > 16)
                                {
                                    try
                                    {
                                        discKey = Encoding.UTF8.GetString(discKey).TrimEnd().ToByteArray();
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Warn(e, $"Failed to convert {discKey.ToHexString()} from hex to binary");
                                    }
                                }
                                result.Add(new(null, discKey, dkeyFile, KeyType.Redump, discKey.ToString()));
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
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Failed to load redump keys from local cache");
            }
            diff = result.Count - diff;
            Log.Info($"Found {diff} cached disc keys");
            return result;
        }
    }
}