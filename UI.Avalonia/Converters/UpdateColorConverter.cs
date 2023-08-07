using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;
using UI.Avalonia.Utils;
using UI.Avalonia.Utils.ColorPalette;

namespace UI.Avalonia.Converters;

public class UpdateColorConverter: IValueConverter
{
    private static readonly IBrush AccentBrush = BrushConverter.Parse(ThemeConsts.AccentColor);
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        IBrush dimGrey = Brushes.Red;
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: Window w })
            return dimGrey;

        if (w.ActualThemeVariant == ThemeVariant.Light)
            dimGrey = BrushConverter.Parse(ThemeConsts.LightThemeDimGray);
        else if (w.ActualThemeVariant == ThemeVariant.Dark)
            dimGrey = BrushConverter.Parse(ThemeConsts.DarkThemeDimGray);
        return value is true ? dimGrey : AccentBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}