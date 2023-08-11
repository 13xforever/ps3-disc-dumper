using System;
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
    [ObservableProperty] private string startButtonCaption = "Start";

    [ObservableProperty] private bool foundDisc;
    [ObservableProperty] private bool dumperIsReady;
    [ObservableProperty] private bool dumpingInProgress;

    [ObservableProperty] private bool discInfoExpanded = SettingsProvider.Settings.ShowDetails;
    [ObservableProperty] private string productCode = "";
    [ObservableProperty] private string gameTitle = "";
    [ObservableProperty] private string discSizeInfo = "";
    [ObservableProperty] private string discKeyName = "";
    [ObservableProperty] private int progress;
    [ObservableProperty] private int progressMax = 10_000;
    [ObservableProperty] private string progressInfo = "";
    [ObservableProperty] private bool? success;
    [ObservableProperty] private bool? validated;

    internal Dumper? dumper;

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
        StepTitle = "Please insert a PS3 game disc";
        StepSubtitle = "";
        LastOperationSuccess = true;
        LastOperationWarning = false;
        StartButtonCaption = "Start";
        FoundDisc = false;
        DumperIsReady = false;
        DumpingInProgress = false;
        CanEditSettings = true;
        ProductCode = "";
        GameTitle = "";
        DiscSizeInfo = "";
        DiscKeyName = "";
        Progress = 0;
        ProgressInfo = "";
        Success = null;
        Validated = null;
        ResetTaskbarProgress();
    }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    [RelayCommand]
    private void ScanDiscs() => Dispatcher.UIThread.Post(() => ScanDiscsAsync(), DispatcherPriority.Background);

    [RelayCommand]
    private void DumpDisc() => Dispatcher.UIThread.Post(() => DumpDiscAsync(), DispatcherPriority.Background);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

    [RelayCommand]
    private void CancelDump()
    {
        dumper?.Cts.Cancel();
    }
    
    private async Task ScanDiscsAsync()
    {
        ResetViewModel();
        
        FoundDisc = true;
        StepTitle = "Scanning disc drives";
        StepSubtitle = "Checking the inserted disc…";
        dumper?.Dispose();
        dumper = new(new());
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

        StepTitle = "Looking for a disc key";
        StepSubtitle = "Checking IRD and Redump data sets…";
        try
        {
            await dumper.FindDiscKeyAsync(settings.IrdDir).WaitAsync(dumper.Cts.Token).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to find a matching key");
            dumper.Cts.Cancel();
            FoundDisc = false;
            StepTitle = "Disc check error";
            StepSubtitle = e.Message;
            LastOperationSuccess = false;
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
        else
        {
            StepTitle = "Ready to dump";
        }
        DumperIsReady = true;
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
        ProgressInfo = "Analyzing the file structure";
        LastOperationSuccess = true;
        LastOperationWarning = false;
        DumpingInProgress = true;
        CanEditSettings = false;
        EnableTaskbarProgress();

        try
        {
            var threadCts = new CancellationTokenSource();
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(threadCts.Token, dumper.Cts.Token);
            var monitor = new Thread(() =>
            {
                try
                {
                    do
                    {
                        if (dumper.TotalSectors > 0 && !dumper.Cts.IsCancellationRequested)
                        {
                            Progress = (int)(dumper.CurrentSector * 10000L / dumper.TotalSectors);
                            ProgressInfo = $"Sector data {(dumper.CurrentSector * dumper.SectorSize).AsStorageUnit()} of {(dumper.TotalSectors * dumper.SectorSize).AsStorageUnit()} / File {dumper.CurrentFileNumber} of {dumper.TotalFileCount}";
                        }
                        Task.Delay(200, combinedToken.Token).GetAwaiter().GetResult();
                    } while (!combinedToken.Token.IsCancellationRequested);
                }
                catch (TaskCanceledException)
                {
                }
            });
            monitor.Start();
            await dumper.DumpAsync(settings.OutputDir).WaitAsync(dumper.Cts.Token);
            threadCts.Cancel();
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
        if (dumper.Cts.IsCancellationRequested
            || LastOperationSuccess is false
            || LastOperationWarning is true)
            return;

        Success = dumper.ValidationStatus is not false;
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