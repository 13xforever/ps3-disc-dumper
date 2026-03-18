using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DiscUtils.Iso9660;
using IrdLibraryClient;
using IrdLibraryClient.IrdFormat;
using Ps3DiscDumper.DiscInfo;
using Ps3DiscDumper.DiscKeyProviders;
using Ps3DiscDumper.POCOs;
using Ps3DiscDumper.Sfb;
using Ps3DiscDumper.Sfo;
using Ps3DiscDumper.Utils;
using Ps3DiscDumper.Utils.MacOS;
using WmiLight;
using FileInfo = System.IO.FileInfo;

namespace Ps3DiscDumper;

public partial class Dumper : IDisposable
{
    public static readonly string Version = Assembly.GetEntryAssembly()?
                                                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                                .InformationalVersion
                                                .Split('+', 2)[0]
                                            ?? "x.y.x-unknown";

    static Dumper()
    {
        Log.Info($"PS3 Disc Dumper v{Version}");
        Log.Info($"Running on {RuntimeInformation.OSDescription}");
    }

    [GeneratedRegex(@"(?<ver>\d+(\.\d+){0,2})[ \-]*(?<pre>.*)", RegexOptions.Singleline | RegexOptions.ExplicitCapture)]
    private static partial Regex VersionParts();
    [GeneratedRegex(@"Host: .+$\s*Vendor: (?<vendor>.+?)\s* Model: (?<model>.+?)\s* Rev: (?<revision>.+)$\s*Type: \s*(?<type>.+?)\s* ANSI  ?SCSI revision: (?<scsi_rev>.+?)\s*$", RegexOptions.Multiline | RegexOptions.ExplicitCapture)]
    private static partial Regex ScsiInfoParts();

    private static readonly HashSet<char> InvalidChars = [.. Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars())];
    private static readonly char[] MultilineSplit = ['\r', '\n'];
    private long currentSector, fileSector;
    private static readonly IDiscKeyProvider[] DiscKeyProviders = [new IrdProvider(), new RedumpProvider()];
    private static readonly Dictionary<string, HashSet<DiscKeyInfo>> AllKnownDiscKeys = new();
    private readonly HashSet<string> TestedDiscKeys = [];
    private byte[] discSfbData;
    private static readonly Dictionary<string, ImmutableArray<byte>> Detectors = new()
    {
        [@"\PS3_GAME\LICDIR\LIC.DAT"] = [.. "PS3LICDA"u8],
        [@"\PS3_GAME\USRDIR\EBOOT.BIN"] = [.. "SCE"u8, 0, 0, 0, 0, 2],
    };
    private byte[] detectionSector;
    private ImmutableArray<byte> detectionBytesExpected;
    private byte[] sectorIV;
    private Stream driveStream;
    private Stream sourceStream;
    private static readonly byte[] Iso9660PrimaryVolumeDescriptorHeader = [0x01, 0x43, 0x44, 0x30, 0x30, 0x31, 0x01, 0x00];
    private const string DiscSfbPath = "PS3_DISC.SFB";
    private const string ParamSfoPath = @"PS3_GAME\PARAM.SFO";
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

    public ParamSfo ParamSfo { get; private set; }
    public string ProductCode { get; private set; }
    public string DiscVersion { get; private set; }
    public string Title { get; private set; }
    public string OutputDir { get; private set; }
    public char Drive { get; set; }
    public string SelectedPhysicalDevice { get; private set; }
    public string InputDevicePath { get; private set; }
    public string InputIsoPath { get; private set; }
    public bool InputIsIso => !string.IsNullOrEmpty(InputIsoPath);
    public bool HasBdmv { get; private set; }
    private List<FileRecord> filesystemStructure;
    private List<DirRecord> emptyDirStructure;
    private CDReader discReader;
    private HashSet<DiscKeyInfo> allMatchingKeys;
    public KeyType DiscKeyType { get; set; }
    public string DiscKeyFilename { get; set; }
    private Decrypter Decrypter { get; set; }
    public int TotalFileCount { get; private set; }
    public int CurrentFileNumber { get; private set; }
    public long TotalSectors { get; private set; }
    public long TotalFileSectors { get; private set; }
    public long ProcessedSectors { get; private set; }
    public long TotalFileSize { get; private set; }
    public long TotalDiscSize { get; private set; }
    public long SectorSize { get; private set; }
    private List<string> DiscFilenames { get; set; }
    public List<(string filename, string error)> BrokenFiles { get; } = [];
    public HashSet<DiscKeyInfo> ValidatingDiscKeys { get; } = [];
    public CancellationTokenSource Cts { get; private set; } = new();
    public bool? ValidationStatus { get; private set; }
    public bool IsIvInitialized => sectorIV?.Length == 16;

    public long CurrentSector
    {
        get
        {
            var tmp = Decrypter?.SectorPosition;
            if (tmp is null)
                return currentSector;
            return currentSector = tmp.Value;
        }
    }

    public long CurrentFileSector
    {
        get
        {
            var tmp = Decrypter?.FileSector;
            if (tmp == null)
                return fileSector;
            return fileSector = tmp.Value;
        }
    }

    [SupportedOSPlatform("windows")]
    private List<string> EnumeratePhysicalDrivesWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new NotImplementedException("This should never happen, shut up msbuild");

        var physicalDriveList = new List<string>();
        try
        {
            // there's no direct mapping from this to device path, I've seen logs with identical logicalUnit for different drives
            using var wmiConnection = new WmiConnection();
            var drives = wmiConnection.CreateQuery("SELECT * FROM Win32_CDROMDrive");
            foreach (var drive in drives)
            {
                // Name and Caption are the same, so idk if they can be different
                var logicalUnit = drive["SCSILogicalUnit"]?.ToString();
                var physicalDrive = $@"\\.\CDROM{logicalUnit}";
                Log.Info($"Found optical media drive {drive["Name"]} ({drive["Drive"]}; {physicalDrive})");
                physicalDriveList.Add(physicalDrive);
            }
            var curCount = physicalDriveList.Count;
            Log.Info($"Found {curCount} cdrom drvie device{(curCount is 1 ? "" : "s")}");

            drives = wmiConnection.CreateQuery("SELECT * FROM Win32_PhysicalMedia");
            foreach (var drive in drives)
            {
                if (drive["Tag"] is string tag
                    && tag.StartsWith(@"\\.\CDROM"))
                    physicalDriveList.Add(tag);
            }
            curCount = physicalDriveList.Count - curCount;
            Log.Info($"Found {curCount} physical media device{(curCount is 1 ? "" : "s")}");
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to enumerate physical media drives using WMI");
            for (var i = 0; i < 32; i++)
                physicalDriveList.Add($@"\\.\CDROM{i}");
        }
        return [.. physicalDriveList.Distinct()];
    }

    [SupportedOSPlatform("linux")]
    private List<string> EnumeratePhysicalDrivesLinux()
    {
        var cdInfo = "";
        try
        {
            cdInfo = File.ReadAllText("/proc/sys/dev/cdrom/info");
            if (File.Exists("/proc/scsi/scsi"))
            {
                var scsiInfo = File.ReadAllText("/proc/scsi/scsi");
                if (scsiInfo is { Length: > 0 })
                {
                    foreach (Match m in ScsiInfoParts().Matches(scsiInfo))
                    {
                        if (m.Groups["type"].Value is not "CD-ROM")
                            continue;

                        Log.Info($"Found optical media drive {m.Groups["vendor"].Value} {m.Groups["model"].Value}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Debug(e, e.Message);
        }
        var lines = cdInfo.Split(MultilineSplit, StringSplitOptions.RemoveEmptyEntries);
        return lines.Where(s => s.StartsWith("drive name:")).Select(l => Path.Combine("/dev", l.Split(':').Last().Trim())).Where(File.Exists)
            .Concat(IOEx.GetFilepaths("/dev", "sr*", SearchOption.TopDirectoryOnly))
            .Distinct()
            .ToList();

    }

    [SupportedOSPlatform("osx")]
    private List<string> EnumeratePhysicalDevicesMacOs()
    {
        var physicalDrives = new List<string>();
        try
        {
            var matching = IOKit.IOServiceMatching(IOKit.BdMediaClass);
            var result = IOKit.IOServiceGetMatchingServices(IOKit.MasterPortDefault, matching, out var iterator);
            if (result != CoreFoundation.KernSuccess)
            {
                Log.Error($"Failed to enumerate blu-ray drives: {result}");
                return physicalDrives;
            }

            const int nameBufferSize = 32;
            Span<byte> bsdNameBuf = stackalloc byte[nameBufferSize];
            for (var drive = IOKit.IOIteratorNext(iterator); drive != IntPtr.Zero; drive = IOKit.IOIteratorNext(iterator))
            {
                var cfBsdName = IOKit.IORegistryEntryCreateCFProperty(drive, IOKit.BsdNameKey, IntPtr.Zero, 0);
                if (cfBsdName != IntPtr.Zero)
                {
                    if (CoreFoundation.CFStringGetCString(cfBsdName, bsdNameBuf, nameBufferSize, CoreFoundation.StringEncodingAscii))
                    {
                        // We change "disk" to "rdisk" in the BSD name to be able to open while mounted.
                        var len = bsdNameBuf.IndexOf((byte)0);
                        if (len < 0)
                            len = nameBufferSize;
                        var bsdName = Encoding.ASCII.GetString(bsdNameBuf[..len]);
                        physicalDrives.Add($"/dev/r{bsdName}");
                    }
                }
                IOKit.IOObjectRelease(drive);
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Unable to enumerate physical drives");
        }
        return physicalDrives;
    }

    private string CheckDiscSfb(byte[] discSfbData)
    {
        var sfb = SfbReader.Parse(discSfbData);
        var flags = new HashSet<char>(sfb.KeyEntries.FirstOrDefault(e => e.Key == "HYBRID_FLAG")?.Value?.ToCharArray() ?? []);
        Log.Debug($"Disc flags: {string.Concat(flags)}");
        if (!flags.Contains('g'))
            Log.Warn("Disc is not a game disc");
        var titleId = sfb.KeyEntries.FirstOrDefault(e => e.Key == "TITLE_ID")?.Value;
        Log.Debug($"Disc product code: {titleId}");
        if (string.IsNullOrEmpty(titleId))
            Log.Warn("Disc product code is empty");
        else if (titleId.Length > 9)
            titleId = titleId[..4] + titleId[^5..];
        return titleId;
    }

    private void CheckParamSfo(ParamSfo sfo)
    {
        Title = sfo.Items.FirstOrDefault(i => i.Key == "TITLE")?.StringValue?.Trim(' ', '\0');
        var titleParts = Title.Split(MultilineSplit, StringSplitOptions.RemoveEmptyEntries);
        if (titleParts.Length > 1)
            Title = string.Join(" ", titleParts);
        ProductCode = sfo.Items.FirstOrDefault(i => i.Key == "TITLE_ID")?.StringValue?.Trim(' ', '\0');
        DiscVersion = sfo.Items.FirstOrDefault(i => i.Key == "VERSION")?.StringValue?.Trim();
        var appVer = sfo.Items.FirstOrDefault(i => i.Key == "APP_VER")?.StringValue.Trim();

        Log.Info($"Game title: {Title}");
        Log.Info($"Game product code: {ProductCode}");
        Log.Info($"Game version: {DiscVersion}");
        Log.Info($"App version: {appVer}");
    }

    private static string NormalizeDiscReaderPath(string path)
        => path.Replace(Path.DirectorySeparatorChar, '\\').Replace(Path.AltDirectorySeparatorChar, '\\');

    private static string ToDiscReaderPath(string path)
        => NormalizeDiscReaderPath(path).TrimStart('\\');

    private static byte[] ReadDiscFileBytes(CDReader reader, string path)
    {
        using var stream = reader.OpenFile(ToDiscReaderPath(path), FileMode.Open, FileAccess.Read);
        if (stream.Length > int.MaxValue)
            throw new IOException($"File is too big to buffer in memory: {path}");

        var result = new byte[(int)stream.Length];
        stream.ReadExactly(result, 0, result.Length);
        return result;
    }

    private static string FixDiscReaderPath(string path)
        => NormalizeDiscReaderPath(path).TrimStart('\\').Replace('\\', Path.DirectorySeparatorChar);

    private static string GetDiscReaderDirectoryName(string path)
    {
        var normalizedPath = NormalizeDiscReaderPath(path);
        var separatorIndex = normalizedPath.LastIndexOf('\\');
        return separatorIndex > 0 ? normalizedPath[..separatorIndex] : "";
    }

    private static (List<FileRecord> files, List<DirRecord> dirs) GetFilesystemStructure(CDReader reader)
    {
        Log.Debug("Scanning filesystem structure…");
        var fsObjects = reader.GetFileSystemEntries(reader.Root.FullName).ToList();
        var nextLevel = new List<string>();
        var filePaths = new List<string>(20_000);
        var dirPaths = new List<string>();
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
            }
            (fsObjects, nextLevel) = (nextLevel, fsObjects);
            nextLevel.Clear();
        }

        var filenames = filePaths.Distinct().ToList();
        var dirNames = dirPaths.Select(NormalizeDiscReaderPath).Distinct().OrderByDescending(n => n.Length).ToList();
        var dirNamesWithFiles = filenames
            .Select(GetDiscReaderDirectoryName)
            .Distinct()
            .Where(d => d.Length > 0)
            .ToList();
        var dirList = dirNames.Except(dirNamesWithFiles)
            .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
            .Select(dir => (dir: FixDiscReaderPath(dir), info: reader.GetDirectoryInfo(dir)))
            .Select(di => new DirRecord(di.dir, new(di.info.CreationTimeUtc, di.info.LastWriteTimeUtc)))
            .ToList();
        Log.Debug("Building file cluster map…");
        var fileList = new List<FileRecord>();
        foreach (var filename in filenames)
        {
            var targetFilename = FixDiscReaderPath(filename);
            var clusterRange = reader.PathToClusters(filename).ToList();
            var startSector = clusterRange.Min(r => r.Offset);
            var lengthInSectors = clusterRange.Sum(r => r.Count);
            var length = reader.GetFileLength(filename);
            var fileInfo = reader.GetFileSystemInfo(filename);
            var recordInfo = new FileRecordInfo(fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc);
            if (fileInfo.Parent is not { } parent)
                continue;

            var parentInfo = new DirRecord(FixDiscReaderPath(parent.FullName), new(parent.CreationTimeUtc, parent.LastWriteTimeUtc));
            fileList.Add(new(targetFilename, startSector, lengthInSectors, length, recordInfo, parentInfo));
        }
        return (files: fileList.OrderBy(r => r.StartSector).ToList(), dirs: dirList);
    }

    private static (List<FileRecord> files, List<DirRecord> dirs) ApplyFilesystemFilters(List<FileRecord> files, List<DirRecord> dirs)
    {
        if (!SettingsProvider.Settings.FilterRequired)
            return (files, dirs);

        var filterDirList = SettingsProvider.Settings.FilterDirList;
        var prefixList = filterDirList.Select(f => f + Path.DirectorySeparatorChar).ToArray();
        files = files
            .Where(r => !filterDirList.Any(f => r.TargetFileName == f)
                        && !prefixList.Any(p => r.TargetFileName.StartsWith(p)))
            .ToList();
        dirs = dirs
            .Where(r => !filterDirList.Any(f => r.TargetDirName == f)
                        && !prefixList.Any(p => r.TargetDirName.StartsWith(p)))
            .ToList();
        return (files, dirs);
    }

    private void SetOutputDir(Func<Dumper, string> outputDirFormatter)
    {
        var path = new string(outputDirFormatter(this).ToCharArray().Where(c => !InvalidChars.Contains(c)).ToArray());
        var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        var pathParts = path.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(p => p.TrimEnd('.'));
        OutputDir = string.Join(Path.DirectorySeparatorChar, pathParts);
        Log.Debug($"Dump folder name: {OutputDir}");
    }

    private void DetectIso(string isoPath, Func<Dumper, string> outputDirFormatter)
    {
        InputIsoPath = Path.GetFullPath(isoPath);
        InputDevicePath = InputIsoPath;
        TotalDiscSize = new FileInfo(InputIsoPath).Length;
        Log.Info("Selected disc image: " + InputIsoPath);

        using var isoStream = File.Open(InputIsoPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new CDReader(isoStream, true, true);
        if (!reader.FileExists(ToDiscReaderPath(DiscSfbPath)))
            throw new DriveNotFoundException($"Specified file is not a valid PS3 disc image: {InputIsoPath}");

        discSfbData = ReadDiscFileBytes(reader, DiscSfbPath);
        var titleId = CheckDiscSfb(discSfbData);
        if (!reader.FileExists(ToDiscReaderPath(ParamSfoPath)))
            throw new InvalidOperationException($"Specified file is not a valid PS3 disc image (param.sfo is missing): {InputIsoPath}");

        using (var stream = reader.OpenFile(ToDiscReaderPath(ParamSfoPath), FileMode.Open, FileAccess.Read))
            ParamSfo = ParamSfo.ReadFrom(stream);
        CheckParamSfo(ParamSfo);
        if (titleId != ProductCode)
            Log.Warn($"Product codes in ps3_disc.sfb ({titleId}) and in param.sfo ({ProductCode}) do not match");

        HasBdmv = reader.DirectoryExists(ToDiscReaderPath("BDMV"));
        var (files, dirs) = GetFilesystemStructure(reader);
        (filesystemStructure, emptyDirStructure) = ApplyFilesystemFilters(files, dirs);
        DiscFilenames = filesystemStructure.Select(f => f.TargetFileName).ToList();
        TotalFileSize = filesystemStructure.Sum(f => f.SizeInBytes);
        TotalFileCount = DiscFilenames.Count;
        SetOutputDir(outputDirFormatter);
    }

    private void CloseSourceStreams()
    {
        if (discReader is IDisposable disposableReader)
            disposableReader.Dispose();
        discReader = null;

        if (sourceStream != null && !ReferenceEquals(sourceStream, driveStream))
            sourceStream.Dispose();
        sourceStream = null;

        driveStream?.Dispose();
        driveStream = null;
    }

    public void DetectDisc(string inDir = "", Func<Dumper, string> outputDirFormatter = null)
    {
        outputDirFormatter ??= d => PatternFormatter.Format(SettingsProvider.Settings.DumpNameTemplate, new()
        {
            [Patterns.ProductCode] = d.ProductCode,
            [Patterns.ProductCodeLetters] = d.ProductCode?[..4],
            [Patterns.ProductCodeNumbers] = d.ProductCode?[4..],
            [Patterns.Title] = d.Title,
            [Patterns.Region] = RegionMapping[d.ProductCode?[2..3] ?? ""],
        });
        CloseSourceStreams();
        InputIsoPath = null;
        HasBdmv = false;
        filesystemStructure = null;
        emptyDirStructure = null;

        if (!string.IsNullOrEmpty(inDir) && File.Exists(inDir))
        {
            DetectIso(inDir, outputDirFormatter);
            return;
        }

        string discSfbPath = null;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom && d.IsReady);
            if (string.IsNullOrEmpty(inDir))
            {
                foreach (var drive in drives)
                {
                    discSfbPath = Path.Combine(drive.Name, "PS3_DISC.SFB");
                    if (!File.Exists(discSfbPath))
                        continue;

                    InputDevicePath = drive.Name;
                    Drive = drive.Name[0];
                    TotalDiscSize = drive.TotalSize;
                    break;
                }
            }
            else
            {
                discSfbPath = Path.Combine(inDir, "PS3_DISC.SFB");
                if (File.Exists(discSfbPath))
                {
                    InputDevicePath = Path.GetPathRoot(discSfbPath);
                    Drive = discSfbPath[0];
                }
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var mountList = inDir is { Length: > 0 }
                ? [inDir]
                : DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom).Select(d => d.RootDirectory.FullName).ToList();
            Log.Debug($"Found {mountList.Count} CDRom mount point{(mountList.Count is 1 ? "" : "s")}:");
            foreach (var mountPath in mountList)
                Log.Debug($"\t{mountPath}");
            if (mountList.Count is 0)
            {
                mountList = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.NoRootDirectory && d.RootDirectory.Name.Contains('\\'))
                    .Select(d => Path.GetDirectoryName(d.RootDirectory.FullName))
                    .Distinct()
                    .ToList();
                Log.Debug($"Found {mountList.Count} potential mount point root{(mountList.Count is 1 ? "" : "s")}:");
                foreach (var mountPath in mountList)
                    Log.Debug($"\t{mountPath}");
            }
            discSfbPath = mountList.SelectMany(mp => IOEx.GetFilepaths(mp, "PS3_DISC.SFB", 2)).FirstOrDefault();
            Log.Debug($"Found PS3_DISC.SFB path: {discSfbPath}");
            if (!string.IsNullOrEmpty(discSfbPath))
                InputDevicePath = Path.GetDirectoryName(discSfbPath)!;
        }
        else
            throw new NotImplementedException("Current OS is not supported");

        if (string.IsNullOrEmpty(InputDevicePath) || string.IsNullOrEmpty(discSfbPath))
            throw new DriveNotFoundException("No valid PS3 disc was detected. Disc must be detected and mounted.");

        Log.Info("Selected disc: " + InputDevicePath);
        discSfbData = File.ReadAllBytes(discSfbPath);
        var titleId = CheckDiscSfb(discSfbData);
        var mountedParamSfoPath = Path.Combine(InputDevicePath, "PS3_GAME", "PARAM.SFO");
        if (!File.Exists(mountedParamSfoPath))
            throw new InvalidOperationException($"Specified folder is not a valid PS3 disc root (param.sfo is missing): {InputDevicePath}");

        using (var stream = File.Open(mountedParamSfoPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            ParamSfo = ParamSfo.ReadFrom(stream);
        CheckParamSfo(ParamSfo);
        if (titleId != ProductCode)
            Log.Warn($"Product codes in ps3_disc.sfb ({titleId}) and in param.sfo ({ProductCode}) do not match");
        HasBdmv = Directory.Exists(Path.Combine(InputDevicePath, "BDMV"));

        // todo: maybe use discutils instead to read TOC as one block
        var files = IOEx.GetFilepaths(InputDevicePath, "*", SearchOption.AllDirectories);
        if (SettingsProvider.Settings.FilterRequired)
        {
            var prefixList = SettingsProvider.Settings.FilterDirList
                .Select(f => Path.Combine(InputDevicePath, f) + Path.DirectorySeparatorChar)
                .ToArray();
            files = files.Where(f => !prefixList.Any(f.StartsWith));
        }
        DiscFilenames = [];
        var totalFilesize = 0L;
        var rootLength = InputDevicePath.Length;
        foreach (var f in files)
        {
            try { totalFilesize += new FileInfo(f).Length; } catch { }
            DiscFilenames.Add(f[rootLength..]);
        }
        TotalFileSize = totalFilesize;
        TotalFileCount = DiscFilenames.Count;
        SetOutputDir(outputDirFormatter);
    }

    public async Task FindDiscKeyAsync(string discKeyCachePath)
    {
        // reload disc keys
        try
        {
            foreach (var keyProvider in DiscKeyProviders)
            {
                Log.Trace($"Getting keys from {keyProvider.GetType().Name}...");
                var newKeys = await keyProvider.EnumerateAsync(discKeyCachePath, ProductCode, Cts.Token).ConfigureAwait(false);
                Log.Trace($"Got {newKeys.Count} keys from {keyProvider.GetType().Name}");
                await Task.Yield();
                lock (AllKnownDiscKeys)
                {
                    foreach (var keyInfo in newKeys)
                    {
                        try
                        {
                            if (!AllKnownDiscKeys.TryGetValue(keyInfo.DecryptedKeyId, out var duplicates))
                                AllKnownDiscKeys[keyInfo.DecryptedKeyId] = duplicates = [];
                            duplicates.Add(keyInfo);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load disc keys");
        }
        // check if user provided something new since the last attempt
        var untestedKeys = new HashSet<string>();
        lock (AllKnownDiscKeys)
            untestedKeys.UnionWith(AllKnownDiscKeys.Keys);
        untestedKeys.ExceptWith(TestedDiscKeys);
        if (untestedKeys.Count == 0)
            throw new KeyNotFoundException("No valid disc decryption key was found");

        CloseSourceStreams();
        if (InputIsIso)
        {
            SelectedPhysicalDevice = InputIsoPath;
            Log.Debug($"Using disc image {SelectedPhysicalDevice} for sector reads");
            driveStream = File.Open(InputIsoPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            sourceStream = File.Open(InputIsoPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            discReader = new(sourceStream, true, true);
        }
        else
        {
            List<string> physicalDrives = [];
            Log.Trace("Trying to enumerate physical drives...");
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    physicalDrives = EnumeratePhysicalDrivesWindows();
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    physicalDrives = EnumeratePhysicalDrivesLinux();
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    physicalDrives = EnumeratePhysicalDevicesMacOs();
                else
                    throw new NotImplementedException("Current OS is not supported");
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
            Log.Debug($"Found {physicalDrives.Count} physical drives");
            await Task.Yield();

            if (physicalDrives is [])
                throw new InvalidOperationException("No optical drives were found");

            SelectedPhysicalDevice = null;
            foreach (var drive in physicalDrives)
            {
                try
                {
                    Log.Trace($"Checking physical drive {drive}...");
                    await using var discStream = File.Open(drive, new FileStreamOptions
                    {
                        Mode = FileMode.Open,
                        Access = FileAccess.Read,
                        Options = FileOptions.Asynchronous | FileOptions.SequentialScan
                    });
                    var tmpDiscReader = new CDReader(discStream, true, true);
                    if (tmpDiscReader.FileExists(DiscSfbPath))
                    {
                        Log.Trace("Found PS3_DISC.SFB, getting sector data...");
                        var discSfbInfo = tmpDiscReader.GetFileInfo(DiscSfbPath);
                        if (discSfbInfo.Length == discSfbData.Length)
                        {
                            var buf = new byte[discSfbData.Length];
                            var sector = tmpDiscReader.PathToClusters(discSfbInfo.FullName).First().Offset;
                            Log.Trace($"PS3_DISC.SFB sector number is {sector}, reading content...");
                            discStream.Seek(sector * tmpDiscReader.ClusterSize, SeekOrigin.Begin);
                            await discStream.ReadExactlyAsync(buf, 0, buf.Length).ConfigureAwait(false);
                            if (buf.SequenceEqual(discSfbData))
                            {
                                SelectedPhysicalDevice = drive;
                                break;
                            }
                            Log.Trace("SFB content check failed, skipping the drive");
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Debug($"Skipping drive {drive}: {e.Message}");
                }
                await Task.Yield();
            }
            if (SelectedPhysicalDevice == null)
                throw new AccessViolationException("Direct disk access to the drive was denied");

            Log.Debug($"Selected physical drive {SelectedPhysicalDevice}");
            driveStream = File.Open(SelectedPhysicalDevice, FileMode.Open, FileAccess.Read, FileShare.Read);
            sourceStream = driveStream;
            discReader = new(driveStream, true, true);
        }

        // find disc license file
        FileRecord detectionRecord = null;
        var expectedBytes = ImmutableArray<byte>.Empty;
        try
        {
            foreach (var path in Detectors.Keys)
                if (discReader.FileExists(path))
                {
                    var clusterRange = discReader.PathToClusters(path).ToList();
                    var fileInfo = discReader.GetFileSystemInfo(path);
                    var recordInfo = new FileRecordInfo(fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc);
                    var parent = fileInfo.Parent;
                    var parentInfo = new DirRecord(parent.FullName.TrimStart('\\'), new(parent.CreationTimeUtc, parent.LastWriteTimeUtc));
                    var startSector = clusterRange.Min(r => r.Offset);
                    var lengthInSectors = clusterRange.Sum(r => r.Count);
                    detectionRecord = new(path, startSector, lengthInSectors, discReader.GetFileLength(path), recordInfo, parentInfo);
                    expectedBytes = Detectors[path];
                    if (detectionRecord.SizeInBytes == 0)
                        continue;

                    Log.Debug($"Using {path} for disc key detection");
                    break;
                }
            await Task.Yield();
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
        if (detectionRecord is null)
            throw new FileNotFoundException("Couldn't find a single disc key detection file, please report");

        if (Cts.IsCancellationRequested)
            return;

        SectorSize = discReader.ClusterSize;

        // select decryption key
        driveStream.Seek(detectionRecord.StartSector * discReader.ClusterSize, SeekOrigin.Begin);
        detectionSector = new byte[discReader.ClusterSize];
        detectionBytesExpected = expectedBytes;
        sectorIV = Decrypter.GetSectorIV(detectionRecord.StartSector);
        Log.Debug($"Initialized {nameof(sectorIV)} ({sectorIV?.Length * 8} bit) for sector {detectionRecord.StartSector}: {sectorIV?.ToHexString()}");
        await driveStream.ReadExactlyAsync(detectionSector, 0, detectionSector.Length).ConfigureAwait(false);
        string discKey = null;
        try
        {
            var validKeys = untestedKeys.AsParallel().Where(k => !Cts.IsCancellationRequested && IsValidDiscKey(k)).Distinct().ToList();
            await Task.Yield();
            if (validKeys.Count > 1)
            {
                Log.Warn($"Expected only one valid decryption key, but found {validKeys.Count}:");
                foreach (var k in validKeys)
                {
                    Log.Debug($"\t{k}:");
                    var kiList = AllKnownDiscKeys[k];
                    foreach (var ki in kiList)
                        Log.Debug($"\t\t{ki.KeyType}: {Path.GetFileName(ki.FullPath)}");
                }
            }
            discKey = validKeys.FirstOrDefault();
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
        if (discKey == null)
            throw new KeyNotFoundException("No valid disc decryption key was found");

        Log.Info($"Selected disc key: {discKey}, known key sources:");
        var keyInfoList = AllKnownDiscKeys[discKey];
        foreach (var ki in keyInfoList)
            Log.Debug($"\t{ki.KeyType}: {Path.GetFileName(ki.FullPath)}");

        if (Cts.IsCancellationRequested)
            return;

        lock (AllKnownDiscKeys)
            AllKnownDiscKeys.TryGetValue(discKey, out allMatchingKeys);
        var discKeyInfo = allMatchingKeys?.FirstOrDefault(k => k.FullPath.Contains(ProductCode, StringComparison.OrdinalIgnoreCase) && k.FullPath.EndsWith(".ird", StringComparison.OrdinalIgnoreCase))
                          ?? allMatchingKeys?.FirstOrDefault(k => k.FullPath.EndsWith(".ird", StringComparison.OrdinalIgnoreCase))
                          ?? allMatchingKeys?.First();
        DiscKeyFilename = Path.GetFileName(discKeyInfo?.FullPath);
        DiscKeyType = discKeyInfo?.KeyType ?? default;
    }

    public async Task DumpAsync(string output)
    {
        // check and create output folder
        var dumpPath = output;
        while (!string.IsNullOrEmpty(dumpPath) && !Directory.Exists(dumpPath))
        {
            var parent = Path.GetDirectoryName(dumpPath);
            if (parent is null || parent == dumpPath)
                dumpPath = null;
            else
                dumpPath = parent;
        }
        if (filesystemStructure is null)
            (filesystemStructure, emptyDirStructure) = await GetFilesystemStructureAsync(Cts.Token).ConfigureAwait(false);
        (filesystemStructure, emptyDirStructure) = ApplyFilesystemFilters(filesystemStructure, emptyDirStructure);
        await Task.Yield();
        var validators = await GetValidationInfoAsync().ConfigureAwait(false);
        if (!string.IsNullOrEmpty(dumpPath))
        {
            var fullOutputPath = Path.GetFullPath(output);
            var drive = DriveInfo.GetDrives()
                .OrderByDescending(d => d.RootDirectory.FullName.Length)
                .ThenBy(d => d.RootDirectory.FullName, StringComparer.Ordinal)
                .FirstOrDefault(d => fullOutputPath.StartsWith(d.RootDirectory.FullName));
            if (drive != null)
            {
                var spaceAvailable = drive.AvailableFreeSpace;
                TotalFileSize = filesystemStructure.Sum(f => f.SizeInBytes);
                TotalFileSectors = filesystemStructure.Sum(f => f.LengthInSectors);
                var diff = TotalFileSize + 100 * 1024 - spaceAvailable;
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
        var outputPathBase = Path.Combine(output, OutputDir);
        Log.Debug($"Output path: {outputPathBase}");
        if (!Directory.Exists(outputPathBase))
            Directory.CreateDirectory(outputPathBase);

        TotalFileCount = filesystemStructure.Count;
        TotalSectors = discReader.TotalClusters;
        ProcessedSectors = 0;
        Log.Debug("Using decryption key: " + allMatchingKeys.First().DecryptedKeyId);
        var decryptionKey = allMatchingKeys.First().DecryptedKey;
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
                if (Path.DirectorySeparatorChar != '\\')
                    convertedName = convertedName.Replace('\\', Path.DirectorySeparatorChar);
                var outputName = Path.Combine(outputPathBase, convertedName);
                if (!Directory.Exists(outputName))
                {
                    Log.Debug("Creating empty directory " + outputName);
                    Directory.CreateDirectory(outputName);
                }
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
                string inputFilePath;
                if (InputIsIso)
                {
                    inputFilePath = targetFilename;
                    if (!discReader.FileExists(ToDiscReaderPath(inputFilePath)))
                    {
                        Log.Error($"Missing {file.TargetFileName}");
                        BrokenFiles.Add((file.TargetFileName, "missing"));
                        continue;
                    }
                }
                else
                {
                    var trailingPeriod = targetFilename.EndsWith('.');
                    inputFilePath = Path.Combine(InputDevicePath, targetFilename);
                    // BLES00932 Demon's Souls: trailing . is trimmed on both Windows and Linux
                    if (trailingPeriod && !File.Exists(inputFilePath))
                    {
                        Log.Warn($"Potential mastering error in {targetFilename}");
                        targetFilename = targetFilename.TrimEnd('.');
                        inputFilePath = Path.Combine(InputDevicePath, targetFilename);
                    }
                    // BLJS92001 Tsugime Ranko: trailing . is replaced with #ABCD in Windows, but is kept as . on Linux
                    if (trailingPeriod && OperatingSystem.IsWindows() && !File.Exists(inputFilePath))
                    {
                        var inputDir = Path.GetDirectoryName(inputFilePath);
                        var testNameBase = Path.GetFileName(inputFilePath);
                        var options = new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = false };
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
                }

                var outputFilePath = Path.Combine(outputPathBase, targetFilename);
                var fileDir = Path.GetDirectoryName(outputFilePath);
                if (!Directory.Exists(fileDir))
                {
                    Log.Debug("Creating directory " + fileDir);
                    Directory.CreateDirectory(fileDir);
                }

                var error = false;
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
                        await using Stream inputStream = InputIsIso
                            ? discReader.OpenFile(ToDiscReaderPath(inputFilePath), FileMode.Open, FileAccess.Read)
                            : File.Open(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        await using var decrypter = new Decrypter(inputStream, driveStream, decryptionKey, file.StartSector, sectorSize, unprotectedRegions);
                        Decrypter = decrypter;
                        await decrypter.CopyToAsync(outputStream, 8 * 1024 * 1024, Cts.Token).ConfigureAwait(false);
                        await outputStream.FlushAsync();
                        error = false;

                        var resultHashes = decrypter.GetHashes();
                        var resultMd5 = resultHashes["MD5"];
                        if (decrypter.WasEncrypted && decrypter.WasUnprotected)
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
                            if (resultMd5 == lastHash || decrypter.LastBlockCorrupted)
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
            .OrderByDescending(d => d.TargetDirName, StringComparer.OrdinalIgnoreCase);
        foreach (var dir in fullDirectoryList)
        {
            try
            {
                var targetDirPath = Path.Combine(outputPathBase, dir.TargetDirName.TrimStart('\\'));
                _ = new DirectoryInfo(targetDirPath)
                {
                    CreationTimeUtc = dir.DirInfo.CreationTimeUtc,
                    LastWriteTimeUtc = dir.DirInfo.LastWriteTimeUtc
                };
            }
            catch (Exception e)
            {
                Log.Warn(e, $"Failed to fix timestamp for directory {dir.TargetDirName}");
            }
        }
        if (BrokenFiles.Count is 0)
            Log.Info("Completed");
        else
            Log.Warn($"Completed with {BrokenFiles.Count} broken file{(BrokenFiles.Count == 1 ? "" : "s")}");
    }

    public static async Task<(Version, GitHubReleaseInfo)> CheckUpdatesAsync()
    {
        try
        {
            using var client = new HttpClient();
            var versionParts = VersionParts();
            var curVerMatch = versionParts.Match(Version);
            var curVerStr = curVerMatch.Groups["ver"].Value;
            var curVerPre = curVerMatch.Groups["pre"].Value;
            client.DefaultRequestHeaders.UserAgent.Add(new("PS3DiscDumper", curVerStr));
            var responseJson = await client.GetStringAsync("https://api.github.com/repos/13xforever/ps3-disc-dumper/releases").ConfigureAwait(false);
            var releaseList = JsonSerializer.Deserialize(responseJson, GithubReleaseSerializer.Default.GitHubReleaseInfoArray)?
                .OrderByDescending(r => System.Version.TryParse(r.TagName.TrimStart('v'), out var v) ? v : null)
                .ToList();
            var latest = releaseList?.FirstOrDefault(r => !r.Prerelease);
            var latestBeta = releaseList?.FirstOrDefault(r => r.Prerelease);
            System.Version.TryParse(curVerStr, out var curVer);
            System.Version.TryParse(latest?.TagName.TrimStart('v') ?? "0", out var latestVer);
            var latestBetaMatch = versionParts.Match(latestBeta?.TagName ?? "");
            var latestBetaVerStr = latestBetaMatch.Groups["ver"].Value;
            var latestBetaVerPre = latestBetaMatch.Groups["pre"].Value;
            System.Version.TryParse("0" + latestBetaVerStr, out var latestBetaVer);
            if (latestVer > curVer || latestVer == curVer && curVerPre is { Length: > 0 })
            {
                Log.Warn($"Newer version available: v{latestVer}\n\n{latest?.Name}\n{"".PadRight(latest?.Name.Length ?? 0, '-')}\n{latest?.Body}\n{latest?.HtmlUrl}\n");
                return (latestVer, latest);
            }

            if (latestBetaVer > curVer
                || (latestBetaVer == curVer
                    && curVerPre is { Length: > 0 }
                    && (latestBetaVerPre is { Length: > 0 } && StringComparer.OrdinalIgnoreCase.Compare(latestBetaVerPre, curVerPre) > 0
                        || latestBetaVerStr is null or "")))
            {
                Log.Warn($"Newer prerelease version available: v{latestBeta!.TagName.TrimStart('v')}\n\n{latestBeta?.Name}\n{"".PadRight(latestBeta?.Name.Length ?? 0, '-')}\n{latestBeta?.Body}\n{latestBeta?.HtmlUrl}\n");
                return (latestBetaVer, latestBeta);
            }
        }
        catch
        {
            Log.Warn("Failed to check for updates");
        }
        return (null, null);
    }


    private async Task<(List<FileRecord> files, List<DirRecord> dirs)> GetFilesystemStructureAsync(CancellationToken cancellationToken)
    {
        var pos = driveStream.Position;
        var buf = new byte[64 * 1024 * 1024];
        driveStream.Seek(0, SeekOrigin.Begin);
        await driveStream.ReadExactlyAsync(buf, 0, buf.Length, cancellationToken).ConfigureAwait(false);
        driveStream.Seek(pos, SeekOrigin.Begin);
        try
        {
            using var memStream = new MemoryStream(buf, false);
            var reader = new CDReader(memStream, true, true);
            return await reader.GetFilesystemStructureAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to buffer TOC");
        }
        return await discReader.GetFilesystemStructureAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<List<DiscInfo.DiscInfo>> GetValidationInfoAsync()
    {
        var discInfoList = new List<DiscInfo.DiscInfo>();
        foreach (var discKeyInfo in allMatchingKeys.Where(ki => ki.KeyType == KeyType.Ird))
        {
            var ird = IrdParser.Parse(await File.ReadAllBytesAsync(discKeyInfo.FullPath).ConfigureAwait(false));
            if (!DiscVersion.Equals(ird.GameVersion))
                continue;

            discInfoList.Add(await ird.ToDiscInfoAsync(Cts.Token).ConfigureAwait(false));
        }
        return discInfoList;
    }

    private bool IsValidDiscKey(string discKeyId)
    {
        HashSet<DiscKeyInfo> keys;
        lock (AllKnownDiscKeys)
            if (!AllKnownDiscKeys.TryGetValue(discKeyId, out keys))
                return false;

        var key = keys.FirstOrDefault()?.DecryptedKey;
        if (key == null)
            return false;

        return IsValidDiscKey(key);
    }

    public bool IsValidDiscKey(byte[] discKey)
    {
        try
        {
            var licBytes = Decrypter.DecryptSector(discKey, detectionSector, sectorIV);
            return licBytes.Take(detectionBytesExpected.Length).SequenceEqual(detectionBytesExpected);
        }
        catch (Exception e)
        {
            Log.Error(e);
            Log.Error($"{nameof(discKey)} ({discKey.Length * 8} bit): {discKey.ToHexString()}");
            Log.Error($"{nameof(sectorIV)} ({sectorIV.Length * 8} bit): {sectorIV.ToHexString()}");
            Log.Error($"{nameof(detectionSector)} ({detectionSector.Length} byte): {detectionSector.ToHexString()}");
            throw;
        }
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
        CloseSourceStreams();
        ((IDisposable)Decrypter)?.Dispose();
        Cts?.Dispose();
    }
}
