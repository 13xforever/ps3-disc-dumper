using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrdLibraryClient;
using Ps3DiscDumper;
using UI.Avalonia;
using UI.Avalonia.Utils.ColorPalette;

namespace UI.Avalonia.ViewModels;

public partial class ViewModelBase: ObservableObject
{
    [ObservableProperty] private static string tintColor = "#ffffff";
    [ObservableProperty] private static double tintOpacity = 1.0;
    [ObservableProperty] private static bool hasSystemAccentColor = Application.Current?.PlatformSettings is not null;
    [ObservableProperty] private static bool micaEnabled = true;
    [ObservableProperty] private static bool acrylicEnabled = false;
    [ObservableProperty] private static bool enableTransparency = SettingsProvider.Settings.EnableTransparency;
    [ObservableProperty] private static bool preferSystemAccent = SettingsProvider.Settings.PreferSystemAccent;
    [ObservableProperty] private static bool stayOnTop = SettingsProvider.Settings.StayOnTop;
    [ObservableProperty] private static bool copyBdmv = SettingsProvider.Settings.CopyBdmv;
    [ObservableProperty] private static bool copyPs3Update = SettingsProvider.Settings.CopyPs3Update;

    [ObservableProperty] private static string dimTextColor = "#00ff00"; //ThemeConsts.LightThemeDimGray;
    [ObservableProperty] private static IPalette colorPalette = ThemeConsts.Debug;
    [ObservableProperty] private static string layer2BackgroundColor = "#ff0000"; //ThemeConsts.LightThemeLayerHover;
    [ObservableProperty] private static string layer2GroundedColor = "#7f0000"; //ThemeConsts.LightThemeLayerHover;
    [NotifyPropertyChangedFor(nameof(SettingsSymbol))]
    [ObservableProperty] private static FontFamily symbolFontFamily = new("avares://ps3-disc-dumper/Assets/Fonts#Font Awesome 6 Free");

    public bool UseSegoeIcons => SymbolFontFamily.Name is "Segoe Fluent Icons";
    public string SettingsSymbol => UseSegoeIcons ? "\ue713" : "\uf013";
    //public string UpdateSymbol => UseSegoeIcons ? "\ue946" : "\uf05a"; //"\uf06a" // exclamation mark in circle
    public string UpdateSymbol => UseSegoeIcons ? "\ue895" : "\uf021";
    public string BackSymbol => UseSegoeIcons ? "\ue72b" : "\uf060";
    public string FolderSymbol => UseSegoeIcons ? "\ue8b7" : "\uf07b";
    public string MoreSymbol => UseSegoeIcons ? "\ue712" : "\uf141";
    public string RenameSymbol => UseSegoeIcons ? "\ue8ac" : "\uf573";
    public string HelpSymbol => UseSegoeIcons ? "\ue8ac" : "\uf573";
    public string HomeSymbol => UseSegoeIcons ? "\ue80f" : "\uf015";
    public string FeedbackSymbol => UseSegoeIcons ? "\ue939" : "\uf086";
    public string TransparencySymbol => UseSegoeIcons ? "\uef20" : "\uf853"; //e8b3
    public string ItemSelectionSymbol => UseSegoeIcons ? "\ue762" : "\uf0ae";
    public string PinSymbol => UseSegoeIcons ? "\ue718" : "\uf08d";
    public string ValidationErrorSymbol => UseSegoeIcons ? "\ue783" : "\uf06a";
    public string ValidationWarningSymbol => UseSegoeIcons ? "\ue7ba" : "\uf071";
    public string ValidationInformationSymbol => UseSegoeIcons ? "\ue946" : "\uf05a";
    public string DiagnosticsSymbol => UseSegoeIcons ? "\ue9d9" : "\uf478";
    public string ColorSymbol => UseSegoeIcons ? "\ue790" : "\uf53f";
    public string SuccessSymbol => UseSegoeIcons ? "\ue73e" : "\uf058";
    public string FailureSymbol => UseSegoeIcons ? "\ueb90" : "\uf06a";

    public static bool IsDebug =>
#if DEBUG
        true;
#else
        false;
#endif

    [ObservableProperty] protected string pageTitle = "PS3 Disc Dumper";// + Dumper.Version;
    [ObservableProperty] private bool canEditSettings = true;

    [RelayCommand]
    private static void OpenUrl(string url)
    {
        ProcessStartInfo psi = OperatingSystem.IsWindows()
            ? new() { FileName = url, UseShellExecute = true, }
            : OperatingSystem.IsLinux()
                ? new() { FileName = "xdg-open", Arguments = url, }
                : new() { FileName = "open", Arguments = url, };
        psi.CreateNoWindow = true;
        try
        {
            using var _ = Process.Start(psi);
        }
        catch (Exception e)
        {
            Log.Warn(e, "Failed to open web URL");
        }
    }

    partial void OnEnableTransparencyChanged(bool value)
    {
        SettingsProvider.Settings = SettingsProvider.Settings with { EnableTransparency = value };
        /* // seems to be broken atm, only works at the time the window is created
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: Window w })
            RenderOptions.SetTextRenderingMode(w, value ? TextRenderingMode.Antialias : TextRenderingMode.SubpixelAntialias);
        */
    }

    partial void OnPreferSystemAccentChanged(bool value)
    {
        SettingsProvider.Settings = SettingsProvider.Settings with { PreferSystemAccent = value };
        App.ApplyAccentSetting();
    }

    partial void OnStayOnTopChanged(bool value)
        => SettingsProvider.Settings = SettingsProvider.Settings with { StayOnTop = value };

    partial void OnCopyBdmvChanged(bool value)
        => SettingsProvider.Settings = SettingsProvider.Settings with { CopyBdmv = value }; 

    partial void OnCopyPs3UpdateChanged(bool value)
        => SettingsProvider.Settings = SettingsProvider.Settings with { CopyPs3Update = value }; 
}
