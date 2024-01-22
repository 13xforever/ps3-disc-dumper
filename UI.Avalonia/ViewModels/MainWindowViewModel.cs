using System;
using System.Diagnostics;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ps3DiscDumper;
using Ps3DiscDumper.POCOs;

namespace UI.Avalonia.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly SettingsViewModel settingsPage = new();
    private readonly MainViewModel mainPage;
    private Settings oldSettings;

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
        {
            oldSettings = SettingsProvider.Settings;
            CurrentPage = settingsPage;
        }
        else
        {
            SettingsProvider.Save();
            CurrentPage = mainPage;
            if (!mainPage.DumperIsReady)
                return;
            
            var newSettings = SettingsProvider.Settings;
            if (newSettings.DumpNameTemplate != oldSettings.DumpNameTemplate
                || newSettings.OutputDir != oldSettings.OutputDir
                || newSettings.IrdDir != oldSettings.IrdDir
                || newSettings.CopyBdmv != oldSettings.CopyBdmv
                || newSettings.CopyPs3Update != oldSettings.CopyPs3Update)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    mainPage.ResetViewModelCommand.Execute(null);
                    mainPage.ScanDiscsCommand.Execute(null);
                }, DispatcherPriority.Background);
            }
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