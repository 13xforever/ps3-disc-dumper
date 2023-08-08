using System;
using System.Collections.Concurrent;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace UI.Avalonia.Converters;

public class ColorConverter: IValueConverter
{
    private static readonly ConcurrentDictionary<string, Color> KnownColors = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string { Length: > 0 } s)
            return Parse(s);
        return default(Color);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color c)
            return c.ToString();
        return default(Color).ToString();
    }

    internal static Color Parse(string color)
    {
        if (KnownColors.TryGetValue(color, out var result))
            return result;

        result = Color.Parse(color);
        KnownColors.TryAdd(color, result);
        return result;
    }
}