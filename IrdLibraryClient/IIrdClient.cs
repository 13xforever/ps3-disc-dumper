using IrdLibraryClient.IrdFormat;

namespace IrdLibraryClient;

public interface IIrdClient
{
    Uri BaseUri { get; }
    Task<List<(string productCode, string irdFilename)>> GetFullListAsync(CancellationToken cancellationToken);
    Task<Ird?> DownloadAsync(string irdName, string localCachePath, CancellationToken cancellationToken);
}