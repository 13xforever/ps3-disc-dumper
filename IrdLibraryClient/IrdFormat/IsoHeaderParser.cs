using DiscUtils.Iso9660;
using DiscUtils.Streams;

namespace IrdLibraryClient.IrdFormat;

public static class IsoHeaderParser
{
    public static async Task<(List<FileRecord> files, List<DirRecord> dirs)> GetFilesystemStructureAsync(this CDReader reader, CancellationToken cancellationToken)
    {
        Log.Debug("Scanning filesystem structure…");
        var fsObjects = reader.GetFileSystemEntries(reader.Root.FullName).ToList();
        var nextLevel = new List<string>();
        var filePaths = new List<string>(20_000);
        var dirPaths = new List<string>();
        var cnt = 0;
        while (fsObjects.Count > 0)
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
                if (++cnt <= 200)
                    continue;
                
                await Task.Yield();
                cnt = 0;
            }
            (fsObjects, nextLevel) = (nextLevel, fsObjects);
            nextLevel.Clear();
        }
            
        var filenames = filePaths.Distinct().ToList();
        var dirNames = dirPaths.Distinct().OrderByDescending(n => n.Length).ToList();
        var dirNamesWithFiles = filenames.Select(Path.GetDirectoryName).Distinct().ToList();
        var dirList = dirNames.Except(dirNamesWithFiles)
            .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
            .Select(dir => (dir: dir!.TrimStart('\\').Replace('\\', Path.DirectorySeparatorChar), info: reader.GetDirectoryInfo(dir)))
            .Select(di => new DirRecord(di.dir, new(di.info.CreationTimeUtc, di.info.LastWriteTimeUtc)))
            .ToList();

        Log.Debug("Building file cluster map…");
        var fileList = new List<FileRecord>();
        foreach (var filename in filenames)
        {
            var targetFilename = filename.TrimStart('\\');
            if (targetFilename.EndsWith('.'))
            {
                Log.Warn($"Fixing potential mastering error in {filename}");
                targetFilename = targetFilename.TrimEnd('.');
            }
            targetFilename = targetFilename.Replace('\\', Path.DirectorySeparatorChar);
            var clusterRange = reader.PathToClusters(filename).ToList();
            if (clusterRange.Count != 1)
                Log.Warn($"{targetFilename} is split in {clusterRange.Count} ranges");
            var startSector = clusterRange.Min(r => r.Offset);
            var lengthInSectors = clusterRange.Sum(r => r.Count);
            var length = reader.GetFileLength(filename);
            var fileInfo = reader.GetFileSystemInfo(filename);
            var recordInfo = new FileRecordInfo(fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc);
            var parent = fileInfo.Parent;
            var parentInfo = new DirRecord(parent.FullName.TrimStart('\\').Replace('\\', Path.DirectorySeparatorChar), new(parent.CreationTimeUtc, parent.LastWriteTimeUtc));
            fileList.Add(new(targetFilename, startSector, lengthInSectors, length, recordInfo, parentInfo));
            if (++cnt <= 200)
                continue;
            
            await Task.Yield();
            cnt = 0;
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

public record FileRecord(string TargetFileName, long StartSector, long LengthInSectors, long SizeInBytes, FileRecordInfo FileInfo, DirRecord Parent);

public record FileRecordInfo(DateTime CreationTimeUtc, DateTime LastWriteTimeUtc);

public record DirRecord(string TargetDirName, FileRecordInfo DirInfo);