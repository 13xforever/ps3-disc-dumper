using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
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
        private readonly string output;
        private string input;
        private readonly CancellationToken cancellationToken;

        public string ProductCode { get; private set; }
        public string Title { get; private set; }
        public string OutputDir { get; private set; }
        public SearchResultItem IrdInfo { get; private set; }
        public Ird Ird { get; private set; }

        public Dumper(string output, CancellationToken cancellationToken)
        {
            this.output = output;
            this.cancellationToken = cancellationToken;
        }

        private string GetMatchingPhysicalDevice(byte[] firstSector)
        {
            var physicalDrives = new List<string>();
            using (var mgmtObjSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia"))
            {
                var drives = mgmtObjSearcher.Get();
                foreach (var drive in drives)
                {
                    var tag = drive.Properties["Tag"].Value as string;
                    if (tag?.StartsWith(@"\\.\CDROM") ?? false)
                        physicalDrives.Add(tag);
                }
            }
            if (physicalDrives.Count == 0)
                throw new InvalidOperationException("No optical drives were found");

            foreach (var drive in physicalDrives)
            {
                try
                {
                    using (var discStream = File.Open(drive, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var reader = new BinaryReader(discStream))
                    {
                        var bytes = reader.ReadBytes(firstSector.Length);
                        if (bytes.SequenceEqual(firstSector))
                            return drive;
                    }
                }
                catch (Exception e)
                {
                    ApiConfig.Log.Debug(e, e.Message);
                }
            }
            throw new InvalidOperationException("No matching physical device was found");
        }

        public async Task DetectDiscAsync()
        {
            var drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom && d.IsReady);

            string discSfbPath = null;
            foreach (var drive in drives)
            {
                discSfbPath = Path.Combine(drive.Name, "PS3_DISC.SFB");
                if (!File.Exists(discSfbPath))
                    continue;

                input = drive.Name;
            }
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(discSfbPath))
                throw new InvalidOperationException("No valid PS3 disc was detected");

            ApiConfig.Log.Info("Selected disc " + input);
            var discSfbData = File.ReadAllBytes(discSfbPath);
            var sfb = SfbReader.Parse(discSfbData);

            var flags = new HashSet<char>(sfb.KeyEntries.FirstOrDefault(e => e.Key == "HYBRID_FLAG")?.Value?.ToCharArray() ?? new char[0]);
            ApiConfig.Log.Debug($"Disc flags: {string.Concat(flags)}");
            if (!flags.Contains('g'))
                ApiConfig.Log.Warn("Disc is not a game disc");
            var titleId = sfb.KeyEntries.FirstOrDefault(e => e.Key == "TITLE_ID")?.Value;
            ApiConfig.Log.Debug($"Disc product code: {titleId}");
            if (string.IsNullOrEmpty(titleId))
                ApiConfig.Log.Warn("Disc product code is empty");
            else if (titleId.Length > 9)
                titleId = titleId.Substring(0, 4) + titleId.Substring(titleId.Length - 5, 5);
            var paramSfoPath = Path.Combine(input, "PS3_GAME", "PARAM.SFO");
            if (!File.Exists(paramSfoPath))
                throw new InvalidOperationException($"Specified folder is not a valid PS3 disc root (param.sfo is missing): {input}");

            ParamSfo sfo;
            using (var stream = File.Open(paramSfoPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                sfo = ParamSfo.ReadFrom(stream);
            var updateVer = sfo.Items.FirstOrDefault(i => i.Key == "PS3_SYSTEM_VER")?.StringValue?.Substring(0, 5).TrimStart('0').Trim();
            var gameVer = sfo.Items.FirstOrDefault(i => i.Key == "VERSION")?.StringValue?.Substring(0, 5).Trim();
            var appVer = sfo.Items.FirstOrDefault(i => i.Key == "APP_VER")?.StringValue?.Substring(0, 5).Trim();
            Title = sfo.Items.FirstOrDefault(i => i.Key == "TITLE")?.StringValue?.Trim(' ', '\0');
            ProductCode = sfo.Items.FirstOrDefault(i => i.Key == "TITLE_ID")?.StringValue?.Trim(' ', '\0');

            ApiConfig.Log.Debug($"Game version: {gameVer}");
            ApiConfig.Log.Debug($"App version: {appVer}");
            ApiConfig.Log.Debug($"Update version: {updateVer}");
            OutputDir = new string($"[{ProductCode}] {Title}".ToCharArray().Where(c => !InvalidChars.Contains(c)).ToArray());
            ApiConfig.Log.Debug($"Output: {OutputDir}");
            ApiConfig.Log.Info($"Game title: {Title}");
            if (titleId != ProductCode)
                ApiConfig.Log.Warn($"Product codes in ps3_disc.sfb ({titleId}) and in param.sfo ({ProductCode}) do not match");

            var irdInfoList = await Client.SearchAsync(ProductCode, cancellationToken).ConfigureAwait(false);
            var irdList = irdInfoList.Data?.Where(
                              i => i.Filename.Substring(0, 9).ToUpperInvariant() == ProductCode?.ToUpperInvariant()
                                   && i.AppVersion == appVer
                                   && i.UpdateVersion == updateVer
                                   && i.GameVersion == gameVer
                          ).ToList() ?? new List<SearchResultItem>(0);
            if (irdList.Count == 0)
            {
                ApiConfig.Log.Error("No matching IRD file was found");
                return;
            }

            if (irdList.Count > 1)
            {
                ApiConfig.Log.Warn($"{irdList.Count} matching IRD files were found???");
                return;
            }
            IrdInfo = irdList[0];
            Ird = await Client.DownloadAsync(IrdInfo, "ird/", cancellationToken).ConfigureAwait(false);
            if (Ird == null)
            {
                ApiConfig.Log.Error("No valid matching IRD file could be loaded");
                return;
            }

            ApiConfig.Log.Info($"Matching IRD: {IrdInfo.Filename}");
        }

        public async Task DumpAsync()
        {
            var fileInfo = Ird.GetFilesystemStructure();
            foreach (var file in fileInfo)
                ApiConfig.Log.Trace($"0x{file.StartSector:x8}: {file.Filename} ({file.Length})");
            var outputPathBase = Path.Combine(output, OutputDir);
            if (!Directory.Exists(outputPathBase))
                Directory.CreateDirectory(outputPathBase);

            var decryptionKey = Decrypter.GetDecryptionKey(Ird);
            var sectorSize = Ird.GetSectorSize();
            var unprotectedRegions = Ird.GetUnprotectedRegions();
            var firstSector = Ird.GetFirstSector(sectorSize);
            var physicalDevice = GetMatchingPhysicalDevice(firstSector);

            var brokenFiles = new List<(string filename, string error)>();
            foreach (var file in fileInfo)
            {
                ApiConfig.Log.Debug($"Extracting {file.Filename}");
                var inputFilename = Path.Combine(input, file.Filename);
                if (!File.Exists(inputFilename))
                {
                    ApiConfig.Log.Error($"Missing {file.Filename}");
                    brokenFiles.Add((file.Filename, "missing"));
                    continue;
                }
                var outputFilename = Path.Combine(outputPathBase, file.Filename);
                var fileDir = Path.GetDirectoryName(outputFilename);
                if (!Directory.Exists(fileDir))
                    Directory.CreateDirectory(fileDir);

                var error = false;
                var expectedMd5 = Ird.Files.First(f => f.Offset == file.StartSector).Md5Checksum.ToHexString();
                var lastMd5 = expectedMd5;
                string resultMd5 = null;
                do
                {
                    try
                    {
                        using (var outputStream = File.Open(outputFilename, FileMode.Create, FileAccess.Write, FileShare.Read))
                        using (var inputStream = File.Open(inputFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (var decrypter = new Decrypter(inputStream, physicalDevice, decryptionKey, file.StartSector, sectorSize, unprotectedRegions))
                        {
                            decrypter.CopyTo(outputStream);
                            outputStream.Flush();
                            resultMd5 = decrypter.GetMd5().ToHexString();
                            if (decrypter.WasEncrypted && decrypter.WasUnprotected)
                                ApiConfig.Log.Debug("Partially decrypted");
                            else if (decrypter.WasEncrypted)
                                ApiConfig.Log.Debug("Decrypted");
                            if (expectedMd5 != resultMd5)
                            {
                                error = true;
                                var msg = $"Expected {expectedMd5}, but was {resultMd5}";
                                if (lastMd5 == resultMd5 || decrypter.LastBlockCorrupted)
                                {
                                    ApiConfig.Log.Error(msg);
                                    brokenFiles.Add((file.Filename, "corrupted"));
                                    break;
                                }
                                ApiConfig.Log.Warn(msg + ", retrying");
                            }
                        }
                        lastMd5 = resultMd5;
                    }
                    catch (Exception e)
                    {
                        ApiConfig.Log.Error(e, e.Message);
                        error = true;
                    }
                } while (error);
            }
            ApiConfig.Log.Info("Completed.");
            if (brokenFiles.Count > 0)
            {
                ApiConfig.Log.Error("Dump is not valid:");
                foreach (var file in brokenFiles)
                    ApiConfig.Log.Error($"{file.error}: {file.filename}");
            }
            else
            {
                ApiConfig.Log.Info("Dump is valid");
            }
        }
    }
}
