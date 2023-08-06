using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;
using UI.Avalonia.Utils;
using UI.Avalonia.Utils.ColorPalette;

namespace UI.Avalonia.Converters;

public class UpdateColorConverter: IValueConverter
{
    private static readonly IBrush AccentBrush = Brush.Parse(ThemeConsts.AccentColor);
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true
            ? parameter is string {Length: >0} currentDimGrey
                ? currentDimGrey
                : ThemeConsts.LightThemeDimGray
            : AccentBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}