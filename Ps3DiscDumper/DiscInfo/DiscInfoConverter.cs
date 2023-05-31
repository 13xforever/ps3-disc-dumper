using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DiscUtils.Iso9660;
using IrdLibraryClient.IrdFormat;
using Ps3DiscDumper.Utils;

namespace Ps3DiscDumper.DiscInfo;

public static class DiscInfoConverter
{
    public static DiscInfo ToDiscInfo(this Ird ird)
    {
        List<FileRecord> fsInfo;
        var sectorSize = 2048L;
        using (var stream = new MemoryStream())
        {
            using (var headerStream = new MemoryStream(ird.Header))
            using (var gzipStream = new GZipStream(headerStream, CompressionMode.Decompress))
                gzipStream.CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new CDReader(stream, true, true);
            (fsInfo, _) = reader.GetFilesystemStructure();
            sectorSize = reader.ClusterSize;
        }
        var checksums = ird.Files.ToDictionary(f => f.Offset, f => f.Md5Checksum.ToHexString());
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
                    Size = f.Length,
                    Hashes = new()
                    {
                        ["MD5"] = checksums[f.StartSector],
                    }
                })
        };
    }
}