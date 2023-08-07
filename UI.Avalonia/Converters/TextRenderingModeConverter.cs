using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace UI.Avalonia.Converters;

public class TextRenderingModeConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is WindowTransparencyLevel wtl
            ? wtl == WindowTransparencyLevel.Mica || wtl == WindowTransparencyLevel.AcrylicBlur
                ? TextRenderingMode.Antialias
                : TextRenderingMode.SubpixelAntialias
            : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}