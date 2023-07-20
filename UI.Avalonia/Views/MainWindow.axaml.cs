using System;
using System.Management;
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
        if (DataContext is MainWindowViewModel vm)
        {
            vm.ResetViewModelCommand.Execute(null);
            //vm.ScanDiscsCommand.Execute(null);
        }
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {

    }
}