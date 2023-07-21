using System;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace UI.Avalonia.Converters;

public class ValidationSymbolConverter: IValueConverter
{
    private static readonly Lazy<bool> HasFluentIcons = new(() => FontManager.Current.SystemFonts.Any(f => f.Name is "Segoe Fluent Icons"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b
            ? HasFluentIcons.Value
                ? b ? "\ue73e" : "\ueb90"
                : b ? "\uf058" : "\uf06a"
            : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}