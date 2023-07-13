using System;
using Avalonia.Controls;

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
}