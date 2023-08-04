using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using UI.Avalonia.Utils;
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
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            desktop.MainWindow.Activated += OnActivated;
            desktop.MainWindow.Deactivated += OnDeactivated;
            desktop.MainWindow.ActualThemeVariantChanged += OnThemeChanged;

            if (desktop is { MainWindow.DataContext: MainWindowViewModel vm })
                vm.MicaEnabled = isMicaCapable.Value;
            /*
            if (isMicaCapable.Value && desktop.MainWindow is Window w)
                RenderOptions.SetTextRenderingMode(w, TextRenderingMode.Antialias);
            */
        }
        base.OnFrameworkInitializationCompleted();
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        if (sender is not Window w)
            return;

        if (isMicaCapable.Value)
            w.TransparencyLevelHint = DesiredTransparencyHints;
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        if (sender is not Window { DataContext: MainWindowViewModel vm } w)
            return;

        if (isMicaCapable.Value)
            w.TransparencyLevelHint = Array.Empty<WindowTransparencyLevel>();
        if (w.ActualThemeVariant == ThemeVariant.Light)
            vm.TintColor = ThemeConsts.LightThemeTintColor;
        else if (w.ActualThemeVariant == ThemeVariant.Dark)
            vm.TintColor = ThemeConsts.DarkThemeTintColor;
    }

    internal static void OnThemeChanged(object? sender, EventArgs e)
    {
        if (sender is not Window { DataContext: MainWindowViewModel vm } window)
            return;

        if (window.ActualThemeVariant == ThemeVariant.Light)
        {
            vm.TintColor = ThemeConsts.LightThemeTintColor;
            vm.DimTextColor = ThemeConsts.LightThemeDimGray;
            vm.HoverLayerColor = ThemeConsts.LightThemeLayerHover;
        }
        else if (window.ActualThemeVariant == ThemeVariant.Dark)
        {
            vm.TintColor = ThemeConsts.DarkThemeTintColor;
            vm.DimTextColor = ThemeConsts.DarkThemeDimGray;
            vm.HoverLayerColor = ThemeConsts.DarkThemeLayerHover;
        }
    }
}