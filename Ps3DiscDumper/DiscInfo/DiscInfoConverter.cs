using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscUtils.Iso9660;
using IrdLibraryClient;
using IrdLibraryClient.IrdFormat;
using Ps3DiscDumper.Utils;

namespace Ps3DiscDumper.DiscInfo;

public static class DiscInfoConverter
{
    public static async Task<DiscInfo> ToDiscInfoAsync(this Ird ird, CancellationToken cancellationToken)
    {
        List<FileRecord> fsInfo;
        var sectorSize = 2048L;
        using (var stream = new MemoryStream())
        {
            using (var headerStream = new MemoryStream(ird.Header))
            await using (var gzipStream = new GZipStream(headerStream, CompressionMode.Decompress))
                await gzipStream.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new CDReader(stream, true, true);
            (fsInfo, _) = await reader.GetFilesystemStructureAsync(cancellationToken).ConfigureAwait(false);
            sectorSize = reader.ClusterSize;
        }
        var checksums = new Dictionary<long, List<string>>(ird.FileCount);
#if DEBUG
        Log.Debug("IRD checksum data:");
#endif        
        foreach (var irdFileInfo in ird.Files)
        {
            if (!checksums.TryGetValue(irdFileInfo.Offset, out var csList))
                checksums[irdFileInfo.Offset] = csList = new(1);
            csList.Add(irdFileInfo.Md5Checksum.ToHexString());
#if DEBUG
            Log.Debug($"{irdFileInfo.Offset,8} (0x{irdFileInfo.Offset:x8}): {irdFileInfo.Md5Checksum.ToHexString()}");
#endif        
        }
        return new()
        {
            ProductCode = ird.ProductCode,
            DiscVersion = ird.GameVersion,
            DiscKeyRawData = ird.Data1.ToHexString(),
            DiscKey = Decrypter.DecryptDiscKey(ird.Data1).ToHexString(),
            Files = fsInfo.ToDictionary(
                f => f.TargetFileName,
                f => new FileInfo
                {
                    Offset = f.StartSector * sectorSize,
                    Size = f.SizeInBytes,
                    Hashes = new()
                    {
                        ["MD5"] = checksums[f.StartSector],
                    }
                })
        };
    }
}