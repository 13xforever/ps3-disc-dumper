using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace UI.Avalonia.Views;

public partial class ErrorStub : Window
{
    public ErrorStub()
    {
        InitializeComponent();
    }
    
    public override void Show()
    {
        base.Show();
        App.OnThemeChanged(this, EventArgs.Empty);
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) => OnLoadedPlatform();

    partial void OnLoadedPlatform();

    private void Exit(object? sender, RoutedEventArgs e) => Close();

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // TODO: remove this workaround once https://github.com/AvaloniaUI/Avalonia/issues/22339 is fixed upstream.
        if (sender is Button btn && btn.GetVisualParent() is TextBlock { IsVisible: false })
            btn.Measure(new(0, double.PositiveInfinity));
    }
}