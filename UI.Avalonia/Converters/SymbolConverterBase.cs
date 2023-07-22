using System;
using System.Linq;
using Avalonia.Media;

namespace UI.Avalonia.Converters;

public class SymbolConverterBase
{
    protected static readonly Lazy<bool> HasFluentIcons = new(() => FontManager.Current.SystemFonts.Any(f => f.Name is "Segoe Fluent Icons"));

}