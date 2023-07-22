using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace UI.Avalonia.Converters;

public class UpdateSymbolConverter: SymbolConverterBase, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not null
            ? HasFluentIcons.Value
                ? "\ue946"
                : "\uf05a" //"\uf06a" // exclamation mark in circle
            : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}