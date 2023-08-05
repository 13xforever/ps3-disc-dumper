using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using UI.Avalonia.ViewModels;

namespace UI.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
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
                vm.CurrentPage.SymbolFontFamily = new("Segoe Fluent Icons");
            if (systemFonts.TryGetGlyphTypeface("Segoe UI Variable Small", FontStyle.Normal, FontWeight.Normal, FontStretch.Normal, out _))
                vm.CurrentPage.SmallFontFamily = new("Segoe UI Variable Small");
            if (systemFonts.TryGetGlyphTypeface("Segoe UI Variable Display", FontStyle.Normal, FontWeight.Normal, FontStretch.Normal, out _))
                vm.CurrentPage.LargeFontFamily = new("Segoe UI Variable Display");
        }
        base.Show();
        App.OnThemeChanged(this, EventArgs.Empty);
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (!IsExtendedIntoWindowDecorations)
        {
            TitleButtons.Margin = new(0, 6, 4, 0);
            var buttons = TitleButtons.GetSelfAndVisualDescendants().OfType<Button>();
            foreach (var b in buttons)
            {
                b.CornerRadius = new(4);
                b.Margin = new(4, 0);
                b.Width = 40;
                b.Height = 36;
            }
        }
        
        OnLoadedPlatform();
        if (DataContext is not MainWindowViewModel mwvm)
            return;
        
        Dispatcher.UIThread.Post(() => { mwvm.CheckUpdatesAsync(); }, DispatcherPriority.Background);
        if (mwvm.CurrentPage is not MainViewModel mvm)
            return;
        
        Dispatcher.UIThread.Post(() =>
        {
            mvm.ResetViewModelCommand.Execute(null);
            mvm.ScanDiscsCommand.Execute(null);
        }, DispatcherPriority.Background);
    }

    partial void OnLoadedPlatform();

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;
        
        vm.Dispose();
    }
}