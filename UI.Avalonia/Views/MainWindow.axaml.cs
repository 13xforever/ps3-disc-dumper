using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
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
        base.Show();
        App.OnThemeChanged(this, EventArgs.Empty);
        
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;
        
        vm.ResetViewModelCommand.Execute(null);
        //vm.ScanDiscsCommand.Execute(null);
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;
        
        vm.Dispose();
    }
}