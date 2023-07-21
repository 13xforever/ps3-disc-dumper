#if WINDOWS
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia.Controls;
using TerraFX.Interop.Windows;

namespace UI.Avalonia.Utils;

[SupportedOSPlatform("windows")]
public static unsafe class WndProcHelper
{
    public delegate void OnWndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam);

    private static delegate* unmanaged<HWND, uint, WPARAM, LPARAM, LRESULT> wrappedWndProc;
    private static OnWndProc userFunc = null!;

    public static void Register(Window window, OnWndProc onWndProc)
    {
        userFunc = onWndProc;
        var windowImpl = window.PlatformImpl!;
        //var platformHandle = windowImpl.GetType().GetProperty("Handle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(windowImpl);
        var windowHandle = (IntPtr)windowImpl.GetType().GetProperty("Hwnd", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(windowImpl)!;
        var hwnd = (HWND)windowHandle;
        wrappedWndProc = (delegate* unmanaged<HWND, uint, WPARAM, LPARAM, LRESULT>)Windows.GetWindowLongPtr(hwnd, GWLP.GWLP_WNDPROC);
        delegate* unmanaged<HWND, uint, WPARAM, LPARAM, LRESULT> wrapperWinProc = &WndProcHook;
        Windows.SetWindowLongPtr(hwnd, GWLP.GWLP_WNDPROC, (nint)wrapperWinProc);
    }

    [UnmanagedCallersOnly]
    public static LRESULT WndProcHook(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        userFunc(hwnd, msg, wParam, lParam);
        return wrappedWndProc(hwnd, msg, wParam, lParam);
    }
}
#endif