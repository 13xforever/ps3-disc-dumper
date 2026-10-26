using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.VisualTree;
using UI.Avalonia.Utils.ColorPalette;

namespace UI.Avalonia.Views;

public partial class ErrorStub : Window
{
    public ErrorStub()
    {
        InitializeComponent();

        // TODO: remove this workaround once https://github.com/AvaloniaUI/Avalonia/issues/22339 is fixed upstream.
        LearnMoreButton.AttachedToVisualTree += (_, _) =>
        {
            if (LearnMoreButton.GetVisualParent() is TextBlock { IsVisible: false })
                LearnMoreButton.Measure(new(0, double.PositiveInfinity));
        };
    }
    
    public override void Show()
    {

        base.Show();
        App.OnThemeChanged(this, EventArgs.Empty);
        App.OnPlatformColorsChanged(this, Application.Current?.PlatformSettings?.GetColorValues() ?? new PlatformColorValues
        {
            AccentColor1 = Color.Parse(ThemeConsts.BrandColor),
            AccentColor2 = Color.Parse(ThemeConsts.BrandColor),
            AccentColor3 = Color.Parse(ThemeConsts.BrandColor),
        });
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) => OnLoadedPlatform();

    partial void OnLoadedPlatform();

    private void Exit(object? sender, RoutedEventArgs e) => Close();
}