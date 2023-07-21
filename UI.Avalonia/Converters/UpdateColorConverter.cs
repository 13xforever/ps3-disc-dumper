using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using UI.Avalonia.Utils;

namespace UI.Avalonia.Converters;

public class UpdateColorConverter: IValueConverter
{
    private static readonly IBrush AccentBrush = Brush.Parse(ThemeConsts.AccentColor);
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true
            ? Brushes.DimGray
            : AccentBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}