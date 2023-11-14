#if WINDOWS
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia.Controls;
using IrdLibraryClient;
using TerraFX.Interop.Windows;

namespace UI.Avalonia.Utils;

[SupportedOSPlatform("windows")]
public static unsafe class WndProcHelper
{
    public delegate void OnWndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam);

    private static delegate* unmanaged<HWND, uint, WPARAM, LPARAM, LRESULT> wrappedWndProc;
    private static OnWndProc userFunc = null!;

    public static bool Register(Window window, OnWndProc onWndProc)
    {
        try
        {
            userFunc = onWndProc;
            var windowImpl = window.PlatformImpl!;
            //var platformHandle = windowImpl.GetType().GetProperty("Handle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(windowImpl);
            var windowHandle = (IntPtr)windowImpl.GetType().GetProperty("Hwnd", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(windowImpl)!;
            var hwnd = (HWND)windowHandle;
            wrappedWndProc = (delegate* unmanaged<HWND, uint, WPARAM, LPARAM, LRESULT>)TerraFX.Interop.Windows.Windows.GetWindowLongPtr(hwnd, GWLP.GWLP_WNDPROC);
            delegate* unmanaged<HWND, uint, WPARAM, LPARAM, LRESULT> wrapperWinProc = &WndProcHook;
            TerraFX.Interop.Windows.Windows.SetWindowLongPtr(hwnd, GWLP.GWLP_WNDPROC, (nint)wrapperWinProc);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to hook the WndProc event loop");
            return false;
        }
    }

    [UnmanagedCallersOnly]
    public static LRESULT WndProcHook(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        try
        {
            userFunc(hwnd, msg, wParam, lParam);
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception in user-defined event loop hook");
        }
        return wrappedWndProc(hwnd, msg, wParam, lParam);
    }
}
#endif