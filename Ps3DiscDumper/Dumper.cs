using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using IrdLibraryClient;
using IrdLibraryClient.IrdFormat;
using IrdLibraryClient.POCOs;
using Ps3DiscDumper.Sfb;
using Ps3DiscDumper.Sfo;
using Ps3DiscDumper.Utils;

namespace Ps3DiscDumper
{
    public class Dumper
    {
        private static readonly IrdClient Client = new IrdClient();
        private static readonly HashSet<char> InvalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());
        private static readonly char[] MultilineSplit = {'\r', '\n'};
        private long currentSector;

        public ParamSfo ParamSfo { get; private set; }
        public string ProductCode { get; private set; }
        public string Title { get; private set; }
        public string OutputDir { get; private set; }
        public char Drive { get; set; }
        private string input;
        public string IrdFilename { get; set; }
        public Ird Ird { get; set; }
        private Decrypter Decrypter { get; set; }
        public int TotalFileCount { get; private set; }
        public int CurrentFileNumber { get; private set; }
        public long TotalSectors { get; private set; }
        public long TotalFileSize { get; private set; }
        private List<string> DiscFilenames { get; set; }
        public List<(string filename, string error)> BrokenFiles { get; } = new List<(string filename, string error)>();
        public CancellationTokenSource Cts { get; private set; }
        public bool? ValidationStatus { get; private set; }

        public Dumper(CancellationTokenSource cancellationTokenSource)
        {
            Cts = cancellationTokenSource;
        }

        public long CurrentSector
        {
            get
            {
                var tmp = Decrypter?.SectorPosition;
                if (tmp == null)
                    return currentSector;
                return currentSector = tmp.Value;
            }
        }

        private List<string> EnumeratePhysicalDrivesWindows()
        {
            var physicalDrives = new List<string>();
#if !NATIVE
            using (var mgmtObjSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia"))
            {
                var drives = mgmtObjSearcher.Get();
                foreach (var drive in drives)
                {
                    var tag = drive.Properties["Tag"].Value as string;
                    if (tag?.IndexOf("CDROM", StringComparison.InvariantCultureIgnoreCase) > -1)
                        physicalDrives.Add(tag);
                }
            }
#else
            for (var i = 0; i < 32; i++)
            {
                physicalDrives.Add($@"\\.\CDROM{i}");
            }
#endif
            return physicalDrives;
        }

        private List<string> EnumeratePhysicalDrivesLinux()
        {
            string cdInfo = "";
            try
            {
                cdInfo = File.ReadAllText("/proc/sys/dev/cdrom/info");
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

        private string CheckDiscSfb(byte[] discSfbData)
        {
            var sfb = SfbReader.Parse(discSfbData);
            var flags = new HashSet<char>(sfb.KeyEntries.FirstOrDefault(e => e.Key == "HYBRID_FLAG")?.Value?.ToCharArray() ?? new char[0]);
            Log.Debug($"Disc flags: {string.Concat(flags)}");
            if (!flags.Contains('g'))
                Log.Warn("Disc is not a game disc");
            var titleId = sfb.KeyEntries.FirstOrDefault(e => e.Key == "TITLE_ID")?.Value;
            Log.Debug($"Disc product code: {titleId}");
            if (string.IsNullOrEmpty(titleId))
                Log.Warn("Disc product code is empty");
            else if (titleId.Length > 9)
                titleId = titleId.Substring(0, 4) + titleId.Substring(titleId.Length - 5, 5);
            return titleId;
        }

        private void CheckParamSfo(ParamSfo sfo)
        {
            Title = sfo.Items.FirstOrDefault(i => i.Key == "TITLE")?.StringValue?.Trim(' ', '\0');
            var titleParts = Title.Split(MultilineSplit, StringSplitOptions.RemoveEmptyEntries);
            if (titleParts.Length > 1)
                Title = string.Join(" ", titleParts);
            ProductCode = sfo.Items.FirstOrDefault(i => i.Key == "TITLE_ID")?.StringValue?.Trim(' ', '\0');

            Log.Info($"Game title: {Title}");
        }

        private (string appVer, string updateVer, string gameVer) GetVersions(ParamSfo sfo)
        {
            var updateVer = sfo.Items.FirstOrDefault(i => i.Key == "PS3_SYSTEM_VER")?.StringValue?.Substring(0, 5).TrimStart('0').Trim();
            var gameVer = sfo.Items.FirstOrDefault(i => i.Key == "VERSION")?.StringValue?.Substring(0, 5).Trim();
            var appVer = sfo.Items.FirstOrDefault(i => i.Key == "APP_VER")?.StringValue?.Substring(0, 5).Trim();

            Log.Debug($"Game version: {gameVer}");
            Log.Debug($"App version: {appVer}");
            Log.Debug($"Update version: {updateVer}");
            return (appVer, updateVer, gameVer);
        }

        public bool IsFullMatch(Ird ird)
        {
            var (appVer, updateVer, gameVer) = GetVersions(ParamSfo);
            return ird.ProductCode == ProductCode
                   && ird.AppVersion == appVer
                   && ird.UpdateVersion == updateVer
                   && ird.GameVersion == gameVer;
        }

        public bool IsFilenameSetMatch(Ird ird)
        {
            var expectedSet = new HashSet<string>(DiscFilenames, StringComparer.InvariantCultureIgnoreCase);
            var irdSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            var fileInfo = ird.GetFilesystemStructure();
            foreach (var file in fileInfo)
            {
                if (Cts.IsCancellationRequested)
                    return false;

                var convertedFilename = Path.DirectorySeparatorChar == '\\' ? file.Filename : file.Filename.Replace('\\', Path.DirectorySeparatorChar);
                irdSet.Add(convertedFilename);
            }
            return expectedSet.SetEquals(irdSet);
        }

        public void DetectDisc(Func<Dumper, string> outputDirFormatter = null)
        {
            outputDirFormatter = outputDirFormatter ?? (d => $"[{d.ProductCode}] {d.Title}");
            string discSfbPath = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom && d.IsReady);
                foreach (var drive in drives)
                {
                    discSfbPath = Path.Combine(drive.Name, "PS3_DISC.SFB");
                    if (!File.Exists(discSfbPath))
                        continue;

                    Drive = drive.Name[0];
                    input = drive.Name;
                    break;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                discSfbPath = IOEx.GetFilepaths("/media", "PS3_DISC.SFB", SearchOption.AllDirectories).FirstOrDefault();
                if (!string.IsNullOrEmpty(discSfbPath))
                    input = Path.GetDirectoryName(discSfbPath);
            }
            else
                throw new NotImplementedException("Current OS is not supported");
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(discSfbPath))
                throw new DriveNotFoundException("No valid PS3 disc was detected. Disc must be detected and mounted.");

            Log.Info("Selected disc: " + input);
            var discSfbData = File.ReadAllBytes(discSfbPath);
            var titleId = CheckDiscSfb(discSfbData);
            var paramSfoPath = Path.Combine(input, "PS3_GAME", "PARAM.SFO");
            if (!File.Exists(paramSfoPath))
                throw new InvalidOperationException($"Specified folder is not a valid PS3 disc root (param.sfo is missing): {input}");

            using (var stream = File.Open(paramSfoPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                ParamSfo = ParamSfo.ReadFrom(stream);
            CheckParamSfo(ParamSfo);
            if (titleId != ProductCode)
                Log.Warn($"Product codes in ps3_disc.sfb ({titleId}) and in param.sfo ({ProductCode}) do not match");

            var files = IOEx.GetFilepaths(input, "*", SearchOption.AllDirectories);
            DiscFilenames = new List<string>();
            var totalFilesize = 0L;
            var rootLength = input.Length;
            foreach (var f in files)
            {
                try { totalFilesize += new FileInfo(f).Length; } catch { }
                DiscFilenames.Add(f.Substring(rootLength));
            }
            TotalFileSize = totalFilesize;

            OutputDir = new string(outputDirFormatter(this).ToCharArray().Where(c => !InvalidChars.Contains(c)).ToArray());
            Log.Debug($"Output: {OutputDir}");
        }

        public async Task FindIrdAsync(string output, string irdCachePath)
        {
            var (appVer, updateVer, gameVer) = GetVersions(ParamSfo);
            Log.Trace("Searching local cache for match...");
            if (Directory.Exists(irdCachePath))
            {
                var matchingIrdFiles = Directory.GetFiles(irdCachePath, "*.ird", SearchOption.TopDirectoryOnly);
                foreach (var irdFile in matchingIrdFiles)
                {
                    try
                    {
                        Ird ird;
                        try
                        {
                            ird = IrdParser.Parse(File.ReadAllBytes(irdFile));
                        }
                        catch (InvalidDataException)
                        {
                            File.Delete(irdFile);
                            continue;
                        }
                        catch (Exception e)
                        {
                            Log.Warn(e);
                            continue;
                        }

                        if (IsFullMatch(ird))
                        {
                            Ird = ird;
                            IrdFilename = Path.GetFileName(irdFile);
                            Log.Info("Using local IRD cache");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e, e.Message);
                    }
                }
            }
            if (Ird == null)
            {
                Log.Trace("Searching IRD Library for match...");
                var irdInfoList = await Client.SearchAsync(ProductCode, Cts.Token).ConfigureAwait(false);
                var irdList = irdInfoList?.Data?.Where(
                                  i => i.Filename.Substring(0, 9).ToUpperInvariant() == ProductCode?.ToUpperInvariant()
                                       && i.AppVersion == appVer
                                       && i.UpdateVersion == updateVer
                                       && i.GameVersion == gameVer
                              ).ToList() ?? new List<SearchResultItem>(0);
                if (irdList.Count == 0)
                    Log.Error("No matching IRD file was found in the Library");
                else if (irdList.Count > 1)
                    Log.Warn($"{irdList.Count} matching IRD files were found???");
                else
                {
                    Ird = await Client.DownloadAsync(irdList[0], irdCachePath, Cts.Token).ConfigureAwait(false);
                    IrdFilename = irdList[0].Filename;
                    Log.Info("Using IRD Library");
                }
            }
            if (Ird == null)
            {
                Log.Error("No valid matching IRD file could be found");
                Log.Info($"If you have matching IRD file, you can put it in '{irdCachePath}' and try dumping the disc again");
                return;
            }

            Log.Info($"Matching IRD: {IrdFilename}");

            var dumpPath = output;
            while (!string.IsNullOrEmpty(dumpPath) && !Directory.Exists(dumpPath))
            {
                var parent = Path.GetDirectoryName(dumpPath);
                if (parent == null || parent == dumpPath)
                    dumpPath = null;
                else
                    dumpPath = parent;
            }
            if (!string.IsNullOrEmpty(dumpPath))
            {
                var root = Path.GetPathRoot(Path.GetFullPath(output));
                var drive = DriveInfo.GetDrives().FirstOrDefault(d => d?.RootDirectory.FullName.StartsWith(root) ?? false);
                if (drive != null)
                {
                    var spaceAvailable = drive.AvailableFreeSpace;
                    TotalFileSize = Ird.GetFilesystemStructure().Sum(f => f.Length);
                    var diff = TotalFileSize + 100 * 1024 - spaceAvailable;
                    if (diff > 0)
                        Log.Warn($"Target drive might require {diff.AsStorageUnit()} of additional free space");
                }
            }

        }

        public async Task DumpAsync(string output)
        {
            var fileInfo = Ird.GetFilesystemStructure();
            foreach (var file in fileInfo)
                Log.Trace($"0x{file.StartSector:x8}: {file.Filename} ({file.Length})");
            var outputPathBase = Path.Combine(output, OutputDir);
            if (!Directory.Exists(outputPathBase))
                Directory.CreateDirectory(outputPathBase);

            TotalFileCount = fileInfo.Count;
            TotalSectors = Ird.GetTotalSectors();
            var decryptionKey = Decrypter.GetDecryptionKey(Ird);
            Log.Debug("Using decryption key: " + decryptionKey.ToHexString());
            var sectorSize = Ird.GetSectorSize();
            var unprotectedRegions = Ird.GetUnprotectedRegions();
            string physicalDevice = null;
            List<string> physicalDrives;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                physicalDrives = EnumeratePhysicalDrivesWindows();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                physicalDrives = EnumeratePhysicalDrivesLinux();
            else
                throw new NotImplementedException("Current OS is not supported");

            if (physicalDrives.Count == 0)
                throw new InvalidOperationException("No optical drives were found");

            var firstSector = Ird.GetFirstSector(sectorSize);
            foreach (var drive in physicalDrives)
            {
                try
                {
                    using (var discStream = File.Open(drive, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var reader = new BinaryReader(discStream))
                    {
                        var bytes = reader.ReadBytes(firstSector.Length);
                        if (bytes.SequenceEqual(firstSector))
                        {
                            physicalDevice = drive;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Debug($"Skipping drive {drive}: {e.Message}");
                }
            }
            if (string.IsNullOrEmpty(physicalDevice))
                throw new DriveNotFoundException("No matching physical device was found");

            foreach (var file in fileInfo)
            {
                if (Cts.IsCancellationRequested)
                    return;

                Log.Info($"Reading {file.Filename} ({file.Length.AsStorageUnit()})");
                CurrentFileNumber++;
                var convertedFilename = Path.DirectorySeparatorChar == '\\' ? file.Filename : file.Filename.Replace('\\', Path.DirectorySeparatorChar);
                var inputFilename = Path.Combine(input, convertedFilename);
 
                if (!File.Exists(inputFilename))
                {
                    Log.Error($"Missing {file.Filename}");
                    BrokenFiles.Add((file.Filename, "missing"));
                    continue;
                }

                var outputFilename = Path.Combine(outputPathBase, convertedFilename);
                var fileDir = Path.GetDirectoryName(outputFilename);
                if (!Directory.Exists(fileDir))
                    Directory.CreateDirectory(fileDir);

                var error = false;
                var expectedMd5 = Ird.Files.First(f => f.Offset == file.StartSector).Md5Checksum.ToHexString();
                var lastMd5 = expectedMd5;
                do
                {
                    try
                    {
                        using (var outputStream = File.Open(outputFilename, FileMode.Create, FileAccess.Write, FileShare.Read))
                        using (var inputStream = File.Open(inputFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (Decrypter = new Decrypter(inputStream, physicalDevice, decryptionKey, file.StartSector, sectorSize, unprotectedRegions))
                        {
                            await Decrypter.CopyToAsync(outputStream, 8*1024*1024, Cts.Token).ConfigureAwait(false);
                            outputStream.Flush();
                            var resultMd5 = Decrypter.GetMd5().ToHexString();
                            if (Decrypter.WasEncrypted && Decrypter.WasUnprotected)
                                Log.Debug("Partially decrypted");
                            else if (Decrypter.WasEncrypted)
                                Log.Debug("Decrypted");
                            if (expectedMd5 != resultMd5)
                            {
                                error = true;
                                var msg = $"Expected {expectedMd5}, but was {resultMd5}";
                                if (lastMd5 == resultMd5 || Decrypter.LastBlockCorrupted)
                                {
                                    Log.Error(msg);
                                    BrokenFiles.Add((file.Filename, "corrupted"));
                                    break;
                                }
                                Log.Warn(msg + ", retrying");
                            }
                            lastMd5 = resultMd5;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, e.Message);
                        error = true;
                    }
                } while (error);
            }
            Log.Info("Completed");
        }
    }
}
