using System;
using System.Collections.Specialized;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrdLibraryClient;
using Ps3DiscDumper;
using Ps3DiscDumper.POCOs;
using Ps3DiscDumper.Utils;
using ReactiveUI;
using UI.Avalonia.Utils;

namespace UI.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty] private string tintColor = "#ffffff";
    [ObservableProperty] private double tintOpacity = 1.0;
    [ObservableProperty] private double materialOpacity = 0.69;
    [ObservableProperty] private double luminosityOpacity = 1.0;
    [ObservableProperty] private string accentColor = ThemeConsts.AccentColor;
    [ObservableProperty] private string dimTextColor = "#646464";
    [ObservableProperty] private FontFamily symbolFontFamily = new("avares://ps3-disc-dumper/Assets/Fonts#Font Awesome 6 Free Solid");
    [ObservableProperty] private FontFamily largeFontFamily = FontManager.Current.DefaultFontFamily;
    [ObservableProperty] private FontFamily smallFontFamily = FontManager.Current.DefaultFontFamily;

    [ObservableProperty] private GitHubReleaseInfo? updateInfo;
    [ObservableProperty] private bool updateIsPrerelease; 

    [ObservableProperty] private string titleWithVersion = "PS3 Disc Dumper";
    [ObservableProperty] private string stepTitle = "Please insert a PS3 game disc";
    [ObservableProperty] private string stepSubtitle = "";

    [ObservableProperty] private bool foundDisc;
    [ObservableProperty] private bool dumperIsReady;
    [ObservableProperty] private bool dumpingInProgress;

    [ObservableProperty] private bool discInfoExpanded = true;
    [ObservableProperty] private string productCode = "";
    [ObservableProperty] private string gameTitle = "";
    [ObservableProperty] private string discSizeInfo = "";
    [ObservableProperty] private string discKeyName = "";
    [ObservableProperty] private int progress;
    [ObservableProperty] private string progressInfo = "";
    [ObservableProperty] private bool? success;
    [ObservableProperty] private bool? validated;

    internal Dumper? dumper;

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

    private Interaction<UpdateInfoWindowViewModel, bool> ShowUpdateInfoDialog { get; } = new();

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
        Success = null;
        Validated = null;
    }

    [RelayCommand]
    private void ScanDiscs() => Dispatcher.UIThread.Post(ScanDiscsAsync, DispatcherPriority.Background);

    [RelayCommand]
    private void DumpDisc() => Dispatcher.UIThread.Post(DumpDiscAsync, DispatcherPriority.Background);

    private async void ScanDiscsAsync()
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
    
    private async void DumpDiscAsync()
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
                            ProgressInfo = $"Raw sectors {(dumper.CurrentSector * dumper.SectorSize).AsStorageUnit()} of {(dumper.TotalSectors * dumper.SectorSize).AsStorageUnit()} / File {dumper.CurrentFileNumber} of {dumper.TotalFileCount}";
                        }
                        Task.Delay(200, combinedToken.Token).GetAwaiter().GetResult();
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
        DumperIsReady = false;
        FoundDisc = false;
        Success = dumper.ValidationStatus is not false;
        if (Success == false)
        {
            StepTitle = "Dump is corrupted";
            if (dumper.BrokenFiles.Count > 0)
                StepSubtitle = $"{dumper.BrokenFiles.Count} invalid file{(dumper.BrokenFiles.Count == 1 ? "" : "s")}";
        }
        else
        {
            StepTitle = "Files are decrypted and copied";
            if (Success == true)
            {
                StepSubtitle = "Disc copy matches another verified copy";
            }
            else
            {
                StepSubtitle = "No reading errors were detected";
            }
        }
    }

    [RelayCommand]
    private async Task ShowUpdateInfoAsync() => await ShowUpdateInfoDialog.Handle(new() { UpdateInfo = UpdateInfo });

    internal async void CheckUpdatesAsync()
    {
        var (ver, rel) = await Dumper.CheckUpdatesAsync();
        if (ver is null || rel is null)
            return;

        UpdateInfo = rel;
        UpdateIsPrerelease = rel.Prerelease;
        /*
         UpdateInfo = $"""
            v{rel.TagName.TrimStart('v')} is available!
            
            {rel.Name}
            {"".PadRight(rel.Name.Length, '-')}
            
            {rel.Body}
            """;
        if (!string.IsNullOrEmpty(rel.HtmlUrl))
            UpdateUrl = rel.HtmlUrl;
        */
    }
    
    public void Dispose()
    {
        dumper?.Dispose();
        OnDisposePlatform();
    }

    partial void OnDisposePlatform();
}