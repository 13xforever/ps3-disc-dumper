using System;

namespace UI.Avalonia.Utils;

public static class OS
{
    public static bool IsWin11 => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);
    public static bool IsWin10 => OperatingSystem.IsWindowsVersionAtLeast(10);
    public static bool IsWin81 => OperatingSystem.IsWindowsVersionAtLeast(8, 1);
    public static bool IsWin80 => OperatingSystem.IsWindowsVersionAtLeast(8);
    public static bool IsWin7 => OperatingSystem.IsWindowsVersionAtLeast(6, 1);
    public static bool IsWinVista => OperatingSystem.IsWindowsVersionAtLeast(6);
}