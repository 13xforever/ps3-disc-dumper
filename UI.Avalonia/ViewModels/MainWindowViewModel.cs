using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ps3DiscDumper;
using Ps3DiscDumper.POCOs;

namespace UI.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly ViewModelBase[] pages =
    {
        new MainViewModel(),
        new SettingsViewModel(),
    };

    [ObservableProperty] private ViewModelBase currentPage;

    public MainWindowViewModel() => CurrentPage = pages[0];

    [ObservableProperty] private GitHubReleaseInfo? updateInfo;
    [ObservableProperty] private bool updateIsPrerelease;
    [ObservableProperty] private string formattedUpdateInfoHeader = "";
    [ObservableProperty] private string formattedUpdateInfoBody = "";
    [ObservableProperty] private string formattedUpdateInfoUrl = "https://github.com/13xforever/ps3-disc-dumper/releases/latest";
    [ObservableProperty] private string formattedUpdateInfoVersion = "";

    [RelayCommand]
    private void OpenUrl(string url)
    {
        ProcessStartInfo psi = OperatingSystem.IsWindows()
            ? new() { FileName = url, UseShellExecute = true, }
            : new() { FileName = "open", Arguments = url, };
        psi.CreateNoWindow = true;
        try { using var _ = Process.Start(psi); } catch { }
    }

    internal async void CheckUpdatesAsync()
    {
        var (ver, rel) = await Dumper.CheckUpdatesAsync();
        if (ver is null || rel is null)
            return;

        UpdateInfo = rel;
        UpdateIsPrerelease = rel.Prerelease;
        FormattedUpdateInfoHeader = rel.Name;
        FormattedUpdateInfoVersion = $"Download v{rel.TagName.TrimStart('v')}";
        FormattedUpdateInfoBody = rel.Body;
    }
    
    public void Dispose()
    {
        foreach (var p in pages)
            if (p is IDisposable dp)
                dp.Dispose();
        OnDisposePlatform();
    }

    partial void OnDisposePlatform();
}