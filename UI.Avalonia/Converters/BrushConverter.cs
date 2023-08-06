using System;
using System.Collections.Concurrent;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace UI.Avalonia.Converters;

public class BrushConverter: IValueConverter
{
    private static readonly ConcurrentDictionary<string, IBrush> KnownBrushes = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string { Length: > 0 } s)
            return Parse(s);
        return default(Brush);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    internal static IBrush Parse(string color)
    {
        if (KnownBrushes.TryGetValue(color, out var result))
            return result;

        var c = Color.Parse(color);
        result = new SolidColorBrush(c, c.A / 255.0);
        KnownBrushes.TryAdd(color, result);
        return result;
    }
}