using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Ps3DiscDumper;
using Ps3DiscDumper.Utils;
using UI.Avalonia.Utils.ColorPalette;
using UI.Avalonia.ViewModels;
using UI.Avalonia.Views;

namespace UI.Avalonia;

public partial class App : Application
{
    private static readonly WindowTransparencyLevel[] DesiredTransparencyHints =
    {
        WindowTransparencyLevel.Mica,
        WindowTransparencyLevel.AcrylicBlur,
        WindowTransparencyLevel.None,
    };

    private readonly Lazy<bool> isMicaCapable = new(() =>
        Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: Window w }
        && w.ActualTransparencyLevel == WindowTransparencyLevel.Mica
    );

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
            if (w.PlatformSettings is { } ps)
                ps.ColorValuesChanged += OnPlatformColorsChanged;

            vm.MicaEnabled = isMicaCapable.Value;
            vm.AcrylicEnabled = w.ActualTransparencyLevel == WindowTransparencyLevel.AcrylicBlur;

            var systemFonts = FontManager.Current.SystemFonts;
            if (systemFonts.TryGetGlyphTypeface("Segoe UI Variable Text", FontStyle.Normal, FontWeight.Normal, FontStretch.Normal, out _))
                w.FontFamily = new("Segoe UI Variable Text");
            else if (systemFonts.TryGetGlyphTypeface("Segoe UI", FontStyle.Normal, FontWeight.Normal, FontStretch.Normal, out _))
                w.FontFamily = new("Segoe UI");
        }
        base.OnFrameworkInitializationCompleted();
    }

    private static void SetSymbolFont(ViewModelBase vm)
    {
        var systemFonts = FontManager.Current.SystemFonts;
        if (systemFonts.TryGetGlyphTypeface("Segoe Fluent Icons", FontStyle.Normal, FontWeight.Normal, FontStretch.Normal, out _))
            vm.SymbolFontFamily = new("Segoe Fluent Icons");
        if (systemFonts.TryGetGlyphTypeface("Segoe UI Variable Small", FontStyle.Normal, FontWeight.Normal, FontStretch.Normal, out _))
            vm.SmallFontFamily = new("Segoe UI Variable Small");
        if (systemFonts.TryGetGlyphTypeface("Segoe UI Variable Display", FontStyle.Normal, FontWeight.Normal, FontStretch.Normal, out _))
            vm.LargeFontFamily = new("Segoe UI Variable Display");
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
        if (sender is not Window { DataContext: MainWindowViewModel vm } w)
            return;

        if (isMicaCapable.Value)
            w.TransparencyLevelHint = Array.Empty<WindowTransparencyLevel>();
        if (w.ActualThemeVariant == ThemeVariant.Light)
            vm.CurrentPage.TintColor = ThemeConsts.LightThemeTintColor;
        else if (w.ActualThemeVariant == ThemeVariant.Dark)
            vm.CurrentPage.TintColor = ThemeConsts.DarkThemeTintColor;
    }

    internal static void OnThemeChanged(object? sender, EventArgs e)
    {
        Window w;
        ViewModelBase vm;
        if (sender is Window { DataContext: MainWindowViewModel { CurrentPage: {} vm1 } } w1)
            (w, vm) = (w1, vm1);
        else if (sender is Window { DataContext: ViewModelBase vm2 } w2)
            (w, vm) = (w2, vm2);
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

    internal static void OnPlatformColorsChanged(object? sender, PlatformColorValues e)
    {
        if (Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime
            {
                MainWindow.DataContext: MainWindowViewModel { CurrentPage: ViewModelBase vm }
            })
            return;

        vm.SystemAccentColor = e.AccentColor1.ToString();
        if (SettingsProvider.Settings.PreferSystemAccent)
            vm.AccentColor = e.AccentColor1.ToString();
        else
            vm.AccentColor = ThemeConsts.BrandColor;
    }
}