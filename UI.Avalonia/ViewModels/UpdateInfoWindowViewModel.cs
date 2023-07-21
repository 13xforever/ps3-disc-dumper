using CommunityToolkit.Mvvm.ComponentModel;
using Ps3DiscDumper.POCOs;

namespace UI.Avalonia.ViewModels;

public partial class UpdateInfoWindowViewModel: ViewModelBase
{
    [ObservableProperty] private string updateVersion = "1.0.0";
    [ObservableProperty] private string updateUrl = "https://github.com/13xforever/ps3-disc-dumper/releases/latest";
    [ObservableProperty] private GitHubReleaseInfo? updateInfo = null;
}