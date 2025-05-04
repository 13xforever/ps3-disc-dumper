using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DiscUtils.Iso9660;
using IrdLibraryClient;
using IrdLibraryClient.IrdFormat;
using NUnit.Framework;
using Ps3DiscDumper;
using Ps3DiscDumper.Utils;

namespace Tests;

[TestFixture]
public class IrdTests
{
    private static readonly IrdClient Client = new IrdClientFreeFr();

    [TestCase("BLES00932", 12721)]
    [TestCase("BLES00238", 85)]
    public async Task FileStructureParseTest(string productCode, int expectedFileCount)
    {
        var searchResults = await Client.GetFullListAsync(CancellationToken.None).ConfigureAwait(false);
        searchResults = searchResults.Where(i => i.productCode.Equals(productCode, StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.That(searchResults, Has.Count.EqualTo(1));

        var ird = await Client.DownloadAsync(searchResults[0].irdFilename, "ird", CancellationToken.None).ConfigureAwait(false);
        Assert.That(ird, Is.Not.Null);
        Assert.That(ird.FileCount, Is.EqualTo(expectedFileCount));

        await using var decompressedStream = GetDecompressHeader(ird);
        var reader = new CDReader(decompressedStream, true, true);
        var (files, _) = await reader.GetFilesystemStructureAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.That(files, Has.Count.EqualTo(expectedFileCount));
    }

    [TestCase("BLUS31604", "0A37A83C")]
    [TestCase("BLUS30259", "4f5e86e7")]
    public async Task DiscDecryptionKeyTest(string productCode, string expectedKey)
    {
        var searchResults = await Client.GetFullListAsync(CancellationToken.None).ConfigureAwait(false);
        searchResults = searchResults.Where(i => i.productCode.Equals(productCode, StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.That(searchResults, Has.Count.EqualTo(1));

        var ird = await Client.DownloadAsync(searchResults[0].irdFilename, "ird", CancellationToken.None).ConfigureAwait(false);
        Assert.That(ird, Is.Not.Null);

        var decryptionKey = Decrypter.DecryptDiscKey(ird.Data1).ToHexString();
        Assert.That(decryptionKey, Does.StartWith(expectedKey.ToLowerInvariant()));
    }

    [Test]
    public void KeyEncryptionTest()
    {
        var randomIrdKey = new byte[16];
        var decryptedKey = Decrypter.DecryptDiscKey(randomIrdKey);
        Assert.That(randomIrdKey.ToHexString(), Is.Not.EqualTo(decryptedKey.ToHexString()));
        var encryptedKey = Decrypter.EncryptDiscKey(decryptedKey);
        Assert.That(encryptedKey.ToHexString(), Is.EqualTo(randomIrdKey.ToHexString()));
    }

    [Test, Explicit("Requires custom data")]
    public async Task TocSizeTest()
    {
        const string path = @"E:\FakeCDs\PS3 Games\ird";
        var result = new List<(string filename, long size)>();
        foreach (var f in Directory.EnumerateFiles(path, "*.ird", SearchOption.TopDirectoryOnly))
        {
            var bytes = await File.ReadAllBytesAsync(f).ConfigureAwait(false);
            var ird = IrdParser.Parse(bytes);
            await using var header = GetDecompressHeader(ird);
            result.Add((Path.GetFileName(f), header.Length));
        }
        Assert.That(result.Count, Is.GreaterThan(0));

        var groupedStats = (from t in result
                group t by t.size into g
                select new {size = g.Key, count = g.Count()}
            ).OrderByDescending(i => i.count)
            .ThenByDescending(i => i.size)
            .ToList();

        var largest = groupedStats.Max(i => i.size);
        var largestItem = result.First(i => i.size == largest);
        Console.WriteLine($"Largest TOC: {largestItem.filename} ({largest.AsStorageUnit()})");

        foreach (var s in groupedStats)
            Console.WriteLine($"{s.count} items of size {s.size}");

        Assert.That(groupedStats, Has.Count.EqualTo(1));
    }

    private static MemoryStream GetDecompressHeader(Ird ird)
    {
        var decompressedStream = new MemoryStream();
        using (var compressedStream = new MemoryStream(ird.Header, false))
        using (var gzip = new GZipStream(compressedStream, CompressionMode.Decompress))
            gzip.CopyTo(decompressedStream);
        decompressedStream.Seek(0, SeekOrigin.Begin);
        return decompressedStream;
    }
}