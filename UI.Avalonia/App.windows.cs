#if WINDOWS
using Avalonia.Controls;
using Avalonia.Platform;
using UI.Avalonia.Utils.ColorPalette;

namespace UI.Avalonia;

public partial class App
{
    partial void OnActivatedPlatform(Window w)
        => w.Resources.Remove("CaptionButtonForeground");

    partial void OnDeactivatedPlatform(Window w)
        => w.Resources["CaptionButtonForeground"] = w.FindResource(w.ActualThemeVariant, "CaptionGlyphInactive");

    partial void UpdateInactiveSystemButtonForegroundColor(PlatformColorValues values)
        => Resources["CaptionGlyphInactiveColor"] = ThemeConsts.InactiveSystemButtonForegroundColor(values.AccentColor1);
}
#endif
