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
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            desktop.MainWindow.ActualThemeVariantChanged += OnThemeChanged;
        }
        base.OnFrameworkInitializationCompleted();
    }

    internal static void OnThemeChanged(object? sender, EventArgs e)
    {
        if (sender is not Window { DataContext: MainWindowViewModel viewModel } window)
            return;

        var useAcrylic = window.ActualTransparencyLevel == WindowTransparencyLevel.Mica
                         || window.ActualTransparencyLevel == WindowTransparencyLevel.AcrylicBlur;
        if (!useAcrylic)
        {
            viewModel.MaterialOpacity = 1.0;
            viewModel.LuminosityOpacity = 1.0;
        }
        if (window.ActualThemeVariant == ThemeVariant.Light)
        {
            viewModel.TintColor = ThemeConsts.LightThemeTintColor;
            viewModel.TintOpacity = ThemeConsts.LightThemeTintOpacity;
            if (useAcrylic)
            {
                viewModel.MaterialOpacity = ThemeConsts.LightThemeMaterialOpacity;
                viewModel.LuminosityOpacity = ThemeConsts.LightThemeLuminosityOpacity;
            }
        }
        else if (window.ActualThemeVariant == ThemeVariant.Dark)
        {
            viewModel.TintColor = ThemeConsts.DarkThemeTintColor;
            viewModel.TintOpacity = ThemeConsts.DarkThemeTintOpacity;
            if (useAcrylic)
            {
                viewModel.MaterialOpacity = ThemeConsts.DarkThemeMaterialOpacity;
                viewModel.LuminosityOpacity = ThemeConsts.DarkThemeLuminosityOpacity;
            }
        }
    }
}