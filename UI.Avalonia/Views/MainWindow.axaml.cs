using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using IrdLibraryClient;
using UI.Avalonia.Utils;
using UI.Avalonia.Utils.ColorPalette;
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
        base.Show();
        App.OnThemeChanged(this, EventArgs.Empty);
        App.OnPlatformColorsChanged(this, PlatformSettings?.GetColorValues() ?? new PlatformColorValues
        {
            AccentColor1 = Color.Parse(ThemeConsts.BrandColor),
            AccentColor2 = Color.Parse(ThemeConsts.BrandColor),
            AccentColor3 = Color.Parse(ThemeConsts.BrandColor),
        });
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
            Log.Debug("Main window is loaded, trying to scan the disc…");
            mvm.ResetViewModelCommand.Execute(null);
            mvm.ScanDiscsCommand.Execute(null);
        }, DispatcherPriority.Background);
    }

    partial void OnLoadedPlatform();
    partial void OnClosingPlatform();

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        OnClosingPlatform();
        if (DataContext is not MainWindowViewModel vm)
            return;
        
        if (vm.CurrentPage is MainViewModel mvm)
            mvm.dumper?.Cts.Cancel();
        
        vm.Dispose();
    }
}