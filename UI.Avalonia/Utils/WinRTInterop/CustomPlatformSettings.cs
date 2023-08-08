using Avalonia.Media;
using UI.Avalonia.Utils.WinRTInterop;

namespace UI.Avalonia.Utils;

public record AccentColorInfo(Color Accent, Color Light1, Color Light2, Color Light3, Color Dark1, Color Dark2, Color Dark3);

public class CustomPlatformSettings
{
    public static AccentColorInfo GetColorValues()
    {
        var uiSettings = NativeWinRTMethods.CreateInstance<IUISettings3>("Windows.UI.ViewManagement.UISettings");
        var accent = uiSettings.GetColorValue(UIColorType.Accent).ToAvalonia();
        var light1 = uiSettings.GetColorValue(UIColorType.AccentLight1).ToAvalonia();
        var light2 = uiSettings.GetColorValue(UIColorType.AccentLight2).ToAvalonia();
        var light3 = uiSettings.GetColorValue(UIColorType.AccentLight3).ToAvalonia();
        var dark1 = uiSettings.GetColorValue(UIColorType.AccentDark1).ToAvalonia();
        var dark2 = uiSettings.GetColorValue(UIColorType.AccentDark2).ToAvalonia();
        var dark3 = uiSettings.GetColorValue(UIColorType.AccentDark3).ToAvalonia();
        return new (accent, light1, light2, light3, dark1, dark2, dark3);
    }
}