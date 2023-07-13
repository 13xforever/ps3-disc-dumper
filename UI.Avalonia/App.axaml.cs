using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using UI.Avalonia.ViewModels;
using UI.Avalonia.Views;

namespace UI.Avalonia;

public partial class App : Application
{
    public MainWindowViewModel MainWindowViewModel = new();
    
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
                DataContext = MainWindowViewModel,
            };
            desktop.MainWindow.ActualThemeVariantChanged += OnThemeChanged;
        }
        base.OnFrameworkInitializationCompleted();
    }

    internal static void OnThemeChanged(object? sender, EventArgs e)
    {
        if (sender is not Window { DataContext: MainWindowViewModel mainWindowViewModel } window)
            return;

        if (window.ActualThemeVariant == ThemeVariant.Light)
        {
            mainWindowViewModel.TintColor = ThemeConsts.LightThemeTintColor;
            mainWindowViewModel.TintOpacity = ThemeConsts.LightThemeTintOpacity;
            mainWindowViewModel.MaterialOpacity = ThemeConsts.LightThemeMaterialOpacity;
            mainWindowViewModel.LuminosityOpacity = ThemeConsts.LightThemeLuminosityOpacity;
        }
        else if (window.ActualThemeVariant == ThemeVariant.Dark)
        {
            mainWindowViewModel.TintColor = ThemeConsts.DarkThemeTintColor;
            mainWindowViewModel.TintOpacity = ThemeConsts.DarkThemeTintOpacity;
            mainWindowViewModel.MaterialOpacity = ThemeConsts.DarkThemeMaterialOpacity;
            mainWindowViewModel.LuminosityOpacity = ThemeConsts.DarkThemeLuminosityOpacity;
        }
    }
}