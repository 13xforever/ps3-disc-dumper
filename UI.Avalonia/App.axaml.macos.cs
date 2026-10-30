#if MACOS
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using IrdLibraryClient;

namespace UI.Avalonia;

public partial class App
{
    partial void ApplyPlatformStyles(Window w)
    {
        if (Current is not { } app)
            return;

        var style = new Style(s => s.Is<ComboBox>());
        style.Setters.Add(new Setter(ComboBox.PaddingProperty, new Thickness(12, 6.5, 0, 5.5)));
        style.Setters.Add(new Setter(Control.MinHeightProperty, 32.0));
        style.Setters.Add(new Setter(ComboBox.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        app.Styles.Add(style);

        LogFontMetrics(w);
    }

    private static void LogFontMetrics(Window w)
    {
        if (!FontManager.Current.TryGetGlyphTypeface(new(w.FontFamily), out var gt))
            return;

        var m = gt.Metrics;
        var scale = w.FontSize / m.DesignEmHeight;
        Log.Debug($"""
           [macos-font] family={gt.FamilyName} size={w.FontSize}px
                        ascent={-m.Ascent * scale:F2}px descent={m.Descent * scale:F2}px
                        lineGap={m.LineGap * scale:F2}px lineHeight={m.LineSpacing * scale:F2}px
           """
        );
    }
}
#endif
