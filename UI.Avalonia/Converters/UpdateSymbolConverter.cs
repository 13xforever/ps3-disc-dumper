using System;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace UI.Avalonia.Converters;

public class UpdateSymbolConverter: IValueConverter
{
    private static readonly Lazy<bool> HasFluentIcons = new(() => FontManager.Current.SystemFonts.Any(f => f.Name is "Segoe Fluent Icons"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string {Length: >0}
            ? HasFluentIcons.Value
                ? "\ue946"
                : "\uf05a" //"\uf06a" // exclamation mark in circle
            : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}