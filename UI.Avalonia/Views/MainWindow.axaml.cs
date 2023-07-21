using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using UI.Avalonia.ViewModels;
using UI.Avalonia.Utils;

namespace UI.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public override void Show()
    {
        base.Show();
        App.OnThemeChanged(this, EventArgs.Empty);
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        OnLoadedPlatform();
        if (DataContext is not MainWindowViewModel vm)
            return;
        
        vm.ResetViewModelCommand.Execute(null);
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