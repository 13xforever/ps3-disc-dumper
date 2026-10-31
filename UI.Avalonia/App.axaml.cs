using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentAvalonia.Styling;
using Ps3DiscDumper;
using Ps3DiscDumper.Utils;
using UI.Avalonia.Utils.ColorPalette;
using UI.Avalonia.ViewModels;
using UI.Avalonia.Views;

namespace UI.Avalonia;

public partial class App : Application
{
    private static readonly WindowTransparencyLevel[] DesiredTransparencyHints =
    [
        WindowTransparencyLevel.Mica,
        WindowTransparencyLevel.AcrylicBlur,
        WindowTransparencyLevel.None,
    ];

    private readonly Lazy<bool> isMicaCapable = new(() =>
        Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: Window w }
        && w.ActualTransparencyLevel == WindowTransparencyLevel.Mica
    );

    private static ResourceDictionary? accentColorsDictionary = null;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var safeToRun = SecurityEx.IsSafe(desktop.Args);
            Window w;
            ViewModelBase vm;
            if (safeToRun)
            {
                var mainViewModel = new MainWindowViewModel();
                vm = mainViewModel.CurrentPage;
                SetSymbolFont(vm);
                w = new MainWindow { DataContext = mainViewModel };
            }
            else
            {
                vm = new ErrorStubViewModel();
                SetSymbolFont(vm);
                w = new ErrorStub { DataContext = vm };
            }
            desktop.MainWindow = w;
            desktop.MainWindow.Activated += OnActivated;
            desktop.MainWindow.Deactivated += OnDeactivated;
            desktop.MainWindow.ActualThemeVariantChanged += OnThemeChanged;
#if LINUX
            desktop.MainWindow.Loaded += OnLoaded;
#endif
            if (w.GetPlatformSettings() is { } ps)
                ps.ColorValuesChanged += OnPlatformColorsChanged;
            
            ApplyAccentSetting();
            vm.MicaEnabled = isMicaCapable.Value;
            vm.AcrylicEnabled = w.ActualTransparencyLevel == WindowTransparencyLevel.AcrylicBlur;

            var systemFonts = FontManager.Current.SystemFonts;
            if (!OperatingSystem.IsMacOS() // apparently macos returns true for this font even though it's not true
                && systemFonts.TryGetGlyphTypeface("Segoe UI Variable Text", FontStyle.Normal, FontWeight.Normal, FontStretch.Normal, out _))
                w.FontFamily = new("Segoe UI Variable Text");
        }
        base.OnFrameworkInitializationCompleted();
    }

    internal static void OnThemeChanged(object? sender, EventArgs e)
    {
        ViewModelBase vm;
        if (sender is not Window w)
            return;
        
        ApplyAccentSetting();
        if (w is { DataContext: MainWindowViewModel { CurrentPage: {} vm1 } })
            vm = vm1;
        else if (w is { DataContext: ViewModelBase vm2 })
            vm = vm2;
        else
            return;

        if (w.ActualThemeVariant == ThemeVariant.Light)
        {
            vm.TintColor = ThemeConsts.LightThemeTintColor;
            vm.TintOpacity = ThemeConsts.LightThemeTintOpacity;
            vm.ColorPalette = ThemeConsts.Light;
        }
        else if (w.ActualThemeVariant == ThemeVariant.Dark)
        {
            vm.TintColor = ThemeConsts.DarkThemeTintColor;
            vm.TintOpacity = ThemeConsts.DarkThemeTintOpacity;
            vm.ColorPalette = ThemeConsts.Dark;
        }
    }

    private static void OnPlatformColorsChanged(object? sender, PlatformColorValues e)
        => ApplyAccentSetting(e);

    internal static void ApplyAccentSetting(PlatformColorValues? pcv = null)
    {
        if (Current?.Styles.OfType<FluentAvaloniaTheme>().FirstOrDefault() is not { } theme)
            return;

        if (SettingsProvider.Settings.PreferSystemAccent && pcv is null)
        {
            if (Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: { } w }
                || w.GetPlatformSettings() is not { } ps)
                return;

            pcv = ps.GetColorValues();
        }
        
        if (SettingsProvider.Settings.PreferSystemAccent)
        {
            var systemAccentColor = pcv!.AccentColor1;
#if WINDOWS
            ResetCustomAccentColor(theme);
#else
            SetCustomAccentColor(systemAccentColor, theme);
#endif
            Current.Resources["CaptionGlyphInactiveColor"] = ThemeConsts.InactiveSystemButtonForegroundColor(systemAccentColor);
        }
        else
        {
            var brandColor = Color.Parse(ThemeConsts.BrandColor);
            SetCustomAccentColor(brandColor, theme);
            Current.Resources["CaptionGlyphInactiveColor"] = ThemeConsts.InactiveSystemButtonForegroundColor(brandColor);
        }
    }

    private static void SetCustomAccentColor(Color color, FluentAvaloniaTheme theme)
    {
        if (Current is null)
            return;

        var accentPalette = AccentColorGenerator.Get(color);
        ResetCustomAccentColor(theme);
        theme.CustomAccentColor = color;
        accentColorsDictionary = new()
        {
            { "SystemAccentColor", color },
            { "SystemAccentColorLight1", accentPalette.AccentColorLight1 },
            { "SystemAccentColorLight2", accentPalette.AccentColorLight2 },
            { "SystemAccentColorLight3", accentPalette.AccentColorLight3 },
            { "SystemAccentColorDark1", accentPalette.AccentColorDark1 },
            { "SystemAccentColorDark2", accentPalette.AccentColorDark2 },
            { "SystemAccentColorDark3", accentPalette.AccentColorDark3 }
        };
        Current.Resources.MergedDictionaries.Add(accentColorsDictionary);
    }

    private static void ResetCustomAccentColor(FluentAvaloniaTheme theme)
    {
        theme.CustomAccentColor = null;
        if (Current is null)
            return;
        
        if (accentColorsDictionary is not null)
            Current.Resources.MergedDictionaries.Remove(accentColorsDictionary);
        accentColorsDictionary = null;
    }

    private static void SetSymbolFont(ViewModelBase vm)
    {
        if (OperatingSystem.IsMacOS())
            return; // TryGetGlyphTypeface always returns true but yields a fallback fonts with missing symbols.

        var systemFonts = FontManager.Current.SystemFonts;
        if (systemFonts.TryGetGlyphTypeface("Segoe Fluent Icons", FontStyle.Normal, FontWeight.Normal, FontStretch.Normal, out _))
            vm.SymbolFontFamily = new("Segoe Fluent Icons");
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        if (sender is not Window w)
            return;

        if (isMicaCapable.Value && SettingsProvider.Settings.EnableTransparency)
            w.TransparencyLevelHint = DesiredTransparencyHints;
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        if (sender is not Window w)
            return;

        if (isMicaCapable.Value)
            w.TransparencyLevelHint = [];
        if (w.DataContext is MainWindowViewModel vm)
        {
            if (w.ActualThemeVariant == ThemeVariant.Light)
                vm.CurrentPage.TintColor = ThemeConsts.LightThemeTintColor;
            else if (w.ActualThemeVariant == ThemeVariant.Dark)
                vm.CurrentPage.TintColor = ThemeConsts.DarkThemeTintColor;
        }
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) => OnLoadedPlatform(sender, e);
    partial void OnLoadedPlatform(object? sender, RoutedEventArgs e);
    partial void ApplyPlatformStyles(Window w);
}
