using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DiscUtils.Iso9660;
using IrdLibraryClient;
using IrdLibraryClient.IrdFormat;
using Ps3DiscDumper.DiscKeyProviders;
using Ps3DiscDumper.Utils;
using Ps3DiscDumper.Utils.MacOS;
using WmiLight;

namespace Ps3DiscDumper;

public partial class KeyFinder
{
    [GeneratedRegex(@"Host: .+$\s*Vendor: (?<vendor>.+?)\s* Model: (?<model>.+?)\s* Rev: (?<revision>.+)$\s*Type: \s*(?<type>.+?)\s* ANSI  ?SCSI revision: (?<scsi_rev>.+?)\s*$", RegexOptions.Multiline | RegexOptions.ExplicitCapture)]
    private static partial Regex ScsiInfoParts();

    private static readonly IDiscKeyProvider[] DiscKeyProviders = [new IrdProvider(), new RedumpProvider()];
    private static readonly Lock AllKnownDiscKeysSync = new();
    private static readonly Dictionary<string, HashSet<DiscKeyInfo>> AllKnownDiscKeys = new();
    private static readonly Dictionary<string, ImmutableArray<byte>> Detectors = new()
    {
        [@"\PS3_GAME\LICDIR\LIC.DAT"] = [.."PS3LICDA"u8],
        [@"\PS3_GAME\USRDIR\EBOOT.BIN"] = [.."SCE"u8, 0, 0, 0, 0, 2],
    };

    private ImmutableArray<byte> detectionBytesExpected;
    private readonly Lock testedDiscKeysSync = new();
    private readonly HashSet<string> testedDiscKeys = []; // todo: fix usage
    
    [SupportedOSPlatform("windows")]
    private static List<(string path, string? name)> EnumeratePhysicalDrivesWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new NotImplementedException("This should never happen, shut up msbuild");
            
        List<(string path, string? name)> physicalDriveList = [];
        List<string> driveNames = [];
        try
        {
            // there's no direct mapping from this to device path, I've seen logs with identical logicalUnit for different drives
            using var wmiConnection = new WmiConnection();
            var drives = wmiConnection.CreateQuery("SELECT * FROM Win32_CDROMDrive");
            foreach (var drive in drives)
            {
                // Name and Caption are the same, so idk if they can be different
                if (drive["Name"] is not string { Length: >0 } driveName)
                    continue;
                
                driveNames.Add(driveName);
                Log.Info($"Found optical media drive {driveName} ({drive["Drive"]})");
                if (driveName != (string?)drive["Caption"])
                    Log.Warn($"Expected Win32_CDROMDrive result to have matching Caption and Name, but found Caption='{drive["Caption"]}', Name='{driveName}'");
            }
            var curCount = physicalDriveList.Count;
            Log.Info($"Found {curCount} cdrom drive device{(curCount is 1 ? "" : "s")}");

            drives = wmiConnection.CreateQuery("SELECT * FROM Win32_PhysicalMedia");
            var i = 0;
            foreach (var drive in drives)
            {
                if (drive["Tag"] is string {Length: >0} tag
                    && tag.StartsWith(@"\\.\CDROM"))
                    physicalDriveList.Add((tag, driveNames.Count > i ? driveNames[i++] : null));
            }
            curCount = physicalDriveList.Count - curCount;
            Log.Info($"Found {curCount} physical media device{(curCount is 1 ? "" : "s")}");
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to enumerate physical media drives using WMI");
            for (var i = 0; i < 32; i++)
                physicalDriveList.Add(($@"\\.\CDROM{i}", null));
        }
        return [.. physicalDriveList.Distinct()];
    }

    [SupportedOSPlatform("linux")]
    private static List<(string path, string? name)> EnumeratePhysicalDrivesLinux()
    {
        List<string> modelList = [];
        var cdInfo = "";
        try
        {
            cdInfo = File.ReadAllText("/proc/sys/dev/cdrom/info");
            if (File.Exists("/proc/scsi/scsi"))
            {
                var scsiInfo = File.ReadAllText("/proc/scsi/scsi");
                if (scsiInfo is { Length: >0 })
                {
                    foreach (Match m in ScsiInfoParts().Matches(scsiInfo))
                    {
                        if (m.Groups["type"].Value is not "CD-ROM")
                            continue;

                        var model = $"{m.Groups["vendor"].Value} {m.Groups["model"].Value}";
                        modelList.Add(model);
                        Log.Info($"Found optical media drive {model}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Debug(e, e.Message);
        }
        var lines = cdInfo.Split(Dumper.MultilineSplit, StringSplitOptions.RemoveEmptyEntries);
        var physicalPaths = lines.Where(s => s.StartsWith("drive name:")).Select(l => Path.Combine("/dev", l.Split(':').Last().Trim())).Where(File.Exists)
            .Concat(IOEx.GetFilepaths("/dev", "sr*", SearchOption.TopDirectoryOnly))
            .Distinct()
            .ToList();
        var result = new List<(string path, string? name)>();
        for (var i = 0; i < physicalPaths.Count; i++)
            result.Add((physicalPaths[i], modelList.Count > i ? modelList[i] : null));
        return result;
    }

    [SupportedOSPlatform("osx")]
    private static List<(string path, string? name)> EnumeratePhysicalDevicesMacOs()
    {
        List<(string path, string? name)> physicalDrives = [];
        try
        {
            var matching = IOKit.IOServiceMatching(IOKit.BdMediaClass);
            var result = IOKit.IOServiceGetMatchingServices(IOKit.MasterPortDefault, matching, out var iterator);
            if (result is not CoreFoundation.KernSuccess)
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
                        physicalDrives.Add(($"/dev/r{bsdName}", null));
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

    public async Task<KeyFinderResult> FindDiscKeyAsync(string discKeyCachePath, DiscDetectionResult discInfo, CancellationToken cancellationToken)
    {
        // reload disc keys
        try
        {
            foreach (var keyProvider in DiscKeyProviders)
            {
                Log.Trace($"Getting keys from {keyProvider.GetType().Name}...");
                var newKeys = await keyProvider.EnumerateAsync(discKeyCachePath, discInfo.ProductCode, cancellationToken).ConfigureAwait(false);
                Log.Trace($"Got {newKeys.Count} keys from {keyProvider.GetType().Name}");
                await Task.Yield();
                lock (AllKnownDiscKeysSync)
                {
                    foreach (var keyInfo in newKeys)
                    {
                        try
                        {
                            if (!AllKnownDiscKeys.TryGetValue(keyInfo.KeyId, out var duplicates))
                                AllKnownDiscKeys[keyInfo.KeyId] = duplicates = [];
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
        lock (AllKnownDiscKeysSync)
        {
            untestedKeys.UnionWith(AllKnownDiscKeys.Keys);
            lock (testedDiscKeysSync)
                untestedKeys.ExceptWith(testedDiscKeys);
            if (untestedKeys.Count is 0)
                throw new KeyNotFoundException("No valid disc decryption key was found");
        }
        
        List<(string path, string? name)> physicalDrives;
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

        string? selectedPhysicalDevice = null;
        string? selectedPhysicalDeviceName = null;
        foreach (var (drive, driveName) in physicalDrives)
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
                using var tmpDiscReader = new CDReader(discStream, true, true);
                if (tmpDiscReader.FileExists("PS3_DISC.SFB"))
                {
                    Log.Trace("Found PS3_DISC.SFB, getting sector data...");
                    var discSfbInfo = tmpDiscReader.GetFileInfo("PS3_DISC.SFB");
                    var discSfbData = discInfo.DiscSfbData;
                    if (discSfbInfo.Length == discSfbData.Length)
                    {
                        var buf = new byte[discSfbData.Length];
                        var sector = tmpDiscReader.PathToClusters(discSfbInfo.FullName).First().Offset;
                        Log.Trace($"PS3_DISC.SFB sector number is {sector}, reading content...");
                        discStream.Seek(sector * tmpDiscReader.ClusterSize, SeekOrigin.Begin);
                        await discStream.ReadExactlyAsync(buf, 0, buf.Length, cancellationToken).ConfigureAwait(false);
                        if (buf.SequenceEqual(discSfbData))
                        {
                            selectedPhysicalDevice = drive;
                            selectedPhysicalDeviceName = driveName;
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
        if (selectedPhysicalDevice is not {Length: >0})
            throw new AccessViolationException("Direct disk access to the drive was denied");

        Log.Debug($"Selected physical drive {selectedPhysicalDevice} ({(selectedPhysicalDeviceName is {Length: >0} ? selectedPhysicalDeviceName : "unknown model")})");
        await using var driveStream = File.Open(selectedPhysicalDevice, FileMode.Open, FileAccess.Read, FileShare.Read);
        // find disc license file
        using var discReader = new CDReader(driveStream, true, true);
        FileRecord? detectionRecord = null;
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
                    var parentInfo = parent is null
                        ? null
                        : new DirRecord(parent.FullName.TrimStart('\\'), new(parent.CreationTimeUtc, parent.LastWriteTimeUtc));
                    var startSector = clusterRange.Min(r => r.Offset);
                    var lengthInSectors = clusterRange.Sum(r => r.Count);
                    detectionRecord = new(path, startSector, lengthInSectors, discReader.GetFileLength(path), recordInfo, parentInfo);
                    expectedBytes = Detectors[path];
                    if (detectionRecord.SizeInBytes is 0)
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

        cancellationToken.ThrowIfCancellationRequested();

        var sectorSize = (int)discReader.ClusterSize;
        // select decryption key
        driveStream.Seek(detectionRecord.StartSector * sectorSize, SeekOrigin.Begin);
        var detectionSector = new byte[sectorSize];
        detectionBytesExpected = expectedBytes;
        var sectorIV = new byte[16];
        Decrypter.GenerateSectorIV(detectionRecord.StartSector, sectorIV);
        Log.Debug($"Initialized {nameof(sectorIV)} ({sectorIV.Length * 8} bit) for sector {detectionRecord.StartSector}: {sectorIV.ToHexString()}");
        await driveStream.ReadExactlyAsync(detectionSector, cancellationToken).ConfigureAwait(false);
        string? discKey = null;
        try
        {
            var validKeys = untestedKeys.AsParallel()
                .Where(k => !cancellationToken.IsCancellationRequested && IsValidDiscKey(k, detectionSector, sectorIV))
                .Distinct()
                .ToList();
            await Task.Yield();
            if (validKeys.Count > 1)
            {
                Log.Warn($"Expected only one valid decryption key, but found {validKeys.Count}:");
                foreach (var k in validKeys)
                {
                    Log.Debug($"\t{k}:");
                    HashSet<DiscKeyInfo> kiList;
                    lock (AllKnownDiscKeysSync)
                        kiList = AllKnownDiscKeys[k];
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
        if (discKey is null)
            throw new KeyNotFoundException("No valid disc decryption key was found");

        Log.Info($"Selected disc key: {discKey}, known key sources:");
        HashSet<DiscKeyInfo> keyInfoList;
        lock (AllKnownDiscKeysSync)
            keyInfoList = AllKnownDiscKeys[discKey];
        foreach (var ki in keyInfoList)
            Log.Debug($"\t{ki.KeyType}: {Path.GetFileName(ki.FullPath)}");
        cancellationToken.ThrowIfCancellationRequested();

        HashSet<DiscKeyInfo>? allMatchingKeys;
        lock (AllKnownDiscKeysSync)
            AllKnownDiscKeys.TryGetValue(discKey, out allMatchingKeys);
        allMatchingKeys ??= [];
        var discKeyInfo = allMatchingKeys.FirstOrDefault(
                k => k.FullPath.Contains(discInfo.ProductCode, StringComparison.OrdinalIgnoreCase)
                                && k.FullPath.EndsWith(".ird", StringComparison.OrdinalIgnoreCase)
            ) ?? allMatchingKeys.FirstOrDefault(
                k => k.FullPath.EndsWith(".ird", StringComparison.OrdinalIgnoreCase)
            )
            ?? allMatchingKeys.FirstOrDefault();
        var discKeyFilename = Path.GetFileName(discKeyInfo?.FullPath);
        var discKeyType = discKeyInfo?.KeyType ?? default;
        return new()
        {
            SelectedPhysicalDevice = selectedPhysicalDevice,
            SelectedPhysicalDeviceName = selectedPhysicalDeviceName,
            SectorSize = sectorSize,
            DiscKeyType = discKeyType,
            DiscKeyFilename = discKeyFilename,
            AllMatchingKeys = allMatchingKeys,
        };
    }

    private bool IsValidDiscKey(in string discKeyId, in byte[] sector, in byte[] iv)
    {
        lock (testedDiscKeysSync)
            if (testedDiscKeys.Contains(discKeyId))
                return false;
        
        HashSet<DiscKeyInfo>? keys;
        lock (AllKnownDiscKeysSync)
            if (!AllKnownDiscKeys.TryGetValue(discKeyId, out keys))
                return false;

        var key = keys.FirstOrDefault()?.Key;
        if (key is null)
            return false;

        var keyValueId = key.ToHexString();
        lock (testedDiscKeysSync)
            if (testedDiscKeys.Contains(keyValueId))
                return false;

        var result = IsValidDiscKey(key, sector, iv);
        lock (testedDiscKeysSync)
        {
            testedDiscKeys.Add(discKeyId);
            testedDiscKeys.Add(keyValueId);
        }
        return result;
    }

    private bool IsValidDiscKey(in byte[] discKey, in byte[] sector, in byte[] iv)
    {
        try
        {
            return Decrypter.DecryptSector(discKey, sector, iv).AsSpan(0, detectionBytesExpected.Length)
                .SequenceEqual(detectionBytesExpected.AsSpan());
        }
        catch (Exception e)
        {
            Log.Error(e);
            Log.Error($"{nameof(discKey)} ({discKey.Length*8} bit): {discKey.ToHexString()}");
            Log.Error($"{nameof(iv)} ({iv.Length * 8} bit): {iv.ToHexString()}");
            Log.Error($"{nameof(sector)} ({sector.Length} byte): {sector.ToHexString()}");
            throw;
        }
    }
}