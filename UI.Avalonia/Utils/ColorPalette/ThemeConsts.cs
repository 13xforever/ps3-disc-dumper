using Avalonia.Media;

namespace UI.Avalonia.Utils.ColorPalette;

public static class ThemeConsts
{
    // ref https://github.com/microsoft/microsoft-ui-xaml/blob/main/dev/Materials/Backdrop/MicaController.h#L34C61-L34C64
    public const string LightThemeTintColor = "#f3f3f3";
    public const double LightThemeTintOpacity = 0.5;

    public const string DarkThemeTintColor = "#202020";
    public const double DarkThemeTintOpacity = 0.8;

    public static readonly IPalette Debug = new DebugPalette();
    public static readonly IPalette Dark = new FluentPaletteDark();
    public static readonly IPalette Light = new FluentPaletteLight();

    public const string BrandColor = "#0094ff";

    internal static Color InactiveSystemButtonForegroundColor(Color accent)
    {
        // 102/255 ~ 40%: same factor as SystemBaseMediumLowColor's alpha
        const int blend = 102;
        var r = (byte)(accent.R + (255 - accent.R) * blend / 255);
        var g = (byte)(accent.G + (255 - accent.G) * blend / 255);
        var b = (byte)(accent.B + (255 - accent.B) * blend / 255);
        return Color.FromRgb(r, g, b);
    }
}
