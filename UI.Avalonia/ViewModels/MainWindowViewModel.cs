using System;
using System.Collections.Specialized;
using System.Management;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ps3DiscDumper;
using Ps3DiscDumper.Utils;
using UI.Avalonia.Utils;

namespace UI.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
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
    [ObservableProperty] private string productCode = "BLUS69420";
    [ObservableProperty] private string gameTitle = "Knack 0";
    [ObservableProperty] private string discSizeInfo = "0 bytes (0 files)";
    [ObservableProperty] private string discKeyName = "knack_0.ird";

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

    ManagementEventWatcher newDeviceWatcher, removedDeviceWatcher;
    
    public MainWindowViewModel()
    {
        if (!OperatingSystem.IsWindows())
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
        ProductCode = "";
        GameTitle = "";
        DiscSizeInfo = "";
        DiscKeyName = "";
    }

    [RelayCommand]
    private async Task ScanDiscsAsync()
    {
        FoundDisc = true;
        StepTitle = "Scanning disc drives";
        StepSubtitle = "Checking the inserted disc…";
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
        StepSubtitle = "";
    }
}