using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using UI.Avalonia.Utils.ColorPalette;

namespace UI.Avalonia.Converters;

public class ValidationColorConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: Window w })
            return default;
        
        IPalette palette = ThemeConsts.Debug;
        if (w.ActualThemeVariant == ThemeVariant.Light)
            palette = ThemeConsts.Light;
        else if (w.ActualThemeVariant == ThemeVariant.Dark)
            palette = ThemeConsts.Dark;
        return value is bool b 
            ? b
                ? BrushConverter.Parse(palette.StatusSuccessForeground1)
                : BrushConverter.Parse(palette.StatusDangerForeground1)
            : default;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}