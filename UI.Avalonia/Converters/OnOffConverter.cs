using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace UI.Avalonia.Converters;

public class OnOffConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "On" : "Off";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}