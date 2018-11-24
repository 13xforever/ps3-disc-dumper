using System.Threading;
using System.Threading.Tasks;
using IrdLibraryClient;
using IrdLibraryClient.IrdFormat;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class IrdTests
    {
        private static readonly IrdClient Client = new IrdClient();

        [TestCase("BLES00932", 12721)]
        public async Task FileStructureParseTest(string productCode, int expectedFileCount)
        {
            var searchResults = await Client.SearchAsync("BLES00932", CancellationToken.None).ConfigureAwait(false);
            Assert.That(searchResults?.Data?.Count, Is.EqualTo(1));

            var ird = await Client.DownloadAsync(searchResults.Data[0], "ird", CancellationToken.None).ConfigureAwait(false);
            Assert.That(ird, Is.Not.Null);
            Assert.That(ird.FileCount, Is.EqualTo(expectedFileCount));

            var files = ird.GetFilesystemStructure();
            Assert.That(files.Count, Is.EqualTo(expectedFileCount));
        }
    }
}