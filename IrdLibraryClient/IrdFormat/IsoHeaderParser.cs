using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DiscUtils.Iso9660;
using DiscUtils.Streams;

namespace IrdLibraryClient.IrdFormat
{
    public static class IsoHeaderParser
    {
        public static List<FileRecord> GetFilesystemStructure(this Ird ird)
        {
            using (var decompressedStream = new MemoryStream())
            {
                using (var compressedStream = new MemoryStream(ird.Header, false))
                using (var gzip = new GZipStream(compressedStream, CompressionMode.Decompress))
                    gzip.CopyTo(decompressedStream);

                decompressedStream.Seek(0, SeekOrigin.Begin);
                var reader = new CDReader(decompressedStream, true, true);
                var filenames = reader.GetFiles(reader.Root.FullName, "*.*", SearchOption.AllDirectories).Distinct().Select(n => n.TrimStart('\\')).ToList();
                var result = new List<FileRecord>();
                foreach (var filename in filenames)
                {
                    var clusterRange = reader.PathToClusters(filename);
                    if (clusterRange.Length != 1)
                    {
                        ApiConfig.Log.Warn($"{filename} is split in {clusterRange.Length} ranges");
                    }
                    result.Add(new FileRecord(filename, clusterRange.Min(r => r.Offset), reader.GetFileLength(filename)));
                }
                return result.OrderBy(r => r.StartSector).ToList();
            }
        }

        public static int GetSectorSize(this Ird ird)
        {
            using (var decompressedStream = new MemoryStream())
            {
                using (var compressedStream = new MemoryStream(ird.Header, false))
                using (var gzip = new GZipStream(compressedStream, CompressionMode.Decompress))
                    gzip.CopyTo(decompressedStream);

                decompressedStream.Seek(0, SeekOrigin.Begin);
                var reader = new CDReader(decompressedStream, true, true);
                ApiConfig.Log.Trace($"Sector size: {reader.ClusterSize}");
                return (int)reader.ClusterSize;
            }
        }

        public static List<(int start, int end)> GetUnprotectedRegions(this Ird ird)
        {
            var result = new List<(int start, int end)>();
            using (var decompressedStream = new MemoryStream())
            {
                using (var compressedStream = new MemoryStream(ird.Header, false))
                using (var gzip = new GZipStream(compressedStream, CompressionMode.Decompress))
                    gzip.CopyTo(decompressedStream);

                decompressedStream.Seek(0, SeekOrigin.Begin);
                var reader = new BigEndianDataReader(decompressedStream);
                var regionCount = reader.ReadInt32();
                ApiConfig.Log.Trace($"Found {regionCount} encrypted regions");

                var unk = reader.ReadUInt32(); // 0?
                if (unk != 0)
                    ApiConfig.Log.Debug($"Unk in sector description was {unk:x16}");

                for (var i = 0; i < regionCount; i++)
                {
                    var start = reader.ReadInt32();
                    var end = reader.ReadInt32();
                    ApiConfig.Log.Trace($"Unprotected region: {start:x8}-{end:x8}");
                    result.Add((start, end));
                }
            }
            return result;
        }

        public static byte[] GetFirstSector(this Ird ird, int sectorSize)
        {
            using (var decompressedStream = new MemoryStream())
            {
                using (var compressedStream = new MemoryStream(ird.Header, false))
                using (var gzip = new GZipStream(compressedStream, CompressionMode.Decompress))
                    gzip.CopyTo(decompressedStream);

                decompressedStream.Seek(0, SeekOrigin.Begin);
                decompressedStream.SetLength(sectorSize);
                return decompressedStream.ToArray();
            }
        }

        public static long GetTotalSectors(this Ird ird)
        {
            using (var decompressedStream = new MemoryStream())
            {
                using (var compressedStream = new MemoryStream(ird.Header, false))
                using (var gzip = new GZipStream(compressedStream, CompressionMode.Decompress))
                    gzip.CopyTo(decompressedStream);

                decompressedStream.Seek(0, SeekOrigin.Begin);
                var reader = new CDReader(decompressedStream, true, true);
                return reader.TotalClusters;
            }
        }
    }

    public class FileRecord
    {
        public FileRecord(string filename, long startSector, long length)
        {
            Filename = filename;
            StartSector = startSector;
            Length = length;
        }

        public string Filename { get; }
        public long StartSector { get; }
        public long Length { get; }
    }
}