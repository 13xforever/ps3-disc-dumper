using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace UI.Avalonia;

public class ColorConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string { Length: > 0 } s)
            return Color.Parse(s);
        return default(Color);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color c)
            return c.ToString();
        return default(Color).ToString();
    }
}