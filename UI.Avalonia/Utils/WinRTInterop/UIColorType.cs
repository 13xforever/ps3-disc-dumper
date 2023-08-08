using System.Runtime.InteropServices;
using Avalonia.Media;

namespace UI.Avalonia.Utils;

public enum UIColorType
{
    Background = 0,
    Foreground = 1,
    AccentDark3 = 2,
    AccentDark2 = 3,
    AccentDark1 = 4,
    Accent = 5,
    AccentLight1 = 6,
    AccentLight2 = 7,
    AccentLight3 = 8,
    Complement = 9
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal record struct WinRTColor
{
    public byte A;
    public byte R;
    public byte G;
    public byte B;

    public static WinRTColor FromArgb(byte a, byte r, byte g, byte b) => new WinRTColor()
    {
        A = a, R = r, G = g, B = b
    };

    public Color ToAvalonia() => new(A, R, G, B);
}