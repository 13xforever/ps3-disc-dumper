using Avalonia.Controls;
using Avalonia.Interactivity;

namespace UI.Avalonia.Views;

public partial class Main : UserControl
{
    public Main() => InitializeComponent();
    
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
    }

    private void OnResized(object? sender, SizeChangedEventArgs e)
    {
        if (e.WidthChanged)
            ProgressBar.Width = 500 - ProgressBar.Margin.Right - e.NewSize.Width;
    }
}