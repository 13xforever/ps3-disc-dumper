using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscUtils.Iso9660;
using DiscUtils.Streams;

namespace IrdLibraryClient.IrdFormat
{
    public static class IsoHeaderParser
    {
        public static (List<FileRecord> files, List<string> dirs) GetFilesystemStructure(this CDReader reader)
        {
            var fsObjects = reader.GetFileSystemEntries(reader.Root.FullName).ToList();
            var nextLevel = new List<string>();
            var filePaths = new List<string>();
            var dirPaths = new List<string>();
            while (fsObjects.Any())
            {
                foreach (var path in fsObjects)
                {
                    if (reader.FileExists(path))
                        filePaths.Add(path);
                    else if (reader.DirectoryExists(path))
                    {
                        dirPaths.Add(path);
                        nextLevel.AddRange(reader.GetFileSystemEntries(path));
                    }
                    else
                        Log.Warn($"Unknown filesystem object: {path}");
                }
                (fsObjects, nextLevel) = (nextLevel, fsObjects);
                nextLevel.Clear();
            }
            
            var filenames = filePaths.Distinct().Select(n => n.TrimStart('\\')).ToList();
            var dirnames = dirPaths.Distinct().Select(n => n.TrimStart('\\')).OrderByDescending(n => n.Length).ToList();
            var deepestDirnames = new List<string>();
            foreach (var dirname in dirnames)
            {
                var tmp = dirname + "\\";
                if (deepestDirnames.Any(n => n.StartsWith(tmp)))
                    continue;
                
                deepestDirnames.Add(dirname);
            }
            dirnames = deepestDirnames.OrderBy(n => n).ToList();
            var dirnamesWithFiles = filenames.Select(Path.GetDirectoryName).Distinct().ToList();
            var emptydirs = dirnames.Except(dirnamesWithFiles).ToList();

            var fileList = new List<FileRecord>();
            foreach (var filename in filenames)
            {
                var clusterRange = reader.PathToClusters(filename);
                if (clusterRange.Length != 1)
                    Log.Warn($"{filename} is split in {clusterRange.Length} ranges");
                if (filename.EndsWith("."))
                    Log.Warn($"Fixing potential mastering error in {filename}");
                fileList.Add(new FileRecord(filename.TrimEnd('.'), clusterRange.Min(r => r.Offset), reader.GetFileLength(filename)));
            }
            fileList = fileList.OrderBy(r => r.StartSector).ToList();
            return (files: fileList, dirs: emptydirs);
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