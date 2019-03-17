using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using DiscUtils.Iso9660;
using IrdLibraryClient;
using IrdLibraryClient.IrdFormat;
using NUnit.Framework;
using Ps3DiscDumper;
using Ps3DiscDumper.Utils;

namespace Tests
{
    [TestFixture]
    public class IrdTests
    {
        private static readonly IrdClient Client = new IrdClient();

        [TestCase("BLES00932", 12721)]
        public async Task FileStructureParseTest(string productCode, int expectedFileCount)
        {
            var searchResults = await Client.SearchAsync(productCode, CancellationToken.None).ConfigureAwait(false);
            Assert.That(searchResults?.Data?.Count, Is.EqualTo(1));

            var ird = await Client.DownloadAsync(searchResults.Data[0], "ird", CancellationToken.None).ConfigureAwait(false);
            Assert.That(ird, Is.Not.Null);
            Assert.That(ird.FileCount, Is.EqualTo(expectedFileCount));

            using (var decompressedStream = GetDecompressHeader(ird))
            {
                var reader = new CDReader(decompressedStream, true, true);
                var files = reader.GetFilesystemStructure();
                Assert.That(files.Count, Is.EqualTo(expectedFileCount));
            }
        }

        [TestCase("BLUS31604", "0A37A83C")]
        public async Task DiscDecryptionKeyTest(string productCode, string expectedKey)
        {
            var searchResults = await Client.SearchAsync(productCode, CancellationToken.None).ConfigureAwait(false);
            Assert.That(searchResults?.Data?.Count, Is.EqualTo(1));

            var ird = await Client.DownloadAsync(searchResults.Data[0], "ird", CancellationToken.None).ConfigureAwait(false);
            Assert.That(ird, Is.Not.Null);

            var decryptionKey = Decrypter.GetDecryptionKey(ird.Data1).ToHexString();
            Assert.That(decryptionKey, Does.StartWith(expectedKey.ToLowerInvariant()));
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
}