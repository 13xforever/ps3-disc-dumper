using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrdLibraryClient;
using Ps3DiscDumper;
using Ps3DiscDumper.Utils;

namespace UI.Avalonia.ViewModels;

public partial class MainViewModel : ViewModelBase, IDisposable
{
    private readonly SettingsViewModel settings;
    
    public MainViewModel(): this(new()){}
    
    public MainViewModel(SettingsViewModel settings) => this.settings = settings;

    [ObservableProperty]
    public partial string StepTitle { get; set; } = "Please insert a PS3 game disc";
    [ObservableProperty]
    public partial string StepSubtitle { get; set; } = "";
    [ObservableProperty]
    public partial bool LastOperationSuccess { get; set; } = true;
    [ObservableProperty]
    public partial bool LastOperationWarning { get; set; } = false;
    [ObservableProperty]
    public partial bool LastOperationNotification { get; set; } = false;
    [ObservableProperty]
    public partial string LearnMoreLink { get; set; } = "";
    [ObservableProperty]
    public partial string StartButtonCaption { get; set; } = "Start";
    [ObservableProperty]
    public partial bool FoundDisc { get; set; }
    [ObservableProperty]
    public partial bool DumperIsReady { get; set; }
    [ObservableProperty]
    public partial bool DumpingInProgress { get; set; }
    [ObservableProperty]
    public partial bool DiscInfoExpanded { get; set; } = SettingsProvider.Settings.ShowDetails;
    [ObservableProperty]
    public partial string DriveName { get; set; } = "";
    [ObservableProperty]
    public partial string ProductCode { get; set; } = "";
    [ObservableProperty]
    public partial string GameTitle { get; set; } = "";
    [ObservableProperty]
    public partial string DiscSizeInfo { get; set; } = "";
    [ObservableProperty]
    public partial string DiscSizeDiffInfoLink { get; set; } = "";
    [ObservableProperty]
    public partial string DiscKeyName { get; set; } = "";
    [ObservableProperty]
    public partial int Progress { get; set; }
    [ObservableProperty]
    public partial int ProgressMax { get; set; } = 10_000;
    [ObservableProperty]
    public partial string ProgressInfo { get; set; } = "";
    [ObservableProperty]
    public partial bool? Success { get; set; }
    [ObservableProperty]
    public partial bool? Validated { get; set; }

    private readonly string[] analyzingMessages =
    [
        "Analyzing the file structure",
        "File structure analysis is taking longer than expected",
        "Still analyzing the file structure",
        "Yep, still analyzing the file structure",
        "You wouldn't believe it, but we're still analyzing the file structure",
        "I can't believe it's taking so long",
        "Hopefully it'll be over soon",
        "How many files are there on this disc",
    ];

    internal Dumper? Dumper;
    internal CancellationTokenSource Cts = new();
    private readonly SemaphoreSlim scanLock = new(1, 1);

    partial void OnDiscInfoExpandedChanged(bool value)
        => SettingsProvider.Settings = SettingsProvider.Settings with { ShowDetails = value };

    partial void OnProgressChanged(int value)
        => SetTaskbarProgress(value);

    [RelayCommand]
    private void ResetViewModel()
    {
        StepTitle = OperatingSystem.IsLinux()
            ? "Please insert and mount a PS3 game disc"
            : "Please insert a PS3 game disc";
        StepSubtitle = "";
        LastOperationSuccess = true;
        LastOperationWarning = false;
        LastOperationNotification = false;
        LearnMoreLink = "";
        StartButtonCaption = "Start";
        FoundDisc = false;
        DumperIsReady = false;
        DumpingInProgress = false;
        CanEditSettings = true;
        DriveName = "";
        ProductCode = "";
        GameTitle = "";
        DiscSizeInfo = "";
        DiscSizeDiffInfoLink = "";
        DiscKeyName = "";
        Progress = 0;
        ProgressInfo = "";
        Success = null;
        Validated = null;
        ResetTaskbarProgress();
    }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    [RelayCommand]
    private void ScanDiscs() => ScanDiscsAsync();

    [RelayCommand]
    private void DumpDisc() => DumpDiscAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

    [RelayCommand]
    private void CancelDump()
    {
        Cts.Cancel();
    }
    
    private async Task ScanDiscsAsync()
    {
        // ReSharper disable once MethodHasAsyncOverload
        if (!scanLock.Wait(0))
        {
            Log.Debug("Another disc scan is already in progress, ignoring call");
            return;
        }
        
        try
        {
            ResetViewModel();

            FoundDisc = true;
            StepTitle = "Scanning disc drives";
            StepSubtitle = "Checking the inserted disc…";
            Cts = new();
            Dumper?.Dispose();
            Dumper = null;
            await Task.Yield();

            DiscDetectionResult? discInfo = null;
            try
            {
                discInfo = Detector.DetectDisc();
            } catch {}
            if (discInfo is null)
            {
                ResetViewModel();
                return;
            }

            Success = null;
            Validated = null;
            ProductCode = discInfo.ProductCode;
            GameTitle = discInfo.Title;
            DiscSizeInfo = $"{discInfo.TotalFileSize.AsStorageUnit()} ({discInfo.TotalFileCount} files)";
            var diffMargin = 100 * 1024 * 1024;
            if (!SettingsProvider.Settings.CopyPs3Update)
                diffMargin = 356 * 1024 * 1024;
            if (discInfo.TotalDiscSize is long discSize && Math.Abs(discSize - discInfo.TotalFileSize) > diffMargin)
                DiscSizeDiffInfoLink = SettingsViewModel.WikiUrlBase + "Dump-size-is-significantly-different-from-disc-size";

            StepTitle = "Looking for a disc key";
            StepSubtitle = "Checking IRD and Redump data sets…";
            await Task.Yield();
            KeyFinderResult? discKeyInfo = null;
            try
            {
                var keyFinder = new KeyFinder();
                discKeyInfo = await keyFinder.FindDiscKeyAsync(
                    settings.IrdDir,
                    discInfo,
                    Cts.Token
                ).WaitAsync(Cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Log.Warn("Disc scan was cancelled");
                ResetViewModel();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to find a matching key");
                StepSubtitle = e.Message;
                LearnMoreLink = e switch
                {
                    AccessViolationException { Message: "Direct disk access to the drive was denied" }
                        => "Direct-disk-access-to-the-drive-was-denied",
                    KeyNotFoundException { Message: "No valid disc decryption key was found" }
                        => "No-valid-disc-decryption-key-was-found",
                    IOException when e.Message.Contains("cyclic redundancy check")
                        => "Data-Error-(cyclic-redundancy-check)",
                    _ => "",
                } is {Length: >0} lnk ? SettingsViewModel.WikiUrlBase + lnk : "";
            }
            if (discKeyInfo is null)
            {
                FoundDisc = false;
                StepTitle = "Disc check error";
                LastOperationSuccess = false;
                return;
            }
        
            StepSubtitle = "";
#if !MACOS            
            DriveName = discKeyInfo.SelectedPhysicalDeviceName ?? "Failed to detect";
#endif
            if (discKeyInfo.DiscKeyFilename is { Length: >0 } keyFilename)
                DiscKeyName = Path.GetFileName(keyFilename);
            else
                DiscKeyName = "No match found";

            var outputDir = OutputDirFormatter.Format(discInfo, d =>
            {
                var items = new NameValueCollection
                {
                    [Patterns.ProductCode] = d.ProductCode,
                    [Patterns.ProductCodeLetters] = d.ProductCode[..4],
                    [Patterns.ProductCodeNumbers] = d.ProductCode[4..],
                    [Patterns.Title] = d.Title,
                    [Patterns.Region] = Dumper.RegionMapping[d.ProductCode[2..3]],
                };
                settings.TestItems = items;
                return PatternFormatter.Format(SettingsProvider.Settings.DumpNameTemplate, items);
            });
            var destination = Path.Combine(settings.OutputDir, outputDir);
            if (Directory.Exists(destination))
            {
                StepTitle = "Dump already exists";
                StepSubtitle = "Existing dump files will be overwritten";
                StartButtonCaption = "Overwrite";
                LastOperationWarning = true;
            }
            else if (Directory.Exists(Path.Combine(discInfo.InputDevicePath, "BDMV")))
            {
                StepTitle = "Ready to dump a hybrid disc";
                StepSubtitle = CopyBdmv
                    ? "All files will be copied, but only PS3 game files will be decrypted"
                    : "Only PS3 game files will be copied";
                LearnMoreLink = SettingsViewModel.HybridDiscWikiLink;
                LastOperationNotification = true;
            }
            else
            {
                StepTitle = "Ready to dump";
            }
            Dumper = new(discInfo, discKeyInfo, Cts);
            DumperIsReady = true;
        }
        finally
        {
            scanLock.Release();
        }
    }
    
    private async Task DumpDiscAsync()
    {
        if (Dumper is null)
        {
            StepTitle = "Unexpected error occured";
            StepSubtitle = "Please restart the application";
            LastOperationSuccess = false;
            return;
        }

        StepTitle = "Dumping the disc";
        StepSubtitle = "Decrypting and copying the data…";
        ProgressInfo = analyzingMessages[0];
        LastOperationSuccess = true;
        LastOperationWarning = false;
        LastOperationNotification = false;
        LearnMoreLink = "";
        DumpingInProgress = true;
        CanEditSettings = false;
        EnableTaskbarProgress();
        await Task.Yield();
        
        try
        {
            var threadCts = new CancellationTokenSource();
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(threadCts.Token, Cts.Token);
            var monitor = new Thread(() =>
            {
                try
                {
                    string[] dotsAnimation = ["", ".", "..", "..."];
                    var animFrameIdx = 0;
                    var curAnalysisMsgIdx = 0;
                    var cnt = 0;
                    do
                    {
                        if (Dumper.TotalSectors > 0 && !Cts.IsCancellationRequested)
                        {
                            //Progress = (int)(dumper.CurrentSector * 10000L / dumper.TotalSectors);
                            //ProgressInfo = $"Sector data {(dumper.CurrentSector * dumper.SectorSize).AsStorageUnit()} of {(dumper.TotalSectors * dumper.SectorSize).AsStorageUnit()} / File {dumper.CurrentFileNumber} of {dumper.TotalFileCount}";
                            Dispatcher.UIThread.Post(() =>
                            {
                                var sectorSize = Dumper.DiscKeyInfo.SectorSize;
                                Progress = (int)((Dumper.ProcessedSectors + Dumper.CurrentFileSector) * 10000L / Dumper.TotalFileSectors);
                                ProgressInfo = $"Sector data {(Dumper.CurrentSector * sectorSize).AsStorageUnit()} of {(Dumper.TotalSectors * sectorSize).AsStorageUnit()} / File {Dumper.CurrentFileNumber} of {Dumper.DiscInfo.TotalFileCount}";
                            });
                        }
                        else
                        {
                            var msg = analyzingMessages[curAnalysisMsgIdx];
                            var dots = dotsAnimation[animFrameIdx];
                            Dispatcher.UIThread.Post(() => ProgressInfo = $"{msg}{dots}");
                            if (++cnt % 5 is 0)
                                animFrameIdx = (animFrameIdx + 1) % dotsAnimation.Length;
                            if (cnt % 600 is 0)
                                curAnalysisMsgIdx = (curAnalysisMsgIdx + 1) % analyzingMessages.Length;
                        }
                        Task.Delay(100, combinedToken.Token).GetAwaiter().GetResult();
                    } while (!combinedToken.Token.IsCancellationRequested);
                }
                catch (TaskCanceledException)
                {
                }
            });
            monitor.Start();
            await Dumper.DumpAsync(settings.OutputDir).WaitAsync(Cts.Token).ConfigureAwait(false);
            await threadCts.CancelAsync().ConfigureAwait(false);
            monitor.Join(100);
        }
        catch (OperationCanceledException e)
        {
            Log.Warn(e, "Cancellation requested");
            StepTitle = "Dump is not valid";
            StepSubtitle = "Operation was cancelled";
            Success = false;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to dump the disc");
            StepTitle = "Failed to dump the disc";
            StepSubtitle = e.Message;
            LastOperationSuccess = false;
            await Cts.CancelAsync().ConfigureAwait(false);
        }

        ResetTaskbarProgress();
        DumpingInProgress = false;
        CanEditSettings = true;
        DumperIsReady = false;
        FoundDisc = false;
        await Task.Yield();
        if (Cts.IsCancellationRequested
            || !LastOperationSuccess
            || LastOperationWarning)
            return;

        Success = Dumper.ValidationStatus is not false && Dumper.BrokenFiles.Count is 0;
        if (Success is false)
        {
            StepTitle = "Dump is not valid";
            if (Dumper.BrokenFiles.Count > 0)
                StepSubtitle = $"{Dumper.BrokenFiles.Count} invalid file{(Dumper.BrokenFiles.Count == 1 ? "" : "s")}";
        }
        else
        {
            StepTitle = "Files are decrypted and copied";
            if (Success == true)
                StepSubtitle = "Files match a verified copy";
        }
    }

    partial void ResetTaskbarProgress();
    partial void EnableTaskbarProgress();
    partial void SetTaskbarProgress(int position);
    
    public void Dispose()
    {
        Dumper?.Dispose();
        OnDisposePlatform();
    }

    partial void OnDisposePlatform();
}