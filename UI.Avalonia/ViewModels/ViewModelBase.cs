using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using UI.Avalonia.Utils;

namespace UI.Avalonia.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty] private string tintColor = "#ffffff";
    [ObservableProperty] private double tintOpacity = 1.0;
    [ObservableProperty] private double materialOpacity = 0.69;
    [ObservableProperty] private double luminosityOpacity = 1.0;
    [ObservableProperty] private string accentColor = ThemeConsts.AccentColor;
    [ObservableProperty] private bool micaEnabled = true;
    [ObservableProperty] private bool acrylicEnabled = false;
    [ObservableProperty] private string dimTextColor = ThemeConsts.LightThemeDimGray;
    [ObservableProperty] private string hoverLayerColor = ThemeConsts.LightThemeLayerHover;
    [NotifyPropertyChangedFor(nameof(SettingsSymbol))]
    [ObservableProperty] private FontFamily symbolFontFamily = new("avares://ps3-disc-dumper/Assets/Fonts#Font Awesome 6 Free");
    [ObservableProperty] private FontFamily largeFontFamily = FontManager.Current.DefaultFontFamily;
    [ObservableProperty] private FontFamily smallFontFamily = FontManager.Current.DefaultFontFamily;
    
    public string SettingsSymbol => SymbolFontFamily.Name is "Segoe Fluent Icons" ? "\ue713" : "\uf013"; 
    
    [ObservableProperty] private string titleWithVersion = "PS3 Disc Dumper";
}