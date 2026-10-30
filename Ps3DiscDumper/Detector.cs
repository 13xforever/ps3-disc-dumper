using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using IrdLibraryClient;
using Ps3DiscDumper.Sfb;
using Ps3DiscDumper.Sfo;
using Ps3DiscDumper.Utils;
using FileInfo = System.IO.FileInfo;

namespace Ps3DiscDumper;

public static class Detector
{
    private static string? CheckDiscSfb(byte[] discSfbData)
    {
        var sfb = SfbReader.Parse(discSfbData);
        var flags = new HashSet<char>(sfb.KeyEntries.FirstOrDefault(e => e.Key == "HYBRID_FLAG")?.Value?.ToCharArray() ?? []);
        Log.Debug($"Disc flags: {string.Concat(flags)}");
        if (!flags.Contains('g'))
            Log.Warn("Disc is not a game disc");
        var titleId = sfb.KeyEntries.FirstOrDefault(e => e.Key == "TITLE_ID")?.Value;
        Log.Debug($"Disc product code: {titleId}");
        if (titleId is not {Length: >0})
            Log.Warn("Disc product code is empty");
        else if (titleId.Length > 9)
            titleId = titleId[..4] + titleId[^5..];
        return titleId;
    }

    private static (string ProductCode, string Title, string? DiscVersion) CheckParamSfo(ParamSfo sfo)
    {
        var title = sfo.Items.FirstOrDefault(i => i.Key is "TITLE")?.StringValue.Trim(' ', '\0');
        if (title?.Split(Dumper.MultilineSplit, StringSplitOptions.RemoveEmptyEntries) is {Length: >0} titleParts)
            title = string.Join(" ", titleParts);
        var productCode = sfo.Items.FirstOrDefault(i => i.Key is "TITLE_ID")?.StringValue.Trim(' ', '\0');
        var discVersion = sfo.Items.FirstOrDefault(i => i.Key is "VERSION")?.StringValue.Trim();
        var appVer = sfo.Items.FirstOrDefault(i => i.Key == "APP_VER")?.StringValue.Trim();

        Log.Info($"Game title: {title}");
        Log.Info($"Game product code: {productCode}");
        Log.Info($"Game version: {discVersion}");
        Log.Info($"App version: {appVer}");

        if (title is null)
            throw new InvalidDataException("PARAM.SFO is missing mandatory TITLE key");
        if (productCode is null)
            throw new InvalidDataException("PARAM.SFO is missing mandatory TITLE_ID key");

        return (productCode, title, discVersion);
    }

    public static DiscDetectionResult DetectDisc(string inDir = "")
    {
        string? discSfbPath = null;
        string? inputDevicePath = null;
        char drive = default;
        long? totalDiscSize = null;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var drives = DriveInfo.GetDrives().Where(d => d is { DriveType: DriveType.CDRom, IsReady: true });
            if (inDir is { Length: >0 })
            {
                discSfbPath = Path.Combine(inDir, "PS3_DISC.SFB");
                if (File.Exists(discSfbPath))
                {
                    inputDevicePath = Path.GetPathRoot(discSfbPath)!;
                    drive = discSfbPath[0];
                }
            }
            else
            {
                foreach (var di in drives)
                {
                    discSfbPath = Path.Combine(di.Name, "PS3_DISC.SFB");
                    if (!File.Exists(discSfbPath))
                        continue;

                    inputDevicePath = di.Name;
                    drive = di.Name[0];
                    totalDiscSize = di.TotalSize;
                    break;
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
                    .Where(d => d.DriveType is DriveType.NoRootDirectory && d.RootDirectory.Name.Contains('\\'))
                    .Select(d => Path.GetDirectoryName(d.RootDirectory.FullName) ?? "")
                    .Distinct()
                    .Where(p => p is {Length: >0})
                    .ToList();
                Log.Debug($"Found {mountList.Count} potential mount point root{(mountList.Count is 1 ? "" : "s")}:");
                foreach (var mountPath in mountList)
                    Log.Debug($"\t{mountPath}");
            }
            discSfbPath = mountList.SelectMany(mp => IOEx.GetFilepaths(mp, "PS3_DISC.SFB", 2)).FirstOrDefault();
            Log.Debug($"Found PS3_DISC.SFB path: {discSfbPath}");
            if (!string.IsNullOrEmpty(discSfbPath))
                inputDevicePath = Path.GetDirectoryName(discSfbPath)!;
        }
        else
            throw new NotImplementedException("Current OS is not supported");

        if (inputDevicePath is not {Length: >0} || discSfbPath is not {Length: >0})
            throw new DriveNotFoundException("No valid PS3 disc was detected. Disc must be detected and mounted.");

        Log.Info("Selected disc: " + inputDevicePath);
        var discSfbData = File.ReadAllBytes(discSfbPath);
        var titleId = CheckDiscSfb(discSfbData);
        var paramSfoPath = Path.Combine(inputDevicePath, "PS3_GAME", "PARAM.SFO");
        if (!File.Exists(paramSfoPath))
            throw new InvalidOperationException($"Specified folder is not a valid PS3 disc root (param.sfo is missing): {inputDevicePath}");

        ParamSfo paramSfo;
        using (var stream = File.Open(paramSfoPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            paramSfo = ParamSfo.ReadFrom(stream);
        var (productCode, title, discVersion) = CheckParamSfo(paramSfo);
        if (titleId is {Length: >0} && titleId != productCode)
            Log.Warn($"Product codes in ps3_disc.sfb ({titleId}) and in param.sfo ({productCode}) do not match");

        // todo: maybe use discutils instead to read TOC as one block
        var files = IOEx.GetFilepaths(inputDevicePath, "*", SearchOption.AllDirectories);
        if (SettingsProvider.Settings.FilterRequired)
        {
            var prefixList = SettingsProvider.Settings.FilterDirList
                .Select(f => Path.Combine(inputDevicePath, f) + Path.DirectorySeparatorChar)
                .ToArray();
            files = files.Where(f => !prefixList.Any(f.StartsWith));
        }
        List<string> discFilenames = [];
        var totalFilesize = 0L;
        var rootLength = inputDevicePath.Length;
        foreach (var f in files)
        {
            try { totalFilesize += new FileInfo(f).Length; } catch { }
            discFilenames.Add(f[rootLength..]);
        }
        return new()
        {
            DiscSfbData =  discSfbData,
            DiscFilenames =  discFilenames,
            ParamSfo = paramSfo,
            ProductCode =  productCode,
            Title =  title,
            InputDevicePath =  inputDevicePath,
            TotalFileSize = totalFilesize,
            TotalFileCount = discFilenames.Count,
            
            DiscVersion = discVersion,
            TotalDiscSize =  totalDiscSize,
            Drive = drive,
        };
    }
}