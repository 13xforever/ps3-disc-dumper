using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using UI.Avalonia.ViewModels;

namespace UI.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public override void Show()
    {
        var systemFonts = FontManager.Current.SystemFonts;
        if (systemFonts.TryGetGlyphTypeface("Segoe UI Variable Text", FontStyle.Normal, FontWeight.Normal, FontStretch.Normal, out _))
            FontFamily = new("Segoe UI Variable Text");
        else if (systemFonts.TryGetGlyphTypeface("Segoe UI", FontStyle.Normal, FontWeight.Normal, FontStretch.Normal, out _))
            FontFamily = new("Segoe UI");
        
        if (DataContext is MainWindowViewModel vm)
        {
            if (systemFonts.TryGetGlyphTypeface("Segoe Fluent Icons", FontStyle.Normal, FontWeight.Normal, FontStretch.Normal, out _))
                vm.SymbolFontFamily = new("Segoe Fluent Icons");
            if (systemFonts.TryGetGlyphTypeface("Segoe UI Variable Small", FontStyle.Normal, FontWeight.Normal, FontStretch.Normal, out _))
                vm.SmallFontFamily = new("Segoe UI Variable Small");
            if (systemFonts.TryGetGlyphTypeface("Segoe UI Variable Display", FontStyle.Normal, FontWeight.Normal, FontStretch.Normal, out _))
                vm.LargeFontFamily = new("Segoe UI Variable Display");
        }
        base.Show();
        App.OnThemeChanged(this, EventArgs.Empty);
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        OnLoadedPlatform();
        if (DataContext is not MainWindowViewModel vm)
            return;

        vm.ResetViewModelCommand.Execute(null);
        vm.CheckUpdatesAsync();
        vm.ScanDiscsCommand.Execute(null);
    }

    partial void OnLoadedPlatform();

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;
        
        vm.Dispose();
    }
}