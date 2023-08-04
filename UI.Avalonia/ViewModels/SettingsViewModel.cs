using CommunityToolkit.Mvvm.ComponentModel;
using Ps3DiscDumper.Utils;

namespace UI.Avalonia.ViewModels;

public partial class SettingsViewModel: ViewModelBase
{
    public SettingsViewModel()
    {
        pageTitle = "Settings";
    }
    
    [ObservableProperty] private string outputDir = "";
    [ObservableProperty] private string irdDir = "ird";
    [ObservableProperty] private string dumpNameTemplate = $"%{Patterns.Title}% [%{Patterns.ProductCode}%]";
    [ObservableProperty] private bool discInfoExpanded = true;
    [ObservableProperty] private bool configured;
}