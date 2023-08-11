namespace UI.Avalonia.Utils.ColorPalette;

public static class ThemeConsts
{
    // ref https://github.com/microsoft/microsoft-ui-xaml/blob/main/dev/Materials/Backdrop/MicaController.h#L34C61-L34C64
    public const string LightThemeTintColor = "#f3f3f3";
    public const double LightThemeTintOpacity = 0.5;
    public const double LightThemeMaterialOpacity = 0.8;
    public const double LightThemeLuminosityOpacity = 0;

    public const string DarkThemeTintColor = "#202020";
    public const double DarkThemeTintOpacity = 0.8;
    public const double DarkThemeMaterialOpacity = 0.8;
    public const double DarkThemeLuminosityOpacity = 0;

    public static readonly IPalette Debug = new DebugPalette();
    public static readonly IPalette Dark = new FluentPaletteDark();
    public static readonly IPalette Light = new FluentPaletteLight();

    public const string BrandColor = "#0094ff";
}