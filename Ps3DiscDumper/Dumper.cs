using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DiscUtils.Iso9660;
using IrdLibraryClient;
using IrdLibraryClient.IrdFormat;
using Ps3DiscDumper.DiscInfo;
using Ps3DiscDumper.Utils;
using FileInfo = System.IO.FileInfo;

namespace Ps3DiscDumper;

public sealed class Dumper : IDisposable
{
    public static readonly string Version = Assembly.GetEntryAssembly()?
                                                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                                .InformationalVersion
                                                .Split('+', 2)[0]
                                            ?? "x.y.x-unknown";
    public static readonly NameValueCollection RegionMapping = new()
    {
        ["A"] = "ASIA",
        ["E"] = "EU",
        ["H"] = "HK",
        ["J"] = "JP",
        ["K"] = "KR",
        ["P"] = "JP",
        ["T"] = "JP",
        ["U"] = "US",
    };

    internal static readonly HashSet<char> InvalidChars = [..Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars())];
    internal static readonly char[] MultilineSplit = ['\r', '\n'];

    private Decrypter? Decrypter { get; set; }
    public readonly DiscDetectionResult DiscInfo;
    public readonly KeyFinderResult DiscKeyInfo;

    public int CurrentFileNumber { get; private set; }
    public long TotalSectors { get; private set; }
    public long TotalFileSectors { get; private set; }
    public long ProcessedSectors { get; private set; }
    public List<(string filename, string error)> BrokenFiles { get; } = [];
    private CancellationTokenSource Cts { get; }
    public bool? ValidationStatus { get; private set; }

    public long CurrentSector
    {
        get
        {
            if (Decrypter?.SectorPosition is long tmp)
                return field = tmp;
            return field;
        }
    }

    public long CurrentFileSector
    {
        get
        {
            if (Decrypter?.FileSector is long tmp)
                return field = tmp;
            return field;
        }
    }

    static Dumper()
    {
        Log.Info($"PS3 Disc Dumper v{Version}");
        Log.Info($"Running on {RuntimeInformation.OSDescription}");
    }

    public Dumper(DiscDetectionResult discInfo, KeyFinderResult discKeyInfo, CancellationTokenSource cts)
    {
        if (discInfo.ProductCode is not { Length: >0 }
            || discKeyInfo.DiscKeyType is KeyType.Unknown)
            throw new InvalidOperationException("Disc was not properly scanned or the decryption key is missing");

        this.DiscInfo = discInfo;
        this.DiscKeyInfo = discKeyInfo;
        Cts = cts;
    }

    public async Task DumpAsync(string output, string? outputDirName = null)
    {
        // check and create output folder
        var dumpPath = output;
        while (dumpPath is {Length: >0} && !Directory.Exists(dumpPath))
        {
            var parent = Path.GetDirectoryName(dumpPath);
            if (parent is null || parent == dumpPath)
                dumpPath = "";
            else
                dumpPath = parent;
        }
        await using var driveStream = File.Open(DiscKeyInfo.SelectedPhysicalDevice, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var discReader = new CDReader(driveStream, true, true);
        var (filesystemStructure, emptyDirStructure) = await GetFilesystemStructureAsync(
            driveStream, discReader, Cts.Token
        ).ConfigureAwait(false);
        var filterDirList = SettingsProvider.Settings.FilterDirList;
        var prefixList = filterDirList.Select(f => f + Path.DirectorySeparatorChar).ToArray();
        if (SettingsProvider.Settings.FilterRequired)
        {
            filesystemStructure = filesystemStructure
                .Where(r => filterDirList.All(f => r.TargetFileName != f)
                            && !prefixList.Any(p => r.TargetFileName.StartsWith(p))
                ).ToList();
            emptyDirStructure = emptyDirStructure
                .Where(r => filterDirList.All(f => r.TargetDirName != f)
                            && !prefixList.Any(p => r.TargetDirName.StartsWith(p))
                ).ToList();
        }
        await Task.Yield();
        var validators = await GetValidationInfoAsync(DiscInfo, DiscKeyInfo, Cts.Token).ConfigureAwait(false);
        if (dumpPath is {Length: >0})
        {
            var fullOutputPath = Path.GetFullPath(output);
            var drive = DriveInfo.GetDrives()
                .OrderByDescending(d => d.RootDirectory.FullName.Length)
                .ThenBy(d => d.RootDirectory.FullName, StringComparer.Ordinal)
                .FirstOrDefault(d => fullOutputPath.StartsWith(d.RootDirectory.FullName));
            if (drive is not null)
            {
                var spaceAvailable = drive.AvailableFreeSpace;
#if DEBUG
                var totalFileSize = filesystemStructure.Sum(f => f.SizeInBytes);
                if (totalFileSize != DiscInfo.TotalFileSize)
                    Log.Warn($"Somehow total file size is different between initial disc scan ({DiscInfo.TotalFileSize}) and before decryption ({totalFileSize})");
#endif
                TotalFileSectors = filesystemStructure.Sum(f => f.LengthInSectors);
                var diff = DiscInfo.TotalFileSize + 100 * 1024 - spaceAvailable;
                if (diff > 0)
                    Log.Warn($"Target drive might require {diff.AsStorageUnit()} of additional free space");
            }
        }
        await Task.Yield();

        foreach (var dir in emptyDirStructure)
            Log.Trace($"Empty dir: {dir}");
        await Task.Yield();
        foreach (var file in filesystemStructure)
            Log.Trace($"0x{file.StartSector:x8}: {file.TargetFileName} ({file.SizeInBytes}, {file.FileInfo.CreationTimeUtc:u})");
        await Task.Yield();
        outputDirName ??= OutputDirFormatter.Format(DiscInfo);
        var outputPathBase = Path.Combine(output, outputDirName);
        Log.Debug($"Output path: {outputPathBase}");
        if (!Directory.Exists(outputPathBase))
            Directory.CreateDirectory(outputPathBase);

#if DEBUG
        var totalFileCount = filesystemStructure.Count;
        if (totalFileCount != DiscInfo.TotalFileCount)
            Log.Warn($"Somehow total file count is different between initial disc scan ({DiscInfo.TotalFileCount}) and  before decryption ({totalFileCount})");
#endif
        TotalSectors = discReader.TotalClusters;
        ProcessedSectors = 0;
        var matchingKey = DiscKeyInfo.AllMatchingKeys.First();
        Log.Debug("Using decryption key: " + matchingKey.KeyId);
        var decryptionKey = matchingKey.Key;
        var sectorSize = (int)discReader.ClusterSize;
        var unprotectedRegions = driveStream.GetUnprotectedRegions();
        ValidationStatus = true;

        foreach (var dir in emptyDirStructure)
        {
            try
            {
                if (Cts.IsCancellationRequested)
                    return;

                var convertedName = dir.TargetDirName;
                if (Path.DirectorySeparatorChar is not '\\')
                    convertedName = convertedName.Replace('\\', Path.DirectorySeparatorChar);
                var outputName = Path.Combine(outputPathBase, convertedName);
                if (Directory.Exists(outputName))
                    continue;
                
                Log.Debug("Creating empty directory " + outputName);
                Directory.CreateDirectory(outputName);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                BrokenFiles.Add((dir.TargetDirName, "Unexpected error: " + ex.Message));
            }
        }
        await Task.Yield();

        foreach (var file in filesystemStructure)
        {
            try
            {
                if (Cts.IsCancellationRequested)
                    return;

                Log.Info($"Reading {file.TargetFileName} ({file.SizeInBytes.AsStorageUnit()})");
                CurrentFileNumber++;
                var targetFilename = file.TargetFileName;

                var trailingPeriod = targetFilename.EndsWith('.');
                var inputFilePath = Path.Combine(DiscInfo.InputDevicePath, targetFilename);
                // BLES00932 Demon's Souls: trailing . is trimmed on both Windows and Linux
                if (trailingPeriod && !File.Exists(inputFilePath))
                {
                    Log.Warn($"Potential mastering error in {targetFilename}");
                    targetFilename = targetFilename.TrimEnd('.');
                    inputFilePath = Path.Combine(DiscInfo.InputDevicePath, targetFilename);
                }
                // BLJS92001 Tsugime Ranko: trailing . is replaced with #ABCD in Windows, but is kept as . on Linux
                if (trailingPeriod && OperatingSystem.IsWindows() && !File.Exists(inputFilePath))
                {
                    var inputDir = Path.GetDirectoryName(inputFilePath)!;
                    var testNameBase = Path.GetFileName(inputFilePath);
                    var options = new EnumerationOptions {IgnoreInaccessible = true, RecurseSubdirectories = false};
                    var lst = Directory.GetFiles(inputDir, testNameBase + "#*", options);
                    if (lst is [var match])
                    {
                        Log.Warn($"Using {match} as a match for {file.TargetFileName}");
                        inputFilePath = match;
                    }
                    else
                    {
                        Log.Error($"Found {lst.Length} potential matches for {file.TargetFileName}");
                    }
                }
                if (!File.Exists(inputFilePath))
                {
                    Log.Error($"Missing {file.TargetFileName}");
                    BrokenFiles.Add((file.TargetFileName, "missing"));
                    continue;
                }

                var outputFilePath = Path.Combine(outputPathBase, targetFilename);
                var fileDir = Path.GetDirectoryName(outputFilePath)!;
                if (!Directory.Exists(fileDir))
                {
                    Log.Debug("Creating directory " + fileDir);
                    Directory.CreateDirectory(fileDir);
                }

                bool error;
                var expectedHashes = (
                    from v in validators
                    where v.Files.ContainsKey(file.TargetFileName)
                    select v.Files[file.TargetFileName].Hashes
                ).ToList();
                var lastHash = "";
                var tries = 2;
                do
                {
                    try
                    {
                        tries--;
                        await using var outputStream = File.Open(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                        await using var inputStream = File.Open(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        await using var decrypter = new Decrypter(inputStream, driveStream, decryptionKey, file.StartSector, sectorSize, unprotectedRegions);
                        Decrypter = decrypter;
                        await decrypter.CopyToAsync(outputStream, 8 * 1024 * 1024, Cts.Token).ConfigureAwait(false);
                        await outputStream.FlushAsync();
                        error = false;

                        var resultHashes = decrypter.GetHashes();
                        var resultMd5 = resultHashes["MD5"];
                        if (decrypter is { WasEncrypted: true, WasUnprotected: true })
                            Log.Debug("Partially decrypted " + file.TargetFileName);
                        else if (decrypter.WasEncrypted)
                            Log.Debug("Decrypted " + file.TargetFileName);

                        if (expectedHashes.Count is 0)
                        {
                            if (ValidationStatus is true)
                                ValidationStatus = null;
                        }
                        else if (!IsMatch(resultHashes, expectedHashes))
                        {
                            error = true;
                            var msg = "Unexpected hash: " + resultMd5;
                            if (resultMd5 == lastHash || decrypter.LastBlockCorrupted || tries == 0)
                            {
                                Log.Error(msg);
                                BrokenFiles.Add((file.TargetFileName, "corrupted"));
                                break;
                            }
                            Log.Warn(msg + ", retrying");
                        }
                        lastHash = resultMd5;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, e.Message);
                        error = true;
                        if (tries == 0)
                        {
                            BrokenFiles.Add((file.TargetFileName, "failed to read"));
                            ValidationStatus = false;
                        }
                    }
                    await Task.Yield();
                } while (error && tries > 0 && !Cts.IsCancellationRequested);

                _ = new FileInfo(outputFilePath)
                {
                    CreationTimeUtc = file.FileInfo.CreationTimeUtc,
                    LastWriteTimeUtc = file.FileInfo.LastWriteTimeUtc
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                BrokenFiles.Add((file.TargetFileName, "Unexpected error: " + ex.Message));
                ValidationStatus = false;
            }
            ProcessedSectors += file.LengthInSectors;
        }

        Log.Info("Fixing directory modification time stamps...");
        var fullDirectoryList = filesystemStructure
            .Select(f => f.Parent)
            .Concat(emptyDirStructure)
            .Distinct()
            .OrderByDescending(d => d?.TargetDirName, StringComparer.OrdinalIgnoreCase);
        foreach (var dir in fullDirectoryList.Where(d => d is not null))
        {
            try
            {
                var targetDirPath = Path.Combine(outputPathBase, dir!.TargetDirName.TrimStart('\\'));
                _ = new DirectoryInfo(targetDirPath)
                {
                    CreationTimeUtc = dir.DirInfo.CreationTimeUtc,
                    LastWriteTimeUtc = dir.DirInfo.LastWriteTimeUtc
                };
            }
            catch (Exception e)
            {
                Log.Warn(e, $"Failed to fix timestamp for directory {dir!.TargetDirName}");
            }
        }
        if (BrokenFiles.Count is 0)
            Log.Info("Completed");
        else
            Log.Warn($"Completed with {BrokenFiles.Count} broken file{(BrokenFiles.Count is 1 ? "" : "s")}");
    }

    private static async Task<(List<FileRecord> files, List<DirRecord> dirs)> GetFilesystemStructureAsync(
        Stream driveStream,
        CDReader discReader,
        CancellationToken cancellationToken)
    {
        var pos = driveStream.Position;
        var buf = new byte[64 * 1024 * 1024];
        try
        {
            try
            {
                driveStream.Seek(0, SeekOrigin.Begin);
                await driveStream.ReadExactlyAsync(buf, 0, buf.Length, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                driveStream.Seek(pos, SeekOrigin.Begin);
            }
            using var memStream = new MemoryStream(buf, false);
            using var reader = new CDReader(memStream, true, true);
            return await reader.GetFilesystemStructureAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to buffer TOC");
        }
        return await discReader.GetFilesystemStructureAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<List<DiscInfo.DiscInfo>> GetValidationInfoAsync(
        DiscDetectionResult discInfo,
        KeyFinderResult keyInfo,
        CancellationToken cancellationToken)
    {
        List<DiscInfo.DiscInfo> discInfoList = [];
        foreach (var discKeyInfo in keyInfo.AllMatchingKeys.Where(ki => ki.KeyType is KeyType.Ird))
        {
            var ird = IrdParser.Parse(await File.ReadAllBytesAsync(discKeyInfo.FullPath, cancellationToken).ConfigureAwait(false));
            if (!string.Equals(discInfo.DiscVersion, ird.GameVersion))
                continue;

            discInfoList.Add(await ird.ToDiscInfoAsync(cancellationToken).ConfigureAwait(false));
        }
        return discInfoList;
    }

    private static bool IsMatch(Dictionary<string, string> hashes, List<Dictionary<string, List<string>>> expectedHashes)
    {
        foreach (var eh in expectedHashes)
        foreach (var h in hashes)
            if (eh.TryGetValue(h.Key, out var expectedHash) && expectedHash.Any(e => e == h.Value))
                return true;
        return false;
    }

    public void Dispose()
    {
        ((IDisposable?)Decrypter)?.Dispose();
        Cts.Dispose();
    }
}