using System;
using System.Collections.Specialized;
using System.IO;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrdLibraryClient;
using Ps3DiscDumper;
using Ps3DiscDumper.Utils;
using UI.Avalonia.Utils;

namespace UI.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty] private string tintColor = "#ffffff";
    [ObservableProperty] private double tintOpacity = 1.0;
    [ObservableProperty] private double materialOpacity = 0.69;
    [ObservableProperty] private double luminosityOpacity = 1.0;
    [ObservableProperty] private string accentColor = ThemeConsts.AccentColor;
    
    [ObservableProperty] private string titleWithVersion = "PS3 Disc Dumper";
    [ObservableProperty] private string stepTitle = "Please insert a PS3 game disc";
    [ObservableProperty] private string stepSubtitle = "";

    [ObservableProperty] private bool foundDisc = false;
    [ObservableProperty] private bool dumperIsReady = false;
    [ObservableProperty] private bool dumpingInProgress = false;
    
    [ObservableProperty] private string productCode = "BLUS69420";
    [ObservableProperty] private string gameTitle = "Knack 0";
    [ObservableProperty] private string discSizeInfo = "0 bytes (0 files)";
    [ObservableProperty] private string discKeyName = "knack_0.ird";
    [ObservableProperty] private int progress = 0;
    [ObservableProperty] private string progressInfo = "1/100 files (10/100 GB)";

    [ObservableProperty] private string updateInfo = "";
    
    
    private Dumper? dumper;
    private string updateUrl = "https://github.com/13xforever/ps3-disc-dumper/releases/latest";

    private static readonly NameValueCollection RegionMapping = new()
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

    ManagementEventWatcher? newDeviceWatcher, removedDeviceWatcher;
    
    public MainWindowViewModel()
    {
        if (!OperatingSystem.IsWindows())
            return;

        return;
#pragma warning disable CA1416
        newDeviceWatcher = new("select * from __InstanceCreationEvent within 1 where TargetInstance ISA 'Win32_CDROMDrive'");
        newDeviceWatcher.EventArrived += (_, _) => { ScanDiscsAsync().Wait(); };
        newDeviceWatcher.Start();

        removedDeviceWatcher = new("select * from __InstanceDeletionEvent within 1 where TargetInstance ISA 'Win32_CDROMDrive'");
        removedDeviceWatcher.EventArrived += (src, evt) =>
        {
            if (dumper is null)
                return;

            if (evt.NewEvent.Properties["TargetInstance"].Value is not ManagementBaseObject obj
                || obj.Properties["Drive"].Value is not string str
                || str[0] != dumper.Drive)
                return;
                
            dumper.Cts.Cancel();
            dumper = null;
            ResetViewModel();
        };
        removedDeviceWatcher.Start();
#pragma warning restore CA1416
    }

    [RelayCommand]
    private void ResetViewModel()
    {
        TitleWithVersion = "PS3 Disc Dumper v" + Dumper.Version;
        StepTitle = "Please insert a PS3 game disc";
        StepSubtitle = "";
        FoundDisc = false;
        DumperIsReady = false;
        DumpingInProgress = false;
        ProductCode = "";
        GameTitle = "";
        DiscSizeInfo = "";
        DiscKeyName = "";
        Progress = 0;
        ProgressInfo = "";
    }

    [RelayCommand]
    private async Task ScanDiscsAsync()
    {
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
                        [Patterns.Region] = RegionMapping[d.ProductCode?[2..4] ?? ""],
                    };
                    return PatternFormatter.Format($"%{Patterns.Title}% [%{Patterns.ProductCode}%]", items);
                });
        } catch {}
        if (dumper.ProductCode is not { Length: > 0 } || dumper.Cts.IsCancellationRequested)
        {
            ResetViewModel();
            return;
        }

        ProductCode = dumper.ProductCode;
        GameTitle = dumper.Title;
        DiscSizeInfo = $"{dumper.TotalFileSize.AsStorageUnit()} ({dumper.TotalFileCount} files)";

        StepTitle = "Looking for a disc key";
        StepSubtitle = "Checking IRD and Redump data sets…";
        try
        {
            await dumper.FindDiscKeyAsync(@".\ird").WaitAsync(dumper.Cts.Token);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to find matching key");
            dumper.Cts.Cancel();
            FoundDisc = false;
            //MessageBox.Show(e.Message, "Disc check error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //ResetViewModel();
            //rescan
            return;
        }
        StepSubtitle = "";
        if (dumper.DiscKeyFilename is {Length: >0})
            DiscKeyName = Path.GetFileName(dumper.DiscKeyFilename);
        else
            DiscKeyName = "No match found";

        StepTitle = "Ready to dump";
        DumperIsReady = true;
    }

    [RelayCommand]
    private async Task DumpDiscAsync()
    {
        if (dumper is null)
        {
            //message box
            return;
        }

        StepTitle = "Dumping the disc";
        StepSubtitle = "Decrypting and copying the data…";
        DumpingInProgress = true;
        
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
                        Task.Delay(1000, combinedToken.Token).GetAwaiter().GetResult();
                    } while (!combinedToken.Token.IsCancellationRequested);
                }
                catch (TaskCanceledException) { }
            });
            monitor.Start();
            await dumper.DumpAsync(@".\").WaitAsync(dumper.Cts.Token);
            threadCts.Cancel();
            monitor.Join(100);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to dump the disc");
            //MessageBox.Show(e.Message, "Disc dumping error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            dumper.Cts.Cancel();
        }

        DumpingInProgress = false;
        if (dumper.ValidationStatus == false)
        {
            StepTitle = "Dump is corrupted";
            if (dumper.BrokenFiles.Count > 0)
                StepSubtitle = $"{dumper.BrokenFiles.Count} invalid file{(dumper.BrokenFiles.Count == 1 ? "" : "s")}";
        }
        else
        {
            StepTitle = "Files are decrypted and copied";
            if (dumper.ValidationStatus == true)
            {
                StepSubtitle = "Disc copy matches another verified copy";
            }
            else
            {
                StepSubtitle = "No reading errors were detected";
            }
        }
    }

    public void Dispose()
    {
        dumper?.Dispose();
        if (!OperatingSystem.IsWindows())
            return;
        
        newDeviceWatcher?.Stop();
        newDeviceWatcher?.Dispose();
        removedDeviceWatcher?.Stop();
        removedDeviceWatcher?.Dispose();
    }
}