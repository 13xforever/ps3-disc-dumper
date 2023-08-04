using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using UI.Avalonia.Utils;

namespace UI.Avalonia.ViewModels;

public partial class ViewModelBase: ObservableObject
{
    [ObservableProperty] private static string tintColor = "#ffffff";
    [ObservableProperty] private static double tintOpacity = 1.0;
    [ObservableProperty] private static double materialOpacity = 0.69;
    [ObservableProperty] private static double luminosityOpacity = 1.0;
    [ObservableProperty] private static string accentColor = ThemeConsts.AccentColor;
    [ObservableProperty] private static bool micaEnabled = true;
    [ObservableProperty] private static bool acrylicEnabled = false;
    [ObservableProperty] private static string dimTextColor = "#00ff00"; //ThemeConsts.LightThemeDimGray;
    [ObservableProperty] private static string layer2BackgroundColor = "#ff0000"; //ThemeConsts.LightThemeLayerHover;
    [ObservableProperty] private static string layer2GroundedColor = "#7f0000"; //ThemeConsts.LightThemeLayerHover;
    [NotifyPropertyChangedFor(nameof(SettingsSymbol))]
    [ObservableProperty] private static FontFamily symbolFontFamily = new("avares://ps3-disc-dumper/Assets/Fonts#Font Awesome 6 Free");
    [ObservableProperty] private static FontFamily largeFontFamily = FontManager.Current.DefaultFontFamily;
    [ObservableProperty] private static FontFamily smallFontFamily = FontManager.Current.DefaultFontFamily;

    private bool UseSegoeIcons => SymbolFontFamily.Name is "Segoe Fluent Icons";
    public string SettingsSymbol => UseSegoeIcons ? "\ue713" : "\uf013";
    public string UpdateSymbol => UseSegoeIcons ? "\ue946" : "\uf05a"; //"\uf06a" // exclamation mark in circle
    public string BackSymbol => UseSegoeIcons ? "\ue72b" : "\uf060";
    public string FolderSymbol => UseSegoeIcons ? "\ue8b7" : "\uf07b";
    public string RenameSymbol => UseSegoeIcons ? "\ue8ac" : "\uf573";
    public string HelpSymbol => UseSegoeIcons ? "\ue8ac" : "\uf573";
    
    [ObservableProperty] protected string pageTitle = "PS3 Disc Dumper";
    [ObservableProperty] private bool canEditSettings = true;
}