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
        if (sender is not Window { DataContext: MainWindowViewModel vm } window)
            return;

        var useAcrylic = window.ActualTransparencyLevel == WindowTransparencyLevel.Mica
                         || window.ActualTransparencyLevel == WindowTransparencyLevel.AcrylicBlur;
        if (!useAcrylic)
        {
            vm.MaterialOpacity = 1.0;
            vm.LuminosityOpacity = 1.0;
        }
        if (window.ActualThemeVariant == ThemeVariant.Light)
        {
            vm.TintColor = ThemeConsts.LightThemeTintColor;
            vm.TintOpacity = ThemeConsts.LightThemeTintOpacity;
            if (useAcrylic)
            {
                vm.MaterialOpacity = ThemeConsts.LightThemeMaterialOpacity;
                vm.LuminosityOpacity = ThemeConsts.LightThemeLuminosityOpacity;
            }
            vm.DimTextColor = ThemeConsts.LightThemeDimGray;
        }
        else if (window.ActualThemeVariant == ThemeVariant.Dark)
        {
            vm.TintColor = ThemeConsts.DarkThemeTintColor;
            vm.TintOpacity = ThemeConsts.DarkThemeTintOpacity;
            if (useAcrylic)
            {
                vm.MaterialOpacity = ThemeConsts.DarkThemeMaterialOpacity;
                vm.LuminosityOpacity = ThemeConsts.DarkThemeLuminosityOpacity;
            }
            vm.DimTextColor = ThemeConsts.DarkThemeDimGray;
        }
    }
}