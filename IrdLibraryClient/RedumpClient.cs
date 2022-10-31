using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IrdLibraryClient.Compression;

namespace IrdLibraryClient;

public class RedumpClient
{
    public static readonly Uri BaseUrl = new Uri("http://redump.org/keys/ps3/");
    private static Stream? LatestSnapshot = null;

    private readonly HttpClient client;

    public RedumpClient() => client = HttpClientFactory.Create(new CompressionMessageHandler());

    public async Task<Stream?> GetKeysZipContent(string localCachePath, CancellationToken cancellationToken)
    {
        if (LatestSnapshot is not null)
        {
            LatestSnapshot.Seek(0, SeekOrigin.Begin);
            return LatestSnapshot;
        }
            
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);
            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                Log.Warn($"Redump key request failed with {response.StatusCode}: {errorBody}");
                return LatestSnapshot;
            }
                
            if (response.Content.Headers.ContentDisposition?.FileName is string filename)
            {
                if (filename.StartsWith('"') && filename.EndsWith('"'))
                    filename = filename[1..^1];
                Log.Info($"Latest redump snapshot: {filename}");
                var localCacheFilename = Path.Combine(localCachePath, filename);
                try
                {
                    if (File.Exists(localCacheFilename))
                    {
                        Log.Info("Using local copy of redump key snapshot");
                        await using var file = File.Open(localCacheFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
                        var result = new MemoryStream();
                        await file.CopyToAsync(result, cancellationToken).ConfigureAwait(false);
                        LatestSnapshot = result;
                        return LatestSnapshot;
                    }
                }
                catch (Exception e)
                {
                    Log.Warn(e, "Error accessing local redump key cache: " + e.Message);
                }
                    
                try
                {
                    var resultBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
                    try
                    {
                        if (!Directory.Exists(localCachePath))
                            Directory.CreateDirectory(localCachePath);
                        Log.Info($"Saving latest redump snapshot in local cache: {filename}...");
                        await File.WriteAllBytesAsync(localCacheFilename, resultBytes, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn(ex, $"Failed to write {filename} to local cache: {ex.Message}");
                    }
                    LatestSnapshot = new MemoryStream(resultBytes);
                    return LatestSnapshot;
                }
                catch (Exception e)
                {
                    Log.Warn(e, $"Failed to download redump snapshot {filename}: {e.Message}");
                }
            }
            else
                Log.Warn("Snapshot response was in an unexpected format, ignoring");
        }
        catch (Exception e)
        {
            Log.Error(e);
        }

        Log.Info("Trying to search local cache...");
        var latestLocalCache = Directory.GetFiles(localCachePath, "Sony*Disc*Keys*.zip").MaxBy(n => n, StringComparer.OrdinalIgnoreCase);
        if (latestLocalCache is not null)
            try
            {
                var fn = Path.GetFileName(latestLocalCache);
                Log.Info($"Using latest local redump key cache: {fn}");
                await using var file = File.Open(latestLocalCache, FileMode.Open, FileAccess.Read, FileShare.Read);
                var result = new MemoryStream();
                await file.CopyToAsync(result, cancellationToken).ConfigureAwait(false);
                result.Seek(0, SeekOrigin.Begin);
                LatestSnapshot = result;
            }
            catch (Exception e)
            {
                Log.Warn(e, "Failed to read local cache file");
            }

        return LatestSnapshot;
    }
}