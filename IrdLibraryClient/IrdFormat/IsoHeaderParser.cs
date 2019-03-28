using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscUtils.Iso9660;
using DiscUtils.Streams;

namespace IrdLibraryClient.IrdFormat
{
    public static class IsoHeaderParser
    {
        public static List<FileRecord> GetFilesystemStructure(this CDReader reader)
        {
            var filenames = reader.GetFiles(reader.Root.FullName, "*.*", SearchOption.AllDirectories).Distinct().Select(n => n.TrimStart('\\')).ToList();
            var result = new List<FileRecord>();
            foreach (var filename in filenames)
            {
                var clusterRange = reader.PathToClusters(filename);
                if (clusterRange.Length != 1)
                    Log.Warn($"{filename} is split in {clusterRange.Length} ranges");
                if (filename.EndsWith("."))
                    Log.Warn($"Fixing potential mastering error in {filename}");
                result.Add(new FileRecord(filename.TrimEnd('.'), clusterRange.Min(r => r.Offset), reader.GetFileLength(filename)));
            }
            return result.OrderBy(r => r.StartSector).ToList();
        }

        public static List<(int start, int end)> GetUnprotectedRegions(this Stream discStream)
        {
            var result = new List<(int start, int end)>();
            discStream.Seek(0, SeekOrigin.Begin);
            var reader = new BigEndianDataReader(discStream);
            var regionCount = reader.ReadInt32();
            Log.Trace($"Found {regionCount} encrypted regions");

            var unk = reader.ReadUInt32(); // 0?
            if (unk != 0)
                Log.Debug($"Unk in sector description was {unk:x16}");

            for (var i = 0; i < regionCount; i++)
            {
                var start = reader.ReadInt32();
                var end = reader.ReadInt32();
                Log.Trace($"Unprotected region: {start:x8}-{end:x8}");
                result.Add((start, end));
            }
            return result;
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