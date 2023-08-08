using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace UI.Avalonia.Converters;

public class UpdateColorConverter: IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not [bool prerelease, string dimGrey, string accent])
            return Brushes.Red;

        return BrushConverter.Parse(prerelease ? dimGrey : accent);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}