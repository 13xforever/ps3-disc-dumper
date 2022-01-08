using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscUtils;
using DiscUtils.Iso9660;
using DiscUtils.Streams;

namespace IrdLibraryClient.IrdFormat
{
    public static class IsoHeaderParser
    {
        public static (List<FileRecord> files, List<DirRecord> dirs) GetFilesystemStructure(this CDReader reader)
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
            
            var filenames = filePaths.Distinct().ToList();
            var dirNames = dirPaths.Distinct().OrderByDescending(n => n.Length).ToList();
            var deepestDirNames = new List<string>();
            foreach (var dirname in dirNames)
            {
                var tmp = dirname + "\\";
                if (deepestDirNames.Any(n => n.StartsWith(tmp)))
                    continue;
                
                deepestDirNames.Add(dirname);
            }
            dirNames = deepestDirNames.OrderBy(n => n).ToList();
            var dirNamesWithFiles = filenames.Select(Path.GetDirectoryName).Distinct().ToList();
            var dirList = dirNames.Except(dirNamesWithFiles)
                .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
                .Select(dir => (dir: dir!.TrimStart('\\').Replace('\\', Path.DirectorySeparatorChar), info: reader.GetDirectoryInfo(dir)))
                .Select(di => new DirRecord(di.dir, new(di.info.CreationTimeUtc, di.info.LastWriteTimeUtc)))
                .ToList();

            var fileList = new List<FileRecord>();
            foreach (var filename in filenames)
            {
                var targetFilename = filename.TrimStart('\\');
                if (targetFilename.EndsWith("."))
                {
                    Log.Warn($"Fixing potential mastering error in {filename}");
                    targetFilename = targetFilename.TrimEnd('.');
                }
                targetFilename = targetFilename.Replace('\\', Path.DirectorySeparatorChar);
                var clusterRange = reader.PathToClusters(filename);
                if (clusterRange.Length != 1)
                    Log.Warn($"{targetFilename} is split in {clusterRange.Length} ranges");
                var startSector = clusterRange.Min(r => r.Offset);
                var length = reader.GetFileLength(filename);
                var fileInfo = reader.GetFileSystemInfo(filename);
                var recordInfo = new FileRecordInfo(fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc);
                var parent = fileInfo.Parent;
                var parentInfo = new DirRecord(parent.FullName.TrimStart('\\').Replace('\\', Path.DirectorySeparatorChar), new(parent.CreationTimeUtc, parent.LastWriteTimeUtc));
                fileList.Add(new(targetFilename, startSector, length, recordInfo, parentInfo));
            }
            fileList = fileList.OrderBy(r => r.StartSector).ToList();
            
            
            return (files: fileList, dirs: dirList);
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

    public record FileRecord(string TargetFileName, long StartSector, long Length, FileRecordInfo FileInfo, DirRecord Parent);

    public record FileRecordInfo(DateTime CreationTimeUtc, DateTime LastWriteTimeUtc);

    public record DirRecord(string TargetDirName, FileRecordInfo DirInfo);
}