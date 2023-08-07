using System;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ps3DiscDumper;
using Ps3DiscDumper.POCOs;

namespace UI.Avalonia.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly SettingsViewModel settingsPage = new();
    private readonly MainViewModel mainPage;

    public MainWindowViewModel() => CurrentPage = mainPage = new(settingsPage);

    [NotifyPropertyChangedFor(nameof(InSettings))]
    [ObservableProperty] private ViewModelBase currentPage;
    public bool InSettings => CurrentPage is SettingsViewModel;
 
    [ObservableProperty] private GitHubReleaseInfo? updateInfo;
    [ObservableProperty] private bool? updateIsPrerelease;
    [ObservableProperty] private string formattedUpdateInfoHeader = "";
    [ObservableProperty] private string formattedUpdateInfoBody = "";
    [ObservableProperty] private string formattedUpdateInfoUrl = $"{SettingsViewModel.ProjectUrl}/releases/latest";
    [ObservableProperty] private string formattedUpdateInfoVersion = "";

    [RelayCommand]
    private void ToggleSettingsPage()
    {
        if (CurrentPage is MainViewModel)
            CurrentPage = settingsPage;
        else
        {
            SettingsProvider.Save();
            CurrentPage = mainPage;
        }
    }

    internal async void CheckUpdatesAsync()
    {
        var (ver, rel) = await Dumper.CheckUpdatesAsync().ConfigureAwait(false);
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
        mainPage.Dispose();
        OnDisposePlatform();
    }

    partial void OnDisposePlatform();
}