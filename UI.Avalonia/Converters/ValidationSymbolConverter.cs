using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace UI.Avalonia.Converters;

public class ValidationSymbolConverter: SymbolConverterBase, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b
            ? HasFluentIcons.Value
                ? b ? "\ue73e" : "\ueb90"
                : b ? "\uf058" : "\uf06a"
            : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}