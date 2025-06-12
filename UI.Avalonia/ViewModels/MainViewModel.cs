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

    [ObservableProperty] private string stepTitle = "Please insert a PS3 game disc";
    [ObservableProperty] private string stepSubtitle = "";
    [ObservableProperty] private bool lastOperationSuccess = true;
    [ObservableProperty] private bool lastOperationWarning = false;
    [ObservableProperty] private bool lastOperationNotification = false;
    [ObservableProperty] private string learnMoreLink = "";
    [ObservableProperty] private string startButtonCaption = "Start";

    [ObservableProperty] private bool foundDisc;
    [ObservableProperty] private bool dumperIsReady;
    [ObservableProperty] private bool dumpingInProgress;

    [ObservableProperty] private bool discInfoExpanded = SettingsProvider.Settings.ShowDetails;
    [ObservableProperty] private string productCode = "";
    [ObservableProperty] private string gameTitle = "";
    [ObservableProperty] private string discSizeInfo = "";
    [ObservableProperty] private string discSizeDiffInfoLink = "";
    [ObservableProperty] private string discKeyName = "";
    [ObservableProperty] private int progress;
    [ObservableProperty] private int progressMax = 10_000;
    [ObservableProperty] private string progressInfo = "";
    [ObservableProperty] private bool? success;
    [ObservableProperty] private bool? validated;

    private string[] AnalyzingMessages =
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

    internal Dumper? dumper;
    private readonly SemaphoreSlim scanLock = new(1, 1);

    partial void OnDiscInfoExpandedChanged(bool value)
    {
        SettingsProvider.Settings = SettingsProvider.Settings with { ShowDetails = value };
    }

    partial void OnProgressChanged(int value)
    {
        SetTaskbarProgress(value);
    }
    
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
        dumper?.Cts.Cancel();
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
            dumper?.Dispose();
            dumper = new();
            await Task.Yield();
            
            try
            {
                dumper.DetectDisc("",
                    d =>
                    {
                        var items = new NameValueCollection
                        {
                            [Patterns.ProductCode] = d.ProductCode,
                            [Patterns.ProductCodeLetters] = d.ProductCode?[..4],
                            [Patterns.ProductCodeNumbers] = d.ProductCode?[4..],
                            [Patterns.Title] = d.Title,
                            [Patterns.Region] = Dumper.RegionMapping[d.ProductCode?[2..3] ?? ""],
                        };
                    settings.TestItems = items;
                    return PatternFormatter.Format(SettingsProvider.Settings.DumpNameTemplate, items);
                });
            } catch {}
            if (dumper.ProductCode is not { Length: > 0 } || dumper.Cts.IsCancellationRequested)
            {
                ResetViewModel();
                return;
            }

            Success = null;
            Validated = null;
            ProductCode = dumper.ProductCode;
            GameTitle = dumper.Title;
            DiscSizeInfo = $"{dumper.TotalFileSize.AsStorageUnit()} ({dumper.TotalFileCount} files)";
            var diffMargin = 100 * 1024 * 1024;
            if (!SettingsProvider.Settings.CopyPs3Update)
                diffMargin = 356 * 1024 * 1024;
            if (Math.Abs(dumper.TotalDiscSize - dumper.TotalFileSize) > diffMargin)
                DiscSizeDiffInfoLink = SettingsViewModel.WikiUrlBase + "Dump-size-is-significantly-different-from-disc-size";

            StepTitle = "Looking for a disc key";
            StepSubtitle = "Checking IRD and Redump data sets…";
            await Task.Yield();
            try
            {
                await dumper.FindDiscKeyAsync(settings.IrdDir).WaitAsync(dumper.Cts.Token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to find a matching key");
                await dumper.Cts.CancelAsync().ConfigureAwait(false);
                FoundDisc = false;
                StepTitle = "Disc check error";
                LastOperationSuccess = false;
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
                return;
            }
        
            StepSubtitle = "";
            if (dumper.DiscKeyFilename is { Length: > 0 })
                DiscKeyName = Path.GetFileName(dumper.DiscKeyFilename);
            else
                DiscKeyName = "No match found";

            var destination = Path.Combine(settings.OutputDir, dumper.OutputDir);
            if (Directory.Exists(destination))
            {
                StepTitle = "Dump already exists";
                StepSubtitle = "Existing dump files will be overwritten";
                StartButtonCaption = "Overwrite";
                LastOperationWarning = true;
            }
            else if (Directory.Exists(Path.Combine(dumper.InputDevicePath, "BDMV")))
            {
                StepTitle = "Ready to dump a hybrid disc";
                if (CopyBdmv)
                    StepSubtitle = "All files will be copied, but only PS3 game files will be decrypted";
                else
                    StepSubtitle = "Only PS3 game files will be copied";
                LearnMoreLink = SettingsViewModel.HybridDiscWikiLink;
                LastOperationNotification = true;
            }
            else
            {
                StepTitle = "Ready to dump";
            }
            DumperIsReady = true;
        }
        finally
        {
            scanLock.Release();
        }
    }
    
    private async Task DumpDiscAsync()
    {
        if (dumper is null)
        {
            StepTitle = "Unexpected error occured";
            StepSubtitle = "Please restart the application";
            LastOperationSuccess = false;
            return;
        }

        StepTitle = "Dumping the disc";
        StepSubtitle = "Decrypting and copying the data…";
        ProgressInfo = AnalyzingMessages[0];
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
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(threadCts.Token, dumper.Cts.Token);
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
                        if (dumper.TotalSectors > 0 && !dumper.Cts.IsCancellationRequested)
                        {
                            //Progress = (int)(dumper.CurrentSector * 10000L / dumper.TotalSectors);
                            //ProgressInfo = $"Sector data {(dumper.CurrentSector * dumper.SectorSize).AsStorageUnit()} of {(dumper.TotalSectors * dumper.SectorSize).AsStorageUnit()} / File {dumper.CurrentFileNumber} of {dumper.TotalFileCount}";
                            Progress = (int)((dumper.ProcessedSectors + dumper.CurrentFileSector) * 10000L / dumper.TotalFileSectors);
                            ProgressInfo = $"Sector data {(dumper.CurrentSector * dumper.SectorSize).AsStorageUnit()} of {(dumper.TotalSectors * dumper.SectorSize).AsStorageUnit()} / File {dumper.CurrentFileNumber} of {dumper.TotalFileCount}";
                        }
                        else
                        {
                            ProgressInfo = $"{AnalyzingMessages[curAnalysisMsgIdx]}{dotsAnimation[animFrameIdx]}";
                            if (++cnt % 5 is 0)
                                animFrameIdx = (animFrameIdx + 1) % dotsAnimation.Length;
                            if (cnt % 600 is 0)
                                curAnalysisMsgIdx = (curAnalysisMsgIdx + 1) % AnalyzingMessages.Length;
                        }
                        Task.Delay(100, combinedToken.Token).GetAwaiter().GetResult();
                    } while (!combinedToken.Token.IsCancellationRequested);
                }
                catch (TaskCanceledException)
                {
                }
            });
            monitor.Start();
            await dumper.DumpAsync(settings.OutputDir).WaitAsync(dumper.Cts.Token).ConfigureAwait(false);
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
            dumper.Cts.Cancel();
        }

        ResetTaskbarProgress();
        DumpingInProgress = false;
        CanEditSettings = true;
        DumperIsReady = false;
        FoundDisc = false;
        await Task.Yield();
        if (dumper.Cts.IsCancellationRequested
            || LastOperationSuccess is false
            || LastOperationWarning is true)
            return;

        Success = dumper.ValidationStatus is not false && dumper.BrokenFiles.Count is 0;
        if (Success == false)
        {
            StepTitle = "Dump is not valid";
            if (dumper.BrokenFiles.Count > 0)
                StepSubtitle = $"{dumper.BrokenFiles.Count} invalid file{(dumper.BrokenFiles.Count == 1 ? "" : "s")}";
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
        dumper?.Dispose();
        OnDisposePlatform();
    }

    partial void OnDisposePlatform();
}